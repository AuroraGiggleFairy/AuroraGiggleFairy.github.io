using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementProgression : BaseOperationRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string progressionName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public ProgressionValue pv;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string valueText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropProgressionName = "name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropValue = "value";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override object LeftSide(Entity target)
	{
		if (target is EntityAlive { Progression: not null } entityAlive)
		{
			pv = entityAlive.Progression.GetProgressionValue(progressionName);
			if (pv != null)
			{
				return pv.GetCalculatedLevel(entityAlive);
			}
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override object RightSide(Entity target)
	{
		return GameEventManager.GetIntValue(target as EntityAlive, valueText);
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropProgressionName, ref progressionName);
		properties.ParseString(PropValue, ref valueText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementProgression
		{
			Invert = Invert,
			operation = operation,
			progressionName = progressionName,
			valueText = valueText
		};
	}
}
