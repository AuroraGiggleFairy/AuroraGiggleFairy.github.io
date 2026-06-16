using UnityEngine;

public class XUiTweenColor : XUiTweenAbs
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TweenColor tween;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color start = Color.black;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color end = Color.white;

	[XuiXmlAttribute("start", false)]
	public Color Start
	{
		get
		{
			return start;
		}
		set
		{
			start = value;
			if (tween != null)
			{
				tween.from = start;
			}
		}
	}

	[XuiXmlAttribute("end", false)]
	public Color End
	{
		get
		{
			return end;
		}
		set
		{
			end = value;
			if (tween != null)
			{
				tween.to = end;
			}
		}
	}

	[XuiXmlAttribute("play_to", false)]
	public void AttributePlayToEnd(Color _value)
	{
		if (!(tween == null))
		{
			Color to = tween.to;
			if (!((double)(Mathf.Abs(to.r - _value.r) + Mathf.Abs(to.g - _value.g) + Mathf.Abs(to.b - _value.b) + Mathf.Abs(to.a - _value.a)) < 0.002))
			{
				base.Enabled = true;
				tween.tweenFactor = 0f;
				tween.SetStartToCurrentValue();
				End = _value;
				tween.PlayForward();
			}
		}
	}

	public XUiTweenColor(XUiView _targetView)
		: base(_targetView)
	{
	}

	public override void CreateTween(GameObject _uiGameObject)
	{
		tween = _uiGameObject.AddMissingComponent<TweenColor>();
		tween.from = start;
		tween.to = end;
		setCommonTweenValues(tween);
	}
}
