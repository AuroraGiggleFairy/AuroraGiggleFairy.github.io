using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TargetBar : XUiController
{
	public enum EVisibility
	{
		Never,
		GodMode,
		Always,
		Boss
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float deltaTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float noTargetFadeTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float noTargetFadeTimeMax = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public EVisibility visibility;

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultBossIcon = "ui_game_symbol_twitch_boss_bar_default";

	[PublicizedFrom(EAccessModifier.Private)]
	public GameEventManager gameEventManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public BossGroup CurrentBossGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt statcurrentFormatterInt = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> statcurrentWMaxFormatterAOfB = new CachedStringFormatter<int, int>([PublicizedFrom(EAccessModifier.Internal)] (int _i, int _i1) => $"{_i}/{_i1}");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat statfillFormatter = new CachedStringFormatterFloat();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive Target { get; set; }

	public override void Init()
	{
		base.Init();
		IsDirty = true;
		viewComponent.IsVisible = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		deltaTime = _dt;
		if (gameEventManager == null)
		{
			gameEventManager = GameEventManager.Current;
		}
		if (!base.xui.playerUI.entityPlayer.IsAlive())
		{
			viewComponent.IsVisible = false;
			return;
		}
		if (gameEventManager.CurrentBossGroup != null)
		{
			CurrentBossGroup = gameEventManager.CurrentBossGroup;
			Target = CurrentBossGroup.BossEntity;
			bool flag = false;
			if (Target != null && Target.IsAlive())
			{
				flag = true;
			}
			if (CurrentBossGroup.MinionCount != 0)
			{
				flag = true;
			}
			if (flag)
			{
				viewComponent.IsVisible = true;
				noTargetFadeTime = 0f;
			}
			else if (noTargetFadeTime >= noTargetFadeTimeMax)
			{
				Target = null;
				viewComponent.IsVisible = false;
				CurrentBossGroup.BossEntity = null;
			}
			else
			{
				noTargetFadeTime += Time.deltaTime;
			}
		}
		else
		{
			if (CurrentBossGroup != null)
			{
				Target = null;
				CurrentBossGroup = null;
			}
			if (visibility == EVisibility.Never)
			{
				viewComponent.IsVisible = false;
				return;
			}
			if (visibility == EVisibility.GodMode && !base.xui.playerUI.entityPlayer.IsGodMode.Value)
			{
				viewComponent.IsVisible = false;
				return;
			}
			bool flag2 = false;
			WorldRayHitInfo hitInfo = base.xui.playerUI.entityPlayer.HitInfo;
			if (hitInfo.bHitValid && (bool)hitInfo.transform && hitInfo.tag.StartsWith("E_", StringComparison.Ordinal))
			{
				Transform hitRootTransform = GameUtils.GetHitRootTransform(hitInfo.tag, hitInfo.transform);
				EntityAlive entityAlive = null;
				if (hitRootTransform != null)
				{
					entityAlive = hitRootTransform.GetComponent<EntityAlive>();
				}
				if (entityAlive != null && entityAlive.IsAlive())
				{
					flag2 = true;
					Target = entityAlive;
				}
			}
			if (Target == null)
			{
				viewComponent.IsVisible = false;
				noTargetFadeTime = noTargetFadeTimeMax;
				return;
			}
			if (flag2)
			{
				viewComponent.IsVisible = true;
				noTargetFadeTime = 0f;
			}
			else if (noTargetFadeTime >= noTargetFadeTimeMax)
			{
				Target = null;
				viewComponent.IsVisible = false;
			}
			else
			{
				noTargetFadeTime += Time.deltaTime;
			}
			if (Target != null && (Target.IsDead() || Target.Health == 0))
			{
				Target = null;
				viewComponent.IsVisible = false;
				noTargetFadeTime = noTargetFadeTimeMax;
			}
		}
		RefreshBindings(IsDirty);
		IsDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "current":
			if (Target == null)
			{
				value = "";
				return true;
			}
			value = statcurrentFormatterInt.Format(Target.Health);
			return true;
		case "currentwithmax":
			if (Target == null)
			{
				value = "";
				return true;
			}
			value = statcurrentWMaxFormatterAOfB.Format(Target.IsAlive() ? ((int)Target.Stats.Health.Value) : 0, (int)Target.Stats.Health.Max);
			return true;
		case "name":
			if (CurrentBossGroup != null)
			{
				value = CurrentBossGroup.BossName;
			}
			else if (Target == null)
			{
				value = "";
			}
			else
			{
				EntityPlayer entityPlayer = Target as EntityPlayer;
				value = ((entityPlayer != null) ? entityPlayer.PlayerDisplayName : Localization.Get(EntityClass.list[Target.entityClass].entityClassName));
			}
			return true;
		case "fill":
		{
			if (Target == null)
			{
				value = "0";
				return true;
			}
			float b = (float)Target.Health / (float)Target.GetMaxHealth();
			float v = Math.Max(lastValue, 0f) * 1.01f;
			value = statfillFormatter.Format(v);
			lastValue = Mathf.Lerp(lastValue, b, deltaTime * 3f);
			return true;
		}
		case "isboss":
			if (gameEventManager == null)
			{
				value = "false";
				return true;
			}
			value = (CurrentBossGroup != null).ToString();
			return true;
		case "isnotboss":
			if (gameEventManager == null)
			{
				value = "false";
				return true;
			}
			value = (CurrentBossGroup == null).ToString();
			return true;
		case "minioncount":
			if (gameEventManager == null || CurrentBossGroup == null)
			{
				value = "";
				return true;
			}
			value = CurrentBossGroup.MinionCount.ToString();
			return true;
		case "boss_sprite":
			if (gameEventManager == null || CurrentBossGroup == null)
			{
				value = "";
				return true;
			}
			value = ((CurrentBossGroup.BossIcon == "") ? defaultBossIcon : CurrentBossGroup.BossIcon);
			return true;
		default:
			return false;
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (!(_name == "visibility"))
		{
			if (_name == "default_boss_icon")
			{
				defaultBossIcon = _value;
				return true;
			}
			return base.ParseAttribute(_name, _value, _parent);
		}
		visibility = EnumUtils.Parse<EVisibility>(_value, _ignoreCase: true);
		return true;
	}
}
