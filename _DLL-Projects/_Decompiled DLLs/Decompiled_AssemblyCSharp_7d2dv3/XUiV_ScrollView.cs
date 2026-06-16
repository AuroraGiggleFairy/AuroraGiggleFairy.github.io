using System.Collections.Generic;
using UnityEngine;

public class XUiV_ScrollView : XUiV_Panel
{
	[PublicizedFrom(EAccessModifier.Private)]
	public UIScrollView scrollView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_ScrollBar scrollBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIScrollView.Movement movement = UIScrollView.Movement.Vertical;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIScrollView.DragEffect dragEffect;

	[PublicizedFrom(EAccessModifier.Private)]
	public float scrollFactor = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool scrollWithoutFocus;

	[XuiXmlAttribute("movement", false)]
	public UIScrollView.Movement Movement
	{
		get
		{
			return movement;
		}
		set
		{
			if (value != movement)
			{
				movement = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("drageffect", false)]
	public UIScrollView.DragEffect DragEffect
	{
		get
		{
			return dragEffect;
		}
		set
		{
			if (value != dragEffect)
			{
				dragEffect = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("scrollfactor", false)]
	public float ScrollFactor
	{
		get
		{
			return scrollFactor;
		}
		set
		{
			if (!Mathf.Approximately(value, scrollFactor))
			{
				scrollFactor = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("scrollwithoutfocus", false)]
	public bool ScrollWithoutFocus
	{
		get
		{
			return scrollWithoutFocus;
		}
		set
		{
			if (value != scrollWithoutFocus)
			{
				scrollWithoutFocus = value;
				SetDirty();
			}
		}
	}

	public XUiV_ScrollView(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		base.createComponents(_go);
		_go.AddComponent<UIScrollView>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		scrollView = uiTransform.gameObject.GetComponent<UIScrollView>();
	}

	public override void InitView()
	{
		base.InitView();
		controller.Parent.OnScroll += OnScrolled;
		xui.OnBuilt += OnXuiLoadDone;
		SetDirty();
	}

	public override void Cleanup()
	{
		base.Cleanup();
		xui.OnBuilt -= OnXuiLoadDone;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnXuiLoadDone()
	{
		xui.OnBuilt -= OnXuiLoadDone;
		applyScrollEventToChildren(controller);
		controller.Parent.TryGetChildView<XUiV_ScrollBar>("", out scrollBar);
		scrollBar?.Connect(OnScrolled);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyScrollEventToChildren(XUiController _controller)
	{
		foreach (XUiController child in _controller.Children)
		{
			applyScrollEventToChildren(child);
			child.ViewComponent.EventOnScroll = true;
			child.OnScroll += OnScrolled;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		controllerScroll(Time.unscaledDeltaTime);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void controllerScroll(float _dt)
	{
		if (!XUiUtils.HotkeysAllowedFor(controller.Parent.ViewComponent))
		{
			return;
		}
		XUiView currentTarget = xui.playerUI.CursorController.CurrentTarget;
		if (!ScrollWithoutFocus && (currentTarget == null || !currentTarget.Controller.IsSelfOrChildOf(controller.Parent)))
		{
			return;
		}
		Vector2 vector = xui.playerUI.playerInput.GUIActions.Camera.Vector;
		float num = Mathf.Abs(vector.x);
		float num2 = Mathf.Abs(vector.y);
		if (movement == UIScrollView.Movement.Vertical)
		{
			if (!((double)num2 < (double)num * 1.8) && (double)num2 > 0.1)
			{
				scrollView.Scroll(vector.y * _dt);
			}
		}
		else if (!((double)num < (double)num2 * 1.8) && (double)num > 0.1)
		{
			scrollView.Scroll(vector.x * _dt);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		base.Clipping = UIDrawCall.Clipping.SoftClip;
		scrollView.movement = movement;
		scrollView.dragEffect = dragEffect;
		scrollView.scrollWheelFactor = scrollFactor;
		scrollView.constrainOnDrag = true;
		scrollView.disableDragIfFits = true;
		scrollView.smoothDragStart = true;
		UIScrollBar uIScrollBar = scrollBar?.ScrollBar;
		if (uIScrollBar != null)
		{
			uIScrollBar.fillDirection = ((movement != UIScrollView.Movement.Horizontal) ? UIProgressBar.FillDirection.TopToBottom : UIProgressBar.FillDirection.LeftToRight);
			switch (movement)
			{
			case UIScrollView.Movement.Horizontal:
				scrollView.horizontalScrollBar = uIScrollBar;
				scrollView.verticalScrollBar = null;
				break;
			case UIScrollView.Movement.Vertical:
				scrollView.horizontalScrollBar = null;
				scrollView.verticalScrollBar = uIScrollBar;
				break;
			}
			scrollView.CheckScrollbars();
		}
		base.updateData();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		ResetPosition();
	}

	public override void OnVisibilityChanged(bool _visibleInScene)
	{
		base.OnVisibilityChanged(_visibleInScene);
		if (_visibleInScene)
		{
			UpdatePosition();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnScrolled(XUiController _sender, float _delta)
	{
		if (!InputUtils.ShiftKeyPressed)
		{
			scrollView.Scroll(_delta);
		}
	}

	public void ResetPosition()
	{
		scrollView.ResetPosition();
	}

	public void UpdatePosition()
	{
		scrollView.UpdatePosition();
	}

	public void MakeVisible(XUiView _viewComponent)
	{
		if (_viewComponent != null)
		{
			Vector3[] worldCorners = _viewComponent.WorldCorners;
			MakeVisible(worldCorners);
		}
	}

	public void MakeVisible(IList<Vector3> _worldCorners)
	{
		if (_worldCorners != null && _worldCorners.Count >= 4)
		{
			Transform cachedTransform = panel.cachedTransform;
			Vector3[] worldCorners = panel.worldCorners;
			Vector3 vector = cachedTransform.InverseTransformPoint(worldCorners[1]);
			Vector3 vector2 = cachedTransform.InverseTransformPoint(worldCorners[3]);
			Vector3 vector3 = cachedTransform.InverseTransformPoint(_worldCorners[1]);
			Vector3 vector4 = cachedTransform.InverseTransformPoint(_worldCorners[3]);
			Vector3 zero = Vector3.zero;
			if (!scrollView.canMoveHorizontally)
			{
				zero.x = 0f;
			}
			else if (vector3.x < vector.x)
			{
				zero.x = vector.x - vector3.x + base.ClippingSoftness.x;
			}
			else if (vector4.x > vector2.x)
			{
				zero.x = vector2.x - vector4.x - base.ClippingSoftness.x;
			}
			if (!scrollView.canMoveVertically)
			{
				zero.y = 0f;
			}
			else if (vector3.y > vector.y)
			{
				zero.y = vector.y - vector3.y - base.ClippingSoftness.y;
			}
			else if (vector4.y < vector2.y)
			{
				zero.y = vector2.y - vector4.y + base.ClippingSoftness.y;
			}
			zero.z = 0f;
			cachedTransform.localPosition += zero;
			Vector2 clipOffset = panel.clipOffset;
			clipOffset.x -= zero.x;
			clipOffset.y -= zero.y;
			panel.clipOffset = clipOffset;
		}
	}

	public void CenterOn(XUiView _target)
	{
		if (_target != null)
		{
			Vector3[] worldCorners = panel.worldCorners;
			Vector3 vector = (worldCorners[2] + worldCorners[0]) * 0.5f;
			Transform cachedTransform = panel.cachedTransform;
			Vector3 vector2 = cachedTransform.InverseTransformPoint(_target.UiTransform.position);
			Vector3 vector3 = cachedTransform.InverseTransformPoint(vector);
			Vector3 vector4 = vector2 - vector3;
			if (!scrollView.canMoveHorizontally)
			{
				vector4.x = 0f;
			}
			if (!scrollView.canMoveVertically)
			{
				vector4.y = 0f;
			}
			vector4.z = 0f;
			cachedTransform.localPosition -= vector4;
			Vector2 clipOffset = panel.clipOffset;
			clipOffset.x += vector4.x;
			clipOffset.y += vector4.y;
			panel.clipOffset = clipOffset;
		}
	}
}
