using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdWorldChunkReset : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "worldchunkreset", "wcr" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Resets all unprotected chunks across the world.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage: wcr [-g|-gmode <n>]\n\nExamples: \n'wcr' - reset all unprotected chunks using the default grouping.\n'wcr -g 2' - reset all unprotected chunks, grouping mode 2.\n\nGrouping modes (-g|-gmode, default 3): \n'1' - NoGrouping: Chunks are reset based on their own protection flags only.\n'2' - SeparatePOIs: Each POI gets the combined protection flags from all chunks overlapping the POI.\n'3' - GroupedPOIs: Like SeparatePOIs, but POIs with overlapping chunks also merge into groups. Standard reset; should not cause POI discontinuities.\n\nNotes: \n - All chunk protection statuses are respected. To bypass protections, see 'rr'.\n - Use with caution! This operation permanently deletes all saved data for affected chunks.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (ChunkResetCommandHelpers.TryParseGroupingMode(_params, out var _grouping))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Running world chunk reset: -g {(int)_grouping} ({_grouping}).");
			ChunkResetCommandHelpers.ExecuteReset(ChunkProtectionLevel.All, _grouping, null, null, "World chunk reset");
		}
	}
}
