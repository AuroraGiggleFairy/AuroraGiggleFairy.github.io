using System;
using System.Collections.Generic;

namespace Webserver.LiveData;

public abstract class EntityFilterList<T> where T : Entity
{
	public void Get(List<T> _list)
	{
		_list.Clear();
		try
		{
			List<Entity> list = GameManager.Instance.World.Entities.list;
			for (int i = 0; i < list.Count; i++)
			{
				Entity e = list[i];
				T val = predicate(e);
				if (val != null)
				{
					_list.Add(val);
				}
			}
		}
		catch (Exception e2)
		{
			Log.Exception(e2);
		}
	}

	public int GetCount()
	{
		int num = 0;
		try
		{
			List<Entity> list = GameManager.Instance.World.Entities.list;
			for (int i = 0; i < list.Count; i++)
			{
				Entity e = list[i];
				if (predicate(e) != null)
				{
					num++;
				}
			}
		}
		catch (Exception e2)
		{
			Log.Exception(e2);
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract T predicate(Entity _e);

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityFilterList()
	{
	}
}
