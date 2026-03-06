using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdBents : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, int> bentsPerName = new Dictionary<string, int>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "bents" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Switches block entities on/off or counts them";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Use 'on' or 'off', or 'info' to count";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Specify 'on', 'off' or 'info'");
		}
		else if (_params[0] == "on")
		{
			setAll(_bOn: true);
		}
		else if (_params[0] == "off")
		{
			setAll(_bOn: false);
		}
		else if (_params[0] == "info")
		{
			bentsPerName.Clear();
			int num = countAll(bentsPerName);
			int num2 = 1;
			foreach (KeyValuePair<string, int> item in bentsPerName)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output($" {num2,3}. {item.Key} = {item.Value}");
				num2++;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Total: " + num);
		}
		else if (_params[0] == "cullon")
		{
			int num3 = 0;
			MicroStopwatch microStopwatch = new MicroStopwatch();
			foreach (Chunk item2 in GameManager.Instance.World.ChunkCache.GetChunkArray())
			{
				num3 += item2.EnableInsideBlockEntities(_bOn: true);
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Setting " + num3 + " to ON took " + microStopwatch.ElapsedMilliseconds);
		}
		else if (_params[0] == "culloff")
		{
			int num4 = 0;
			MicroStopwatch microStopwatch2 = new MicroStopwatch();
			foreach (Chunk item3 in GameManager.Instance.World.ChunkCache.GetChunkArray())
			{
				num4 += item3.EnableInsideBlockEntities(_bOn: false);
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Setting " + num4 + " to OFF took " + microStopwatch2.ElapsedMilliseconds);
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown parameter");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setAll(bool _bOn, Transform _t = null)
	{
		if (_t == null)
		{
			foreach (ChunkGameObject usedChunkGameObject in GameManager.Instance.World.m_ChunkManager.GetUsedChunkGameObjects())
			{
				setAll(_bOn, usedChunkGameObject.transform);
			}
			return;
		}
		for (int i = 0; i < _t.childCount; i++)
		{
			Transform child = _t.GetChild(i);
			if (child.name == "_BlockEntities")
			{
				child.gameObject.SetActive(_bOn);
			}
			else
			{
				setAll(_bOn, child);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int countAll(Dictionary<string, int> _bentsPerName, Transform _t = null)
	{
		int num = 0;
		if (_t == null)
		{
			foreach (ChunkGameObject usedChunkGameObject in GameManager.Instance.World.m_ChunkManager.GetUsedChunkGameObjects())
			{
				num += countAll(_bentsPerName, usedChunkGameObject.transform);
			}
			return num;
		}
		for (int i = 0; i < _t.childCount; i++)
		{
			Transform child = _t.GetChild(i);
			if (child.name == "_BlockEntities")
			{
				num += child.childCount;
				for (int j = 0; j < child.childCount; j++)
				{
					string name = child.GetChild(j).name;
					_bentsPerName[name] = ((!_bentsPerName.TryGetValue(name, out var value)) ? 1 : (value + 1));
				}
			}
			else
			{
				num += countAll(_bentsPerName, child);
			}
		}
		return num;
	}
}
