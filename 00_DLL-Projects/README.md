# 00 DLL Projects

First-class source and development support for AGF DLL-based 7 Days to Die mods.

This is an upstream workspace input alongside `00_Images`: DLL projects are built
here, while assembled mods continue through `01_Draft`, `02_ActiveBuild`, and
`03_ReleaseSource`.

## Current layout

```text
00_DLL-Projects/
|-- Projects/                       C# DLL projects (each with its own .csproj)
|-- Generators/                     DLL-adjacent source generators
|-- Directory.Build.props           shared MSBuild settings (Steam dir, Harmony DLL path)
`-- README.md
```

There is intentionally no shared `BuildTemp/` folder. Each project builds its
own `obj/`/`bin/` output directly inside its own folder under `Projects/`
(standard MSBuild default behavior) - these are gitignored
(`00_DLL-Projects/**/obj/`, `00_DLL-Projects/**/bin/`) rather than swept into
one shared scratch location.

`Support/`, `Tools/`, and `References/` (empty) were removed during the
2026-07-20/21 reorg - see `WORKSPACE-ORGANIZATION-PLAN.md` Progress Log.
Discord server administration now lives under `DiscordManagement/ServerPlan`.

## DLL synchronization

Preview DLL synchronization from the repository root with:

```bat
python 00_DLL-Projects\Tools\SCRIPT-PushUpdatedDLLs.py
```

Use `--apply` only after reviewing the preview and closing the game. The sync
tool discovers the `Projects/DLL_*` project folders and excludes generators,
build output (`obj`/`bin`), and other non-project directories while walking
the tree.

## Folder move troubleshooting

If Windows denies future project-folder moves, temporarily disable C# Dev Kit for
the workspace and run `dotnet build-server shutdown`. Its project system and the
.NET compiler server can retain handles on loaded project trees.
