using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_AssembleWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack itemStack = ItemStack.Empty.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass itemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnComplete;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor qualitycolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat qualityfillFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt qualityFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemDisplayEntry itemDisplayEntry;

	public virtual ItemStack ItemStack
	{
		get
		{
			return itemStack;
		}
		set
		{
			itemStack = value;
			if (!itemStack.IsEmpty())
			{
				itemClass = itemStack.itemValue.ItemClass;
				itemDisplayEntry = UIDisplayInfoManager.Current.GetDisplayStatsForTag(itemClass.DisplayType);
			}
			else
			{
				itemClass = null;
			}
			RefreshBindings();
		}
	}

	public override void Init()
	{
		base.Init();
		btnComplete = GetChildById("btnComplete");
		if (btnComplete != null)
		{
			btnComplete.OnPress += BtnComplete_OnPress;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnComplete_OnPress(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.CloseAllOpenWindows();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = !itemStack.IsEmpty();
		switch (bindingName)
		{
		case "itemicon":
			value = "";
			if (flag)
			{
				value = itemStack.itemValue.GetPropertyOverride("CustomIcon", (itemClass.CustomIcon != null) ? itemClass.CustomIcon.Value : itemClass.GetIconName());
			}
			return true;
		case "itemicontint":
		{
			Color32 v2 = Color.white;
			if (itemClass != null)
			{
				v2 = itemStack.itemValue.ItemClass.GetIconTint(itemStack.itemValue);
			}
			value = itemicontintcolorFormatter.Format(v2);
			return true;
		}
		case "itemname":
			value = (flag ? itemClass.GetLocalizedItemName() : "");
			return true;
		case "itemqualitycolor":
			value = "255,255,255,255";
			if (flag)
			{
				Color32 v = QualityInfo.GetQualityColor(itemStack.itemValue.Quality);
				value = qualitycolorFormatter.Format(v);
			}
			return true;
		case "itemqualityfill":
			value = (flag ? qualityfillFormatter.Format(((float)itemStack.itemValue.MaxUseTimes - itemStack.itemValue.UseTimes) / (float)itemStack.itemValue.MaxUseTimes) : "1");
			return true;
		case "itemquality":
			value = (flag ? qualityFormatter.Format(itemStack.itemValue.Quality) : "0");
			return true;
		case "itemqualitytitle":
			value = Localization.Get("xuiQuality");
			return true;
		case "itemstattitle1":
			value = (flag ? GetStatTitle(0) : "");
			return true;
		case "itemstat1":
			value = (flag ? GetStatValue(0) : "");
			return true;
		case "itemstattitle2":
			value = (flag ? GetStatTitle(1) : "");
			return true;
		case "itemstat2":
			value = (flag ? GetStatValue(1) : "");
			return true;
		case "itemstattitle3":
			value = (flag ? GetStatTitle(2) : "");
			return true;
		case "itemstat3":
			value = (flag ? GetStatValue(2) : "");
			return true;
		case "itemstattitle4":
			value = (flag ? GetStatTitle(3) : "");
			return true;
		case "itemstat4":
			value = (flag ? GetStatValue(3) : "");
			return true;
		case "itemstattitle5":
			value = (flag ? GetStatTitle(4) : "");
			return true;
		case "itemstat5":
			value = (flag ? GetStatValue(4) : "");
			return true;
		case "itemstattitle6":
			value = (flag ? GetStatTitle(5) : "");
			return true;
		case "itemstat6":
			value = (flag ? GetStatValue(5) : "");
			return true;
		case "itemstattitle7":
			value = (flag ? GetStatTitle(6) : "");
			return true;
		case "itemstat7":
			value = (flag ? GetStatValue(6) : "");
			return true;
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetStatTitle(int index)
	{
		if (itemDisplayEntry == null || itemDisplayEntry.DisplayStats.Count <= index)
		{
			return "";
		}
		if (itemDisplayEntry.DisplayStats[index].TitleOverride != null)
		{
			return itemDisplayEntry.DisplayStats[index].TitleOverride;
		}
		return UIDisplayInfoManager.Current.GetLocalizedName(itemDisplayEntry.DisplayStats[index].StatType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetStatValue(int index)
	{
		if (itemDisplayEntry == null || itemDisplayEntry.DisplayStats.Count <= index)
		{
			return "";
		}
		DisplayInfoEntry infoEntry = itemDisplayEntry.DisplayStats[index];
		return XUiM_ItemStack.GetStatItemValueTextWithModInfo(itemStack, base.xui.playerUI.entityPlayer, infoEntry);
	}

	public override void Update(float _dt)
	{
		if (!(GameManager.Instance == null) || GameManager.Instance.World != null)
		{
			if (isDirty)
			{
				RefreshBindings();
				isDirty = false;
			}
			base.Update(_dt);
		}
	}

	public virtual void OnChanged()
	{
		XUiC_AssembleWindowGroup.GetWindowGroup(base.xui).ItemStack = ItemStack;
	}
}
