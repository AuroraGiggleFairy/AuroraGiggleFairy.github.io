using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapStats : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ulong> mapdaytimeFormatter = new CachedStringFormatter<ulong>([PublicizedFrom(EAccessModifier.Internal)] (ulong _worldTime) => ValueDisplayFormatters.WorldTime(_worldTime, "{0}/{1:00}:{2:00}"));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<float> tempFormatter = new CachedStringFormatter<float>([PublicizedFrom(EAccessModifier.Internal)] (float _f) => ValueDisplayFormatters.Temperature(_f));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt mapwindFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> levelFormatter = new CachedStringFormatter<int>([PublicizedFrom(EAccessModifier.Internal)] (int _i) => _i.ToString("+0;-#"));

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "mapdaytime":
			value = "";
			if (XUi.IsGameRunning() && base.xui.playerUI.entityPlayer != null)
			{
				value = mapdaytimeFormatter.Format(GameManager.Instance.World.worldTime);
			}
			return true;
		case "mapdaytimetitle":
			value = Localization.Get("xuiDayTime");
			return true;
		case "maptemperature":
			value = "";
			if (XUi.IsGameRunning() && base.xui.playerUI.entityPlayer != null)
			{
				value = tempFormatter.Format(WeatherManager.Instance.GetCurrentTemperatureValue());
			}
			return true;
		case "mapwind":
			value = "";
			if (XUi.IsGameRunning() && base.xui.playerUI.entityPlayer != null)
			{
				value = mapwindFormatter.Format(Mathf.RoundToInt(WeatherManager.GetWindSpeed()));
			}
			return true;
		case "mapwindtitle":
			value = Localization.Get("xuiWind");
			return true;
		case "mapelevation":
			value = "";
			if (XUi.IsGameRunning() && base.xui.playerUI.entityPlayer != null)
			{
				int v = Mathf.RoundToInt(base.xui.playerUI.entityPlayer.GetPosition().y - WeatherManager.SeaLevel());
				value = levelFormatter.Format(v);
			}
			return true;
		case "mapelevationtitle":
			value = Localization.Get("xuiElevation");
			return true;
		case "playercoretemp":
			value = "";
			if (XUi.IsGameRunning() && base.xui.playerUI.entityPlayer != null)
			{
				value = XUiM_Player.GetCoreTemp(base.xui.playerUI.entityPlayer);
			}
			return true;
		case "playercoretemptitle":
			value = Localization.Get("xuiFeelsLike");
			return true;
		case "showtime":
			value = "true";
			if (XUi.IsGameRunning() && base.xui.playerUI.entityPlayer != null)
			{
				value = (EffectManager.GetValue(PassiveEffects.NoTimeDisplay, null, 0f, base.xui.playerUI.entityPlayer) == 0f).ToString();
			}
			return true;
		default:
			return false;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		RefreshBindings();
	}
}
