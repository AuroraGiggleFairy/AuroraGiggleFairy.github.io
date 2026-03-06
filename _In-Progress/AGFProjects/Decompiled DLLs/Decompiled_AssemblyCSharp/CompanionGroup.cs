using System.Collections.Generic;

public class CompanionGroup
{
	public OnCompanionGroupChanged OnGroupChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityAlive> MemberList = new List<EntityAlive>();

	public EntityAlive this[int index] => MemberList[index];

	public int Count => MemberList.Count;

	public void Add(EntityAlive entity)
	{
		MemberList.Add(entity);
		OnGroupChanged?.Invoke();
	}

	public void Remove(EntityAlive entity)
	{
		MemberList.Remove(entity);
		OnGroupChanged?.Invoke();
	}

	public int IndexOf(EntityAlive entity)
	{
		return MemberList.IndexOf(entity);
	}
}
