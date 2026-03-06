using UnityEngine;

public class XUiV_Widget : XUiView
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public UIWidget widget;

	public XUiV_Widget(string _id)
		: base(_id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UIWidget>();
	}

	public override void InitView()
	{
		base.InitView();
		widget = uiTransform.gameObject.GetComponent<UIWidget>();
		widget.depth = depth;
		widget.pivot = pivot;
		parseAnchors(widget);
		RefreshBoxCollider();
	}

	public override void UpdateData()
	{
		if (isDirty)
		{
			widget.pivot = pivot;
			parseAnchors(widget);
			RefreshBoxCollider();
		}
		base.UpdateData();
	}
}
