using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdLights : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Light[] allLights;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "lights" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Light debugging";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Light debugging:\nv - toggle viewer enable\noff - viewer lights off\non - viewer lights on\nclearreg - clear registered\ndisableall - \nenableall - \nlist - list all lights\nlistfile - list all lights to Lights.txt\nliste - list lights effecting the player position\nlodviewdistance <d> - set the light LOD view distance (0 uses defaults)";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			return;
		}
		switch (_params[0].ToLower())
		{
		case "v":
			LightViewer.SetEnabled(!LightViewer.IsEnabled);
			return;
		case "lodviewdistance":
		case "lvd":
		{
			float result = 0f;
			if (_params.Count >= 2)
			{
				float.TryParse(_params[1], out result);
			}
			LightLOD.DebugViewDistance = result;
			return;
		}
		}
		if (_params.Count == 1)
		{
			if (_params[0].EqualsCaseInsensitive("on"))
			{
				if (Camera.main != null)
				{
					LightViewer component = Camera.main.GetComponent<LightViewer>();
					if (component != null)
					{
						LightViewer.IsAllOff = false;
						component.TurnOnAllLights();
					}
				}
			}
			else if (_params[0].EqualsCaseInsensitive("off"))
			{
				if (Camera.main != null)
				{
					LightViewer component2 = Camera.main.GetComponent<LightViewer>();
					if (component2 != null)
					{
						LightViewer.IsAllOff = true;
						component2.TurnOffAllLights();
					}
				}
			}
			else if (_params[0].EqualsCaseInsensitive("showlevel"))
			{
				LightManager.ShowLightLevel(!LightManager.ShowLightLevelOn);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(LightManager.ShowLightLevelOn ? "Showing " : "Hiding light level");
			}
			else if (_params[0].EqualsCaseInsensitive("showsearchpattern"))
			{
				LightManager.ShowSearchPattern(!LightManager.ShowSearchPatternOn);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(LightManager.ShowSearchPatternOn ? "Showing " : "Hiding search pattern");
			}
			else if (_params[0].EqualsCaseInsensitive("clearreg"))
			{
				LightManager.Clear();
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Cleared.");
			}
			else if (_params[0].EqualsCaseInsensitive("liste"))
			{
				Dictionary<Vector3, Light> lightsEffecting = LightManager.GetLightsEffecting(GameManager.Instance.World.GetPrimaryPlayer().position);
				if (lightsEffecting != null)
				{
					int num = 0;
					{
						foreach (KeyValuePair<Vector3, Light> item in lightsEffecting)
						{
							Vector3 position = item.Value.transform.position;
							SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("#{0} {1} {2}", num++, item.Value.name, position.ToCultureInvariantString("g")));
						}
						return;
					}
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No lights");
			}
			else if (_params[0].EqualsCaseInsensitive("list"))
			{
				allLights = Object.FindObjectsOfType<Light>();
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(allLights.Length + " lights:");
				for (int i = 0; i < allLights.Length; i++)
				{
					Light light = allLights[i];
					string line = string.Format("#{0} {1} {2} {3}", i, light.name, light.transform.position.ToCultureInvariantString("g"), light.enabled ? "enabled" : "disabled");
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(line);
				}
			}
			else if (_params[0].EqualsCaseInsensitive("listfile"))
			{
				StreamWriter streamWriter = SdFile.CreateText("Lights.txt");
				if (streamWriter != null)
				{
					allLights = Object.FindObjectsOfType<Light>();
					streamWriter.WriteLine(allLights.Length + " lights:");
					for (int j = 0; j < allLights.Length; j++)
					{
						Light light2 = allLights[j];
						string value = string.Format("#{0} {1} {2} {3}", j, light2.name, light2.transform.position.ToCultureInvariantString("g"), light2.enabled ? "enabled" : "disabled");
						streamWriter.WriteLine(value);
					}
					streamWriter.Close();
				}
			}
			else if (_params[0].EqualsCaseInsensitive("regtest"))
			{
				LightLOD[] array = Object.FindObjectsOfType<LightLOD>();
				for (int k = 0; k < array.Length; k++)
				{
					array[k].TestRegistration();
				}
			}
			else if (_params[0].EqualsCaseInsensitive("disableall"))
			{
				allLights = Object.FindObjectsOfType<Light>();
				Light[] array2 = allLights;
				for (int l = 0; l < array2.Length; l++)
				{
					array2[l].enabled = false;
				}
			}
			else if (_params[0].EqualsCaseInsensitive("enableall"))
			{
				allLights = Object.FindObjectsOfType<Light>();
				Light[] array2 = allLights;
				for (int l = 0; l < array2.Length; l++)
				{
					array2[l].enabled = true;
				}
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown command");
			}
			return;
		}
		float num2 = 1f;
		if (_params.Count > 1)
		{
			num2 = StringParsers.ParseFloat(_params[1]);
			if (_params[0].EqualsCaseInsensitive("level"))
			{
				int entityId = Mathf.FloorToInt(num2);
				EntityAlive entityAlive = GameManager.Instance.World.GetEntity(entityId) as EntityAlive;
				if ((bool)entityAlive)
				{
					float selfLight;
					float stealthLightLevel = LightManager.GetStealthLightLevel(entityAlive, out selfLight);
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("LightLevel for player(" + entityId + ") = " + (stealthLightLevel + selfLight).ToCultureInvariantString());
				}
			}
			else
			{
				if (_params[0].EqualsCaseInsensitive("disable"))
				{
					allLights = Object.FindObjectsOfType<Light>();
					if ((int)num2 < allLights.Length)
					{
						allLights[(int)num2].enabled = false;
					}
					return;
				}
				if (_params[0].EqualsCaseInsensitive("enable"))
				{
					allLights = Object.FindObjectsOfType<Light>();
					if ((int)num2 < allLights.Length)
					{
						allLights[(int)num2].enabled = true;
					}
					return;
				}
			}
		}
		if (_params[0].EqualsCaseInsensitive("sec"))
		{
			if (Camera.main != null)
			{
				LightViewer component3 = Camera.main.GetComponent<LightViewer>();
				if (component3 != null)
				{
					component3.SetUpdateFrequency(num2);
				}
			}
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown command");
		}
	}
}
