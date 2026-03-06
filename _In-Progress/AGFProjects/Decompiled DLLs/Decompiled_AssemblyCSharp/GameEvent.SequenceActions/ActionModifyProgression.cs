using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionModifyProgression : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum ModifyTypes
	{
		Set,
		Add,
		Remove
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ModifyTypes ModifyType;

	public string[] ProgressionNames;

	public string[] Values;

	public static string PropModifyType = "modify_type";

	public static string PropProgressionNames = "progression_names";

	public static string PropValues = "values";

	public override void OnClientPerform(Entity target)
	{
		if (!(target is EntityPlayerLocal entityPlayerLocal))
		{
			return;
		}
		for (int i = 0; i < ProgressionNames.Length; i++)
		{
			string value = ((Values != null && Values.Length > i) ? Values[i] : "1");
			int num = 1;
			ProgressionValue progressionValue = entityPlayerLocal.Progression.GetProgressionValue(ProgressionNames[i]);
			num = GameEventManager.GetIntValue(entityPlayerLocal, value, 1);
			switch (ModifyType)
			{
			case ModifyTypes.Set:
				progressionValue.Level = num;
				break;
			case ModifyTypes.Add:
				progressionValue.Level += num;
				break;
			}
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropProgressionNames))
		{
			ProgressionNames = properties.Values[PropProgressionNames].Replace(" ", "").Split(',');
			if (properties.Values.ContainsKey(PropValues))
			{
				Values = properties.Values[PropValues].Replace(" ", "").Split(',');
			}
			else
			{
				Values = null;
			}
		}
		else
		{
			ProgressionNames = null;
			Values = null;
		}
		properties.ParseEnum(PropModifyType, ref ModifyType);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionModifyProgression
		{
			ModifyType = ModifyType,
			ProgressionNames = ProgressionNames,
			Values = Values,
			targetGroup = targetGroup
		};
	}
}
