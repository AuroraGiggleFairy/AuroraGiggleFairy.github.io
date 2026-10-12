using System;
using System.IO;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;

class Program
{
    static void Main()
    {
        string path = @"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\7DaysToDie_Data\Managed\Assembly-CSharp.dll";
        var decompiler = new CSharpDecompiler(path, new DecompilerSettings());

        // Let's search for references to DensityTerrain or DensityAir in Chunk, World, BlockPlacement, etc.
        Console.WriteLine("Searching for MarchingCubes density references...");
        foreach (var t in decompiler.TypeSystem.MainModule.TypeDefinitions)
        {
            if (t.Name == "ChunkCluster" || t.Name == "Chunk" || t.Name == "World" || t.Name == "Block" || t.Name == "BlockShapeTerrain")
            {
                foreach (var m in t.Methods)
                {
                    // Check methods in BlockShapeTerrain
                    if (t.Name == "BlockShapeTerrain" || m.Name.Contains("Density") || m.Name.Contains("Terrain"))
                    {
                        Console.WriteLine(t.Name + "." + m.Name);
                    }
                }
            }
        }
    }
}
