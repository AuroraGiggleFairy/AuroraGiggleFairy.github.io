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
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty && base.ViewComponent.IsVisible)
		{
			IsDirty = false;
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
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
		RefreshBindings();
	}
}
