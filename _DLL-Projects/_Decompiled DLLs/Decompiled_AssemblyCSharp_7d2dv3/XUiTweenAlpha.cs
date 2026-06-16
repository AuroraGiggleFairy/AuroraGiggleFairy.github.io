using UnityEngine;

public class XUiTweenAlpha : XUiTweenAbs
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TweenAlpha tween;

	[PublicizedFrom(EAccessModifier.Private)]
	public float start;

	[PublicizedFrom(EAccessModifier.Private)]
	public float end = 1f;

	[XuiXmlAttribute("start", false)]
	public float Start
	{
		get
		{
			return start;
		}
		set
		{
			start = Mathf.Clamp01(value);
			if (tween != null)
			{
				tween.from = start;
			}
		}
	}

	[XuiXmlAttribute("end", false)]
	public float End
	{
		get
		{
			return end;
		}
		set
		{
			end = Mathf.Clamp01(value);
			if (tween != null)
			{
				tween.to = end;
			}
		}
	}

	[XuiXmlAttribute("play_to", false)]
	public void AttributePlayToEnd(float _value)
	{
		if (!(tween == null) && !((double)Mathf.Abs(tween.to - _value) < 0.0001))
		{
			base.Enabled = true;
			tween.tweenFactor = 0f;
			tween.SetStartToCurrentValue();
			End = _value;
			tween.PlayForward();
		}
	}

	public XUiTweenAlpha(XUiView _targetView)
		: base(_targetView)
	{
	}

	public override void CreateTween(GameObject _uiGameObject)
	{
		tween = _uiGameObject.AddMissingComponent<TweenAlpha>();
		tween.from = start;
		tween.to = end;
		setCommonTweenValues(tween);
	}
}
