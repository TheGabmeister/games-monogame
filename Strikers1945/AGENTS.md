# AGENTS.md

Guidance for coding agents working in this repository.

## Project Overview

This is a MonoGame DesktopGL project using .NET 9 and MonoGame.Extended.

- Entry point: `Program.cs`
- Main game class: `Game1.cs`
- Project file: `Extended.csproj`
- Content pipeline file: `Content/Content.mgcb`
- MonoGame.Extended pipeline DLLs: `pipeline-references/`

## Build and Run

Use:

```powershell
dotnet run
```

If the local MonoGame tools are missing, restore them first:

```powershell
dotnet tool restore
```

The project uses MonoGame's content builder task, so `dotnet run` also invokes `dotnet mgcb` against `Content/Content.mgcb`.

## Content Pipeline Notes

Keep this reference in `Content/Content.mgcb`:

```text
/reference:..\pipeline-references\MonoGame.Extended.Content.Pipeline.dll
```

Do not replace it with a deep relative path into the user's NuGet cache such as `..\..\..\..\Users\...\.nuget\packages\...`. `mgcb` can misread that form as content to build and fail with a vague `MSB3073` error.

When adding game assets, add them through `Content/Content.mgcb` so they are built into `.xnb` files and loaded through `Content.Load<T>()`.

## ECS (MonoGame.Extended.ECS)

Namespaces are `MonoGame.Extended.ECS` (`World`, `WorldBuilder`, `Entity`, `Aspect`) and `MonoGame.Extended.ECS.Systems` — **not** `MonoGame.Extended.Entities`. System base classes: `EntityProcessingSystem` (per-entity `Process`), `EntityUpdateSystem` (one `Update`, loop `ActiveEntities` yourself), `EntityDrawSystem`.

Local ECS code lives in `Components/` (plain data classes) and `Systems/`.

Reference samples on this machine at `D:\MonoGame-Extended-Samples` (clone of the official samples repo):

- `Demos/Sandbox` — simplest; the rain demo. Best starting point.
- `Games/Platformer` — `EntityFactory` pattern, tile collisions.
- `Games/StarWarrior` — full game; 8 components / 10 systems. The reference architecture.

`Games/SpaceGame` has an `Entities/` folder but does **not** use the ECS framework — don't cite it as an ECS example.

## Coding Style

- Keep game logic in `Game1.cs` unless the feature is large enough to justify a new type.
- Prefer MonoGame and MonoGame.Extended APIs already referenced by the project.
- Keep content paths relative to `Content.RootDirectory`, which is set to `Content`.
- Avoid unrelated formatting churn in generated or tool-managed files.

## Troubleshooting

If `dotnet run` fails with:

```text
MonoGame.Content.Builder.Task.targets(...): error MSB3073
```

run the `mgcb` command without `/quiet` to reveal the real content pipeline error. The MSBuild error is usually only a wrapper around the underlying `mgcb` failure.
