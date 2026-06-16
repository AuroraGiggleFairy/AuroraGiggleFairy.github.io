using UnityEngine.Scripting;

[Preserve]
public class XUiC_CaptionedTextBox : XUiController
{
	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput textInput;

	public string Text
	{
		get
		{
			return textInput.Text;
		}
		set
		{
			textInput.Text = value;
		}
	}

	[XuiXmlBinding("box_enabled")]
	public bool BindingEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return viewComponent?.Enabled ?? true;
		}
	}

	[XuiXmlBinding("box_hovered")]
	public bool BindingHovered
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return textInput?.UIInputController.ViewComponent.IsHovered ?? false;
		}
	}

	[XuiXmlBinding("box_empty")]
	public bool BindingEmpty
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return string.IsNullOrEmpty(textInput?.Text);
		}
	}

	[XuiXmlBinding("box_selected")]
	public bool BindingSelected
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return textInput?.IsSelected ?? false;
		}
	}

	[XuiXmlBinding("box_attributes")]
	public ObservableDictionary<string, object> BindingCustomAttributes
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return CustomAttributes;
		}
	}

	public event XUiEvent_InputOnSubmitEventHandler OnSubmitHandler;

	public event XUiEvent_InputOnChangedEventHandler OnChangeHandler;

	public override void Init()
	{
		base.Init();
		CustomAttributes.EntryModified += OnCustomAttributeModified;
		textInput.UIInputController.OnHover += Event_OnHover;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnCustomAttributeModified(object _sender, DictionaryChangedEventArgs<string, object> _e)
	{
		IsDirty = true;
	}

	[XuiBindEvent("OnInteraction", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnAnyEvent(XUiController _sender, EXUiControllerInteractionType _type)
	{
		IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Event_OnHover(XUiController _sender, bool _isOver)
	{
		IsDirty = true;
	}

	[XuiBindEvent("OnInputSelectedHandler", "textInput")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Event_OnInputSelected(XUiController _sender, bool _selected)
	{
		if (!_selected)
		{
			switchToButtonIfEmpty();
		}
		IsDirty = true;
	}

	[XuiBindEvent("OnChangeHandler", "textInput")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Event_OnInputChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.OnChangeHandler?.Invoke(_sender, _text, _changeFromCode);
		if (!textInput.IsSelected)
		{
			switchToButtonIfEmpty();
		}
	}

	[XuiBindEvent("OnSubmitHandler", "textInput")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Event_OnSubmitChanged(XUiController _sender, string _text)
	{
		this.OnSubmitHandler?.Invoke(_sender, _text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void switchToButtonIfEmpty()
	{
		if (string.IsNullOrWhiteSpace(textInput.Text))
		{
			textInput.Text = "";
			ThreadManager.RunTaskAfterFrames([PublicizedFrom(EAccessModifier.Private)] () =>
			{
				IsDirty = true;
			});
		}
	}
}
