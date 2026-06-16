using UnityEngine;

public class XUiTweenWidth : XUiTweenAbs
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TweenWidth tween;

	[PublicizedFrom(EAccessModifier.Private)]
	public int start = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public int end = 40;

	[XuiXmlAttribute("start", false)]
	public int Start
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
	public int End
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
	public void AttributePlayToEnd(int _value)
	{
		if (!(tween == null) && Mathf.Abs(tween.to - _value) >= 1)
		{
			base.Enabled = true;
			tween.tweenFactor = 0f;
			tween.SetStartToCurrentValue();
			End = _value;
			tween.PlayForward();
		}
	}

	public XUiTweenWidth(XUiView _targetView)
		: base(_targetView)
	{
	}

	public override void CreateTween(GameObject _uiGameObject)
	{
		tween = _uiGameObject.AddMissingComponent<TweenWidth>();
		tween.from = start;
		tween.to = end;
		setCommonTweenValues(tween);
	}
}
