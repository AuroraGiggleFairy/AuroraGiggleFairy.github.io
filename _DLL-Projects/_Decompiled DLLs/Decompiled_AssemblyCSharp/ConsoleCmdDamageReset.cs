using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdDamageReset : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "damagereset" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Reset damage on all blocks in the currently loaded POI";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "\r\n\t\t\t|Usage:\r\n\t\t\t|    damagereset [include doors]\r\n\t\t\t|By default the command only resets non-door blocks to full health. If the optional argument is \"true\" doors are also repaired.\r\n\t\t\t".Unindent();
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.Instance.IsEditMode())
		{
			PrefabEditModeManager instance = PrefabEditModeManager.Instance;
			if (instance != null && instance.IsActive())
			{
				bool ignoreDoors = true;
				if (_params.Count > 0)
				{
					ignoreDoors = !ConsoleHelper.ParseParamBool(_params[0], _invalidStringsAsFalse: true);
				}
				World world = GameManager.Instance.World;
				List<Chunk> chunkArrayCopySync = world.ChunkCache.GetChunkArrayCopySync();
				int fixedBlocks = 0;
				for (int i = 0; i < chunkArrayCopySync.Count; i++)
				{
					Chunk chunk = chunkArrayCopySync[i];
					Vector3i chunkWorldPos = chunk.GetWorldPos();
					chunk.LoopOverAllBlocks([PublicizedFrom(EAccessModifier.Internal)] (int _x, int _y, int _z, BlockValue _bv) =>
					{
						int damage = _bv.damage;
						if (damage > 0)
						{
							Block block = _bv.Block;
							if (!ignoreDoors || !(block is BlockDoor))
							{
								Vector3i vector3i = chunkWorldPos + new Vector3i(_x, _y, _z);
								block.DamageBlock(world, 0, vector3i, _bv, -damage, -1);
								SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Repaired block at {vector3i}, had {damage} damage points");
								int num = fixedBlocks;
								fixedBlocks = num + 1;
							}
						}
					});
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Repaired {fixedBlocks} blocks");
				return;
			}
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command has to be run while in Prefab Editor!");
	}
}
