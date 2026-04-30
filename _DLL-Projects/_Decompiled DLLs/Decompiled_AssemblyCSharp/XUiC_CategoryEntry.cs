using UnityEngine.Scripting;

[Preserve]
public class XUiC_CategoryEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string categoryName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string categoryDisplayName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string spriteName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool selected;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button button;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList CategoryList { get; set; }

	public string CategoryName
	{
		get
		{
			return categoryName;
		}
		set
		{
			categoryName = value;
			IsDirty = true;
		}
	}

	public string CategoryDisplayName
	{
		get
		{
			return categoryDisplayName;
		}
		set
		{
			categoryDisplayName = value;
			IsDirty = true;
		}
	}

	public string SpriteName
	{
		get
		{
			return spriteName;
		}
		set
		{
			spriteName = value;
			IsDirty = true;
		}
	}

	public new bool Selected
	{
		get
		{
			return selected;
		}
		set
		{
			selected = value;
			button.Selected = selected;
		}
	}

	public override void Init()
	{
		base.Init();
		button = (XUiV_Button)base.ViewComponent;
		base.OnPress += XUiC_CategoryEntry_OnPress;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiC_CategoryEntry_OnPress(XUiController _sender, int _mouseButton)
	{
		if (spriteName != string.Empty)
		{
			if (CategoryList.CurrentCategory == this && CategoryList.AllowUnselect)
			{
				CategoryList.CurrentCategory = null;
			}
			else
			{
				CategoryList.CurrentCategory = this;
			}
			CategoryList.HandleCategoryChanged();
		}
	}

	public void PlayButtonClickSound()
	{
		button.PlayClickSound();
	}

	public override void Update(float _dt)
	{
		if (IsDirty)
		{
			base.ViewComponent.IsNavigatable = !string.IsNullOrEmpty(SpriteName);
			RefreshBindings(_forceAll: true);
			IsDirty = false;
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (!(bindingName == "categoryicon"))
		{
			if (bindingName == "categorydisplayname")
			{
				value = categoryDisplayName;
				return true;
			}
			return false;
		}
		value = spriteName;
		return true;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "categoryname":
			if (!string.IsNullOrEmpty(_value))
			{
				CategoryName = _value;
			}
			return true;
		case "spritename":
			if (!string.IsNullOrEmpty(_value))
			{
				SpriteName = _value;
			}
			return true;
		case "displayname":
			if (!string.IsNullOrEmpty(_value))
			{
				CategoryDisplayName = _value;
			}
			return true;
		case "displayname_key":
			if (!string.IsNullOrEmpty(_value))
			{
				CategoryDisplayName = Localization.Get(_value);
			}
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		}
	}
}
