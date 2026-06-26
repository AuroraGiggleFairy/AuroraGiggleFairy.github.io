using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSaveChunkAgeMap : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "agemap" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Output debug map for chunk age/protection/save status.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "\"agemap\" or \"agemap [x]\", where [x] is a float value specifying the maximum age to normalise results to in in-game days. Defaults to EnumGamePrefs.MaxChunkAge when not specified.\n\tOutputs a TGA texture representing a map of all chunks with chunk age, protection status and save status data split across each colour channel:\n\tR [scalar]: Effective chunk age proportionate to maximum age, taking POI-based grouping rules into account. More red = older, closer to expiry.\n\tG [scalar]: Raw chunk age proportionate to maximum age, ignoring POI-based grouping. Can be compared with the red channel to asses the impacts of grouping.\n\tB [scalar]: Protection level. More blue = more protected.\n\tA [binary]: Saved status. Opaque = saved, transparent = not saved.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!(GameManager.Instance.World.ChunkCache.ChunkProvider is ChunkProviderGenerateWorld chunkProviderGenerateWorld))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Failed to create chunk age map: ChunkProviderGenerateWorld could not be found for current world instance.");
			return;
		}
		float result;
		if (_params.Count == 0)
		{
			result = GamePrefs.GetInt(EnumGamePrefs.MaxChunkAge);
		}
		else if (_params.Count != 1 || !float.TryParse(_params[0], out result) || result < float.Epsilon)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			return;
		}
		try
		{
			chunkProviderGenerateWorld.SaveChunkAgeDebugTexture(result);
		}
		catch (Exception ex)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Failed to create chunk age map with exception: " + ex.Message);
		}
	}
}
