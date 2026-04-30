using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionResetMap : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool removeDiscovery;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool removeWaypoints;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRemoveDiscovery = "remove_discovery";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRemoveWaypoints = "remove_waypoints";

	public override void OnClientPerform(Entity target)
	{
		if (!(target is EntityPlayerLocal entityPlayerLocal))
		{
			return;
		}
		if (removeDiscovery)
		{
			entityPlayerLocal.ChunkObserver.mapDatabase.Clear();
		}
		if (!removeWaypoints)
		{
			return;
		}
		for (int i = 0; i < entityPlayerLocal.Waypoints.Collection.list.Count; i++)
		{
			Waypoint waypoint = entityPlayerLocal.Waypoints.Collection.list[i];
			if (waypoint.navObject != null)
			{
				NavObjectManager.Instance.UnRegisterNavObject(waypoint.navObject);
			}
		}
		entityPlayerLocal.WaypointInvites.Clear();
		entityPlayerLocal.Waypoints.Collection.Clear();
		entityPlayerLocal.markerPosition = Vector3i.zero;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseBool(PropRemoveDiscovery, ref removeDiscovery);
		properties.ParseBool(PropRemoveWaypoints, ref removeWaypoints);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionResetMap
		{
			removeDiscovery = removeDiscovery,
			removeWaypoints = removeWaypoints
		};
	}
}
