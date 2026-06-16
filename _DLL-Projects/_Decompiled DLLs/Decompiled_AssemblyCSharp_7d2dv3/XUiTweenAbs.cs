using System;
using UnityEngine;

public abstract class XUiTweenAbs : IXUiElement
{
	public enum ETweenType
	{
		Alpha,
		Color,
		Fill,
		Height,
		Position,
		Rotation,
		Scale,
		Width
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiView targetView;

	[PublicizedFrom(EAccessModifier.Private)]
	public UITweener tweenGeneric;

	[PublicizedFrom(EAccessModifier.Private)]
	public UITweener.Style repeatStyle;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float duration = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UITweener.Method method;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool enabled;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimationCurve curve = parseCurve("Linear");

	[XuiXmlAttribute("repeat", false)]
	public UITweener.Style RepeatStyle
	{
		get
		{
			return repeatStyle;
		}
		set
		{
			if (repeatStyle != value)
			{
				repeatStyle = value;
				if (tweenGeneric != null)
				{
					tweenGeneric.style = value;
				}
			}
		}
	}

	[XuiXmlAttribute("duration", false)]
	public float Duration
	{
		get
		{
			return duration;
		}
		set
		{
			if (!Mathf.Approximately(duration, value))
			{
				duration = value;
				if (tweenGeneric != null)
				{
					tweenGeneric.duration = value;
				}
			}
		}
	}

	[XuiXmlAttribute("method", false)]
	public UITweener.Method Method
	{
		get
		{
			return method;
		}
		set
		{
			if (method != value)
			{
				method = value;
				if (tweenGeneric != null)
				{
					tweenGeneric.method = value;
				}
			}
		}
	}

	[XuiXmlAttribute("enabled", false)]
	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			if (enabled != value)
			{
				enabled = value;
				if (tweenGeneric != null)
				{
					tweenGeneric.enabled = value;
				}
			}
		}
	}

	public XUiController Controller => targetView.Controller;

	public static AnimationCurve SecondHalfLinear
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(0.5f, 0f, 0f, 2f), new Keyframe(1f, 1f, 2f, 2f));
		}
	}

	[XuiXmlAttribute("curve", false)]
	public void AttributeCurve(string _curveString)
	{
		curve = parseCurve(_curveString);
		if (tweenGeneric != null)
		{
			tweenGeneric.animationCurve = curve;
		}
	}

	[XuiXmlAttribute("play_forward", false)]
	public void AttributePlayForward(bool _play)
	{
		if (_play)
		{
			Enabled = true;
			if (tweenGeneric != null)
			{
				tweenGeneric.PlayForward();
			}
		}
	}

	[XuiXmlAttribute("play_reverse", false)]
	public void AttributePlayReverse(bool _play)
	{
		if (_play)
		{
			Enabled = true;
			if (tweenGeneric != null)
			{
				tweenGeneric.PlayReverse();
			}
		}
	}

	[XuiXmlAttribute("set_to_start", false)]
	public void AttributeSetToStart(bool _set)
	{
		if (_set)
		{
			Enabled = false;
			if (tweenGeneric != null)
			{
				tweenGeneric.tweenFactor = 0f;
				tweenGeneric.Sample(0f, isFinished: false);
			}
		}
	}

	[XuiXmlAttribute("set_to_end", false)]
	public void AttributeSetToEnd(bool _set)
	{
		if (_set)
		{
			Enabled = false;
			if (tweenGeneric != null)
			{
				tweenGeneric.tweenFactor = 1f;
				tweenGeneric.Sample(1f, isFinished: false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiTweenAbs(XUiView _targetView)
	{
		targetView = _targetView;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void setCommonTweenValues(UITweener _tween)
	{
		tweenGeneric = _tween;
		_tween.style = repeatStyle;
		_tween.duration = duration;
		_tween.method = method;
		_tween.animationCurve = curve;
		_tween.enabled = enabled;
	}

	public abstract void CreateTween(GameObject _uiGameObject);

	public string GetXuiHierarchy()
	{
		return XUiUtils.GetXuiHierarchy(targetView.Controller);
	}

	public void ParseInitialAttributeValue(string _attribute, string _value)
	{
		if (_value.Contains("{"))
		{
			new BindingInfoNcalc(this, _attribute, _value);
		}
		else if (!ParsingMethodCache.Instance.TryParseDirect(this, _attribute, _value))
		{
			Log.Error("[XUi] Unknown Tween attribute '" + _attribute + "', hierarchy: " + GetXuiHierarchy());
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static AnimationCurve parseCurve(string _curveString)
	{
		return _curveString switch
		{
			"EaseInOut" => AnimationCurve.EaseInOut(0f, 0f, 1f, 1f), 
			"Linear" => AnimationCurve.Linear(0f, 0f, 1f, 1f), 
			"SecondHalfLinear" => SecondHalfLinear, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
