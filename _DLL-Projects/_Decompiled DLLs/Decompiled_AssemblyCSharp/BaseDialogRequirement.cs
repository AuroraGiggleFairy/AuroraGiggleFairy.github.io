using System.Collections.Generic;

public abstract class BaseDialogRequirement
{
	public enum RequirementTypes
	{
		Buff,
		QuestStatus,
		QuestsAvailable,
		QuestTier,
		QuestTierHighest,
		QuestEditorTag,
		Skill,
		Admin,
		DroneState,
		DroneStateExclude,
		CVar
	}

	public enum RequirementVisibilityTypes
	{
		AlternateText,
		Hide
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string ID { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Value { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Tag { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Dialog Owner { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public RequirementVisibilityTypes RequirementVisibilityType { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Description
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string StatusText
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public virtual RequirementTypes RequirementType => RequirementTypes.Buff;

	public virtual List<string> GetRequirementIDTypes()
	{
		return null;
	}

	public virtual string GetRequiredDescription(EntityPlayer player)
	{
		return "";
	}

	public virtual void SetupRequirement()
	{
	}

	public virtual bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		return false;
	}

	public virtual BaseDialogRequirement Clone()
	{
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public BaseDialogRequirement()
	{
	}
}
