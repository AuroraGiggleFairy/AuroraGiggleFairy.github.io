using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ShapeInfoWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Block blockData;

	[PublicizedFrom(EAccessModifier.Private)]
	public string shapeName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue itemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemDisplayEntry itemDisplayEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty && base.ViewComponent.IsVisible)
		{
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "blockname":
			_value = shapeName;
			return true;
		case "blockicon":
			_value = ((blockData == null) ? "" : blockData.GetIconName());
			return true;
		case "blockicontint":
		{
			Color32 v = Color.white;
			if (blockData != null)
			{
				v = blockData.CustomIconTint;
			}
			_value = itemicontintcolorFormatter.Format(v);
			return true;
		}
		case "itemstattitle1":
			_value = GetStatTitle(0);
			return true;
		case "itemstat1":
			_value = GetStatValue(0);
			return true;
		case "itemstattitle2":
			_value = GetStatTitle(1);
			return true;
		case "itemstat2":
			_value = GetStatValue(1);
			return true;
		case "itemstattitle3":
			_value = GetStatTitle(2);
			return true;
		case "itemstat3":
			_value = GetStatValue(2);
			return true;
		case "itemstattitle4":
			_value = GetStatTitle(3);
			return true;
		case "itemstat4":
			_value = GetStatValue(3);
			return true;
		case "itemstattitle5":
			_value = GetStatTitle(4);
			return true;
		case "itemstat5":
			_value = GetStatValue(4);
			return true;
		default:
			return false;
		}
	}

	public void SetShape(Block _newBlockData)
	{
		blockData = _newBlockData;
		if (_newBlockData != null)
		{
			if (_newBlockData.GetAutoShapeType() == EAutoShapeType.None)
			{
				shapeName = blockData.GetLocalizedBlockName();
			}
			else
			{
				shapeName = blockData.GetLocalizedAutoShapeShapeName();
			}
		}
		else
		{
			shapeName = "";
		}
		itemValue = _newBlockData.ToBlockValue().ToItemValue();
		itemDisplayEntry = UIDisplayInfoManager.Current.GetDisplayStatsForTag(_newBlockData.DisplayType);
		RefreshBindings();
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
		return XUiM_ItemStack.GetStatItemValueTextWithCompareInfo(itemValue, ItemValue.None, base.xui.playerUI.entityPlayer, infoEntry);
	}
}
