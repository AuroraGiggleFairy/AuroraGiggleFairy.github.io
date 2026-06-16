using System.Collections.Generic;

public class LongSetGroups
{
	public class Group
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSetLong keys = new HashSetLong();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<HashSetLong> sets = new List<HashSetLong>();

		public IEnumerable<long> Keys => keys;

		public IEnumerable<HashSetLong> Sets => sets;

		public int Count => keys.Count;

		public void AddSet(HashSetLong addSet)
		{
			sets.Add(addSet);
			keys.UnionWith(addSet);
		}

		public bool RemoveSet(HashSetLong removeSet, List<HashSetLong> orphanedSets)
		{
			bool flag = false;
			for (int num = sets.Count - 1; num >= 0; num--)
			{
				if (sets[num].SetEquals(removeSet))
				{
					sets.RemoveAt(num);
					flag = true;
					break;
				}
			}
			if (flag)
			{
				if (sets.Count > 1)
				{
					HashSetLong hashSetLong = sets[sets.Count - 1];
					for (int num2 = sets.Count - 2; num2 >= 0; num2--)
					{
						HashSetLong hashSetLong2 = sets[num2];
						if (!hashSetLong.Overlaps(hashSetLong2))
						{
							orphanedSets.Add(hashSetLong2);
							sets.RemoveAt(num2);
						}
					}
				}
				keys.Clear();
				foreach (HashSetLong set in sets)
				{
					keys.UnionWith(set);
				}
			}
			return flag;
		}

		public bool Contains(long key)
		{
			return keys.Contains(key);
		}

		public void MergeFrom(Group other)
		{
			foreach (HashSetLong set in other.sets)
			{
				bool flag = false;
				foreach (HashSetLong set2 in sets)
				{
					if (set2.SetEquals(set))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					sets.Add(set);
					keys.UnionWith(set);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Group> groups = new List<Group>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, Group> groupsByLongKey = new Dictionary<long, Group>();

	public IReadOnlyCollection<Group> Groups => groups;

	public int Count => groups.Count;

	public int GroupedLongsCount => groupsByLongKey.Count;

	public Group MergeOrCreateGroup(HashSetLong longsToGroup)
	{
		Group obj = null;
		foreach (long item in longsToGroup)
		{
			if (!groupsByLongKey.TryGetValue(item, out var value))
			{
				continue;
			}
			if (obj == null)
			{
				obj = value;
			}
			else
			{
				if (obj == value)
				{
					continue;
				}
				obj.MergeFrom(value);
				groups.Remove(value);
				foreach (long key in value.Keys)
				{
					groupsByLongKey[key] = obj;
				}
			}
		}
		if (obj == null)
		{
			obj = new Group();
			obj.AddSet(new HashSetLong(longsToGroup));
			groups.Add(obj);
		}
		else
		{
			obj.AddSet(new HashSetLong(longsToGroup));
		}
		foreach (long item2 in longsToGroup)
		{
			groupsByLongKey[item2] = obj;
		}
		return obj;
	}

	public void RemoveGroupedKeys(HashSetLong keys)
	{
		List<HashSetLong> list = new List<HashSetLong>();
		for (int num = groups.Count - 1; num >= 0; num--)
		{
			Group obj = groups[num];
			if (obj.RemoveSet(keys, list))
			{
				foreach (long key in keys)
				{
					if (!obj.Contains(key))
					{
						groupsByLongKey.Remove(key);
					}
				}
				foreach (HashSetLong item in list)
				{
					foreach (long item2 in item)
					{
						groupsByLongKey.Remove(item2);
					}
				}
				if (obj.Count == 0)
				{
					groups.RemoveAt(num);
				}
			}
		}
		foreach (HashSetLong item3 in list)
		{
			MergeOrCreateGroup(item3);
		}
	}

	public bool TryGetGroup(long key, out Group group)
	{
		return groupsByLongKey.TryGetValue(key, out group);
	}

	public void Clear()
	{
		groupsByLongKey.Clear();
		groups.Clear();
	}
}
