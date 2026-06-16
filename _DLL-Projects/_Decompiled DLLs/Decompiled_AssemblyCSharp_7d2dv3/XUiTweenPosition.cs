using UnityEngine;

public class XUiTweenPosition : XUiTweenAbs
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TweenPosition tween;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 start = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 end = new Vector3(0f, 8f, 0f);

	[XuiXmlAttribute("start", false)]
	public Vector3 Start
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
	public Vector3 End
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
	public void AttributePlayToEnd(Vector3 _value)
	{
		if (!(tween == null) && !((double)(tween.to - _value).sqrMagnitude < 0.1))
		{
			base.Enabled = true;
			tween.tweenFactor = 0f;
			tween.SetStartToCurrentValue();
			End = _value;
			tween.PlayForward();
		}
	}

	public XUiTweenPosition(XUiView _targetView)
		: base(_targetView)
	{
	}

	public override void CreateTween(GameObject _uiGameObject)
	{
		tween = _uiGameObject.AddMissingComponent<TweenPosition>();
		tween.from = start;
		tween.to = end;
		setCommonTweenValues(tween);
	}
}
