using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ListEntry<T> : XUiController where T : XUiListEntry<T>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public delegate bool NullBindingDelegate(ref string _value, string _bindingName);

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isHovered;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite background;

	public XUiC_List<T> List;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 bgColorUnselected = new Color32(64, 64, 64, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 bgColorHovered = new Color32(96, 96, 96, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 bgColorSelected = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public string bgSpriteNameUnselected = "menu_empty";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string bgSpriteNameSelected = "ui_game_select_row";

	[PublicizedFrom(EAccessModifier.Private)]
	public static NullBindingDelegate nullBindings;

	[PublicizedFrom(EAccessModifier.Private)]
	public T entryData;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool selected;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool forceHovered;

	public bool HasEntry
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return entryData != null;
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
			if (value)
			{
				if (List.SelectedEntry != null)
				{
					List.SelectedEntry.SelectedChanged(isSelected: false);
					List.SelectedEntry.selected = false;
				}
			}
			else if (List.SelectedEntry == this)
			{
				SelectedChanged(isSelected: false);
				selected = false;
				List.ClearSelection();
			}
			selected = value;
			if (selected)
			{
				List.SelectedEntry = this;
			}
			SelectedChanged(selected);
		}
	}

	public bool ForceHovered
	{
		get
		{
			return forceHovered;
		}
		set
		{
			if (value != forceHovered)
			{
				forceHovered = value;
				updateHoveredEffect();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static XUiC_ListEntry()
	{
		MethodInfo method = typeof(T).GetMethod("GetNullBindingValues", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
		{
			typeof(string).MakeByRefType(),
			typeof(string)
		}, null);
		if (method != null)
		{
			nullBindings = Delegate.CreateDelegate(typeof(NullBindingDelegate), method) as NullBindingDelegate;
		}
		else
		{
			Log.Warning("[XUi] List entry type \"" + typeof(T).FullName + "\" does not have a static GetNullBindingValues method");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SelectedChanged(bool isSelected)
	{
		if (background != null)
		{
			background.Color = (isSelected ? bgColorSelected : bgColorUnselected);
			background.SpriteName = (isSelected ? bgSpriteNameSelected : bgSpriteNameUnselected);
		}
	}

	public override void Init()
	{
		base.Init();
		for (int i = 0; i < children.Count; i++)
		{
			XUiView xUiView = children[i].ViewComponent;
			if (xUiView.ID.EqualsCaseInsensitive("background"))
			{
				background = xUiView as XUiV_Sprite;
			}
		}
		base.OnPress += XUiC_ListEntry_OnPress;
		base.ViewComponent.Enabled = HasEntry;
		IsDirty = true;
	}

	public void XUiC_ListEntry_OnPress(XUiController _sender, int _mouseButton)
	{
		if (base.ViewComponent.Enabled)
		{
			if (!Selected)
			{
				Selected = true;
			}
			List.OnListEntryClicked(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateHoveredEffect()
	{
		if (background != null && HasEntry && !Selected)
		{
			if (forceHovered || isHovered)
			{
				background.Color = bgColorHovered;
			}
			else
			{
				background.Color = bgColorUnselected;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		isHovered = _isOver;
		updateHoveredEffect();
		RefreshBindings();
		base.OnHovered(_isOver);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings();
			IsDirty = false;
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		if (base.ParseAttribute(name, value, _parent))
		{
			return true;
		}
		switch (name)
		{
		case "background_color_unselected":
			bgColorUnselected = StringParsers.ParseColor32(value);
			break;
		case "background_color_hovered":
			bgColorHovered = StringParsers.ParseColor32(value);
			break;
		case "background_color_selected":
			bgColorSelected = StringParsers.ParseColor32(value);
			break;
		case "background_sprite_unselected":
			bgSpriteNameUnselected = value;
			break;
		case "background_sprite_selected":
			bgSpriteNameSelected = value;
			break;
		default:
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "hasentry")
		{
			_value = (entryData != null).ToString();
			return true;
		}
		if (_bindingName == "hovered")
		{
			_value = isHovered.ToString();
			return true;
		}
		if (entryData != null)
		{
			return entryData.GetBindingValue(ref _value, _bindingName);
		}
		if (nullBindings != null)
		{
			return nullBindings(ref _value, _bindingName);
		}
		return false;
	}

	public virtual void SetEntry(T _data)
	{
		if (_data != entryData)
		{
			entryData = _data;
			base.ViewComponent.Enabled = HasEntry;
			if ((!Selected || !HasEntry) && background != null)
			{
				background.Color = bgColorUnselected;
			}
		}
		base.ViewComponent.IsNavigatable = (base.ViewComponent.IsSnappable = HasEntry);
		IsDirty = true;
	}

	public T GetEntry()
	{
		return entryData;
	}
}
