// The directory was not found.

System.IO.DirectoryNotFoundException: Could not find a part of the path 'C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods\0RedQueen\RedQueenMod.dll'.
   at Microsoft.Win32.SafeHandles.SafeFileHandle.CreateFile(String fullPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
   at Microsoft.Win32.SafeHandles.SafeFileHandle.Open(String fullPath, FileMode mode, FileAccess access, FileShare share, FileOptions options, Int64 preallocationSize, Nullable`1 unixCreateMode)
   at System.IO.Strategies.OSFileStreamStrategy..ctor(String path, FileMode mode, FileAccess access, FileShare share, FileOptions options, Int64 preallocationSize, Nullable`1 unixCreateMode)
   at System.IO.Strategies.FileStreamHelpers.ChooseStrategyCore(String path, FileMode mode, FileAccess access, FileShare share, FileOptions options, Int64 preallocationSize, Nullable`1 unixCreateMode)
   at System.IO.FileStream..ctor(String path, FileMode mode, FileAccess access)
   at ICSharpCode.ILSpyX.LoadedAssembly.<>c__DisplayClass54_0.<<LoadAsync>g__PrepareStream|0>d.MoveNext() in /_/ICSharpCode.ILSpyX/LoadedAssembly.cs:line 409
--- End of stack trace from previous location ---
   at ICSharpCode.ILSpyX.LoadedAssembly.LoadAsync(Task`1 streamTask) in /_/ICSharpCode.ILSpyX/LoadedAssembly.cs:line 320
   at ICSharpCode.ILSpy.TreeNodes.AssemblyTreeNode.Decompile(Language language, ITextOutput output, DecompilationOptions options)