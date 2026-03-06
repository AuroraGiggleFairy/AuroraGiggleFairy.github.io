public abstract class BaseObjectiveModifier
{
	public DynamicProperties Properties;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public BaseObjective OwnerObjective { get; set; }

	public BaseObjectiveModifier()
	{
	}

	public void HandleAddHooks()
	{
		AddHooks();
	}

	public void HandleRemoveHooks()
	{
		RemoveHooks();
	}

	public virtual void AddHooks()
	{
	}

	public virtual void RemoveHooks()
	{
	}

	public virtual BaseObjectiveModifier Clone()
	{
		return null;
	}

	public virtual void ParseProperties(DynamicProperties properties)
	{
		Properties = properties;
		OwnerObjective.OwnerQuestClass.HandleVariablesForProperties(properties);
	}
}
