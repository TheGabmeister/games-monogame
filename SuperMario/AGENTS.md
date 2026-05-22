# AGENTS.md

## Project

This is a reusable MonoGame + Nez template project.

The app entry point is `Source/Program.cs`, which creates `SuperMario.Game1`.
`Game1` inherits from `Nez.Core` and sets the initial Nez scene.

## Build

Use:

```powershell
dotnet build SuperMario.slnx -v minimal
```

The expected result is a clean build with 0 warnings and 0 errors.

## Nez Dependency

Nez is referenced as a local project dependency, not a Git submodule.

Current reference:

```xml
<ProjectReference Include="D:\Nez\Nez.Portable\Nez.MG38.csproj" />
```

This project uses MonoGame 3.8 packages, so use `Nez.MG38.csproj` for Nez.

Do not add Nez source under this template unless explicitly asked.

## Layout

- `Source/Game1.cs` boots Nez and assigns the first scene.
- `Source/Scenes/MainScene.cs` contains the starter scene.
- `Source/Constants.cs` holds shared template constants such as window size and title.
- `Source/Assets.cs` is intended to hold generated or curated content path constants.
- `Content/Content.mgcb` is the MonoGame content pipeline file.
- `Assets/` contains project icon and Windows manifest files.

## Nez Patterns

Prefer Nez scenes, entities, and components over raw MonoGame `Update`/`Draw` code.

For player or entity controls, follow the Nez samples' pattern: create `VirtualButton`,
`VirtualIntegerAxis`, or `VirtualJoystick` fields inside the controlling component and
call `Deregister()` when the component is removed.

Use raw `Nez.Input` only for very small one-off sample/demo behavior.

## Content

If Nez effects or post-processors are used, add or link Nez default content:

- `D:\Nez\DefaultContent\effects` -> `Content\nez\effects`
- `D:\Nez\DefaultContent\textures` -> `Content\nez\textures`

Keep content paths in sync with `Content/Content.mgcb`.

