using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPrintChunkExpiryInfo : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "expiryinfo" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Prints location and expiry day/time for the next [x] chunks set to expire.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "expiryinfo [x]\n" + GetDescription();
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!(GameManager.Instance.World.ChunkCache.ChunkProvider is ChunkProviderGenerateWorld chunkProviderGenerateWorld))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Failed to retrieve chunk expiry info: ChunkProviderGenerateWorld could not be found for current world instance.");
			return;
		}
		if (_params.Count != 1 || !int.TryParse(_params[0], out var result) || result < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			return;
		}
		try
		{
			List<KeyValuePair<long, ulong>> expiryTimes = new List<KeyValuePair<long, ulong>>();
			chunkProviderGenerateWorld.IterateChunkExpiryTimes([PublicizedFrom(EAccessModifier.Internal)] (long chunkKey, ulong expiry) =>
			{
				expiryTimes.Add(new KeyValuePair<long, ulong>(chunkKey, expiry));
			});
			result = Mathf.Min(result, expiryTimes.Count);
			if (result == 0)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No chunks are currently set to expire. Ensure max chunk age is enabled.");
				return;
			}
			expiryTimes.Sort([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<long, ulong> a, KeyValuePair<long, ulong> b) => a.Value.CompareTo(b.Value));
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Chunk\t\tExpiry");
			for (int num = 0; num < result; num++)
			{
				long key = expiryTimes[num].Key;
				ulong value = expiryTimes[num].Value;
				int num2 = WorldChunkCache.extractX(key);
				int num3 = WorldChunkCache.extractZ(key);
				var (num4, num5, num6) = GameUtils.WorldTimeToElements(value);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"{num2}, {num3}\t\tDay {num4}, {$"{num5:D2}:{num6:D2}"}");
			}
		}
		catch (Exception ex)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Failed to retrieve chunk expiry info with exception: " + ex.Message);
		}
	}
}
