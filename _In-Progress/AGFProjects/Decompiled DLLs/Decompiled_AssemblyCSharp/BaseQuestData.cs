using System.Collections.Generic;

public class BaseQuestData
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public int questCode;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<int> entityList = new List<int>();

	public void AddSharedQuester(int _entityID)
	{
		if (!entityList.Contains(_entityID))
		{
			entityList.Add(_entityID);
			if (GameManager.Instance.World.GetEntity(_entityID) is EntityPlayer player)
			{
				OnAdd(player);
			}
		}
	}

	public void RemoveSharedQuester(EntityPlayer _player)
	{
		if (entityList.Contains(_player.entityId))
		{
			entityList.Remove(_player.entityId);
		}
		if (entityList.Count == 0)
		{
			OnRemove(_player);
			RemoveFromDictionary();
		}
	}

	public bool ContainsEntity(int _entityID)
	{
		return entityList.Contains(_entityID);
	}

	public virtual void SetModifier(string _name)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void RemoveFromDictionary()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnCreated()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnAdd(EntityPlayer player)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnRemove(EntityPlayer player)
	{
	}

	public void Remove()
	{
		entityList.Clear();
		OnRemove(null);
	}
}
