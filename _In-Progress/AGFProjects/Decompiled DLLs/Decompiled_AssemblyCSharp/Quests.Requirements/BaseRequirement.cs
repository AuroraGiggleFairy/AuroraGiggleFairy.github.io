namespace Quests.Requirements;

public abstract class BaseRequirement
{
	public enum RequirementTypes
	{
		Buff,
		Holding,
		Level,
		Wearing
	}

	public DynamicProperties Properties;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string ID { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Value { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool Complete
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Quest OwnerQuest { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public QuestClass Owner { get; set; }

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

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int Phase { get; set; }

	public virtual void HandleVariables()
	{
		ID = OwnerQuest.ParseVariable(ID);
		Value = OwnerQuest.ParseVariable(Value);
	}

	public virtual void SetupRequirement()
	{
	}

	public virtual bool CheckRequirement()
	{
		return false;
	}

	public virtual BaseRequirement Clone()
	{
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public BaseRequirement()
	{
	}
}
