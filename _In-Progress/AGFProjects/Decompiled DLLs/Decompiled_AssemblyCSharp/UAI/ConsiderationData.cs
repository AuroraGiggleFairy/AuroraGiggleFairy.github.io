using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class ConsiderationData
{
	public List<Entity> EntityTargets;

	public List<Vector3> WaypointTargets;

	public ConsiderationData()
	{
		EntityTargets = new List<Entity>();
		WaypointTargets = new List<Vector3>();
	}
}
