# Doom map view

Checked against the Doom Test game copy on 2026-09-28. Parked. No change was made.

The map you open inside a level looks rough on purpose of how it is drawn, not because a picture is missing. Walls are a one-pixel-per-block sketch on the normal 7 Days map. The player arrow is the normal world-map arrow, so on a small level it covers about 12 blocks.

## What the map is

Opening the map inside a level replaces the terrain colors with lines from that level’s wall data. Those lines are baked when the mod is built, from the Doom maps, at 32 Doom units per block.

Doom 1 and Doom 2 do not have a map image per level. Their automap is drawn live from the same walls. The installs are useful for comparing the look. They are not a source of missing level pictures.

The only real pictures in the mod are the four end-of-level backdrops. Those are the tally screen, not this map.

| File | Size | Used for |
|---|---|---|
| `inter_wimap0.png` | 560×200 | Doom 1 episode 1 tally |
| `inter_wimap1.png` | 560×200 | Doom 1 episode 2 tally |
| `inter_wimap2.png` | 560×200 | Doom 1 episode 3 tally |
| `inter_interpic.png` | 2048×1280 | Doom 2 (and Doom 1 episode 4) tally |

Vanilla WIMAP art is 320×200. The files on disk are already 560×200, so they are wide before the UI stretches them. The tally window sets `keepsourceaspectratio="false"` and pins the picture to the full camera.

## Why the walls look broken

Each wall is one pixel on the game’s 2048×2048 map texture, and one pixel is one block. E1M1 is 144×89 blocks, so the whole level is a small patch of that texture. The map window then stretches a slice of it across a 712×712 view.

- Zoomed in, a one-block line becomes a thick staircase.
- Zoomed out, point filtering skips pixels, so walls break into gaps.
- Diagonals and anything shorter than a block lose the most.

`DoomLevelsMpFix` sets that texture to point filtering so the lines are not blurred. That is `Patch_MapFilter.cs`.

You also only see walls you have already looked at, and only the ones Doom’s automap would show: solid walls, steps, and ceiling changes. A two-sided line with the same floor and ceiling on both sides is left out on purpose.

The game’s map shader is locked to one texel per block. A cleaner picture cannot come from a higher-resolution image in that texture. Crisp walls would have to be drawn as lines in the map window itself.

## Why the arrow is the wrong size

The arrow is `ui_game_symbol_map_player_arrow`. In `XUiC_MapArea`:

- Sprite base size is 50. Player icon scale defaults to 1.
- Zoom factor is `1 / (zoom * 2)`, so width is about `25 / zoom`.
- Width and height are clamped between 9 and 100.
- The map itself moves at about `2.12 / zoom` screen units per block (`factorScreenSizeToDTM` is 2.1190476).

Until the clamp kicks in, the arrow is about 12 blocks wide. On the world map that reads as a small marker. On a level that is 70 to 200 blocks across, it covers a room.

Zoom makes it worse. Past the clamp, the arrow stops tracking the map: far out it stays at least 9 pixels and covers dozens of blocks; far in it caps at 100 pixels while the walls keep growing. Doom’s own arrow stays a small fixed size on screen while the map scales under it.

Doom’s own zoom range inside a level is 0.15 to 6.15 (`GameMap.LevelMinZoom` / `LevelMaxZoom`). Vanilla’s closest zoom is 0.7. At zoom 1 the visible span is 336 blocks (`MapSizeZoom1`).

## Why the arrow is not centered

`PositionMapAt` snaps the view to the 16-block chunk under you, then the Doom patch pulls that point back inside the level (`GameMap.ClampToLevel`). Vanilla does the same chunk snap and then clamps to the world. On the world map, 8 blocks of error is invisible. On these levels it shoves the arrow off the middle of the screen. The arrow and the walls still use the same world coordinates.

## Where it lives

- Draw: `00_Support/DoomLevelsSource/DoomLevels/mod/UI/Map.cs`
- Seen-walls sweep: `.../mod/Logic/Automap.cs`
- Zoom, drag, center: `.../mod/Patches/GameMap.cs`
- Line bake (Doom units / 32): `.../src/Output/SoundBake.cs` `WriteLines`
- Live levels: Doom Test `Mods/zzzz_DoomClassicMaps` (`Config/doomlevels.xml`, `DoomLevels.dll`)
- Point filter: `00_DLL-Projects/Projects/DLL_DoomLevelsMpFix/Patch_MapFilter.cs`
- Vanilla map window and arrow math: Doom Test `XUiC_MapArea` and `Data/Config/XUi_InGame/windows.xml` (`mapView` is 712×712)

## If this is picked up later

- Draw the walls in the map window, so they stay one pixel thick at any zoom.
- While you are in a level, size the arrow to about one block, or a small fixed screen size, instead of `25 / zoom`.
- Center on the real position, not the chunk under you.
