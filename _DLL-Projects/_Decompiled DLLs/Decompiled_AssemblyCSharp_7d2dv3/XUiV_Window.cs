using System;
using UnityEngine;

public class XUiV_Window : XUiView
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string anchor;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIPanel panel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView previousNavigationTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView previousNavigationLockView;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 oldTransformPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUi.StackPanel stackPanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public float targetAlpha;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool fade = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int frameClosed;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fadeInTime = 0.05f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fadeTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delayToFadeTime = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delayTimer;

	public override UIRect UiRect
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return null;
		}
	}

	[XuiXmlAttribute("anchor", false)]
	public string Anchor
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return anchor;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			anchor = value;
			SetDirty();
		}
	}

	[XuiXmlAttribute("cursor_area", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsCursorArea
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("lock_navigation", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool LockNavigation
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsOpen
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsInStackPanel => stackPanel != null;

	public override Vector3[] WorldCorners => panel.worldCorners;

	public float PanelAlpha => panel.alpha;

	[XuiXmlAttribute("fade_window", false)]
	public bool Fade
	{
		get
		{
			return fade;
		}
		set
		{
			fade = value;
			SetDirty();
		}
	}

	[XuiXmlAttribute("fade_time", false)]
	public float FadeInTime
	{
		get
		{
			return fadeInTime;
		}
		set
		{
			fadeInTime = value;
			SetDirty();
		}
	}

	[XuiXmlAttribute("fade_delay", false)]
	public float DelayToFadeTime
	{
		get
		{
			return delayToFadeTime;
		}
		set
		{
			delayToFadeTime = value;
			SetDirty();
		}
	}

	public XUiV_Window(XUi _xui, string _id)
		: base(_xui, _id)
	{
		xui.AddWindow(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		_go.AddComponent<UIPanel>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		panel = uiTransform.gameObject.GetComponent<UIPanel>();
	}

	public override void InitView()
	{
		base.InitView();
		setRootNode();
		base.Controller.OnVisiblity += updateVisibility;
		panel.depth = base.Depth + 1;
		panel.alpha = 0f;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (uiTransform != null)
		{
			UnityEngine.Object.DestroyImmediate(uiTransform.gameObject);
		}
		xui.RemoveWindow(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void setRootNode()
	{
		Transform transform;
		if (stackPanel != null)
		{
			transform = stackPanel.Transform;
		}
		else if (!string.IsNullOrEmpty(Anchor))
		{
			transform = xui.GetAnchor(Anchor);
			if (transform == null)
			{
				Log.Error("Specified window anchor \"" + Anchor + "\" not found for window \"" + base.ID + "\"");
				throw new Exception();
			}
		}
		else
		{
			transform = xui.transform;
		}
		uiTransform.parent = transform;
		uiTransform.gameObject.layer = 12;
		uiTransform.localScale = Vector3.one;
		uiTransform.localPosition = new Vector3(base.PaddedPosition.x, base.PaddedPosition.y, 0f);
		uiTransform.localEulerAngles = new Vector3(0f, 0f, rotation);
	}

	public override void Update(float _dt)
	{
		if ((double)Time.timeScale < 0.01 || !fade)
		{
			delayTimer = delayToFadeTime + 1f;
			fadeTimer = fadeInTime;
		}
		if (delayTimer < delayToFadeTime)
		{
			delayTimer += _dt;
		}
		if (delayTimer < delayToFadeTime)
		{
			base.Update(_dt);
			return;
		}
		panel.alpha = Mathf.Lerp(panel.alpha, targetAlpha, fadeTimer / fadeInTime);
		fadeTimer += _dt;
		if (IsCursorArea && oldTransformPosition != uiTransform.position && base.IsVisible)
		{
			xui.UpdateWindowSoftCursorBounds(this);
			oldTransformPosition = uiTransform.position;
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		setRootNode();
		base.updateData();
		panel.SetDirty();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.IsVisible = true;
		if (frameClosed == Time.frameCount)
		{
			panel.alpha = 1f;
			targetAlpha = 1f;
			fadeTimer = fadeInTime + 1f;
			delayTimer = delayToFadeTime + 1f;
		}
		else
		{
			panel.alpha = 0f;
			targetAlpha = 1f;
			fadeTimer = 0f;
			delayTimer = 0f;
		}
		if (IsCursorArea)
		{
			oldTransformPosition = Vector3.zero;
		}
		applyNavigationChanges();
		IsOpen = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyNavigationChanges()
	{
		CursorControllerAbs cursorController = xui.playerUI.CursorController;
		previousNavigationTarget = cursorController.navigationTargetLater ?? cursorController.CurrentTarget;
		if (LockNavigation)
		{
			previousNavigationLockView = cursorController.lockNavigationToView;
			cursorController.SetNavigationLockView(this);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		base.IsVisible = false;
		panel.alpha = 0f;
		targetAlpha = 0f;
		fadeTimer = fadeInTime + 1f;
		delayTimer = delayToFadeTime + 1f;
		frameClosed = Time.frameCount;
		IsOpen = false;
		if (LockNavigation)
		{
			xui.playerUI.CursorController.SetNavigationLockView(previousNavigationLockView, previousNavigationTarget);
		}
		previousNavigationTarget = null;
		previousNavigationLockView = null;
	}

	public void ForceVisible(float _alpha = -1f)
	{
		delayTimer = delayToFadeTime + 1f;
		fadeTimer = fadeInTime + 1f;
		if (_alpha >= 0f)
		{
			targetAlpha = _alpha;
		}
		panel.alpha = targetAlpha;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateVisibility(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
	{
		if (IsCursorArea)
		{
			if (_visibleInScene)
			{
				xui.UpdateWindowSoftCursorBounds(this);
			}
			else
			{
				xui.RemoveWindowFromSoftCursorBounds(this);
			}
		}
	}

	[XuiXmlAttribute("panel", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributePanel(string _value)
	{
		if (!string.IsNullOrEmpty(_value) && !xui.StackPanels.dict.TryGetValue(_value, out stackPanel))
		{
			Log.Error("[XUi] Could not find StackPanel '" + _value + "' for view '" + id + "' in window group '" + controller.WindowGroup.Id + "'");
		}
	}
}
