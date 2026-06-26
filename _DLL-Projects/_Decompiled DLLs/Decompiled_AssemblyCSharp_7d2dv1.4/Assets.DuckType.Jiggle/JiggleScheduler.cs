using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.DuckType.Jiggle;

public static class JiggleScheduler
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<Jiggle, int> s_Records = new Dictionary<Jiggle, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Jiggle> s_OrderedRecords = new List<Jiggle>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Jiggle m_UpdateTriggerJiggle = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isDirty;

	public static void Register(Jiggle jiggleBone)
	{
		s_Records[jiggleBone] = GetHierarchyDepth(jiggleBone.transform);
		isDirty = true;
	}

	public static void Deregister(Jiggle jiggleBone)
	{
		s_Records.Remove(jiggleBone);
		isDirty = true;
	}

	public static void Update(Jiggle jiggle)
	{
		if (isDirty)
		{
			isDirty = false;
			UpdateOrderedRecords();
		}
		if (!(jiggle == m_UpdateTriggerJiggle))
		{
			return;
		}
		foreach (Jiggle s_OrderedRecord in s_OrderedRecords)
		{
			if (s_OrderedRecord.enabled && !s_OrderedRecord.UpdateWithPhysics)
			{
				s_OrderedRecord.ScheduledUpdate(Time.deltaTime);
			}
		}
	}

	public static void FixedUpdate(Jiggle jiggle)
	{
		if (!(jiggle == m_UpdateTriggerJiggle))
		{
			return;
		}
		foreach (Jiggle s_OrderedRecord in s_OrderedRecords)
		{
			if (s_OrderedRecord.enabled && s_OrderedRecord.UpdateWithPhysics)
			{
				s_OrderedRecord.ScheduledUpdate(Time.fixedDeltaTime);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateOrderedRecords()
	{
		s_OrderedRecords = (from x in s_Records
			orderby x.Value
			select x.Key).ToList();
		m_UpdateTriggerJiggle = s_OrderedRecords.FirstOrDefault();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int GetHierarchyDepth(Transform t)
	{
		if (!(t == null))
		{
			return GetHierarchyDepth(t.parent) + 1;
		}
		return -1;
	}
}
