# ModSync — player paths

The flowchart is the canvas: **ModSync player paths**.

Setup gets Mods ready first: anything besides Harmony and ModSync is moved to backup. Then, if the admin put a pack link in the `.bat`, that zip is downloaded into Mods. Join only pulls leftovers from the server.

If there is no pack link, the folder is still prepared, and the first join gets the files from the server.

## What each choice does

**Make a copy**  
New folder next to Steam. Their main game is not touched.

**Main Steam game, or any other folder**  
Other mods are backed up first so the folder can receive the pack or the server files.

**Pack link in the .bat**  
The heavy download happens in the console, game closed. Every folder with `ModInfo.xml` goes into `Mods`. Readmes and other leftover files are ignored. Nested zip/rar files are opened. Wrapped or doubled folder names are flattened.

**No pack link**  
Same as before: first join downloads from the server.

**Join**  
If the pack is already in place, they play. If anything is still missing, the server fills the gaps.

**Desktop shortcut**  
Named for the server. Starts that game folder with EAC on or off as the admin set. At the main menu it joins the server.

**Run the .bat again**  
Reinstalls ModSync. Does not download the pack again if that folder already has this seed.

**Join again later**  
No backup. Missing files from the server. Extra mods are set aside.
