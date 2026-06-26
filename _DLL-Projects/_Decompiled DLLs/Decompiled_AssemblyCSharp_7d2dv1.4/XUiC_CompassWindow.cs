using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CompassWindow : XUiController
{
	public static string ID = "";

	public List<UISprite> waypointSpriteList = new List<UISprite>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ulong> daytimeFormatter = new CachedStringFormatter<ulong>([PublicizedFrom(EAccessModifier.Internal)] (ulong _worldTime) => ValueDisplayFormatters.WorldTime(_worldTime, Localization.Get("xuiDayTimeLong")));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt dayFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> timeFormatter = new CachedStringFormatter<int, int>([PublicizedFrom(EAccessModifier.Internal)] (int _hour, int _min) => $"{_hour:00}:{_min:00}");

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showSleeperVolumes = true;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer
	{
		get; [PublicizedFrom(EAccessModifier.Internal)]
		set;
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		for (int i = 0; i < 50; i++)
		{
			GameObject gameObject = new GameObject();
			gameObject.transform.parent = base.ViewComponent.UiTransform;
			waypointSpriteList.Add(gameObject.AddComponent<UISprite>());
			waypointSpriteList[waypointSpriteList.Count - 1].atlas = base.xui.GetAtlasByName("UIAtlas", "menu_empty");
			waypointSpriteList[waypointSpriteList.Count - 1].transform.localScale = Vector3.one;
			waypointSpriteList[waypointSpriteList.Count - 1].spriteName = "menu_empty";
			waypointSpriteList[waypointSpriteList.Count - 1].SetDimensions(20, 20);
			waypointSpriteList[waypointSpriteList.Count - 1].color = Color.clear;
			waypointSpriteList[waypointSpriteList.Count - 1].pivot = UIWidget.Pivot.Center;
			waypointSpriteList[waypointSpriteList.Count - 1].depth = 12;
			waypointSpriteList[waypointSpriteList.Count - 1].gameObject.layer = 12;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (localPlayer == null)
		{
			localPlayer = base.xui.playerUI.entityPlayer;
		}
		showSleeperVolumes = true;
		base.ViewComponent.IsVisible = !localPlayer.IsDead() && base.xui.playerUI.windowManager.IsHUDEnabled();
		if (localPlayer != null && localPlayer.playerCamera != null)
		{
			int waypointSpriteIndex = 0;
			updateNavObjects(localPlayer, ref waypointSpriteIndex);
			updateMarkers(localPlayer, ref waypointSpriteIndex, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.SleepingBag));
			updateMarkers(localPlayer, ref waypointSpriteIndex, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.LandClaim));
			updateMarkers(localPlayer, ref waypointSpriteIndex, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.MapMarker));
			updateMarkers(localPlayer, ref waypointSpriteIndex, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.MapQuickMarker));
			updateMarkers(localPlayer, ref waypointSpriteIndex, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.Backpack));
			if (showSleeperVolumes)
			{
				updateMarkers(localPlayer, ref waypointSpriteIndex, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.Quest));
			}
			updateMarkers(localPlayer, ref waypointSpriteIndex, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.TreasureChest));
			updateMarkers(localPlayer, ref waypointSpriteIndex, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.FetchItem));
			updateMarkers(localPlayer, ref waypointSpriteIndex, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.HiddenCache));
			updateMarkers(localPlayer, ref waypointSpriteIndex, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.RestorePower));
			if (showSleeperVolumes)
			{
				updateMarkers(localPlayer, ref waypointSpriteIndex, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.SleeperVolume));
			}
			updateMarkers(localPlayer, ref waypointSpriteIndex, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.VendingMachine));
			if (GameStats.GetBool(EnumGameStats.AirDropMarker))
			{
				updateMarkers(localPlayer, ref waypointSpriteIndex, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.SupplyDrop));
			}
			for (int i = waypointSpriteIndex; i < waypointSpriteList.Count; i++)
			{
				waypointSpriteList[i].color = Color.clear;
			}
		}
		if (GameManager.Instance != null && GameManager.Instance.World != null && XUi.IsGameRunning())
		{
			RefreshBindings();
		}
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "showtime":
			if (localPlayer != null)
			{
				value = (EffectManager.GetValue(PassiveEffects.NoTimeDisplay, null, 0f, localPlayer) == 0f).ToString();
			}
			else
			{
				value = "true";
			}
			return true;
		case "daytime":
			value = "";
			if (XUi.IsGameRunning())
			{
				value = daytimeFormatter.Format(GameManager.Instance.World.worldTime);
			}
			return true;
		case "day":
			value = "0";
			if (XUi.IsGameRunning())
			{
				int v = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
				value = dayFormatter.Format(v);
			}
			return true;
		case "daytitle":
			value = Localization.Get("xuiDay");
			return true;
		case "time":
			value = "";
			if (XUi.IsGameRunning())
			{
				(int Days, int Hours, int Minutes) tuple2 = GameUtils.WorldTimeToElements(GameManager.Instance.World.worldTime);
				int item = tuple2.Hours;
				int item2 = tuple2.Minutes;
				value = timeFormatter.Format(item, item2);
			}
			return true;
		case "timetitle":
			value = Localization.Get("xuiTime");
			return true;
		case "daycolor":
			value = "FFFFFF";
			if (XUi.IsGameRunning())
			{
				ulong worldTime = GameManager.Instance.World.worldTime;
				int num = GameStats.GetInt(EnumGameStats.BloodMoonWarning);
				var (num2, num3, _) = GameUtils.WorldTimeToElements(worldTime);
				if (num != -1 && GameStats.GetInt(EnumGameStats.BloodMoonDay) == num2 && num <= num3)
				{
					value = "FF0000";
				}
			}
			return true;
		case "compass_rotation":
			if (localPlayer != null && localPlayer.playerCamera != null)
			{
				value = localPlayer.playerCamera.transform.eulerAngles.y.ToString();
			}
			else
			{
				value = "0.0";
			}
			return true;
		case "compass_language":
			if (GamePrefs.GetBool(EnumGamePrefs.OptionsUiCompassUseEnglishCardinalDirections))
			{
				value = Localization.DefaultLanguage;
			}
			else
			{
				value = Localization.language;
			}
			return true;
		default:
			return base.GetBindingValue(ref value, bindingName);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateMarkers(EntityPlayer localPlayer, ref int waypointSpriteIndex, List<Waypoint> waypoints)
	{
		int num = 256;
		Entity entity = ((localPlayer.AttachedToEntity != null) ? localPlayer.AttachedToEntity : localPlayer);
		Vector2 vector = new Vector2(entity.GetPosition().x, entity.GetPosition().z);
		Vector2 rhs = new Vector2(entity.transform.forward.x, entity.transform.forward.z);
		rhs.Normalize();
		Vector2 rhs2 = new Vector2(entity.transform.right.x, entity.transform.right.z);
		rhs2.Normalize();
		float a = 0.25f;
		float b = 1f;
		float num2 = (float)base.ViewComponent.Size.x * 0.5f;
		float num3 = num2 * 1.1f;
		for (int i = 0; i < waypoints.Count; i++)
		{
			if (i >= waypoints.Count)
			{
				waypointSpriteList[i].color = Color.clear;
				break;
			}
			if (i >= waypointSpriteList.Count)
			{
				break;
			}
			Vector2 vector2 = new Vector2(waypoints[i].pos.x, waypoints[i].pos.z) - vector;
			float magnitude = vector2.magnitude;
			if (magnitude > (float)num)
			{
				waypointSpriteList[i].color = Color.clear;
				continue;
			}
			Vector2 normalized = vector2.normalized;
			if (Vector2.Dot(normalized, rhs) < 0.75f)
			{
				waypointSpriteList[i].color = Color.clear;
				continue;
			}
			float num4 = Mathf.Lerp(a, b, 1f - magnitude / (float)num);
			waypointSpriteList[i].atlas = base.xui.GetAtlasByName("UIAtlas", waypoints[i].icon);
			waypointSpriteList[i].spriteName = waypoints[i].icon;
			if (waypoints[i].bTracked)
			{
				waypointSpriteList[i].color = new Color(1f, 1f, 1f, num4);
			}
			else
			{
				waypointSpriteList[i].color = new Color(0.5f, 0.5f, 0.5f, num4);
			}
			waypointSpriteList[i].SetDimensions((int)(25f * num4), (int)(25f * num4));
			waypointSpriteList[i].transform.localPosition = new Vector3(num2 + Vector2.Dot(normalized, rhs2) * num3, -16f);
			waypointSpriteIndex++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateMarkers(EntityPlayerLocal localPlayer, ref int waypointSpriteIndex, List<MapObject> _mapObjectList)
	{
		_ = 10000;
		float num = 0.25f;
		_ = 1f;
		float num2 = (float)base.ViewComponent.Size.x * 0.5f;
		float num3 = num2 * 1.15f;
		Transform transform = localPlayer.playerCamera.transform;
		Entity entity = ((localPlayer.AttachedToEntity != null) ? localPlayer.AttachedToEntity : localPlayer);
		Vector2 vector = new Vector2(entity.GetPosition().x, entity.GetPosition().z);
		Vector2 rhs = new Vector2(transform.forward.x, transform.forward.z);
		rhs.Normalize();
		Vector2 rhs2 = new Vector2(transform.right.x, transform.right.z);
		rhs2.Normalize();
		for (int i = 0; i < _mapObjectList.Count; i++)
		{
			MapObject mapObject = _mapObjectList[i];
			mapObject.RefreshData();
			if (waypointSpriteIndex >= waypointSpriteList.Count)
			{
				break;
			}
			if (!mapObject.IsOnCompass())
			{
				continue;
			}
			if (mapObject is MapObjectZombie)
			{
				showSleeperVolumes = false;
			}
			Vector2 vector2 = new Vector2(mapObject.GetPosition().x, mapObject.GetPosition().z) - vector;
			float magnitude = vector2.magnitude;
			bool flag = true;
			if (_mapObjectList[i].type == EnumMapObjectType.TreasureChest)
			{
				float num4 = (_mapObjectList[i] as MapObjectTreasureChest).DefaultRadius;
				float value = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, num4, localPlayer);
				value = Mathf.Clamp(value, 0f, num4);
				if (magnitude < value)
				{
					float num5 = Mathf.PingPong(Time.time, 0.25f);
					float num6 = 1.25f + num5;
					waypointSpriteList[waypointSpriteIndex].atlas = base.xui.GetAtlasByName("UIAtlas", mapObject.GetMapIcon());
					waypointSpriteList[waypointSpriteIndex].spriteName = mapObject.GetMapIcon();
					waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num6), (int)(25f * num6));
					waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num2, -24f);
					Color mapIconColor = mapObject.GetMapIconColor();
					waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(mapIconColor, Color.red, num5 * 4f);
					waypointSpriteIndex++;
					flag = false;
				}
			}
			string spriteName = mapObject.GetCompassIcon();
			if (_mapObjectList[i].type == EnumMapObjectType.HiddenCache)
			{
				waypointSpriteList[waypointSpriteIndex].flip = UIBasicSprite.Flip.Nothing;
				if (mapObject.GetPosition().y < localPlayer.GetPosition().y - 2f)
				{
					spriteName = _mapObjectList[i].GetCompassDownIcon();
				}
				else if (mapObject.GetPosition().y > localPlayer.GetPosition().y + 2f)
				{
					spriteName = _mapObjectList[i].GetCompassUpIcon();
				}
				waypointSpriteList[waypointSpriteIndex].depth = 100;
				waypointSpriteList[waypointSpriteIndex].atlas = base.xui.GetAtlasByName("UIAtlas", spriteName);
				waypointSpriteList[waypointSpriteIndex].spriteName = spriteName;
				if ((mapObject.GetPosition() - entity.GetPosition()).magnitude < 10f)
				{
					float num7 = Mathf.PingPong(Time.time, 0.25f);
					float num8 = 1.25f + num7;
					waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num8), (int)(25f * num8));
					waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num2, -24f);
					Color mapIconColor2 = mapObject.GetMapIconColor();
					waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(mapIconColor2, Color.grey, num7 * 4f);
					waypointSpriteIndex++;
					flag = false;
				}
			}
			if (_mapObjectList[i].UseUpDownCompassIcons())
			{
				waypointSpriteList[waypointSpriteIndex].flip = UIBasicSprite.Flip.Nothing;
				if (mapObject.GetPosition().y < localPlayer.GetPosition().y - 2f)
				{
					spriteName = _mapObjectList[i].GetCompassDownIcon();
				}
				else if (mapObject.GetPosition().y > localPlayer.GetPosition().y + 3f)
				{
					spriteName = _mapObjectList[i].GetCompassUpIcon();
				}
				waypointSpriteList[waypointSpriteIndex].depth = 100;
				waypointSpriteList[waypointSpriteIndex].atlas = base.xui.GetAtlasByName("UIAtlas", spriteName);
				waypointSpriteList[waypointSpriteIndex].spriteName = spriteName;
			}
			if (!flag)
			{
				continue;
			}
			Vector2 normalized = vector2.normalized;
			if (!mapObject.IsCompassIconClamped() && Vector2.Dot(normalized, rhs) < 0.75f)
			{
				waypointSpriteList[waypointSpriteIndex].color = Color.clear;
				continue;
			}
			float num9 = mapObject.GetCompassIconScale(magnitude);
			waypointSpriteList[waypointSpriteIndex].color = mapObject.GetMapIconColor();
			if (mapObject.IsTracked() && _mapObjectList[i].NearbyCompassBlink() && (mapObject.GetPosition() - entity.GetPosition()).magnitude <= 6f)
			{
				Color mapIconColor3 = mapObject.GetMapIconColor();
				float num10 = Mathf.PingPong(Time.time, 0.5f);
				waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(Color.grey, mapIconColor3, num10 * 4f);
				if (num10 > 0.25f)
				{
					num9 += num10 - 0.25f;
				}
			}
			waypointSpriteList[waypointSpriteIndex].atlas = base.xui.GetAtlasByName("UIAtlas", spriteName);
			waypointSpriteList[waypointSpriteIndex].spriteName = spriteName;
			waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num9), (int)(25f * num9));
			if (Vector2.Dot(normalized, rhs) >= 0.75f)
			{
				waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num2 + Vector2.Dot(normalized, rhs2) * num3, -16f);
			}
			else
			{
				waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num2 + ((Vector2.Dot(normalized, rhs2) < 0f) ? (-0.675f) : 0.675f) * num3, -16f);
			}
			if (mapObject.type == EnumMapObjectType.Entity)
			{
				waypointSpriteList[waypointSpriteIndex].depth = 12 + (int)(num9 * 100f);
			}
			if (!mapObject.IsTracked())
			{
				Color mapIconColor4 = mapObject.GetMapIconColor();
				waypointSpriteList[waypointSpriteIndex].color = new Color(mapIconColor4.r * 0.75f, mapIconColor4.g * 0.75f, mapIconColor4.b * 0.75f) * num9;
			}
			waypointSpriteIndex++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateNavObjects(EntityPlayerLocal localPlayer, ref int waypointSpriteIndex)
	{
		_ = 10000;
		float num = 0.25f;
		_ = 1f;
		float num2 = (float)base.ViewComponent.Size.x * 0.5f;
		float num3 = num2 * 1.15f;
		Transform transform = localPlayer.playerCamera.transform;
		Entity entity = ((localPlayer.AttachedToEntity != null) ? localPlayer.AttachedToEntity : localPlayer);
		Vector2 vector = new Vector2(entity.GetPosition().x, entity.GetPosition().z);
		Vector2 rhs = new Vector2(transform.forward.x, transform.forward.z);
		rhs.Normalize();
		Vector2 rhs2 = new Vector2(transform.right.x, transform.right.z);
		rhs2.Normalize();
		List<NavObject> navObjectList = NavObjectManager.Instance.NavObjectList;
		for (int i = 0; i < navObjectList.Count; i++)
		{
			NavObject navObject = navObjectList[i];
			if (!navObject.IsValid() || !navObject.HasRequirements)
			{
				continue;
			}
			if (waypointSpriteIndex >= waypointSpriteList.Count)
			{
				break;
			}
			if (!navObject.NavObjectClass.IsOnCompass(navObject.IsActive) || navObject.hiddenOnCompass)
			{
				continue;
			}
			NavObjectCompassSettings currentCompassSettings = navObject.CurrentCompassSettings;
			Vector2 vector2 = new Vector2(navObject.GetPosition().x + Origin.position.x, navObject.GetPosition().z + Origin.position.z) - vector;
			float magnitude = vector2.magnitude;
			bool flag = true;
			string spriteName = navObject.GetSpriteName(currentCompassSettings);
			float maxDistance = navObject.GetMaxDistance(currentCompassSettings, localPlayer);
			if ((maxDistance != -1f && magnitude > maxDistance) || (currentCompassSettings.MinDistance > 0f && magnitude < currentCompassSettings.MinDistance))
			{
				continue;
			}
			waypointSpriteList[waypointSpriteIndex].depth = 12 + currentCompassSettings.DepthOffset;
			if (currentCompassSettings.HotZone != null)
			{
				float num4 = 1f;
				if (currentCompassSettings.HotZone.HotZoneType == NavObjectCompassSettings.HotZoneSettings.HotZoneTypes.Treasure)
				{
					float extraData = navObject.ExtraData;
					num4 = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, extraData, localPlayer);
					num4 = Mathf.Clamp(num4, 0f, extraData);
				}
				else if (currentCompassSettings.HotZone.HotZoneType == NavObjectCompassSettings.HotZoneSettings.HotZoneTypes.Custom)
				{
					num4 = currentCompassSettings.HotZone.CustomDistance;
				}
				if (magnitude < num4)
				{
					float num5 = Mathf.PingPong(Time.time, 0.25f);
					float num6 = 1.25f + num5;
					waypointSpriteList[waypointSpriteIndex].atlas = base.xui.GetAtlasByName("UIAtlas", currentCompassSettings.HotZone.SpriteName);
					waypointSpriteList[waypointSpriteIndex].spriteName = currentCompassSettings.HotZone.SpriteName;
					waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num6), (int)(25f * num6));
					waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num2, -24f);
					Color color = currentCompassSettings.HotZone.Color;
					waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(color, Color.red, num5 * 4f);
					waypointSpriteIndex++;
					flag = false;
				}
			}
			if (currentCompassSettings.ShowVerticalCompassIcons)
			{
				waypointSpriteList[waypointSpriteIndex].flip = UIBasicSprite.Flip.Nothing;
				float num7 = localPlayer.GetPosition().y - Origin.position.y;
				if (navObject.GetPosition().y < num7 + currentCompassSettings.ShowDownOffset)
				{
					spriteName = currentCompassSettings.DownSpriteName;
				}
				else if (navObject.GetPosition().y > num7 + currentCompassSettings.ShowUpOffset)
				{
					spriteName = currentCompassSettings.UpSpriteName;
				}
				waypointSpriteList[waypointSpriteIndex].depth = 100;
				waypointSpriteList[waypointSpriteIndex].atlas = base.xui.GetAtlasByName("UIAtlas", spriteName);
				waypointSpriteList[waypointSpriteIndex].spriteName = spriteName;
			}
			if (!flag)
			{
				continue;
			}
			Vector2 normalized = vector2.normalized;
			if (!currentCompassSettings.IconClamped && Vector2.Dot(normalized, rhs) < 0.75f)
			{
				waypointSpriteList[waypointSpriteIndex].color = Color.clear;
				continue;
			}
			float num8 = navObject.GetCompassIconScale(magnitude);
			waypointSpriteList[waypointSpriteIndex].color = (navObject.UseOverrideColor ? navObject.OverrideColor : currentCompassSettings.Color);
			if (currentCompassSettings.HasPulse && (navObject.GetPosition() - entity.GetPosition()).magnitude <= 6f)
			{
				Color b = (navObject.UseOverrideColor ? navObject.OverrideColor : currentCompassSettings.Color);
				float num9 = Mathf.PingPong(Time.time, 0.5f);
				waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(Color.grey, b, num9 * 4f);
				if (num9 > 0.25f)
				{
					num8 += num9 - 0.25f;
				}
			}
			waypointSpriteList[waypointSpriteIndex].atlas = base.xui.GetAtlasByName("UIAtlas", spriteName);
			waypointSpriteList[waypointSpriteIndex].spriteName = spriteName;
			waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num8), (int)(25f * num8));
			if (Vector2.Dot(normalized, rhs) >= 0.75f)
			{
				waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num2 + Vector2.Dot(normalized, rhs2) * num3, -16f);
			}
			else
			{
				waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num2 + ((Vector2.Dot(normalized, rhs2) < 0f) ? (-0.675f) : 0.675f) * num3, -16f);
			}
			if (!navObject.IsActive)
			{
				Color color2 = (navObject.UseOverrideColor ? navObject.OverrideColor : currentCompassSettings.Color);
				if (currentCompassSettings.MinFadePercent != -1f)
				{
					if (currentCompassSettings.MinFadePercent > num8)
					{
						num8 = currentCompassSettings.MinFadePercent;
					}
					waypointSpriteList[waypointSpriteIndex].color = color2 * num8;
				}
				else
				{
					waypointSpriteList[waypointSpriteIndex].color = color2;
				}
			}
			waypointSpriteIndex++;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.playerUI.windowManager.CloseIfOpen("windowpaging");
	}
}
