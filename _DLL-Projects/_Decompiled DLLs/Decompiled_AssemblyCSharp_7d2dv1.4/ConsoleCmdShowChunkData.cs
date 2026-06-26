using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdShowChunkData : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "showchunkdata", "sc" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		foreach (KeyValuePair<int, EntityPlayer> item in GameManager.Instance.World.Players.dict)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Player: " + item.Value.EntityName);
			Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(Utils.Fastfloor(item.Value.position.x), Utils.Fastfloor(item.Value.position.y), Utils.Fastfloor(item.Value.position.z));
			if (chunk == null)
			{
				continue;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" On Chunk: " + chunk?.ToString() + " Mem used: " + chunk.GetUsedMem());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" Tile Entities:");
			DictionaryList<Vector3i, TileEntity> tileEntities = chunk.GetTileEntities();
			for (int i = 0; i < tileEntities.list.Count; i++)
			{
				TileEntity tileEntity = tileEntities.list[i];
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  - " + tileEntity);
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" Entities:");
			List<Entity>[] entityLists = chunk.entityLists;
			foreach (List<Entity> list in entityLists)
			{
				for (int k = 0; k < list.Count; k++)
				{
					Entity entity = list[k];
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  - " + entity);
				}
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" DominantBiome: " + GameManager.Instance.World.Biomes.GetBiome(chunk.DominantBiome));
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" AreaMasterDominantBiome: " + ((chunk.AreaMasterDominantBiome != byte.MaxValue) ? GameManager.Instance.World.Biomes.GetBiome(chunk.AreaMasterDominantBiome).ToString() : "-"));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "shows some date of the current chunk";
	}
}
