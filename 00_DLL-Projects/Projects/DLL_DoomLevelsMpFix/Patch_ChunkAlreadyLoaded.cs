using System.Reflection;
using HarmonyLib;

namespace DoomLevelsMpFix
{
	[HarmonyPatch(typeof(NetPackageChunk), nameof(NetPackageChunk.ProcessPackage))]
	internal static class Patch_NetPackageChunk_AlreadyLoaded
	{
		private static readonly FieldInfo ChunkField = AccessTools.Field(typeof(NetPackageChunk), "chunk");
		private static readonly FieldInfo OverwriteField = AccessTools.Field(typeof(NetPackageChunk), "bOverwriteExisting");

		private static bool Prefix(NetPackageChunk __instance, World _world)
		{
			if (_world?.ChunkCache == null || ChunkField == null || OverwriteField == null)
			{
				return true;
			}

			if ((bool)OverwriteField.GetValue(__instance))
			{
				return true;
			}

			Chunk incoming = ChunkField.GetValue(__instance) as Chunk;
			if (incoming == null)
			{
				return true;
			}

			if (_world.ChunkCache.GetChunkSync(incoming.Key) == null)
			{
				return true;
			}

			return false;
		}
	}
}
