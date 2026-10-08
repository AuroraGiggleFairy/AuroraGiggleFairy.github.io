# ModSync wording

Every line a player or admin can see, in the order they meet it.

**How to use this:** edit the text inside the code blocks, then tell me to apply it.
Each block lists the file it lives in so nothing gets missed.

`{...}` marks a value filled in at runtime. Keep those, and keep `\n` line breaks
where they appear in the C# strings.

Two places own this text:

| Where the player sees it | File that holds the text |
|---|---|
| The `.bat` console (setup) | `engine/INSTALL-ServerSync.ps1` |
| The `.bat` console (header and startup errors) | `generate.py` |
| In-game progress and failure boxes | `DLL_NoEAC-ModSync/ModSyncClient.cs` |
| The console during the restart | `DLL_NoEAC-ModSync/ModSyncClient.cs` |
| Server console and log | `DLL_NoEAC-ModSync/ModSyncServer.cs` |

---

## 1. Setup console — the player double-clicks the bat

### 1.1 Window title and banner
Source: `generate.py`, header

```
ModSync - {ServerName}
```

```
REM  ModSync for {ServerName}
REM
REM  Double-click this file. It sets up your game for this
REM  server and puts a shortcut on your desktop.
REM  The mods themselves come from the server when you play.
REM
REM  Close 7 Days to Die before running this.
```

### 1.2 Server name shown at the top
Source: `INSTALL-ServerSync.ps1:412`

```
  {ServerName}
```

### 1.3 Could not find the game
Source: `INSTALL-ServerSync.ps1:179-201`

```
  ModSync could not find 7 Days to Die by itself.

  Steam folders it checked:
    {folder}

  In Steam, right-click 7 Days to Die, choose Manage, then
  Browse local files. Copy the folder path from the address bar
  at the top of the window and paste it below.

  Paste the folder path (or press Enter to quit):
```

When Steam itself is missing, the middle line becomes:

```
  It could not find Steam itself on this PC.
```

Bad paste:

```
  That folder has no 7DaysToDie.exe in it. Try again.
```

### 1.4 Returning player
Source: `INSTALL-ServerSync.ps1:417-418`

```
  Last time you used: {folder name}
  Press Enter to use it again, or type C to choose a different one.
```

### 1.5 Choosing which game to use
Source: `INSTALL-ServerSync.ps1:435-465`

```
  Which game should this server use?

    1) Make a copy just for this server  (recommended)
    2) {SteamFolderName}  (your main Steam game)
    3) {OtherInstallName}

  A copy just for this server is safest. Your main game keeps its own mods.

  Type a number:
```

If the server copy already exists, option 1 becomes:

```
    1) {CopyFolderName}  (already set up for this server)
```

Bad input:

```
  Please type one of the numbers above.
```

### 1.6 Confirmation before a folder is handed to the server
Source: `INSTALL-ServerSync.ps1:333-362`

Shown when the chosen folder is not already set up for this server, and it is not a
fresh copy. Skipped when `ModSync-Managed.txt` is already there.

```
  Please read this before continuing.

  You picked:  {full path}

  {ServerName} will decide which mods this game uses.
  Files it replaces, and mods it does not use, are moved to:
  {path}\ModSync-Removed
  Nothing is deleted. Each join keeps its own dated folder.

  This setup downloads a server pack first.
  That pack is compared the same way before it is copied in.
  Anything it would replace, and anything that is not in the pack,
  is moved into ModSync-Removed first.

  This game already has {count} mod(s) of your own:
    - {mod name}
    ...and {count} more

  Type YES to use this game, or press Enter to pick a different one.
```

The pack lines are only there when the bat has a download link. The mod list is only
there when the folder already has mods. Saying YES does not move anything yet. The
pack install, or the later join, does the move.

Declined:

```
  Nothing was changed.
```

### 1.7 Making the copy
Source: `INSTALL-ServerSync.ps1:478-490`

```
  Copying the game. This is big, so it can take a while...
  Copy finished.
```

```
  Note: 0_TFP_Harmony was not found in your Steam game.
```

Using a folder as-is:

```
  Using: {folder name}
```

### 1.8 Finished
Source: `INSTALL-ServerSync.ps1:391-395`

```
  All set.

  A shortcut named "{ServerName}" is on your desktop.
  Use it to play. The first time, it downloads the server mods and
  restarts itself once. If the server asks for a password, type it in the game.
```

### 1.9 Setup errors
Source: `INSTALL-ServerSync.ps1` and `generate.py`

```
  Could not start ModSync. Ask the admin to send the file again.
  ModSync did not finish. Nothing was broken.
```

```
This file is not meant to be run directly. Use the ModSync .bat.
The server address is missing. Ask {ServerName}'s admin for a new ModSync file.
Could not find the ModSync file to read the mod out of.
The ModSync file is incomplete. Ask the admin to send it again.
The ModSync file is damaged. Ask the admin to send it again.
No 7 Days to Die folder was given, so nothing was changed.
Copy failed. Details: {log path}
```

---

## 2. In game — progress box while joining

Source: `ModSyncClient.cs`

### 2.1 Checking
`ModSyncClient.cs:51`

```
Checking mods with the server...
```

### 2.2 Nothing to do
`ModSyncClient.cs:314`

```
Joining server...
```

### 2.3 Downloading
`ModSyncClient.cs:BuildProgressText`

```
Downloading server mods
File {n} of {total}
{done} of {total}  ({rate}/s)
About {time} left
```

The rate and the time remaining only appear after five seconds.
Time reads as `{n} seconds`, `{n} minutes`, or `{n} hours`.

### 2.4 Waiting in line on a busy server
`ModSyncClient.cs:378`

```
Waiting for the server to reach you
You are {place} of {total} in line
{size} to download when your turn comes
```

### 2.5 Done, about to restart
`ModSyncClient.cs:518`

```
Mods downloaded.
Restarting the game to finish...
```

### 2.6 Failures
`ModSyncClient.cs:105` wraps every failure with:

```
Mod sync failed:
{reason}
```

Reasons:

```
Not enough free space for the server's mods.
Need about {size} plus room to spare, but only {size} is free.
```

```
Tried 3 times without getting any further.
Check that the game folder is not read-only and that the disk is not full.
```

```
Could not read the server mod list: {error}
Could not prepare the download folder: {error}
Could not save a mod file: {error}
Could not restart to apply mods: {error}
```

---

## 3. Console window during the restart

Source: `ModSyncClient.cs:LaunchApplier`

```
ModSync - installing server mods

  Finishing the mod check...
  The game will start again by itself. Please wait.

  Moving mods out of the user Mods folder.
  Keeping them in {AppData}\7DaysToDie\Mods - Backup\{date time}
  Keeping the previous copies in ModSync-Removed\{date time}
  Setting aside {count} mod(s) this server does not run...
    - {mod name}
  Clearing {count} old file(s) out of mods the server keeps...
  Saving {count} file(s) the server is about to replace...
  Putting the mods in place...
```

If a file is open in another program, nothing is moved and the game stays closed.
The download is thrown away. The next login starts over.

```
  This file is open in another program, so it cannot be updated:

  Mods\{path}

  Close that program, then log in again.

  Nothing was changed.
```

More than one file uses "These files" and "Close the programs that have them open, then log in again."

---

## 4. Admin side

### 4.1 Generator output
Source: `generate.py`

```
  Packed {count} file(s) from {mod folder}
  Wrote {path}  ({size} KB)
  Send that one file to players. Nothing else is needed.
```

Config complaints:

```
Set SERVER_NAME in ADMIN-CONFIG.txt to your server's name.
Set SERVER_IP in ADMIN-CONFIG.txt.
Set SERVER_PORT in ADMIN-CONFIG.txt to the server's port number.
Change CLIENT_BAT_NAME in ADMIN-CONFIG.txt, or leave it blank.
No ModSync.dll in {path}. Build DLL_NoEAC-ModSync first.
Could not find the ModSync mod in 01_Draft. Build DLL_NoEAC-ModSync, or set MOD_SOURCE in ADMIN-CONFIG.txt.
```

### 4.2 Server log
Source: `ModSyncServer.cs`, all prefixed `[ModSync] `

```
Server half ready. Serving mods from {path}
Checking {count} mod file(s). The first run after a mod change reads them all, which can take a few minutes on a large pack.
Manifest built: {count} files, {size} in {count} chunk(s), took {n}s.
Mod set is {size}: up to {count} downloader(s) at once, {n} MB/s shared.
Client {ip} asked for the mod manifest (protocol {n}).
Sending {count} file(s), {size} to {ip}
Queued {ip} at place {n} for {size}.
Finished sending to {ip}
Mod list changed while {ip} was comparing. Asking it to recheck.
```

### 4.3 Files ModSync leaves in a game folder

Worth knowing when reading a player's report.

```
ModSync-Managed.txt    written by setup only. The game does not read it. A later setup run uses it so it does not ask again
ModSync-Backup.txt     log of what a pack install moved aside
ModSync-Hashes.txt     remembered file hashes, so launches are fast
ModSync-Staging        part-finished download, removed when applied. Also removed when an open file stops the update
ModSync-Removed        one dated folder per join, and per pack install, holding mods and files that were removed or replaced
```

The game also loads `%AppData%\Roaming\7DaysToDie\Mods`. On a join, everything in that folder is moved to `Mods - Backup\{date time}` beside it. The `Mods` folder stays, empty. That AppData folder is shared by every install that uses the default user data path.
