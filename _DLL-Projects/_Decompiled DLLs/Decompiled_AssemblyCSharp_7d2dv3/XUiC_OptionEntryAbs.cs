using System;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_OptionEntryAbs : XUiController
{
	[XuiBindParent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionsDialogBase parentOptionsDialog;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiC_ComboBoxBase comboGeneric;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool optionHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hoveringInitialized;

	[XuiXmlAttribute("prefname", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string PrefName
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("apply_immediately", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool ApplyImmediately
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
		[PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[XuiXmlAttribute("apply_defaults", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool ApplyDefaults
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
		[PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[XuiXmlBinding("option_changed")]
	public abstract bool IsChanged { get; }

	[XuiXmlBinding("option_is_default")]
	public abstract bool IsDefault { get; }

	[XuiXmlBinding("option_hovered")]
	public bool OptionHovered
	{
		get
		{
			return optionHovered;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (optionHovered != value)
			{
				optionHovered = value;
				IsDirty = true;
			}
		}
	}

	public event Action<XUiC_OptionEntryAbs> ValueChanged;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void invokeValueChanged()
	{
		this.ValueChanged?.Invoke(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void initCurrentValue();

	public abstract void DiscardCurrentChange();

	public abstract void ApplySelection();

	public abstract void ResetToDefault();

	public override void OnOpen()
	{
		base.OnOpen();
		initHovering();
		initCurrentValue();
	}

	public override void OnClose()
	{
		base.OnClose();
		DiscardCurrentChange();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initHovering()
	{
		if (!hoveringInitialized)
		{
			hoveringInitialized = true;
			ApplyEventToChildren(this);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void ApplyEventToChildren(XUiController _controller)
		{
			if (_controller.ViewComponent.HasAnyEvent)
			{
				_controller.ViewComponent.EventOnHover = true;
				_controller.OnHover += OnHoveredElementChanged;
			}
			foreach (XUiController child in _controller.Children)
			{
				ApplyEventToChildren(child);
			}
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			IsDirty = false;
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHoveredElementChanged(XUiController _sender, bool _state)
	{
		OptionHovered = _state;
		parentOptionsDialog.HoveredOption = (_state ? this : null);
	}

	public static void DebugLog(string _s)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_OptionEntryAbs()
	{
	}
}
