using Steamworks;

namespace Platform.Steam;

public class RichPresence : IRichPresence
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public string currentTimeOfDay = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition currentBiome;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance currentPrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool currentIndoors;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityVehicle currentDriving;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest currentQuest;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool currentBloodmoon;

	public void Init(IPlatform _owner)
	{
	}

	public void UpdateRichPresence(IRichPresence.PresenceStates state)
	{
		World world = GameManager.Instance.World;
		switch (state)
		{
		case IRichPresence.PresenceStates.Menu:
			SteamFriends.ClearRichPresence();
			SteamFriends.SetRichPresence("steam_display", "#Status_AtMainMenu");
			localPlayer = null;
			break;
		case IRichPresence.PresenceStates.Loading:
			SteamFriends.ClearRichPresence();
			SteamFriends.SetRichPresence("steam_display", "#Status_LoadingGame");
			localPlayer = null;
			break;
		case IRichPresence.PresenceStates.Connecting:
			SteamFriends.ClearRichPresence();
			SteamFriends.SetRichPresence("steam_display", "#Status_ConnectingToServer");
			localPlayer = null;
			break;
		case IRichPresence.PresenceStates.InGame:
		{
			if (localPlayer == null)
			{
				localPlayer = world.GetPrimaryPlayer();
			}
			if (localPlayer == null)
			{
				break;
			}
			GameServerInfo gameServerInfo = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
			SteamFriends.SetRichPresence("steam_player_group", gameServerInfo.GetValue(GameInfoString.UniqueId));
			SteamFriends.SetRichPresence("steam_player_group_size", world.Players.Count.ToString());
			SteamFriends.SetRichPresence("day", $"{GameUtils.WorldTimeToDays(world.worldTime):0}");
			float num = SkyManager.TimeOfDay();
			string text = "";
			text = ((num < (float)world.DawnHour || num >= (float)world.DuskHour) ? "Night" : ((!(num >= (float)world.DawnHour) || !(num < 12f)) ? "Afternoon" : "Morning"));
			if (localPlayer.biomeStandingOn == null)
			{
				SteamFriends.ClearRichPresence();
				break;
			}
			EntityVehicle entityVehicle = localPlayer.AttachedToEntity as EntityVehicle;
			bool flag = localPlayer.Stats.AmountEnclosed > 0f;
			Quest activeQuest = localPlayer.QuestJournal.ActiveQuest;
			bool flag2 = GameManager.Instance.World.IsWorldEvent(World.WorldEvent.BloodMoon);
			if (localPlayer.biomeStandingOn == currentBiome && localPlayer.prefab == currentPrefab && !(text != currentTimeOfDay) && flag == currentIndoors && !(entityVehicle != currentDriving) && activeQuest == currentQuest)
			{
				break;
			}
			currentTimeOfDay = text;
			currentBiome = localPlayer.biomeStandingOn;
			currentPrefab = localPlayer.prefab;
			currentIndoors = flag;
			currentDriving = entityVehicle;
			currentQuest = activeQuest;
			currentBloodmoon = flag2;
			SteamFriends.SetRichPresence("timeofday", currentTimeOfDay);
			bool flag3 = false;
			if (currentBloodmoon)
			{
				SteamFriends.SetRichPresence("description", "Surviving the Bloodmoon in the " + currentBiome.LocalizedName);
				flag3 = true;
			}
			if (!flag3 && currentQuest != null)
			{
				flag3 = true;
				if (currentQuest.QuestTags.Test_AnySet(QuestEventManager.restorePowerTag))
				{
					SteamFriends.SetRichPresence("description", "Restoring power in the " + currentBiome.LocalizedName);
				}
				else if (currentQuest.QuestTags.Test_AnySet(QuestEventManager.infestedTag))
				{
					SteamFriends.SetRichPresence("description", "Clearing Infestation in the " + currentBiome.LocalizedName);
				}
				else if (currentQuest.QuestTags.Test_AnySet(QuestEventManager.fetchTag))
				{
					SteamFriends.SetRichPresence("description", "Recovering Supplies in the " + currentBiome.LocalizedName);
				}
				else if (currentQuest.QuestTags.Test_AnySet(QuestEventManager.treasureTag))
				{
					SteamFriends.SetRichPresence("description", "Digging up Supplies in the " + currentBiome.LocalizedName);
				}
				else if (currentQuest.QuestTags.Test_AnySet(QuestEventManager.clearTag))
				{
					SteamFriends.SetRichPresence("description", "Clearing Zombies in the " + currentBiome.LocalizedName);
				}
				else
				{
					flag3 = false;
				}
			}
			if (!flag3)
			{
				if ((bool)currentDriving)
				{
					if (currentDriving is EntityBicycle)
					{
						SteamFriends.SetRichPresence("description", "Pedaling through the " + currentBiome.LocalizedName);
					}
					else if (currentDriving is EntityVGyroCopter)
					{
						SteamFriends.SetRichPresence("description", "Flying through the " + currentBiome.LocalizedName);
					}
					else
					{
						SteamFriends.SetRichPresence("description", "Cruising through the " + currentBiome.LocalizedName);
					}
				}
				else
				{
					if (currentPrefab != null)
					{
						flag3 = true;
						if (currentPrefab.prefab.bTraderArea)
						{
							SteamFriends.SetRichPresence("description", "Visiting " + currentPrefab.prefab.LocalizedName + " in the " + currentBiome.LocalizedName);
						}
						else if (currentIndoors)
						{
							switch (world.GetGameRandom().RandomRange(3))
							{
							case 0:
								SteamFriends.SetRichPresence("description", "Scavenging in the " + currentBiome.LocalizedName);
								break;
							case 1:
								SteamFriends.SetRichPresence("description", "Looting Supplies in the " + currentBiome.LocalizedName);
								break;
							case 2:
								SteamFriends.SetRichPresence("description", "Finding Supplies in the " + currentBiome.LocalizedName);
								break;
							}
						}
						else
						{
							flag3 = false;
						}
					}
					if (!flag3)
					{
						switch (world.GetGameRandom().RandomRange(4))
						{
						case 0:
							SteamFriends.SetRichPresence("description", "Exploring the " + currentBiome.LocalizedName);
							break;
						case 1:
							SteamFriends.SetRichPresence("description", "Wandering the " + currentBiome.LocalizedName);
							break;
						case 2:
							SteamFriends.SetRichPresence("description", "Roaming the " + currentBiome.LocalizedName);
							break;
						case 3:
							SteamFriends.SetRichPresence("description", "Navigating the " + currentBiome.LocalizedName);
							break;
						}
					}
				}
			}
			SteamFriends.SetRichPresence("steam_display", "#Status_InGame");
			break;
		}
		}
	}
}
