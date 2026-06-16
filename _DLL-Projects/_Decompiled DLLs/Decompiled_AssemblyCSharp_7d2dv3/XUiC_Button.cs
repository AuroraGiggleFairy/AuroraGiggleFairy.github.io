using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Button : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdToActivateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdToActivateElapsed;

	public bool HoldToActivate => holdToActivateTime > 0f;

	[XuiXmlAttribute("hold_to_activate_time", false)]
	public float HoldToActivateTime
	{
		get
		{
			return holdToActivateTime;
		}
		set
		{
			if (!Mathf.Approximately(holdToActivateTime, value))
			{
				holdToActivateTime = value;
				IsDirty = true;
			}
		}
	}

	public float HoldToActivateElapsed
	{
		get
		{
			return holdToActivateElapsed;
		}
		set
		{
			if (!Mathf.Approximately(holdToActivateElapsed, value))
			{
				holdToActivateElapsed = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlBinding("hold_to_activate_state")]
	public float HoldToActivateState
	{
		get
		{
			if (!HoldToActivate)
			{
				return 0f;
			}
			return Mathf.Clamp01(holdToActivateElapsed / holdToActivateTime);
		}
	}

	[XuiXmlBinding("btn_enabled")]
	public bool BindingEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return viewComponent.Enabled;
		}
	}

	[XuiXmlBinding("btn_hovered")]
	public bool BindingHovered
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return viewComponent.IsHovered;
		}
	}

	[XuiXmlBinding("btn_attributes")]
	public ObservableDictionary<string, object> BindingCustomAttributes
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return CustomAttributes;
		}
	}

	public override void Init()
	{
		base.Init();
		if (!base.ViewComponent.HasAnyEvent)
		{
			throw new Exception("[XUi] Button requires the view to listen to events. Hierarchy: " + GetXuiHierarchy());
		}
		CustomAttributes.EntryModified += OnCustomAttributeModified;
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
		if (HoldToActivate && isPressed)
		{
			HoldToActivateElapsed += Time.deltaTime;
		}
		handleDirtyUpdateDefault();
	}

	public override void Pressed(int _mouseButton)
	{
		if (!HoldToActivate)
		{
			base.Pressed(_mouseButton);
		}
	}

	[XuiBindEvent("OnVisiblity", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void onVisibilityChanged(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
	{
		if (!_visibleInScene)
		{
			isPressed = false;
			HoldToActivateElapsed = 0f;
		}
	}

	[XuiBindEvent("OnMouseUpDown", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void onMouse(XUiController _sender, bool _mouseDown)
	{
		if (!HoldToActivate)
		{
			return;
		}
		isPressed = _mouseDown;
		if (!_mouseDown)
		{
			if (HoldToActivateState >= 1f)
			{
				base.ViewComponent.PlayClickSound();
				base.Pressed(-1);
			}
			HoldToActivateElapsed = 0f;
		}
	}
}
