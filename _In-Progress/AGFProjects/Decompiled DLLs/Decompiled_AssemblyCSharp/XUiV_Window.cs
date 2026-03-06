using System;
using UnityEngine;

public class XUiV_Window : XUiView
{
	[PublicizedFrom(EAccessModifier.Private)]
	public new string anchor;

	public UIPanel Panel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInStackpanel;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool cursorArea;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 oldTransformPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public float targetAlpha;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool fade = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fadeInTime = 0.05f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fadeTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delayToFadeTime = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delayTimer;

	public string Anchor
	{
		get
		{
			return anchor;
		}
		set
		{
			anchor = value;
			isDirty = true;
		}
	}

	public bool IsCursorArea => cursorArea;

	public bool IsOpen => isOpen;

	public bool IsInStackpanel => isInStackpanel;

	public float TargetAlpha
	{
		get
		{
			return targetAlpha;
		}
		set
		{
			if (value != targetAlpha)
			{
				targetAlpha = value;
				fadeTimer = 0f;
			}
		}
	}

	public XUiV_Window(string _id)
		: base(_id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UIPanel>();
	}

	public override void InitView()
	{
		base.InitView();
		base.Controller.OnVisiblity += UpdateVisibility;
		Panel = uiTransform.gameObject.GetComponent<UIPanel>();
		Panel.depth = base.Depth + 1;
		Panel.alpha = 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setRootNode()
	{
		if (rootNode == null)
		{
			Transform transform;
			if (Anchor == null)
			{
				transform = base.xui.transform;
			}
			else
			{
				transform = base.xui.transform.Find(Anchor);
				if (transform == null)
				{
					Log.Error("Specified window anchor \"" + Anchor + "\" not found for window \"" + base.ID + "\"");
					throw new Exception();
				}
			}
			rootNode = transform;
			base.setRootNode();
		}
		else if (uiTransform != null)
		{
			uiTransform.parent = rootNode;
			UITable component = rootNode.GetComponent<UITable>();
			if (component != null)
			{
				component.repositionNow = true;
			}
			IsVisible = true;
			uiTransform.gameObject.layer = 12;
			uiTransform.localScale = Vector3.one;
			uiTransform.localPosition = new Vector3(base.PaddedPosition.x, base.PaddedPosition.y, 0f);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if ((double)Time.timeScale < 0.01 || !fade)
		{
			delayTimer = delayToFadeTime + 1f;
			fadeTimer = fadeInTime;
		}
		if (delayTimer < delayToFadeTime)
		{
			delayTimer += _dt;
		}
		if (!(delayTimer < delayToFadeTime))
		{
			if (fadeTimer > fadeInTime)
			{
				fadeTimer = fadeInTime;
			}
			Panel.alpha = Mathf.Lerp(Panel.alpha, targetAlpha, fadeTimer / fadeInTime);
			fadeTimer += _dt;
			if (cursorArea && oldTransformPosition != base.UiTransform.position && IsVisible)
			{
				base.xui.UpdateWindowSoftCursorBounds(this);
				oldTransformPosition = base.UiTransform.position;
			}
		}
	}

	public void ForceVisible(float _alpha = -1f)
	{
		delayTimer = 100f;
		fadeTimer = 100f;
		if (_alpha >= 0f)
		{
			targetAlpha = _alpha;
		}
		Panel.alpha = targetAlpha;
	}

	public override void UpdateData()
	{
		if (uiTransform != null)
		{
			setRootNode();
		}
		base.UpdateData();
		Panel.SetDirty();
		isDirty = false;
	}

	public override bool ParseAttribute(string attribute, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(attribute, value, _parent);
		if (!flag)
		{
			switch (attribute)
			{
			case "anchor":
				Anchor = value;
				break;
			case "panel":
			{
				Transform transform = base.xui.transform.Find("StackPanels").transform;
				if (value != "")
				{
					rootNode = transform.FindInChilds(value);
					isInStackpanel = true;
				}
				break;
			}
			case "cursor_area":
				cursorArea = StringParsers.ParseBool(value);
				break;
			case "fade_delay":
				delayToFadeTime = StringParsers.ParseFloat(value);
				break;
			case "fade_time":
				fadeInTime = StringParsers.ParseFloat(value);
				break;
			case "fade_window":
				fade = StringParsers.ParseBool(value);
				break;
			default:
				return false;
			}
			return true;
		}
		return flag;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		Panel.alpha = 0f;
		targetAlpha = 1f;
		fadeTimer = 0f;
		delayTimer = 0f;
		if (cursorArea)
		{
			oldTransformPosition = Vector3.zero;
		}
		isOpen = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		Panel.alpha = 0f;
		targetAlpha = 0f;
		fadeTimer = fadeInTime;
		delayTimer = delayToFadeTime;
		isOpen = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateVisibility(XUiController _sender, bool _visible)
	{
		if (cursorArea)
		{
			if (_visible)
			{
				base.xui.UpdateWindowSoftCursorBounds(this);
			}
			else
			{
				base.xui.RemoveWindowFromSoftCursorBounds(this);
			}
		}
	}
}
