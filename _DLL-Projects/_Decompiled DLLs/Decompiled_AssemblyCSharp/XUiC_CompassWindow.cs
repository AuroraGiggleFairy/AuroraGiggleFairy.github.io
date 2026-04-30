using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CompassWindow : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	public List<UISprite> waypointSpriteList = new List<UISprite>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ulong> daytimeFormatter = new CachedStringFormatter<ulong>([PublicizedFrom(EAccessModifier.Internal)] (ulong _worldTime) => ValueDisplayFormatters.WorldTime(_worldTime, Localization.Get("xuiDayTimeLong")));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt dayFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> timeFormatter = new CachedStringFormatter<int, int>([PublicizedFrom(EAccessModifier.Internal)] (int _hour, int _min) => $"{_hour:00}:{_min:00}");

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showSleeperVolumes;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		for (int i = 0; i < 50; i++)
		{
			GameObject gameObject = new GameObject();
			gameObject.transform.parent = base.ViewComponent.UiTransform;
			UISprite uISprite = gameObject.AddComponent<UISprite>();
			waypointSpriteList.Add(uISprite);
			uISprite.atlas = base.xui.GetAtlasByName("UIAtlas", "menu_empty");
			uISprite.transform.localScale = Vector3.one;
			uISprite.spriteName = "menu_empty";
			uISprite.SetDimensions(20, 20);
			uISprite.color = Color.clear;
			uISprite.pivot = UIWidget.Pivot.Center;
			uISprite.depth = 12;
			uISprite.gameObject.layer = 12;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!localPlayer)
		{
			localPlayer = base.xui.playerUI.entityPlayer;
			if (!localPlayer)
			{
				return;
			}
		}
		base.ViewComponent.IsVisible = !localPlayer.IsDead() && base.xui.playerUI.windowManager.IsHUDEnabled();
		if (localPlayer.playerCamera != null)
		{
			World world = GameManager.Instance.World;
			showSleeperVolumes = true;
			int waypointSpriteIndex = 0;
			updateNavObjects(localPlayer, ref waypointSpriteIndex);
			updateMarkers(localPlayer, ref waypointSpriteIndex, world.GetObjectOnMapList(EnumMapObjectType.SleepingBag));
			updateMarkers(localPlayer, ref waypointSpriteIndex, world.GetObjectOnMapList(EnumMapObjectType.LandClaim));
			updateMarkers(localPlayer, ref waypointSpriteIndex, world.GetObjectOnMapList(EnumMapObjectType.MapMarker));
			updateMarkers(localPlayer, ref waypointSpriteIndex, world.GetObjectOnMapList(EnumMapObjectType.MapQuickMarker));
			updateMarkers(localPlayer, ref waypointSpriteIndex, world.GetObjectOnMapList(EnumMapObjectType.Backpack));
			if (showSleeperVolumes)
			{
				updateMarkers(localPlayer, ref waypointSpriteIndex, world.GetObjectOnMapList(EnumMapObjectType.Quest));
			}
			updateMarkers(localPlayer, ref waypointSpriteIndex, world.GetObjectOnMapList(EnumMapObjectType.TreasureChest));
			updateMarkers(localPlayer, ref waypointSpriteIndex, world.GetObjectOnMapList(EnumMapObjectType.FetchItem));
			updateMarkers(localPlayer, ref waypointSpriteIndex, world.GetObjectOnMapList(EnumMapObjectType.HiddenCache));
			updateMarkers(localPlayer, ref waypointSpriteIndex, world.GetObjectOnMapList(EnumMapObjectType.RestorePower));
			if (showSleeperVolumes)
			{
				updateMarkers(localPlayer, ref waypointSpriteIndex, world.GetObjectOnMapList(EnumMapObjectType.SleeperVolume));
			}
			updateMarkers(localPlayer, ref waypointSpriteIndex, world.GetObjectOnMapList(EnumMapObjectType.VendingMachine));
			if (GameStats.GetBool(EnumGameStats.AirDropMarker))
			{
				updateMarkers(localPlayer, ref waypointSpriteIndex, world.GetObjectOnMapList(EnumMapObjectType.SupplyDrop));
			}
			Color clear = Color.clear;
			for (int i = waypointSpriteIndex; i < waypointSpriteList.Count; i++)
			{
				waypointSpriteList[i].color = clear;
			}
		}
		if (XUi.IsGameRunning())
		{
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
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
				(int Days, int Hours, int Minutes) tuple = GameUtils.WorldTimeToElements(GameManager.Instance.World.worldTime);
				int item = tuple.Hours;
				int item2 = tuple.Minutes;
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
		case "playercoretemp":
			value = (localPlayer ? XUiM_Player.GetCoreTemp(localPlayer) : "");
			return true;
		case "coretempcolor":
			value = "FFFFFF";
			if (XUi.IsGameRunning() && (bool)localPlayer)
			{
				float customVar2 = localPlayer.Buffs.GetCustomVar("_coretemp");
				value = GetTemperatureColor(customVar2);
			}
			return true;
		case "outsidetemp":
			value = (localPlayer ? XUiM_Player.GetOutsideTemp(localPlayer) : "");
			return true;
		case "outsidetempcolor":
			value = "FFFFFF";
			if (XUi.IsGameRunning() && (bool)localPlayer)
			{
				float customVar = localPlayer.Buffs.GetCustomVar("_outsidetemp");
				value = GetTemperatureColor(customVar);
			}
			return true;
		default:
			return base.GetBindingValueInternal(ref value, bindingName);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetTemperatureColor(float _temp)
	{
		string result = "FFFFFF";
		if (_temp <= 32f)
		{
			result = "0099FF";
		}
		else if (_temp <= 50f)
		{
			result = "00FFFF";
		}
		else if (_temp >= 85f && _temp < 100f)
		{
			result = "FF8000";
		}
		else if (_temp >= 100f)
		{
			result = "FF0000";
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateMarkers(EntityPlayerLocal localPlayer, ref int waypointSpriteIndex, List<MapObject> _mapObjectList)
	{
		int count = _mapObjectList.Count;
		if (count == 0)
		{
			return;
		}
		float num = (float)base.ViewComponent.Size.x * 0.5f;
		float num2 = num * 1.15f;
		Transform cameraTransform = localPlayer.cameraTransform;
		Entity entity = ((localPlayer.AttachedToEntity != null) ? localPlayer.AttachedToEntity : localPlayer);
		Vector3 position = entity.GetPosition();
		Vector2 vector = new Vector2(position.x, position.z);
		Vector3 forward = cameraTransform.forward;
		Vector2 rhs = new Vector2(forward.x, forward.z);
		rhs.Normalize();
		Vector3 right = cameraTransform.right;
		Vector2 rhs2 = new Vector2(right.x, right.z);
		rhs2.Normalize();
		for (int i = 0; i < count; i++)
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
			Vector3 position2 = mapObject.GetPosition();
			Vector2 vector2 = new Vector2(position2.x, position2.z) - vector;
			float magnitude = vector2.magnitude;
			bool flag = true;
			if (mapObject.type == EnumMapObjectType.TreasureChest)
			{
				float num3 = (mapObject as MapObjectTreasureChest).DefaultRadius;
				float value = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, num3, localPlayer);
				value = Utils.FastClamp(value, 0f, num3);
				if (magnitude < value)
				{
					float num4 = Mathf.PingPong(Time.time, 0.25f);
					float num5 = 1.25f + num4;
					waypointSpriteList[waypointSpriteIndex].atlas = base.xui.GetAtlasByName("UIAtlas", mapObject.GetMapIcon());
					waypointSpriteList[waypointSpriteIndex].spriteName = mapObject.GetMapIcon();
					waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num5), (int)(25f * num5));
					waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num, -24f);
					Color mapIconColor = mapObject.GetMapIconColor();
					waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(mapIconColor, Color.red, num4 * 4f);
					waypointSpriteIndex++;
					flag = false;
				}
			}
			string spriteName = mapObject.GetCompassIcon();
			if (mapObject.type == EnumMapObjectType.HiddenCache)
			{
				waypointSpriteList[waypointSpriteIndex].flip = UIBasicSprite.Flip.Nothing;
				if (position2.y < localPlayer.GetPosition().y - 2f)
				{
					spriteName = mapObject.GetCompassDownIcon();
				}
				else if (position2.y > localPlayer.GetPosition().y + 2f)
				{
					spriteName = mapObject.GetCompassUpIcon();
				}
				waypointSpriteList[waypointSpriteIndex].depth = 100;
				waypointSpriteList[waypointSpriteIndex].atlas = base.xui.GetAtlasByName("UIAtlas", spriteName);
				waypointSpriteList[waypointSpriteIndex].spriteName = spriteName;
				if ((position2 - entity.GetPosition()).magnitude < 10f)
				{
					float num6 = Mathf.PingPong(Time.time, 0.25f);
					float num7 = 1.25f + num6;
					waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num7), (int)(25f * num7));
					waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num, -24f);
					Color mapIconColor2 = mapObject.GetMapIconColor();
					waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(mapIconColor2, Color.grey, num6 * 4f);
					waypointSpriteIndex++;
					flag = false;
				}
			}
			if (mapObject.UseUpDownCompassIcons())
			{
				waypointSpriteList[waypointSpriteIndex].flip = UIBasicSprite.Flip.Nothing;
				if (position2.y < localPlayer.GetPosition().y - 2f)
				{
					spriteName = mapObject.GetCompassDownIcon();
				}
				else if (position2.y > localPlayer.GetPosition().y + 3f)
				{
					spriteName = mapObject.GetCompassUpIcon();
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
			float num8 = mapObject.GetCompassIconScale(magnitude);
			waypointSpriteList[waypointSpriteIndex].color = mapObject.GetMapIconColor();
			if (mapObject.IsTracked() && mapObject.NearbyCompassBlink() && (position2 - entity.GetPosition()).magnitude <= 6f)
			{
				Color mapIconColor3 = mapObject.GetMapIconColor();
				float num9 = Mathf.PingPong(Time.time, 0.5f);
				waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(Color.grey, mapIconColor3, num9 * 4f);
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
				waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num + Vector2.Dot(normalized, rhs2) * num2, -16f);
			}
			else
			{
				waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num + ((Vector2.Dot(normalized, rhs2) < 0f) ? (-0.675f) : 0.675f) * num2, -16f);
			}
			if (mapObject.type == EnumMapObjectType.Entity)
			{
				waypointSpriteList[waypointSpriteIndex].depth = 12 + (int)(num8 * 100f);
			}
			if (!mapObject.IsTracked())
			{
				Color mapIconColor4 = mapObject.GetMapIconColor();
				waypointSpriteList[waypointSpriteIndex].color = new Color(mapIconColor4.r * 0.75f, mapIconColor4.g * 0.75f, mapIconColor4.b * 0.75f) * num8;
			}
			waypointSpriteIndex++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateNavObjects(EntityPlayerLocal localPlayer, ref int waypointSpriteIndex)
	{
		float num = (float)base.ViewComponent.Size.x * 0.5f;
		float num2 = num * 1.15f;
		Transform cameraTransform = localPlayer.cameraTransform;
		Entity entity = ((localPlayer.AttachedToEntity != null) ? localPlayer.AttachedToEntity : localPlayer);
		Vector3 position = entity.GetPosition();
		Vector2 vector = new Vector2(position.x, position.z);
		Vector3 forward = cameraTransform.forward;
		Vector2 rhs = new Vector2(forward.x, forward.z);
		rhs.Normalize();
		Vector3 right = cameraTransform.right;
		Vector2 rhs2 = new Vector2(right.x, right.z);
		rhs2.Normalize();
		List<NavObject> navObjectList = NavObjectManager.Instance.NavObjectList;
		for (int i = 0; i < navObjectList.Count; i++)
		{
			NavObject navObject = navObjectList[i];
			if (navObject.hiddenOnCompass || !navObject.IsValid())
			{
				continue;
			}
			if (waypointSpriteIndex >= waypointSpriteList.Count)
			{
				break;
			}
			NavObjectCompassSettings currentCompassSettings = navObject.CurrentCompassSettings;
			if (currentCompassSettings == null)
			{
				continue;
			}
			Vector3 position2 = navObject.GetPosition();
			Vector2 vector2 = new Vector2(position2.x + Origin.position.x, position2.z + Origin.position.z) - vector;
			float magnitude = vector2.magnitude;
			if (magnitude < currentCompassSettings.MinDistance)
			{
				continue;
			}
			float maxDistance = navObject.GetMaxDistance(currentCompassSettings, localPlayer);
			if (maxDistance != -1f && magnitude > maxDistance)
			{
				continue;
			}
			bool flag = true;
			string spriteName = navObject.GetSpriteName(currentCompassSettings);
			waypointSpriteList[waypointSpriteIndex].depth = 12 + currentCompassSettings.DepthOffset;
			if (currentCompassSettings.HotZone != null)
			{
				float num3 = 1f;
				if (currentCompassSettings.HotZone.HotZoneType == NavObjectCompassSettings.HotZoneSettings.HotZoneTypes.Treasure)
				{
					float extraData = navObject.ExtraData;
					num3 = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, extraData, localPlayer);
					num3 = Utils.FastClamp(num3, 0f, extraData);
				}
				else if (currentCompassSettings.HotZone.HotZoneType == NavObjectCompassSettings.HotZoneSettings.HotZoneTypes.Custom)
				{
					num3 = currentCompassSettings.HotZone.CustomDistance;
				}
				if (magnitude < num3)
				{
					float num4 = Mathf.PingPong(Time.time, 0.25f);
					float num5 = 1.25f + num4;
					waypointSpriteList[waypointSpriteIndex].atlas = base.xui.GetAtlasByName("UIAtlas", currentCompassSettings.HotZone.SpriteName);
					waypointSpriteList[waypointSpriteIndex].spriteName = currentCompassSettings.HotZone.SpriteName;
					waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num5), (int)(25f * num5));
					waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num, -24f);
					Color color = currentCompassSettings.HotZone.Color;
					waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(color, Color.red, num4 * 4f);
					waypointSpriteIndex++;
					flag = false;
				}
			}
			if (currentCompassSettings.ShowVerticalCompassIcons)
			{
				waypointSpriteList[waypointSpriteIndex].flip = UIBasicSprite.Flip.Nothing;
				float num6 = localPlayer.GetPosition().y - Origin.position.y;
				if (position2.y < num6 + currentCompassSettings.ShowDownOffset)
				{
					spriteName = currentCompassSettings.DownSpriteName;
				}
				else if (position2.y > num6 + currentCompassSettings.ShowUpOffset)
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
			float num7 = navObject.GetCompassIconScale(magnitude);
			waypointSpriteList[waypointSpriteIndex].color = (navObject.UseOverrideColor ? navObject.OverrideColor : currentCompassSettings.Color);
			if (currentCompassSettings.HasPulse && (position2 - entity.GetPosition()).magnitude <= 6f)
			{
				Color b = (navObject.UseOverrideColor ? navObject.OverrideColor : currentCompassSettings.Color);
				float num8 = Mathf.PingPong(Time.time, 0.5f);
				waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(Color.grey, b, num8 * 4f);
				if (num8 > 0.25f)
				{
					num7 += num8 - 0.25f;
				}
			}
			waypointSpriteList[waypointSpriteIndex].atlas = base.xui.GetAtlasByName("UIAtlas", spriteName);
			waypointSpriteList[waypointSpriteIndex].spriteName = spriteName;
			waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num7), (int)(25f * num7));
			if (Vector2.Dot(normalized, rhs) >= 0.75f)
			{
				waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num + Vector2.Dot(normalized, rhs2) * num2, -16f);
			}
			else
			{
				waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num + ((Vector2.Dot(normalized, rhs2) < 0f) ? (-0.675f) : 0.675f) * num2, -16f);
			}
			if (!navObject.IsActive)
			{
				Color color2 = (navObject.UseOverrideColor ? navObject.OverrideColor : currentCompassSettings.Color);
				if (currentCompassSettings.MinFadePercent != -1f)
				{
					if (currentCompassSettings.MinFadePercent > num7)
					{
						num7 = currentCompassSettings.MinFadePercent;
					}
					waypointSpriteList[waypointSpriteIndex].color = color2 * num7;
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
