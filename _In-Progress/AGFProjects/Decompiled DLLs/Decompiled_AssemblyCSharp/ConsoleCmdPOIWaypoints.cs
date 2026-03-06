using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPOIWaypoints : ConsoleCmdAbstract
{
	public class POIWaypoint : Waypoint
	{
		public POIWaypoint()
		{
			IsSaved = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int SmallPoiVolumeLimit = 100;

	public override bool IsExecuteOnClient => true;

	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "poiwaypoints", "pwp" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Adds waypoints for specified POIs.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return getDescription() + "\n\npwp * - adds waypoints to all POIs in the world.\npwp <name> - adds waypoints to all POIs that starts with the name.\npwp <distance> - adds waypoints to all POIs with the specified distance.\npwp * <distance> - adds waypoints to all POIs within the specified distance.\npwp <name> <distance> - adds waypoints to all POIs within the specified distance that start with the name.\npwp -clear - removes all POI waypoints.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
		}
		else if (_params.Count == 1)
		{
			float result;
			if (_params[0] == "-clear")
			{
				NavObjectManager instance = NavObjectManager.Instance;
				EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
				for (int num = primaryPlayer.Waypoints.Collection.list.Count - 1; num >= 0; num--)
				{
					Waypoint waypoint = primaryPlayer.Waypoints.Collection.list[num];
					if (waypoint is POIWaypoint pOIWaypoint)
					{
						instance.UnRegisterNavObject(pOIWaypoint.navObject);
						primaryPlayer.Waypoints.Collection.Remove(waypoint);
					}
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("POI Waypoints have been cleared.");
			}
			else if (_params[0] == "*")
			{
				CreateWaypoints("", 0f);
			}
			else if (float.TryParse(_params[0], out result))
			{
				CreateWaypoints("", result);
			}
			else
			{
				CreateWaypoints(_params[0], 0f);
			}
		}
		else
		{
			if (_params.Count != 2)
			{
				return;
			}
			if (float.TryParse(_params[1], out var result2))
			{
				if (_params[0] == "*")
				{
					CreateWaypoints("", result2);
				}
				else
				{
					CreateWaypoints(_params[0], result2);
				}
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[1] + "\" is not a valid distance.");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateWaypoints(string filterName, float distance)
	{
		GameManager instance = GameManager.Instance;
		int num = 0;
		if (instance != null && instance.GetDynamicPrefabDecorator() != null)
		{
			NavObjectManager instance2 = NavObjectManager.Instance;
			List<PrefabInstance> dynamicPrefabs = instance.GetDynamicPrefabDecorator().GetDynamicPrefabs();
			if (dynamicPrefabs != null)
			{
				float num2 = distance * distance;
				EntityPlayer primaryPlayer = instance.World.GetPrimaryPlayer();
				foreach (PrefabInstance item in dynamicPrefabs)
				{
					if ((distance == 0f || (primaryPlayer.position - item.boundingBoxPosition).sqrMagnitude < num2) && item.boundingBoxSize.Volume() >= 100 && item.name.StartsWith(filterName))
					{
						POIWaypoint pOIWaypoint = new POIWaypoint();
						pOIWaypoint.pos = item.boundingBoxPosition;
						pOIWaypoint.name.Update(item.prefab.PrefabName, PlatformManager.MultiPlatform.User.PlatformUserId);
						pOIWaypoint.icon = "ui_game_symbol_map_trader";
						pOIWaypoint.ownerId = null;
						pOIWaypoint.lastKnownPositionEntityId = -1;
						if (!primaryPlayer.Waypoints.ContainsWaypoint(pOIWaypoint))
						{
							NavObject navObject = instance2.RegisterNavObject("waypoint", pOIWaypoint.pos, pOIWaypoint.icon, hiddenOnCompass: true);
							navObject.UseOverrideColor = true;
							navObject.OverrideColor = Color.cyan;
							navObject.IsActive = true;
							navObject.name = pOIWaypoint.name.Text;
							pOIWaypoint.navObject = navObject;
							primaryPlayer.Waypoints.Collection.Add(pOIWaypoint);
							num++;
						}
					}
				}
			}
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Added {0} POI waypoints.", num);
	}
}
