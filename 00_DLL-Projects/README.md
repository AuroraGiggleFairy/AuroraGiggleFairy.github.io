# 00 DLL Projects

First-class source and development support for AGF DLL-based 7 Days to Die mods.

This is an upstream workspace input alongside `00_Images`: DLL projects are built
here, while assembled mods continue through `01_Draft`, `02_ActiveBuild`, and
`03_ReleaseSource`.

## Current layout

```text
00_DLL-Projects/
|-- Projects/       C# DLL projects
|-- Generators/     DLL-adjacent source generators
|-- Support/        project-specific retained support material
|-- References/     research and local-only decompiled references
|-- Tools/          DLL-development utilities and templates
`-- BuildTemp/      local-only build scratch
```

Discord server administration now lives under `20_DiscordManagement/ServerPlan`.

## DLL synchronization

Preview DLL synchronization from the repository root with:

```bat
python SCRIPT-SyncNoEACDlls.py
```

Use `--apply` only after reviewing the preview and closing the game. The sync
tool discovers the `Projects/DLL_*` project folders and excludes generators,
support material, references, and build scratch data.

## Folder move troubleshooting

If Windows denies future project-folder moves, temporarily disable C# Dev Kit for
the workspace and run `dotnet build-server shutdown`. Its project system and the
.NET compiler server can retain handles on loaded project trees.
