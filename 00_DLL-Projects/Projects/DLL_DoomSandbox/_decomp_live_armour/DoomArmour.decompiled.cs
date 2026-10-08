using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Xml.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Scripting;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: TargetFramework(".NETFramework,Version=v4.8", FrameworkDisplayName = ".NET Framework 4.8")]
[assembly: AssemblyCompany("DoomArmour")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0")]
[assembly: AssemblyProduct("DoomArmour")]
[assembly: AssemblyTitle("DoomArmour")]
[assembly: AssemblyVersion("1.0.0.0")]
namespace DoomArmour;

public static class Armour
{
	public const string GreenVar = "armourType";

	public const string BlueVar = "armourTypeBlue";

	public const string PoolVar = "doomArmour";

	public const string PctVar = ".doomArmourPct";

	public const float GreenShare = 0.4f;

	public const float BlueShare = 0.6f;

	public const string ReinforcedPerk = "perkReinforcedArmour";

	public const string PainPerk = "perkPainTolerance";

	public const string AdrenalinePerk = "perkAdrenalineRush";

	public const string GraceBuff = "buffNutrientGracePeriod";

	public const int AdrenalineLevel = 3;

	public const int AdrenalineHealth = 50;

	public const int AdrenalineGain = 1;

	public static float Share(EntityAlive entity)
	{
		EntityBuffs val = (((Object)(object)entity == (Object)null) ? null : entity.Buffs);
		if (val == null)
		{
			return 0f;
		}
		if (val.GetCustomVar("armourTypeBlue") >= 1f)
		{
			return 0.6f;
		}
		if (!(val.GetCustomVar("armourType") >= 1f))
		{
			return 0f;
		}
		return 0.4f;
	}

	public static float Max(EntityAlive entity)
	{
		Stat val = (((Object)(object)entity == (Object)null) ? null : entity.Stats)?.Stamina;
		if (val != null)
		{
			return val.ModifiedMax;
		}
		return 0f;
	}

	public static float Held(EntityAlive entity)
	{
		EntityBuffs val = (((Object)(object)entity == (Object)null) ? null : entity.Buffs);
		if (val != null)
		{
			return Mathf.Max(0f, val.GetCustomVar("doomArmour"));
		}
		return 0f;
	}

	public static void Change(EntityAlive entity, float wanted)
	{
		EntityBuffs val = (((Object)(object)entity == (Object)null) ? null : entity.Buffs);
		if (val != null)
		{
			float num = Mathf.Clamp(Held(entity) + wanted, 0f, Max(entity));
			if (num != val.GetCustomVar("doomArmour"))
			{
				val.SetCustomVar("doomArmour", num, true, (CVarOperation)0, true);
			}
			Refresh(entity);
		}
	}

	public static void Set(EntityAlive entity, float wanted)
	{
		Change(entity, wanted - Held(entity));
	}

	public static void Refresh(EntityAlive entity)
	{
		EntityBuffs val = (((Object)(object)entity == (Object)null) ? null : entity.Buffs);
		if (val != null)
		{
			float num = Max(entity);
			val.SetCustomVar(".doomArmourPct", (num <= 0f) ? 0f : (Held(entity) / num), false, (CVarOperation)0, false);
		}
	}

	public static int Level(EntityAlive entity, string perk)
	{
		Progression val = (((Object)(object)entity == (Object)null) ? null : entity.Progression);
		if (val == null)
		{
			return 0;
		}
		ProgressionValue progressionValue = val.GetProgressionValue(perk);
		if (progressionValue != null)
		{
			return (int)progressionValue.GetCalculatedLevel(entity);
		}
		return 0;
	}

	public static int Grace(EntityAlive entity)
	{
		if (!((Object)(object)entity != (Object)null) || entity.Buffs == null || !entity.Buffs.HasBuff("buffNutrientGracePeriod"))
		{
			return 0;
		}
		return 1;
	}

	public static int Refund(EntityAlive entity)
	{
		if (entity.Health > 50)
		{
			return 0;
		}
		return (Level(entity, "perkAdrenalineRush") >= 3) ? 1 : 0;
	}

	public static int Resist(EntityAlive entity, DamageSource source)
	{
		int num = ((source == null) ? (-1) : source.getEntityId());
		if (num == -1 || num == ((Entity)entity).entityId)
		{
			return 0;
		}
		return Level(entity, "perkPainTolerance") + Refund(entity);
	}
}
public class DoomArmourApi : IModApi
{
	public void InitMod(Mod _modInstance)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		new Harmony("com.doommod.armour").PatchAll();
		Log.Out("[DoomArmour] Loaded from " + _modInstance.Path);
	}
}
[Preserve]
public class MinEventActionArmour : MinEventActionTargetedBase
{
	private CVarOperation operation = (CVarOperation)2;

	private float value;

	private bool percent;

	public override void Execute(MinEventParams _params)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected I4, but got Unknown
		for (int i = 0; i < base.targets.Count; i++)
		{
			EntityAlive val = base.targets[i];
			if (!((Object)(object)val == (Object)null) && !((Entity)val).isEntityRemote)
			{
				float num = (percent ? (value * Armour.Max(val)) : value);
				CVarOperation val2 = operation;
				switch ((int)val2)
				{
				case 2:
					Armour.Change(val, num);
					break;
				case 3:
					Armour.Change(val, 0f - num);
					break;
				case 0:
				case 1:
					Armour.Set(val, num);
					break;
				}
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (((MinEventActionTargetedBase)this).ParseXmlAttribute(_attribute))
		{
			return true;
		}
		switch (_attribute.Name.LocalName)
		{
		case "operation":
			operation = EnumUtils.Parse<CVarOperation>(_attribute.Value, true);
			return true;
		case "value":
			value = StringParsers.ParseFloat(_attribute.Value, 0, -1, NumberStyles.Any);
			return true;
		case "percent":
			percent = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
			return true;
		default:
			return false;
		}
	}
}
[HarmonyPatch(typeof(EntityAlive), "damageEntityLocal")]
public static class DamageSplit
{
	public static void Prefix(EntityAlive __instance, DamageSource _damageSource, ref int _strength)
	{
		if (_strength > 0)
		{
			EntityPlayer val = (EntityPlayer)(object)((__instance is EntityPlayer) ? __instance : null);
			if (val != null)
			{
				Split(val, ref _strength);
				_strength -= Mathf.Min(_strength, Armour.Resist((EntityAlive)(object)val, _damageSource));
			}
		}
	}

	private static void Split(EntityPlayer player, ref int _strength)
	{
		float num = Armour.Share((EntityAlive)(object)player);
		if (num <= 0f)
		{
			return;
		}
		int num2 = Mathf.FloorToInt(Armour.Held((EntityAlive)(object)player));
		if (num2 > 0)
		{
			int num3 = Mathf.RoundToInt((float)_strength * num);
			if (num3 > 0)
			{
				int num4 = Mathf.Min(Armour.Level((EntityAlive)(object)player, "perkReinforcedArmour"), num3);
				int num5 = Mathf.Min(num2, num3 - num4);
				int num6 = Armour.Grace((EntityAlive)(object)player) + Armour.Refund((EntityAlive)(object)player);
				_strength -= num4 + num5;
				Armour.Change((EntityAlive)(object)player, num6 - num5);
			}
		}
	}
}
[Preserve]
public class XUiC_DoomArmourBar : XUiController
{
	private float _held = -1f;

	private float _max;

	private float _fill;

	private float _share = -1f;

	private bool _visible = true;

	private static EntityPlayerLocal Player()
	{
		World val = (((Object)(object)GameManager.Instance == (Object)null) ? null : GameManager.Instance.World);
		if (val != null)
		{
			return ((WorldBase)val).GetPrimaryPlayer();
		}
		return null;
	}

	private bool Visible(EntityPlayerLocal player)
	{
		if ((Object)(object)player == (Object)null || ((Entity)player).IsDead())
		{
			return false;
		}
		GUIWindowManager windowManager = base.xui.playerUI.windowManager;
		if (!windowManager.IsFullHUDDisabled())
		{
			if (!base.xui.DragAndDropWindow.InMenu)
			{
				return !windowManager.IsHUDPartialHidden();
			}
			return true;
		}
		return false;
	}

	public override void Update(float _dt)
	{
		((XUiController)this).Update(_dt);
		EntityPlayerLocal val = Player();
		if (!((Object)(object)val == (Object)null))
		{
			float num = Armour.Held((EntityAlive)(object)val);
			float num2 = Armour.Max((EntityAlive)(object)val);
			float num3 = ((num2 <= 0f) ? 0f : Mathf.Clamp01(num / num2));
			float num4 = ((_held < 0f) ? num3 : Mathf.Lerp(_fill, num3, _dt * 3f));
			bool flag = Visible(val);
			float num5 = Armour.Share((EntityAlive)(object)val);
			bool num6 = num != _held || num2 != _max || flag != _visible || num5 != _share || Mathf.Abs(num4 - _fill) > 0.0005f;
			if (num != _held || num2 != _max)
			{
				Armour.Refresh((EntityAlive)(object)val);
			}
			_held = num;
			_max = num2;
			_fill = num4;
			_share = num5;
			_visible = flag;
			if (num6 || base.IsDirty)
			{
				base.IsDirty = false;
				((XUiController)this).RefreshBindings();
			}
		}
	}

	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "armourcurrent":
			_value = Mathf.RoundToInt(_held).ToString();
			return true;
		case "armourmax":
			_value = Mathf.RoundToInt(_max).ToString();
			return true;
		case "armourcurrentwithmax":
			_value = Mathf.RoundToInt(_held) + "/" + Mathf.RoundToInt(_max);
			return true;
		case "armourfill":
			_value = _fill.ToString("0.####", CultureInfo.InvariantCulture);
			return true;
		case "armourvisible":
			_value = (_visible ? "true" : "false");
			return true;
		default:
			return ((XUiController)this).GetBindingValueInternal(ref _value, _bindingName);
		}
	}
}
