using UnityEngine;

public class XUiV_ScrollBar : XUiView
{
	[PublicizedFrom(EAccessModifier.Private)]
	public UIScrollBar scrollBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite thumb;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public float onOpenValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int openFrame;

	public override UIRect UiRect
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return null;
		}
	}

	public UIScrollBar ScrollBar => scrollBar;

	public float ScrollPosition
	{
		get
		{
			return scrollBar.value;
		}
		set
		{
			scrollBar.value = value;
			SetDirty();
		}
	}

	public override Vector3[] WorldCorners
	{
		get
		{
			if (background == null)
			{
				return thumb?.WorldCorners;
			}
			return background.WorldCorners;
		}
	}

	public XUiV_ScrollBar(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		base.createComponents(_go);
		_go.AddComponent<UIScrollBar>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		scrollBar = uiTransform.gameObject.GetComponent<UIScrollBar>();
		scrollBar.value = 0f;
	}

	public override void InitView()
	{
		base.InitView();
		if (!base.Controller.TryGetChildView<XUiV_Sprite>("thumb", out thumb))
		{
			Log.Error("[XUi] XUiV_ScrollBar without child sprite 'thumb'. Hierarchy: " + GetXuiHierarchy());
		}
		else if (!thumb.HasAnyEvent)
		{
			Log.Error("[XUi] XUiV_ScrollBar child sprite 'thumb' has no events enabled. Hierarchy: " + GetXuiHierarchy());
		}
		else if (base.Controller.TryGetChildView<XUiV_Sprite>("background", out background) && !background.HasAnyEvent)
		{
			Log.Error("[XUi] XUiV_ScrollBar child sprite 'background' has no events enabled. Hierarchy: " + GetXuiHierarchy());
		}
		else
		{
			SetDirty();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		scrollBar.alpha = 0f;
		if (thumb != null)
		{
			Color color = thumb.Color;
			color.a = 0f;
			thumb.Color = color;
		}
		if (background != null)
		{
			Color color2 = background.Color;
			color2.a = 0f;
			background.Color = color2;
		}
		onOpenValue = scrollBar.value;
		openFrame = Time.frameCount;
	}

	public override void OnVisibilityChanged(bool _visibleInScene)
	{
		base.OnVisibilityChanged(_visibleInScene);
		if (_visibleInScene)
		{
			onOpenValue = scrollBar.value;
			openFrame = Time.frameCount;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (Time.frameCount <= openFrame + 1)
		{
			float value = scrollBar.value;
			if (!Mathf.Approximately(onOpenValue, value))
			{
				scrollBar.value = onOpenValue;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		refreshBoxCollider();
		base.updateData();
	}

	public void Connect(XUiEvent_OnScrollEventHandler _onScrolled)
	{
		scrollBar.foregroundWidget = thumb.Sprite;
		thumb.Controller.OnScroll += _onScrolled;
		if (background != null)
		{
			scrollBar.backgroundWidget = background.Sprite;
			background.Controller.OnScroll += _onScrolled;
		}
	}
}
