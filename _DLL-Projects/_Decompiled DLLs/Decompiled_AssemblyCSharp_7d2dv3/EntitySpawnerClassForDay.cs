using System.Collections.Generic;

public class EntitySpawnerClassForDay
{
	public bool bDynamicSpawner;

	public bool bWrapDays;

	public bool bClampDays;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntitySpawnerClass> days = new List<EntitySpawnerClass>();

	public void AddForDay(int _day, EntitySpawnerClass _class)
	{
		if (_day != 0 && days.Count == 0)
		{
			days.Add(_class);
		}
		while (days.Count <= _day)
		{
			days.Add(null);
		}
		days[_day] = _class;
	}

	public EntitySpawnerClass Day(int _day)
	{
		if (days.Count == 0)
		{
			return null;
		}
		if (bWrapDays && _day > 0 && _day >= days.Count)
		{
			if (days.Count > 1)
			{
				_day %= days.Count - 1;
				if (_day == 0)
				{
					_day = days.Count - 1;
				}
			}
			else
			{
				_day = 1;
			}
			if (_day == 0)
			{
				_day++;
			}
		}
		else if (bClampDays && _day >= days.Count && days.Count > 0)
		{
			_day = days.Count - 1;
		}
		if (_day >= days.Count || days[_day] == null)
		{
			return days[0];
		}
		return days[_day];
	}

	public int Count()
	{
		return days.Count;
	}
}
