using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdBents : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int totalBents;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, int> bentsPerName = new Dictionary<string, int>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "bents" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Switches block entities on/off";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Use on or off or only the command to toggle";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		GameObject gameObject = GameObject.Find("/Chunks");
		if (gameObject == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Parent not found!");
		}
		else if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Specify on or off");
		}
		else if (_params[0] == "on")
		{
			setAll(gameObject.transform, _bOn: true);
		}
		else if (_params[0] == "off")
		{
			setAll(gameObject.transform, _bOn: false);
		}
		else if (_params[0] == "info")
		{
			totalBents = 0;
			bentsPerName.Clear();
			countAll(gameObject.transform);
			int num = 1;
			foreach (KeyValuePair<string, int> item in bentsPerName)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(num + ". " + item.Key + " = " + item.Value);
				num++;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Total: " + totalBents);
		}
		else if (_params[0] == "cullon")
		{
			int num2 = 0;
			MicroStopwatch microStopwatch = new MicroStopwatch();
			foreach (Chunk item2 in GameManager.Instance.World.ChunkCache.GetChunkArray())
			{
				num2 += item2.EnableInsideBlockEntities(_bOn: true);
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Setting " + num2 + " to ON took " + microStopwatch.ElapsedMilliseconds);
		}
		else if (_params[0] == "culloff")
		{
			int num3 = 0;
			MicroStopwatch microStopwatch2 = new MicroStopwatch();
			foreach (Chunk item3 in GameManager.Instance.World.ChunkCache.GetChunkArray())
			{
				num3 += item3.EnableInsideBlockEntities(_bOn: false);
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Setting " + num3 + " to OFF took " + microStopwatch2.ElapsedMilliseconds);
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown parameter");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setAll(Transform _t, bool _bOn)
	{
		for (int i = 0; i < _t.childCount; i++)
		{
			Transform child = _t.GetChild(i);
			if (child.name == "_BlockEntities")
			{
				child.gameObject.SetActive(_bOn);
			}
			else
			{
				setAll(child, _bOn);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void countAll(Transform _t)
	{
		for (int i = 0; i < _t.childCount; i++)
		{
			Transform child = _t.GetChild(i);
			if (child.name == "_BlockEntities")
			{
				totalBents += child.childCount;
				for (int j = 0; j < child.childCount; j++)
				{
					bentsPerName[child.GetChild(j).name] = ((!bentsPerName.ContainsKey(child.GetChild(j).name)) ? 1 : (bentsPerName[child.GetChild(j).name] + 1));
				}
			}
			else
			{
				countAll(child);
			}
		}
	}
}
