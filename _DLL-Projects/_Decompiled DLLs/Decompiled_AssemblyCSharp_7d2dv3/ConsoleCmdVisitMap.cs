using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdVisitMap : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public MapVisitor mapVisitor;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Chunk.DensityMismatchInformation> densityMismatches;

	public bool IsRunning
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (mapVisitor != null)
			{
				return mapVisitor.IsRunning();
			}
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Visit an given area of the map. Optionally run the density check on each visited chunk.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "\n\t\t\t|Usage:\n\t\t\t|  1. visitmap <x1> <z1> <x2> <z2> [check]\n\t\t\t|  2. visitmap full [check]\n\t\t\t|  3. visitmap stop\n\t\t\t|1. Start visiting the map in the rectangle specified with the two edges defined by\n\t\t\t|   coordinate pairs x1/z1 and x2/z2. If the parameter \"check\" is added each visited\n\t\t\t|   chunk will be checked for density issues.\n\t\t\t|2. Start visiting the full map. If the parameter \"check\" is added each visited\n\t\t\t|   chunk will be checked for density issues.\n\t\t\t|3. Stop the current visitmap run.\n\t\t\t".Unindent();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "visitmap" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count != 1 && _params.Count != 2 && _params.Count != 4 && _params.Count != 5)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 1, 2, 4 or 5, found " + _params.Count + ".");
			return;
		}
		bool checkDensities = _params[_params.Count - 1].EqualsCaseInsensitive("check");
		if (_params.Count == 1)
		{
			if (_params[0].EqualsCaseInsensitive("stop"))
			{
				stop();
			}
			else if (_params[0].EqualsCaseInsensitive("full"))
			{
				GameManager.Instance.World.GetWorldExtent(out var _minSize, out var _maxSize);
				visit(_minSize, _maxSize, _checkDensities: false);
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Illegal arguments");
			}
		}
		else if (_params.Count == 2)
		{
			if (_params[0].EqualsCaseInsensitive("full"))
			{
				GameManager.Instance.World.GetWorldExtent(out var _minSize2, out var _maxSize2);
				visit(_minSize2, _maxSize2, checkDensities);
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Illegal arguments");
			}
		}
		else if (_params.Count == 4 || _params.Count == 5)
		{
			int result2;
			int result3;
			int result4;
			if (!int.TryParse(_params[0], out var result))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("The given x1 coordinate is not a valid integer");
			}
			else if (!int.TryParse(_params[1], out result2))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("The given z1 coordinate is not a valid integer");
			}
			else if (!int.TryParse(_params[2], out result3))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("The given x2 coordinate is not a valid integer");
			}
			else if (!int.TryParse(_params[3], out result4))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("The given z2 coordinate is not a valid integer");
			}
			else
			{
				visit(new Vector3i(result, 0, result2), new Vector3i(result3, 0, result4), checkDensities);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void visit(Vector3i _start, Vector3i _end, bool _checkDensities)
	{
		if (mapVisitor != null && mapVisitor.IsRunning())
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("VisitMap already running. You can stop it with \"visitmap stop\".");
			return;
		}
		mapVisitor = new MapVisitor(_start, _end);
		mapVisitor.OnVisitChunk += LogChunk;
		mapVisitor.OnVisitChunk += RenderMinimap;
		if (_checkDensities)
		{
			densityMismatches = new List<Chunk.DensityMismatchInformation>();
			mapVisitor.OnVisitChunk += CheckDensities;
		}
		mapVisitor.OnVisitMapDone += OnDone;
		mapVisitor.Start();
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Started visiting the map between {mapVisitor.WorldPosStart} and {mapVisitor.WorldPosEnd}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stop()
	{
		if (!IsRunning)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("VisitMap not running.");
			return;
		}
		mapVisitor.Stop();
		mapVisitor = null;
		densityMismatches = null;
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("VisitMap stopped.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckDensities(Chunk _chunk, int _done, int _total, float _elapsed)
	{
		densityMismatches.AddRange(_chunk.CheckDensities());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogChunk(Chunk _chunk, int _done, int _total, float _elapsed)
	{
		if (_done % 200 == 0)
		{
			float value = (float)(_total - _done) * (_elapsed / (float)_done);
			Log.Out("VisitMap ({3:00}%): {0} / {1} chunks done (estimated time left {2} seconds)", _done, _total, value.ToCultureInvariantString("0.00"), Mathf.RoundToInt(100f * ((float)_done / (float)_total)));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderMinimap(Chunk _chunk, int _done, int _total, float _elapsed)
	{
		_chunk.GetMapColors();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDone(int _chunks, float _duration)
	{
		Log.Out("VisitMap done, visited {0} chunks in {1} seconds (average {2} chunks/sec).", _chunks, _duration.ToCultureInvariantString("0.00"), ((float)_chunks / _duration).ToCultureInvariantString("0.00"));
		writeDensityMismatchFile();
		mapVisitor = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeDensityMismatchFile()
	{
		if (densityMismatches == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder("[\n");
		for (int i = 0; i < densityMismatches.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(densityMismatches[i].ToJsonString());
			stringBuilder.Append('\n');
		}
		stringBuilder.Append("]");
		SdFile.WriteAllText(((Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXServer) ? (Application.dataPath + "/../../") : (Application.dataPath + "/../")) + "densitymismatch.json", stringBuilder.ToString());
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.AppendLine("x;y;z;Density;IsTerrain;BvType");
		for (int j = 0; j < densityMismatches.Count; j++)
		{
			stringBuilder2.AppendLine($"{densityMismatches[j].x};{densityMismatches[j].y};{densityMismatches[j].z};{densityMismatches[j].density};{densityMismatches[j].isTerrain};{densityMismatches[j].bvType}");
		}
		SdFile.WriteAllText(((Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXServer) ? (Application.dataPath + "/../../") : (Application.dataPath + "/../")) + "densitymismatch.csv", stringBuilder2.ToString());
		densityMismatches = null;
	}
}
