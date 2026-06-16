using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPlaceObserver : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public MapVisitor mapVisitor;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly DictionaryList<Vector2i, ChunkManager.ChunkObserver> observers = new DictionaryList<Vector2i, ChunkManager.ChunkObserver>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Place a chunk observer on a given position.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n  1. chunkobserver add <x> <z> [size]\n  2. chunkobserver remove <x> <z>\n  3. chunkobserver list\n1. Place an observer on the chunk that contains the coordinate x/z.\n   Optionally specifying the box radius in chunks, defaulting to 1.\n2. Remove the observer from the chunk with the coordinate, if any.\n3. List all currently placed observers";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "chunkobserver", "co" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 1 && _params[0].EqualsCaseInsensitive("list"))
		{
			listObservers();
		}
		else if ((_params[0].EqualsCaseInsensitive("add") && (_params.Count == 3 || _params.Count == 4)) || (_params[0].EqualsCaseInsensitive("remove") && _params.Count == 3))
		{
			if (!int.TryParse(_params[1], out var result))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("The given x coordinate is not a valid integer");
				return;
			}
			if (!int.TryParse(_params[2], out var result2))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("The given z coordinate is not a valid integer");
				return;
			}
			int result3;
			if (_params.Count == 4)
			{
				if (!int.TryParse(_params[3], out result3) || result3 < 1 || result3 > 15)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("The given size is not a valid integer or exceeds the allowed range of 1-15");
					return;
				}
			}
			else
			{
				result3 = 1;
			}
			Vector2i pos = new Vector2i(result, result2);
			if (_params[0].EqualsCaseInsensitive("remove"))
			{
				if (removeObserver(pos))
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Observer removed from " + pos.ToString());
				}
				else
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No observer on " + pos.ToString());
				}
			}
			else if (addObserver(pos, result3))
			{
				int num = 2 * (result3 - 1) + 1;
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Observer added to " + pos.ToString() + " with radius " + result3 + " (size " + num + "x" + num + ")");
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Already an observer on " + pos.ToString());
			}
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Illegal arguments");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool addObserver(Vector2i _pos, int _radius)
	{
		if (observers.dict.ContainsKey(_pos))
		{
			return false;
		}
		Vector3 initialPosition = new Vector3(_pos.x, 0f, _pos.y);
		ChunkManager.ChunkObserver value = GameManager.Instance.AddChunkObserver(initialPosition, _bBuildVisualMeshAround: false, _radius, -1);
		observers.Add(_pos, value);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool removeObserver(Vector2i _pos)
	{
		if (!observers.dict.ContainsKey(_pos))
		{
			return false;
		}
		ChunkManager.ChunkObserver observer = observers.dict[_pos];
		GameManager.Instance.RemoveChunkObserver(observer);
		observers.Remove(_pos);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void listObservers()
	{
		int num = 0;
		foreach (KeyValuePair<Vector2i, ChunkManager.ChunkObserver> item in observers.dict)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($" {++num,3}: {item.Key.ToString()}");
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(num + " observers");
	}
}
