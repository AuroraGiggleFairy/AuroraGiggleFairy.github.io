using System;
using System.Collections;
using System.Collections.Generic;
using SteelSeries.GameSense;
using SteelSeries.GameSense.DeviceZone;
using UnityEngine;

public class GameSenseManager : IEntityBuffsChanged
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum EDirection45
	{
		N,
		NE,
		E,
		SE,
		S,
		SW,
		W,
		NW,
		None
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<string> registeredEvents = new HashSet<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const string GameSenseGameName = "7dtd";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string GameSenseGameNameFull = "7 Days to Die";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string GameSenseGameDeveloper = "The Fun Pimps";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EventHealth = "HEALTH";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EventAmmo = "AMMO";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EventTime = "TIME";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EventDurability = "DURABILITY";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EventStealth = "STEALTH";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EventBleeding = "BLEEDING";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string BleedingBuffName = "buffInjuryBleeding";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EventBloodmoon = "BLOODMOON";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int BloodmoonBlinkWarningHours = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EventHit = "HIT";

	[PublicizedFrom(EAccessModifier.Private)]
	public const float HitEventDuration = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EventDukes = "DUKES";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string DukesStringValueKey = "dukesstring";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string DukesLabelLocKey = "gamesenseDukesLabel";

	[PublicizedFrom(EAccessModifier.Private)]
	public WeakReference<EntityPlayerLocal> entityPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastTimePercent = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int previousSlotIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int previousAmmoPercentage = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int previousDurability = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int previousStealth = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int bloodMoonDay;

	[PublicizedFrom(EAccessModifier.Private)]
	public int bloodMoonWarning;

	[PublicizedFrom(EAccessModifier.Private)]
	public (int duskHour, int dawnHour) duskDawnTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastBloodMoonValue = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public EDirection45 lastDirection = EDirection45.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public float stopHitEventTime = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine stopHitEventCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dukesNextTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue dukesItem;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static GameSenseManager Instance
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static bool GameSenseInstalled => SdFile.Exists(GSClient._getPropsPath());

	[PublicizedFrom(EAccessModifier.Private)]
	static GameSenseManager()
	{
		if (!GameManager.IsDedicatedServer && !Application.isEditor && GameUtils.GetLaunchArgument("nogamesense") == null)
		{
			Instance = new GameSenseManager();
		}
	}

	public void Init()
	{
		if (!GameSenseInstalled)
		{
			Log.Out("GameSense server not found (no props file), disabling");
			Instance = null;
			return;
		}
		GameObject gameObject = new GameObject("GameSense");
		gameObject.hideFlags = HideFlags.HideAndDontSave;
		gameObject.AddComponent<GSClient>();
		GSClient.Instance.RegisterGame("7dtd", "7 Days to Die", "The Fun Pimps");
		GameStats.OnChangedDelegates += GameStats_OnChanged;
		ThreadManager.StartCoroutine(BindEventsCo());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator BindEventsCo()
	{
		yield return new WaitForSeconds(0.1f);
		BindEventTime();
		BindEventAmmo();
		BindEventHealth();
		BindEventDurability();
		BindEventStealth();
		BindEventBleeding();
		BindEventBloodmoon();
		BindEventCompass();
		BindEventHit();
		BindEventDukes();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventTime()
	{
		ColorRanges color = ColorRanges.Create(new ColorRange[8]
		{
			new ColorRange(0u, 16u, ColorStatic.Create(byte.MaxValue, 0, 0)),
			new ColorRange(17u, 32u, ColorStatic.Create(byte.MaxValue, 160, 0)),
			new ColorRange(33u, 49u, ColorStatic.Create(byte.MaxValue, byte.MaxValue, 0)),
			new ColorRange(50u, 66u, ColorStatic.Create(byte.MaxValue, byte.MaxValue, 0)),
			new ColorRange(67u, 74u, ColorStatic.Create(byte.MaxValue, 160, 0)),
			new ColorRange(75u, 82u, ColorStatic.Create(byte.MaxValue, 80, 0)),
			new ColorRange(83u, 91u, ColorStatic.Create(byte.MaxValue, 40, 0)),
			new ColorRange(92u, 100u, ColorStatic.Create(byte.MaxValue, 0, 0))
		});
		AbstractHandler abstractHandler = ColorHandler.Create(ScriptableObject.CreateInstance<RGBPerkeyZoneFunctionKeys>(), IlluminationMode.Percent, color);
		BindEventWrapper("TIME", 0, 100, EventIconId.Clock, false, abstractHandler);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventAmmo()
	{
		AbstractHandler abstractHandler = ColorHandler.Create(ScriptableObject.CreateInstance<RGBPerkeyZoneNumberKeys>(), IlluminationMode.Percent, ColorGradient.Create(new RGB(160, 0, byte.MaxValue), new RGB(0, 0, byte.MaxValue)));
		BindEventWrapper("AMMO", 0, 100, EventIconId.Ammo, false, abstractHandler);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventHealth()
	{
		AbstractHandler abstractHandler = ColorHandler.Create(ScriptableObject.CreateInstance<RGBPerkeyZoneRowQ>(), IlluminationMode.Percent, ColorGradient.Create(new RGB(byte.MaxValue, 0, 0), new RGB(0, byte.MaxValue, 0)), RateRange.Create(new FreqRepeatLimitPair[2]
		{
			new FreqRepeatLimitPair(0u, 10u, 2u, 0u),
			new FreqRepeatLimitPair(11u, 20u, 1u, 0u)
		}));
		BindEventWrapper("HEALTH", 0, 100, EventIconId.Health, false, abstractHandler);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventDurability()
	{
		RGBPerkeyZoneCustom rGBPerkeyZoneCustom = ScriptableObject.CreateInstance<RGBPerkeyZoneCustom>();
		rGBPerkeyZoneCustom.zone = new byte[4] { 83, 84, 85, 86 };
		AbstractHandler abstractHandler = ColorHandler.Create(rGBPerkeyZoneCustom, IlluminationMode.Percent, ColorGradient.Create(new RGB(130, 130, 0), new RGB(0, byte.MaxValue, 200)));
		BindEventWrapper("DURABILITY", 0, 100, EventIconId.Item, false, abstractHandler);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventStealth()
	{
		RGBPerkeyZoneCustom rGBPerkeyZoneCustom = ScriptableObject.CreateInstance<RGBPerkeyZoneCustom>();
		rGBPerkeyZoneCustom.zone = new byte[5] { 224, 225, 57, 43, 53 };
		AbstractHandler abstractHandler = ColorHandler.Create(rGBPerkeyZoneCustom, IlluminationMode.Percent, ColorGradient.Create(new RGB(50, 120, 50), new RGB(0, 250, 250)));
		BindEventWrapper("STEALTH", 0, 100, EventIconId.Default, false, abstractHandler);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventBleeding()
	{
		AbstractHandler abstractHandler = ColorHandler.Create(ScriptableObject.CreateInstance<RGBPerkeyZoneNavCluster>(), IlluminationMode.Color, ColorStatic.Create(byte.MaxValue, 0, 0), RateStatic.Create(5u));
		BindEventWrapper("BLEEDING", 0, 1, EventIconId.Default, false, abstractHandler);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventBloodmoon()
	{
		ColorRanges color = ColorRanges.Create(new ColorRange[11]
		{
			new ColorRange(0u, 0u, ColorStatic.Create(0, 0, 0)),
			new ColorRange(1u, 10u, ColorStatic.Create(60, 0, 0)),
			new ColorRange(11u, 20u, ColorStatic.Create(70, 0, 0)),
			new ColorRange(21u, 30u, ColorStatic.Create(80, 0, 0)),
			new ColorRange(31u, 40u, ColorStatic.Create(90, 0, 0)),
			new ColorRange(41u, 50u, ColorStatic.Create(105, 0, 0)),
			new ColorRange(51u, 60u, ColorStatic.Create(120, 0, 0)),
			new ColorRange(61u, 70u, ColorStatic.Create(135, 0, 0)),
			new ColorRange(71u, 80u, ColorStatic.Create(150, 0, 0)),
			new ColorRange(81u, 90u, ColorStatic.Create(190, 0, 0)),
			new ColorRange(91u, 100u, ColorStatic.Create(byte.MaxValue, 0, 0))
		});
		AbstractHandler abstractHandler = ColorHandler.Create(ScriptableObject.CreateInstance<RGBPerkeyZoneRowA>(), IlluminationMode.Color, color, RateRange.Create(new FreqRepeatLimitPair[1]
		{
			new FreqRepeatLimitPair(81u, 90u, 1u, 0u)
		}));
		BindEventWrapper("BLOODMOON", 0, 100, EventIconId.Default, false, abstractHandler);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventCompass()
	{
		Tuple<string, byte>[] array = new Tuple<string, byte>[8]
		{
			new Tuple<string, byte>("NORTH", 96),
			new Tuple<string, byte>("NORTHEAST", 97),
			new Tuple<string, byte>("EAST", 94),
			new Tuple<string, byte>("SOUTHEAST", 91),
			new Tuple<string, byte>("SOUTH", 90),
			new Tuple<string, byte>("SOUTHWEST", 89),
			new Tuple<string, byte>("WEST", 92),
			new Tuple<string, byte>("NORTHWEST", 95)
		};
		foreach (Tuple<string, byte> tuple in array)
		{
			RGBPerkeyZoneCustom rGBPerkeyZoneCustom = ScriptableObject.CreateInstance<RGBPerkeyZoneCustom>();
			rGBPerkeyZoneCustom.zone = new byte[1] { tuple.Item2 };
			AbstractHandler abstractHandler = ColorHandler.Create(rGBPerkeyZoneCustom, IlluminationMode.Color, ColorStatic.Create(50, byte.MaxValue, 0));
			BindEventWrapper(tuple.Item1, 0, 1, EventIconId.Compass, false, abstractHandler);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventHit()
	{
		AbstractHandler abstractHandler = ColorHandler.Create(ScriptableObject.CreateInstance<MouseZoneAll>(), IlluminationMode.Color, ColorStatic.Create(byte.MaxValue, 0, 0), RateStatic.Create(8u));
		BindEventWrapper("HIT", 0, 1, EventIconId.Headshot, false, abstractHandler);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventDukes()
	{
		LineData lineData = LineData.Create(LineDataText.Create("Dukes: ", "", bold: true, 0u), LineDataAccessor.ContextFrameKey("dukesstring"));
		FrameDataMultiLine frameDataMultiLine = FrameDataMultiLine.Create(new LineData[1] { lineData }, new FrameModifiers(10000u, repeats: false), EventIconId.Money);
		ScreenedZoneOne dz = ScriptableObject.CreateInstance<ScreenedZoneOne>();
		AbstractFrameData[] datas = new FrameDataMultiLine[1] { frameDataMultiLine };
		ScreenHandler screenHandler = ScreenHandler.Create(dz, ScreenMode.screen, datas);
		BindEventWrapper("DUKES", 0, 1, EventIconId.Money, true, screenHandler);
	}

	public void Cleanup()
	{
		GameStats.OnChangedDelegates -= GameStats_OnChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventWrapper(string _eventName, int _minValue, int _maxValue, EventIconId _icon, bool _optionalValue, params AbstractHandler[] _handlers)
	{
		GSClient.Instance.BindEvent(_eventName, _minValue, _maxValue, _icon, _handlers, _optionalValue);
		registeredEvents.Add(_eventName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetEventValues()
	{
		foreach (string registeredEvent in registeredEvents)
		{
			GSClient.Instance.SendEvent(registeredEvent, 0);
		}
	}

	public void SessionStarted(EntityPlayerLocal _playerEntity)
	{
		ResetEventValues();
		entityPlayer = new WeakReference<EntityPlayerLocal>(_playerEntity);
		_playerEntity.PlayerStats.AddBuffChangedDelegate(this);
		lastTimePercent = -1;
		previousAmmoPercentage = -1;
		previousSlotIndex = -1;
		previousDurability = -1;
		previousStealth = -1;
		bloodMoonWarning = GameStats.GetInt(EnumGameStats.BloodMoonWarning);
		duskDawnTime = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
		lastBloodMoonValue = -1;
		lastDirection = EDirection45.None;
		dukesItem = ItemClass.GetItem(TraderInfo.CurrencyItem);
	}

	public void SessionEnded()
	{
		ResetEventValues();
		if (entityPlayer.TryGetTarget(out var target))
		{
			target.PlayerStats.RemoveBuffChangedDelegate(this);
		}
		entityPlayer = null;
		dukesItem = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GameStats_OnChanged(EnumGameStats _gameStat, object _newValue)
	{
		if (_gameStat == EnumGameStats.BloodMoonDay)
		{
			bloodMoonDay = GameStats.GetInt(EnumGameStats.BloodMoonDay);
		}
	}

	public void Update()
	{
		if (GSClient.Instance.isClientActive())
		{
			UpdateEventAmmo();
			UpdateEventDurability();
			UpdateEventStealth();
			UpdateEventDukes();
		}
	}

	public void UpdateEventTime(ulong _worldTime)
	{
		if (GSClient.Instance.isClientActive())
		{
			UpdateEventBloodmoon(_worldTime);
			int num = (int)(_worldTime % 24000) / 240;
			if (lastTimePercent != num)
			{
				lastTimePercent = num;
				GSClient.Instance.SendEvent("TIME", num);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEventAmmo()
	{
		if (entityPlayer == null || !entityPlayer.TryGetTarget(out var target) || target == null)
		{
			return;
		}
		int focusedItemIdx = target.inventory.GetFocusedItemIdx();
		ItemValue itemValue = target.inventory.GetItem(focusedItemIdx).itemValue;
		ItemClass itemClass;
		if (itemValue == null || itemValue.type == 0 || (itemClass = itemValue.ItemClass) == null)
		{
			previousAmmoPercentage = 0;
			GSClient.Instance.SendEvent("AMMO", 0);
			return;
		}
		int num;
		if (!(itemClass.Actions[0] is ItemActionAttack itemActionAttack) || itemActionAttack is ItemActionMelee || (num = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, itemValue, 0f, target)) <= 0)
		{
			previousAmmoPercentage = 0;
			GSClient.Instance.SendEvent("AMMO", 0);
			return;
		}
		int meta = itemValue.Meta;
		int num2 = 100 * meta / num;
		if (num2 != previousAmmoPercentage)
		{
			previousAmmoPercentage = num2;
			GSClient.Instance.SendEvent("AMMO", num2);
		}
	}

	public void UpdateEventHealth(int _value)
	{
		if (GSClient.Instance.isClientActive())
		{
			GSClient.Instance.SendEvent("HEALTH", _value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEventDurability()
	{
		if (entityPlayer == null || !entityPlayer.TryGetTarget(out var target) || target == null)
		{
			return;
		}
		int focusedItemIdx = target.inventory.GetFocusedItemIdx();
		ItemValue itemValue = target.inventory.GetItem(focusedItemIdx).itemValue;
		ItemClass itemClass;
		if (itemValue == null || itemValue.type == 0 || (itemClass = itemValue.ItemClass) == null || !itemClass.ShowQualityBar)
		{
			previousDurability = 0;
			GSClient.Instance.SendEvent("DURABILITY", 0);
			return;
		}
		int num = (int)(100f * itemValue.PercentUsesLeft);
		if (num != previousDurability)
		{
			previousDurability = num;
			GSClient.Instance.SendEvent("DURABILITY", num);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEventStealth()
	{
		if (entityPlayer != null && entityPlayer.TryGetTarget(out var target) && !(target == null))
		{
			int num = (int)(100f * target.Stealth.ValuePercentUI);
			if (num != previousStealth)
			{
				previousStealth = num;
				GSClient.Instance.SendEvent("STEALTH", num);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEventBleeding(bool _isBleeding)
	{
		GSClient.Instance.SendEvent("BLEEDING", _isBleeding ? 1 : 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEventBloodmoon(ulong _worldTime)
	{
		int num = 0;
		var (num2, num3, num4) = GameUtils.WorldTimeToElements(_worldTime);
		if (GameUtils.IsBloodMoonTime(_worldTime, duskDawnTime, bloodMoonDay))
		{
			num = 100;
		}
		else if (bloodMoonDay == num2 && bloodMoonWarning >= 0 && bloodMoonWarning <= num3)
		{
			int num5 = duskDawnTime.duskHour - 2;
			if (num3 > num5)
			{
				num = 85;
			}
			else
			{
				float num6 = (float)num3 + (float)num4 / 60f - (float)bloodMoonWarning;
				float num7 = num5 - bloodMoonWarning;
				num = (int)(80f * num6 / num7);
			}
		}
		if (num != lastBloodMoonValue)
		{
			lastBloodMoonValue = num;
			GSClient.Instance.SendEvent("BLOODMOON", num);
		}
	}

	public void UpdateEventCompass(float _rotation)
	{
		if (GSClient.Instance.isClientActive())
		{
			EDirection45 eDirection = ((!((double)_rotation > 337.5) && !((double)_rotation <= 22.5)) ? (((double)_rotation <= 67.5) ? EDirection45.NE : (((double)_rotation <= 112.5) ? EDirection45.E : (((double)_rotation <= 157.5) ? EDirection45.SE : (((double)_rotation <= 202.5) ? EDirection45.S : (((double)_rotation <= 247.5) ? EDirection45.SW : ((!((double)_rotation <= 292.5)) ? EDirection45.NW : EDirection45.W)))))) : EDirection45.N);
			if (lastDirection != eDirection)
			{
				lastDirection = eDirection;
				GSClient.Instance.SendEvent("NORTH", (eDirection == EDirection45.N) ? 1 : 0);
				GSClient.Instance.SendEvent("NORTHEAST", (eDirection == EDirection45.NE) ? 1 : 0);
				GSClient.Instance.SendEvent("EAST", (eDirection == EDirection45.E) ? 1 : 0);
				GSClient.Instance.SendEvent("SOUTHEAST", (eDirection == EDirection45.SE) ? 1 : 0);
				GSClient.Instance.SendEvent("SOUTH", (eDirection == EDirection45.S) ? 1 : 0);
				GSClient.Instance.SendEvent("SOUTHWEST", (eDirection == EDirection45.SW) ? 1 : 0);
				GSClient.Instance.SendEvent("WEST", (eDirection == EDirection45.W) ? 1 : 0);
				GSClient.Instance.SendEvent("NORTHWEST", (eDirection == EDirection45.NW) ? 1 : 0);
			}
		}
	}

	public void UpdateEventHit()
	{
		if (GSClient.Instance.isClientActive())
		{
			stopHitEventTime = Time.unscaledTime + 0.5f;
			if (stopHitEventCoroutine == null)
			{
				GSClient.Instance.SendEvent("HIT", 1);
				stopHitEventCoroutine = ThreadManager.StartCoroutine(StopHitCo());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator StopHitCo()
	{
		while (Time.unscaledTime < stopHitEventTime)
		{
			yield return null;
		}
		GSClient.Instance.SendEvent("HIT", 0);
		stopHitEventCoroutine = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEventDukes()
	{
		float unscaledTime = Time.unscaledTime;
		if (!(unscaledTime < dukesNextTime))
		{
			dukesNextTime = unscaledTime + 5f;
			if (entityPlayer != null && entityPlayer.TryGetTarget(out var target) && !(target == null))
			{
				int itemCount = target.inventory.GetItemCount(dukesItem);
				itemCount += target.bag.GetItemCount(dukesItem);
				ContextFrameObject frame = new ContextFrameObject { ["dukesstring"] = ValueDisplayFormatters.FormatNumberWithMetricPrefix(itemCount, _allowDecimals: true, 2) };
				GSClient.Instance.SendEvent("DUKES", itemCount, frame);
			}
		}
	}

	public void EntityBuffAdded(BuffValue _buff)
	{
		if (_buff.BuffClass.Name.EqualsCaseInsensitive("buffInjuryBleeding"))
		{
			UpdateEventBleeding(_isBleeding: true);
		}
	}

	public void EntityBuffRemoved(BuffValue _buff)
	{
		if (_buff.BuffClass.Name.EqualsCaseInsensitive("buffInjuryBleeding"))
		{
			UpdateEventBleeding(_isBleeding: false);
		}
	}
}
