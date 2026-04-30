using System.Text;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkillCraftingInfoEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string color_bg_bought;

	[PublicizedFrom(EAccessModifier.Private)]
	public string color_bg_available;

	[PublicizedFrom(EAccessModifier.Private)]
	public string color_bg_locked;

	[PublicizedFrom(EAccessModifier.Private)]
	public string color_lbl_available;

	[PublicizedFrom(EAccessModifier.Private)]
	public string color_lbl_locked;

	[PublicizedFrom(EAccessModifier.Private)]
	public int listIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProgressionClass.DisplayData data;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxEntriesWithoutPaging = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hiddenEntriesWithPaging = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float, bool> attributeSubtractionFormatter = new CachedStringFormatter<string, float, bool>([PublicizedFrom(EAccessModifier.Internal)] (string _s, float _f, bool _b) => string.Format("{0}: {1}", _s, _f.ToCultureInvariantString("0.#") + (_b ? "%" : "")));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float> attributeSetValueFormatter = new CachedStringFormatter<string, float>([PublicizedFrom(EAccessModifier.Internal)] (string _s, float _f) => string.Format("{0}: {1}", _s, _f.ToCultureInvariantString("0.#")));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor durabilitycolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor nextdurabilitycolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringBuilder effectsStringBuilder = new StringBuilder();

	public int ListIndex
	{
		set
		{
			if (value != listIndex)
			{
				listIndex = value;
				IsDirty = true;
			}
		}
	}

	public ProgressionClass.DisplayData Data
	{
		get
		{
			return data;
		}
		set
		{
			if (value != data)
			{
				data = value;
				IsDirty = true;
			}
		}
	}

	public bool IsSelected
	{
		get
		{
			return isSelected;
		}
		set
		{
			isSelected = value;
			IsDirty = true;
		}
	}

	public int MaxEntriesWithoutPaging
	{
		set
		{
			if (maxEntriesWithoutPaging != value)
			{
				maxEntriesWithoutPaging = value;
				IsDirty = true;
			}
		}
	}

	public int HiddenEntriesWithPaging
	{
		set
		{
			if (hiddenEntriesWithPaging != value)
			{
				hiddenEntriesWithPaging = value;
				IsDirty = true;
			}
		}
	}

	public override void Init()
	{
		base.Init();
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "color_bg_bought":
			color_bg_bought = _value;
			return true;
		case "color_bg_available":
			color_bg_available = _value;
			return true;
		case "color_bg_locked":
			color_bg_locked = _value;
			return true;
		case "color_lbl_available":
			color_lbl_available = _value;
			return true;
		case "color_lbl_locked":
			color_lbl_locked = _value;
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		bool flag = data != null;
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		switch (_bindingName)
		{
		case "nothiddenbypager":
			_value = "true";
			return true;
		case "hasentry":
			_value = flag.ToString();
			return true;
		case "color_bg":
			_value = color_bg_available;
			if (flag)
			{
				_value = ((data.GetQualityLevel(entityPlayer.Progression.GetProgressionValue(data.Owner.Name).Level) == 0) ? color_bg_locked : color_bg_available);
			}
			return true;
		case "color_fg":
			_value = color_lbl_available;
			if (flag)
			{
				_value = ((data.GetQualityLevel(entityPlayer.Progression.GetProgressionValue(data.Owner.Name).Level) == 0) ? color_lbl_locked : color_lbl_available);
			}
			return true;
		case "iconatlas":
			if (!flag)
			{
				_value = "ItemIconAtlas";
			}
			else
			{
				_value = ((entityPlayer.Progression.GetProgressionValue(data.Owner.Name).Level >= data.QualityStarts[0]) ? "ItemIconAtlas" : "ItemIconAtlasGreyscale");
			}
			return true;
		case "itemicon":
			_value = (flag ? data.GetIcon(entityPlayer.Progression.GetProgressionValue(data.Owner.Name).Level) : "");
			return true;
		case "itemcolor":
			_value = (flag ? data.GetIconTint(entityPlayer.Progression.GetProgressionValue(data.Owner.Name).Level) : "FFFFFF");
			return true;
		case "currentqualitytext":
			_value = (flag ? data.GetQualityLevel(entityPlayer.Progression.GetProgressionValue(data.Owner.Name).Level).ToString() : "");
			return true;
		case "currentqualitycolor":
		{
			if (!flag)
			{
				_value = "FFFFFF";
				return true;
			}
			Color32 v = QualityInfo.GetTierColor(data.GetQualityLevel(entityPlayer.Progression.GetProgressionValue(data.Owner.Name).Level));
			_value = durabilitycolorFormatter.Format(v);
			return true;
		}
		case "nextqualitytext":
		{
			if (!flag)
			{
				_value = "";
				return true;
			}
			int num = data.GetQualityLevel(entityPlayer.Progression.GetProgressionValue(data.Owner.Name).Level);
			if (num < data.QualityStarts.Length + 1)
			{
				num++;
			}
			_value = num.ToString();
			return true;
		}
		case "nextqualitycolor":
		{
			if (!flag)
			{
				_value = "";
				return true;
			}
			int num2 = data.GetQualityLevel(entityPlayer.Progression.GetProgressionValue(data.Owner.Name).Level);
			if (num2 < data.QualityStarts.Length + 1)
			{
				num2++;
			}
			_value = nextdurabilitycolorFormatter.Format(QualityInfo.GetTierColor(num2));
			return true;
		}
		case "nextpoints":
			_value = (flag ? $"{entityPlayer.Progression.GetProgressionValue(data.Owner.Name).Level}/{data.GetNextPoints(entityPlayer.Progression.GetProgressionValue(data.Owner.Name).Level).ToString()}" : "");
			return true;
		case "showquality":
			_value = (flag ? (data.HasQuality && data.GetQualityLevel(entityPlayer.Progression.GetProgressionValue(data.Owner.Name).Level) > 0).ToString() : "false");
			return true;
		case "showlock":
			_value = (flag ? (data.GetQualityLevel(entityPlayer.Progression.GetProgressionValue(data.Owner.Name).Level) == 0).ToString() : "false");
			return true;
		case "showcomplete":
			_value = (flag ? data.IsComplete(entityPlayer.Progression.GetProgressionValue(data.Owner.Name).Level).ToString() : "false");
			return true;
		case "notcomplete":
			_value = (flag ? (!data.IsComplete(entityPlayer.Progression.GetProgressionValue(data.Owner.Name).Level)).ToString() : "false");
			return true;
		case "notcompletequality":
			_value = (flag ? (data.HasQuality && !data.IsComplete(entityPlayer.Progression.GetProgressionValue(data.Owner.Name).Level)).ToString() : "false");
			return true;
		case "notcompletenoquality":
			_value = (flag ? (!data.HasQuality && !data.IsComplete(entityPlayer.Progression.GetProgressionValue(data.Owner.Name).Level)).ToString() : "false");
			return true;
		case "text":
			_value = (flag ? data.GetName(entityPlayer.Progression.GetProgressionValue(data.Owner.Name).Level) : "");
			return true;
		case "show_selected":
			_value = isSelected.ToString();
			return true;
		default:
			return false;
		}
	}

	public override void Update(float _dt)
	{
		if (IsDirty)
		{
			IsDirty = false;
			RefreshBindings(IsDirty);
		}
		base.Update(_dt);
	}
}
