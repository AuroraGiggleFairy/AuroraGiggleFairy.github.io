using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdChunkCache : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override int DefaultPermissionLevel => 1000;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "chunkcache", "cc" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		int num = 1;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int[] array = new int[1];
		lock (GameManager.Instance.World.ChunkClusters[0].GetSyncRoot())
		{
			foreach (Chunk item in GameManager.Instance.World.ChunkClusters[0].GetChunkArray())
			{
				int usedMem = item.GetUsedMem();
				item.GetTextureChannelMemory(out var texMem);
				for (int i = 0; i < array.Length; i++)
				{
					array[i] += texMem[i];
					num5 += texMem[i];
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(num++ + ". " + item.X + ", " + item.Z + "  M=" + usedMem / 1024 + "k" + (item.IsDisplayed ? "D" : ""));
				num2 += (item.IsDisplayed ? 1 : 0);
				num3 += usedMem;
				num4 += item.MeshLayerCount;
			}
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Chunks: " + GameManager.Instance.World.ChunkClusters[0].Count());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Chunk Memory: " + num3 / 1048576 + "MB");
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Texture Memory Total: {(float)num5 / 1048576f:F2}MB");
		for (int j = 0; j < array.Length; j++)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Texture Memory {j}: {(float)array[j] / 1048576f:F2}MB");
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Displayed: " + num2);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("VML: " + num4);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "shows all loaded chunks in cache";
	}
}
