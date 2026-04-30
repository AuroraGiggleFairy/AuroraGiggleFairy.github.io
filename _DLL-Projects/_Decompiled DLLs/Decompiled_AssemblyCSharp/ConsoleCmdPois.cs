using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPois : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject parentGO;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "pois" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Switches distant POIs on/off";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Use on or off or only the command to toggle";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		GameObject gameObject = GameObject.Find("/PrefabsLOD");
		if (gameObject != null)
		{
			parentGO = gameObject;
		}
		if (parentGO == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Distant POIs not active!");
			return;
		}
		if (_params.Count == 0)
		{
			parentGO.SetActive(!parentGO.activeSelf);
		}
		else if (_params[0] == "on")
		{
			parentGO.SetActive(value: true);
		}
		else
		{
			if (!(_params[0] == "off"))
			{
				if (int.TryParse(_params[0], out var result))
				{
					GameManager.Instance.prefabLODManager.SetPOIDistance(128 * result);
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Setting to POI chunk distance " + result + " =" + 128 * result + "m");
				}
				else
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown parameter");
				}
				return;
			}
			parentGO.SetActive(value: true);
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("POIs set to " + (parentGO.activeSelf ? "on" : "off"));
	}
}
