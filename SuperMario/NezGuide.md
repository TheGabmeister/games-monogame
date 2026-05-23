# Nez Guide — Quirks & Gotchas

Hard-won knowledge from working with Nez. Read before reinventing.

## Triggers & Colliders

### Never replace a collider during a trigger callback

If `ITriggerListener.OnTriggerEnter` (or any code reached from it) does:

```csharp
Entity.RemoveComponent<BoxCollider>();
Entity.AddComponent(new BoxCollider(...));
```

…the next frame crashes with `NullReferenceException` in `ColliderTriggerHelper.NotifyTriggerListeners` (line 97). Nez's trigger helper holds a `Pair<Collider>` referencing the old collider in `_previousTriggerIntersections`. When it processes the exit event, `collisionPair.First.Entity` is null because the collider was detached. Note: line 109 null-checks `collisionPair.Second.Entity`, but line 97 does **not** check `First`.

**Fix:** resize the existing collider in-place:

```csharp
var box = Entity.GetComponent<BoxCollider>();
box.SetSize(newWidth, newHeight);
box.SetLocalOffset(Vector2.Zero);
```

### Destroying entities in trigger callbacks

Destroying the *other* entity (`collisionPair.Second`) inside `OnTriggerEnter` is safe — Nez null-checks it before processing exits. Destroying *yourself* via `Entity.Destroy()` is also safe with `ITriggerListener`, because the destroy is queued for end-of-frame. **But** if you also mutate your own collider in the same callback (see above), the queued destroy can't save you.

### `ITriggerListener` over polling

Don't poll for overlaps with `Physics.BoxcastBroadphase` in `Update`. Mark one collider as `IsTrigger = true`, implement `ITriggerListener` on any component on either entity, and Nez's `Mover` will fire `OnTriggerEnter`/`OnTriggerExit` automatically. The listener is found via `Entity.GetComponents<ITriggerListener>()` — both sides of the pair are notified.

### `BoxCollider` constructor convention

`new BoxCollider(x, y, width, height)` takes a **top-left offset** but stores it internally as a center offset:

```csharp
_localOffset = new Vector2(x + width / 2, y + height / 2);
```

To match a sprite drawn from the entity center, always pass `(-w/2, -h/2, w, h)`. The two-arg overload `BoxCollider(width, height)` already does this.

### Resizing in-place updates physics broadphase

`BoxCollider.SetSize` calls `Physics.UpdateCollider(this)` when the entity is in a scene. You don't need to manually re-register.

## HUD rendering with `ScreenSpaceRenderer`

`ScreenSpaceRenderer`'s camera is created lazily on the first `OnSceneBackBufferSizeChanged`; before then the renderer falls back to an **identity transform**. So HUD entities should be positioned in **raw backbuffer pixels with `(0,0)` at the upper-left** — not the centered (0,0)-is-screen-center convention you'd expect from a normal camera.

Standard pair for a gameplay scene with a HUD:

```csharp
AddRenderer(new RenderLayerExcludeRenderer(0, RenderLayers.Hud));   // world
AddRenderer(new ScreenSpaceRenderer(1, RenderLayers.Hud));          // HUD
```

## Renderables

### Mutate `PrototypeSpriteRenderer` in place

`PrototypeSpriteRenderer` exposes fluent `SetWidth(float)`, `SetHeight(float)`, plus the inherited `SetColor(Color)`. To change size or color, mutate the existing instance rather than removing and re-adding:

```csharp
var renderer = Entity.GetComponent<PrototypeSpriteRenderer>();
renderer.SetWidth(w);
renderer.SetHeight(h);
renderer.SetColor(color);
```

Origin is centered (`Vector2Ext.HalfVector()`, set in `OnAddedToEntity`), so resize pivots from the entity center.

Unlike colliders (which *must* be mutated in place because of `ColliderTriggerHelper`), renderers don't crash if you remove-and-re-add — but the swap loses any state on the old instance and invalidates cached references held by other components.

## Physics Layers

Layers in `Constants.cs` are **bit positions** (0, 1, 2…), not bitmasks. Use them with `1 << PhysicsLayers.X`:

```csharp
collider.PhysicsLayer = 1 << PhysicsLayers.Player;
collider.CollidesWithLayers = (1 << PhysicsLayers.Environment) | (1 << PhysicsLayers.Item);
```

Both sides of a desired collision must opt in via `CollidesWithLayers`. Trigger detection respects layers too.

## Mover

`Mover.CalculateMovement(ref motion, out CollisionResult)` returns only **one** `CollisionResult` even if multiple collisions occurred. For ground detection use `result.Normal.Y < 0` (Nez Y-down means floor normal points up — negative). Zero out velocity component on the collided axis manually:

```csharp
if (result.Normal.Y < 0 && _velocity.Y > 0) _velocity.Y = 0;
```

`Mover.ApplyMovement` internally calls `ColliderTriggerHelper.Update`, which is where trigger enter/exit events get dispatched. This means your `Update()` is **already inside that pipeline** when triggers fire — see "Never replace a collider during a trigger callback."

## Tweens

### Cleanup is the caller's job

Tweens registered with `TweenManager` keep ticking even after their target entity is destroyed. A tween writing to an orphaned `Transform` won't crash — Transform is a managed object that lives until GC — but it's leaked work.

Stop tweens in `OnRemovedFromEntity`:

```csharp
TweenManager.StopAllTweensWithTarget(Entity.Transform);
```

The API is `StopAllTweensWithTarget(object target)` — taking the target object (typically the `Transform` for position tweens). The closely-named `AllTweensWithTargetEntity(Entity)` is a **getter** that returns the list; there is no `StopAllTweensWithTargetEntity`. Easy to misremember.

### Infinite oscillation with pauses — use `LoopType.PingPong`

For an emerge/pause/retract/pause loop (e.g. a Piranha Plant bobbing out of a pipe), use ping-pong with `delayBetweenLoops`. No callback chains, no `Core.Schedule`:

```csharp
Entity.Transform.TweenPositionTo(exposedPos, EmergeDuration)
    .SetEaseType(EaseType.Linear)
    .SetLoops(LoopType.PingPong, -1, PauseDuration)
    .Start();
```

`SetLoops(PingPong, -1, delay)` = infinite ping-pong with the same pause on both halves. There's no API for asymmetric end-pauses; if you need them, use a state machine.

`LoopType` and `EaseType` live in `Nez.Tweens`; remember `using Nez.Tweens;`.

## Schedule

`Core.Schedule(seconds, callback)` returns an `ITimer` that fires after the delay. Pitfalls:

- **No auto-cancel** when the calling component or entity dies. The callback still fires and writes to a dead object. Save the returned `ITimer` and call `.Stop()` in `OnRemovedFromEntity` if cleanup matters.
- **One-shot by default**; use the overload that takes `repeats: true` for periodic ticks. `ITimer` also exposes `Reset()` and `RemainingTime()` if you want to manage it.

Use Schedule when the work genuinely belongs in the future and the component would have nothing else to do meanwhile (respawn delays, long despawn timers). For short one-shots inside an already-ticking component, a `float` field decremented in `Update` is usually simpler and avoids the cleanup chore.

## Rotation

- Tiled rotation is **degrees, clockwise, around the object's top-left corner**.
- Nez `Entity.Rotation` is **radians**; `Entity.RotationDegrees` is the degree-shortcut.
- `BoxCollider` inherits its rotation from the entity (via the underlying `Polygon` shape), so setting `Entity.RotationDegrees` rotates both sprite and collider.
- When importing a Tiled object's `(X, Y)` as an entity center, you must rotate the half-width/half-height offset around the Tiled origin to get the true center:

```csharp
var offset = new Vector2(obj.Width / 2f, obj.Height / 2f);
var rad = MathHelper.ToRadians(obj.Rotation);
var cos = Mathf.Cos(rad); var sin = Mathf.Sin(rad);
offset = new Vector2(cos * offset.X - sin * offset.Y, sin * offset.X + cos * offset.Y);
return new Vector2(obj.X + offset.X, obj.Y + offset.Y);
```

## Tiled (`Nez.Tiled`)

### Loading

`Content.LoadTiledMap("Content/foo.tmx")` reads the raw `.tmx` from the output directory — it does **not** go through the MGCB pipeline. The csproj must copy the file:

```xml
<Content Include="Content\**\*.tmx" CopyToOutputDirectory="PreserveNewest" />
```

Path is relative to the executable's working directory.

### Object types

- `TmxObject.Type` reads Tiled's **Class** field (Tiled renamed "Type" to "Class" in 1.9+; XML still uses `type=`).
- `TmxObject.Name` is the optional unique label.
- `TmxObject.Properties` is `Dictionary<string, string>` for custom properties.
- Iterate via `map.GetObjectGroup("entities").Objects`.

### Object ordering

Object iteration order in the `.tmx` is the order they were created in Tiled. **Don't rely on it for dependency resolution.** If one entity needs a reference to another (e.g. a Mushroom referencing the PlayerController), either:

1. Spawn everything as marker entities first, then resolve dependencies in a second pass (use `Scene.FindEntitiesWithTag`), or
2. Look up the reference lazily at use-time (`Entity.Scene.FindEntity("player").GetComponent<PlayerController>()`).

### Queued entities and tag lookup

Entities created during `Scene.OnStart()` are queued first. Nez moves them into the live entity list and tag dictionary when `Scene.Update()` calls `Entities.UpdateLists()`.

This means `Scene.FindEntitiesWithTag(tag)` will not find an entity that was just created earlier in the same `OnStart()` call. The tag dictionary has not caught up yet.

For startup markers like `PlayerStart`, prefer a marker component and `Scene.FindComponentOfType<PlayerStart>()`. Nez's component lookup checks both live entities and queued entities, so it works during `OnStart()`.

Avoid using tag `0` for meaningful tags. Nez entities default to `Tag == 0`, so project tags should start at `1`.

## Input

```csharp
_jumpButton = new VirtualButton();
_jumpButton.AddKeyboardKey(Keys.Space);
```

`VirtualButton` / `VirtualIntegerAxis` / `VirtualJoystick` need explicit `Deregister()` in `OnRemovedFromEntity`, otherwise they keep ticking and leak.

`VirtualIntegerAxis.AddKeyboardKeys(OverlapBehavior, negativeKey, positiveKey)` — the **negative** key comes first. `Keys.Left` then `Keys.Right` gives -1 / +1.

Raw `Nez.Input` (e.g. `Input.IsKeyPressed`) is fine for one-off demo code but bypasses the virtual input abstraction.

## Events

Two systems coexist:

- **`Nez.Systems.Emitter<TEnum>`** — type-safe pub/sub keyed on an enum. `Core.Emitter` exists for engine events (`CoreEvents.GraphicsDeviceReset`, etc.). `Input.Emitter` for input events. **There is no `Scene.Emitter`** — create your own static `Emitter<GameEvents>` for game-wide events.
- **C# `event Action`** on a specific component — preferred when one specific subscriber owns a direct reference. Don't route through a global emitter just because.

## Scenes & Content

- Each `Scene` has its own `NezContentManager`. Assets loaded via `scene.Content.Load<T>(...)` are **disposed when the scene is swapped out** — perfect for level-specific content.
- `Core.Content` (the global content manager) persists for the app lifetime — use it for assets that should survive scene transitions (UI fonts, music, shared atlases).
- `Entity.Destroy()` removes the entity from the scene but **does not unload assets** the components reference. The content manager owns asset lifetime, not entities.

## GlobalManager

- Registered via `Core.RegisterGlobalManager(new Foo())` from `Game1.Initialize()`.
- `OnEnabled` is called once when registered. `Update` ticks every frame independent of the current scene.
- Persists across scene swaps — perfect for `GameManager`, `MusicManager`, save systems.
- Access from anywhere via `Core.GetGlobalManager<T>()`. This is service-locator-style — convenient but hides dependencies. Use deliberately.

## Debug Rendering

Set `Core.DebugRenderEnabled = true` from `Game1.Initialize()` (after `base.Initialize()`) to draw collider outlines, entity origins, etc. The Nez debug console (tilde `~`) is always available — `inspect <entityName>` opens the runtime inspector.

## Component Dependencies

Nez has no `[RequireComponent]` attribute (Unity equivalent does not exist). To enforce that a component depends on a sibling:

- **Assert in `OnAddedToEntity`** — closest to `[RequireComponent]`'s intent. Fails loudly at attach time, before any frame ticks:

```csharp
public override void OnAddedToEntity()
{
    if (Entity.GetComponent<RenderableComponent>() == null)
        throw new InvalidOperationException(
            $"{nameof(MyComponent)} requires a RenderableComponent.");
}
```

- **Inject via constructor** — stronger compile-time-ish guarantee; the caller can't construct one without supplying the dependency. Use when the reference is stable. Don't use if the dependency might be removed and re-added on the same entity (the cached reference goes stale).

Neither pattern auto-adds the dependency à la Unity. The component must already exist when the dependent is attached, which usually means ordering correctly inside a factory method.

## `Component` Lifecycle

- `OnAddedToEntity` — entity reference is set; safe to grab sibling components with `Entity.GetComponent<T>()`.
- `OnRemovedFromEntity` — last chance to clean up subscriptions, `Deregister()` virtual inputs, etc.
- `IUpdatable.Update()` — opt-in by implementing the interface; not all components tick.
- Component constructors run **before** `OnAddedToEntity`, so don't touch `Entity` from a constructor.
