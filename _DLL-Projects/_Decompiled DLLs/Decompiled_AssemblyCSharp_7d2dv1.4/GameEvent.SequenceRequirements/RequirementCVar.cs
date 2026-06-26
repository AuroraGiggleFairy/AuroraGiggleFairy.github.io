using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementCVar : BaseOperationRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string cvar = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string valueText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCvar = "cvar";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropValue = "value";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float LeftSide(Entity target)
	{
		if (!(target is EntityAlive entityAlive))
		{
			return 0f;
		}
		return entityAlive.Buffs.GetCustomVar(cvar);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float RightSide(Entity target)
	{
		return GameEventManager.GetFloatValue(target as EntityAlive, valueText);
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropCvar, ref cvar);
		properties.ParseString(PropValue, ref valueText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementCVar
		{
			Invert = Invert,
			operation = operation,
			cvar = cvar,
			valueText = valueText
		};
	}
}
