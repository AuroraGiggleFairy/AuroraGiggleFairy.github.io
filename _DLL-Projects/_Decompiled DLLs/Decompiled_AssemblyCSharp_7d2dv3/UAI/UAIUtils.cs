using System.Collections.Generic;
using UnityEngine;

namespace UAI;

public static class UAIUtils
{
	public class NearestWaypointSorter : IComparer<Vector3>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public Entity self;

		public NearestWaypointSorter(Entity _self)
		{
			self = _self;
		}

		public int Compare(Vector3 _obj1, Vector3 _obj2)
		{
			float distanceSq = self.GetDistanceSq(_obj1);
			float distanceSq2 = self.GetDistanceSq(_obj2);
			if (distanceSq < distanceSq2)
			{
				return -1;
			}
			if (!(distanceSq <= distanceSq2))
			{
				return 1;
			}
			return 0;
		}
	}

	public class NearestEntitySorter : IComparer<Entity>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public Entity self;

		public NearestEntitySorter(Entity _self)
		{
			self = _self;
		}

		public int Compare(Entity _obj1, Entity _obj2)
		{
			float distanceSq = self.GetDistanceSq(_obj1);
			float distanceSq2 = self.GetDistanceSq(_obj2);
			if (distanceSq < distanceSq2)
			{
				return -1;
			}
			if (!(distanceSq <= distanceSq2))
			{
				return 1;
			}
			return 0;
		}
	}

	public static float DistanceSqr(Vector3 pointA, Vector3 pointB)
	{
		Vector3 vector = pointA - pointB;
		return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
	}

	public static float DistanceSqr(Vector2 pointA, Vector2 pointB)
	{
		Vector2 vector = pointA - pointB;
		return vector.x * vector.x + vector.y * vector.y;
	}

	public static EntityAlive ConvertToEntityAlive(object obj)
	{
		EntityAlive result = null;
		try
		{
			result = (EntityAlive)obj;
		}
		catch
		{
		}
		return result;
	}
}
