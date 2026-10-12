using System;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace BiomePaints
{
    public static class TextureInjector
    {
        private static bool isAlreadyInjected = false;

        private static readonly string[] PaintNames = new string[]
        {
            "agf_forest_ground",
            "agf_burnt_forest_ground",
            "agf_desert_ground",
            "agf_snow_ground",
            "agf_wasteland_ground"
        };

        private static readonly string[] BundleFiles = new string[]
        {
            "Atlas_001.unity3d",
            "Atlas_002.unity3d",
            "Atlas_003.unity3d",
            "Atlas_004.unity3d",
            "Atlas_005.unity3d"
        };

        private static readonly int[] TextureIds = new int[] { 701, 702, 703, 704, 705 };
        private static int injectedSliceStart = -1;

        public static int GetTextureIdForSlice(int sliceIndex)
        {
            if (injectedSliceStart >= 0 && sliceIndex >= injectedSliceStart && sliceIndex < injectedSliceStart + TextureIds.Length)
            {
                return TextureIds[sliceIndex - injectedSliceStart];
            }
            return -1;
        }

        public static void InjectBiomeTextures()
        {
            if (isAlreadyInjected)
            {
                return;
            }

            if (MeshDescription.meshes == null || MeshDescription.meshes.Length <= MeshDescription.MESH_OPAQUE)
            {
                Log.Warning("[BiomePaints] MeshDescription.meshes is not initialized yet.");
                return;
            }

            MeshDescription meshOpaque = MeshDescription.meshes[MeshDescription.MESH_OPAQUE];
            if (meshOpaque == null || meshOpaque.textureAtlas == null)
            {
                Log.Warning("[BiomePaints] MeshDescription opaque texture atlas is null.");
                return;
            }

            TextureAtlas atlas = meshOpaque.textureAtlas;

            int maxTexId = 705;
            if (atlas.uvMapping != null && atlas.uvMapping.Length > maxTexId && atlas.uvMapping[701].textureName == PaintNames[0])
            {
                injectedSliceStart = atlas.uvMapping[701].index;
                isAlreadyInjected = true;
                return;
            }

            Log.Out("[BiomePaints] Starting terrain biome texture injection into Mesh 0...");

            // Temporarily set globalTextureMipmapLimit to 0 so Graphics.CopyTexture does not fail
            // with "different mipmap limits" when game texture quality is set to Half or Quarter.
            int savedMipLimit = QualitySettings.globalTextureMipmapLimit;
            Log.Out($"[BiomePaints] Current globalTextureMipmapLimit is {savedMipLimit}");

            try
            {
                QualitySettings.globalTextureMipmapLimit = 0;

                // 1. Expand uvMapping array if needed
                if (atlas.uvMapping == null || atlas.uvMapping.Length <= maxTexId)
                {
                    UVRectTiling[] newUvMapping = new UVRectTiling[maxTexId + 1];
                    if (atlas.uvMapping != null)
                    {
                        Array.Copy(atlas.uvMapping, newUvMapping, atlas.uvMapping.Length);
                    }
                    atlas.uvMapping = newUvMapping;
                }

                int startSliceIndex = 0;
                bool hasTextures = !GameManager.IsDedicatedServer && atlas.diffuseTexture != null;

                if (hasTextures)
                {
                    Texture2DArray origDiffuse = atlas.diffuseTexture as Texture2DArray;
                    Texture2DArray origNormal = atlas.normalTexture as Texture2DArray;
                    Texture2DArray origSpecular = atlas.specularTexture as Texture2DArray;

                    if (origDiffuse == null)
                    {
                        Log.Error("[BiomePaints] atlas.diffuseTexture is not a Texture2DArray!");
                        return;
                    }

                    startSliceIndex = origDiffuse.depth;
                    injectedSliceStart = startSliceIndex;
                    int addCount = PaintNames.Length;
                    int newDepth = startSliceIndex + addCount;

                    Log.Out($"[BiomePaints] Expanding Texture2DArray from {startSliceIndex} to {newDepth} slices (dims: {origDiffuse.width}x{origDiffuse.height}, mips: {origDiffuse.mipmapCount})...");

                    Texture2DArray newDiffuse = ExpandArray(origDiffuse, newDepth);
                    Texture2DArray newNormal = origNormal != null ? ExpandArray(origNormal, newDepth) : null;
                    Texture2DArray newSpecular = origSpecular != null ? ExpandArray(origSpecular, newDepth) : null;

                    string modPath = ModAPI.ModInstance?.Path;
                    if (string.IsNullOrEmpty(modPath))
                    {
                        Log.Error("[BiomePaints] ModInstance path is null or empty!");
                        return;
                    }

                    string resFolder = Path.Combine(modPath, "Resources");

                    for (int i = 0; i < PaintNames.Length; i++)
                    {
                        string pName = PaintNames[i];
                        string bPath = Path.Combine(resFolder, BundleFiles[i]);
                        if (!File.Exists(bPath))
                        {
                            Log.Error($"[BiomePaints] Bundle not found at {bPath}");
                            continue;
                        }

                        AssetBundle ab = AssetBundle.LoadFromFile(bPath);
                        if (ab == null)
                        {
                            Log.Error($"[BiomePaints] Failed to load AssetBundle: {bPath}");
                            continue;
                        }

                        Texture2D diff = ab.LoadAsset<Texture2D>($"assets/{pName}_diffuse.png");
                        Texture2D norm = ab.LoadAsset<Texture2D>($"assets/{pName}_normal.png");
                        Texture2D spec = ab.LoadAsset<Texture2D>($"assets/{pName}_specular.png");

                        int slice = startSliceIndex + i;

                        if (diff != null)
                        {
                            CopySlice(diff, newDiffuse, slice);
                        }
                        else
                        {
                            Log.Warning($"[BiomePaints] Missing diffuse in bundle for {pName}");
                        }

                        if (norm != null && newNormal != null)
                        {
                            CopySlice(norm, newNormal, slice);
                        }

                        if (spec != null && newSpecular != null)
                        {
                            CopySlice(spec, newSpecular, slice);
                        }

                        ab.Unload(true);
                    }

                    // Update atlas references
                    atlas.diffuseTexture = newDiffuse;
                    meshOpaque.TexDiffuse = newDiffuse;

                    if (newNormal != null)
                    {
                        atlas.normalTexture = newNormal;
                        meshOpaque.TexNormal = newNormal;
                    }

                    if (newSpecular != null)
                    {
                        atlas.specularTexture = newSpecular;
                        meshOpaque.TexSpecular = newSpecular;
                    }

                    // Update materials
                    UpdateMaterials(meshOpaque, newDiffuse, newNormal, newSpecular);
                }

                // 2. Register uvMapping entries
                MaterialBlock dirtMat = MaterialBlock.fromString("Mdirt") ?? (atlas.uvMapping != null && atlas.uvMapping.Length > 1 ? atlas.uvMapping[1].material : null);

                for (int i = 0; i < PaintNames.Length; i++)
                {
                    int textureId = TextureIds[i];
                    int slice = startSliceIndex + i;

                    UVRectTiling uv = new UVRectTiling
                    {
                        blockW = 8,
                        blockH = 8,
                        uv = new Rect(0f, 0f, 1f, 1f),
                        bGlobalUV = true,
                        bSwitchUV = false,
                        color = Color.white,
                        material = dirtMat,
                        textureName = PaintNames[i],
                        index = slice
                    };

                    atlas.uvMapping[textureId] = uv;
                    Log.Out($"[BiomePaints] Registered uvMapping[{textureId}] -> slice {slice} ({PaintNames[i]}) with blockW=8, blockH=8, bGlobalUV=true");
                }

                isAlreadyInjected = true;
                Log.Out("[BiomePaints] Biome paints injected successfully!");
            }
            finally
            {
                QualitySettings.globalTextureMipmapLimit = savedMipLimit;
            }
        }

        private static Texture2DArray ExpandArray(Texture2DArray src, int newDepth)
        {
            Texture2DArray dst = new Texture2DArray(
                src.width,
                src.height,
                newDepth,
                src.graphicsFormat,
                TextureCreationFlags.MipChain | TextureCreationFlags.DontInitializePixels,
                src.mipmapCount
            );

            dst.filterMode = src.filterMode;
            dst.wrapMode = src.wrapMode;
            dst.anisoLevel = 16;
            dst.mipMapBias = -0.75f;
            dst.name = src.name;

            for (int slice = 0; slice < src.depth; slice++)
            {
                for (int mip = 0; mip < src.mipmapCount; mip++)
                {
                    Graphics.CopyTexture(src, slice, mip, dst, slice, mip);
                }
            }

            return dst;
        }

        private static void CopySlice(Texture2D src, Texture2DArray dst, int dstSlice)
        {
            if (src == null || dst == null)
            {
                return;
            }

            int srcMips = src.mipmapCount;
            int dstMips = dst.mipmapCount;

            int srcStartMip = 0;
            int dstStartMip = 0;

            int curSrcW = src.width;
            int curDstW = dst.width;

            while (curSrcW > curDstW && srcStartMip < srcMips)
            {
                curSrcW /= 2;
                srcStartMip++;
            }

            while (curDstW > curSrcW && dstStartMip < dstMips)
            {
                curDstW /= 2;
                dstStartMip++;
            }

            int mipsToCopy = Math.Min(srcMips - srcStartMip, dstMips - dstStartMip);
            Log.Out($"[BiomePaints] CopySlice: {src.name} ({src.width}x{src.height}, mips={srcMips}) -> slice {dstSlice} (copied {mipsToCopy} mips, srcStartMip={srcStartMip}, dstStartMip={dstStartMip})");
            if (mipsToCopy <= 0)
            {
                Log.Error($"[BiomePaints] CopySlice ERROR: 0 mips to copy for {src.name}!");
            }
            for (int i = 0; i < mipsToCopy; i++)
            {
                Graphics.CopyTexture(src, 0, srcStartMip + i, dst, dstSlice, dstStartMip + i);
            }
        }

        private static void UpdateMaterials(MeshDescription meshDesc, Texture2DArray diff, Texture2DArray norm, Texture2DArray spec)
        {
            if (meshDesc.materials != null)
            {
                foreach (Material mat in meshDesc.materials)
                {
                    if (mat != null)
                    {
                        mat.mainTexture = diff;
                        if (norm != null) mat.SetTexture("_BumpMap", norm);
                        if (spec != null) mat.SetTexture("_MetallicGlossMap", spec);
                    }
                }
            }
            if (meshDesc.materialDistant != null)
            {
                meshDesc.materialDistant.mainTexture = diff;
                if (norm != null) meshDesc.materialDistant.SetTexture("_BumpMap", norm);
                if (spec != null) meshDesc.materialDistant.SetTexture("_MetallicGlossMap", spec);
            }
        }
    }
}
