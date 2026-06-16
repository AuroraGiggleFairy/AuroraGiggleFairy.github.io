using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class AutomationScript
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class Vector3Converter : JsonConverter<Vector3>
	{
		public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("x");
			writer.WriteValue(value.x);
			writer.WritePropertyName("y");
			writer.WriteValue(value.y);
			writer.WritePropertyName("z");
			writer.WriteValue(value.z);
			writer.WriteEndObject();
		}

		public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			return new Vector3(jObject.Value<float>("x"), jObject.Value<float>("y"), jObject.Value<float>("z"));
		}
	}

	public string name = "unnamed";

	public string defaultSessionDir = string.Empty;

	public List<AutomationStep> steps = new List<AutomationStep>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly JsonSerializerSettings s_jsonSettings = new JsonSerializerSettings
	{
		Formatting = Formatting.Indented,
		Converters = 
		{
			(JsonConverter)new StringEnumConverter(),
			(JsonConverter)new Vector3Converter()
		}
	};

	public string ResolveSessionDir()
	{
		if (!string.IsNullOrEmpty(defaultSessionDir))
		{
			return defaultSessionDir;
		}
		return Constants.cVersionInformation.ShortString + "_" + GamePrefs.GetString(EnumGamePrefs.GameWorld) + "_" + GamePrefs.GetString(EnumGamePrefs.GameName) + DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss.fff");
	}

	public int CountStepsOfType(AutomationStep.StepType stepType)
	{
		return steps.Count([PublicizedFrom(EAccessModifier.Internal)] (AutomationStep s) => s.type == stepType);
	}

	public string Validate()
	{
		if (steps.Count == 0)
		{
			return "Script '" + name + "' has no steps.";
		}
		int num = 0;
		for (int i = 0; i < steps.Count; i++)
		{
			AutomationStep automationStep = steps[i];
			if (automationStep.type == AutomationStep.StepType.LoadGame)
			{
				if (string.IsNullOrEmpty(automationStep.world))
				{
					return $"Script '{name}' step [{i}] LoadGame: 'world' is empty.";
				}
				if (string.IsNullOrEmpty(automationStep.gameName))
				{
					return $"Script '{name}' step [{i}] LoadGame: 'gameName' is empty.";
				}
			}
			if (automationStep.type == AutomationStep.StepType.MovePingPong)
			{
				if (automationStep.duration <= 0f)
				{
					return $"Script '{name}' step [{i}] MovePingPong: duration must be > 0.";
				}
				if (automationStep.pingPongCount < 1)
				{
					return $"Script '{name}' step [{i}] MovePingPong: pingPongCount must be >= 1.";
				}
				if (automationStep.position == automationStep.positionB)
				{
					return $"Script '{name}' step [{i}] MovePingPong: point A and point B are identical.";
				}
			}
			if (automationStep.type == AutomationStep.StepType.StartPerfSession)
			{
				if (automationStep.runCount < 1)
				{
					return $"Script '{name}' step [{i}] StartPerfSession: runCount must be >= 1.";
				}
				if (num > 0)
				{
					return $"Script '{name}' step [{i}] StartPerfSession: nested sessions are not supported.";
				}
				num++;
			}
			if (automationStep.type == AutomationStep.StepType.StopPerfSession)
			{
				if (num == 0)
				{
					return $"Script '{name}' step [{i}] StopPerfSession: no matching StartPerfSession.";
				}
				num--;
			}
		}
		if (num > 0)
		{
			return "Script '" + name + "': StartPerfSession without matching StopPerfSession.";
		}
		return null;
	}

	public string Describe()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Script '{name}'  steps={steps.Count}");
		for (int i = 0; i < steps.Count; i++)
		{
			stringBuilder.AppendLine(steps[i].Describe(i));
		}
		return stringBuilder.ToString().TrimEnd();
	}

	public static string GetScriptsDirectory()
	{
		string text = AutomationRunner.GetAutomationDataPath() + "Scripts";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		return text;
	}

	public void SaveToFile(string scriptName = null)
	{
		Log.Error("[AutomationScript] Disabled for this build type.");
	}

	public static AutomationScript LoadFromFile(string scriptName)
	{
		Log.Error("[AutomationRunner] Disabled for this build type.");
		return null;
	}

	public static List<string> ListSavedScripts()
	{
		return (from n in Directory.GetFiles(GetScriptsDirectory(), "*.json").Select(Path.GetFileNameWithoutExtension)
			orderby n
			select n).ToList();
	}
}
