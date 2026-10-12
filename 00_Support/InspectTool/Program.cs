using System;
using System.IO;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;

class Program
{
    static void Main()
    {
        string dllPath = @"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\7DaysToDie_Data\Managed\Assembly-CSharp.dll";
        var decompiler = new CSharpDecompiler(dllPath, new DecompilerSettings());
        
        File.WriteAllText(@"C:\GitHub\7D2D-Mods\00_Support\VoxelMesh_decompiled.cs", 
            decompiler.DecompileTypeAsString(new ICSharpCode.Decompiler.TypeSystem.FullTypeName("VoxelMesh")));
        Console.WriteLine("VoxelMesh decompiled!");
    }
}
