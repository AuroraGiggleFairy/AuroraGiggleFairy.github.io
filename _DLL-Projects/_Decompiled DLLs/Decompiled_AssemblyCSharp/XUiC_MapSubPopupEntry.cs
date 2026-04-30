using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapSubPopupEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int index;

	[PublicizedFrom(EAccessModifier.Private)]
	public string spriteName;

	public override void Init()
	{
		base.Init();
		base.OnPress += onPressed;
		base.OnVisiblity += XUiC_MapSubPopupEntry_OnVisiblity;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiC_MapSubPopupEntry_OnVisiblity(XUiController _sender, bool _visible)
	{
		select(_bSelect: false);
	}

	public void SetIndex(int _idx)
	{
		index = _idx;
	}

	public void SetSpriteName(string _s)
	{
		spriteName = _s;
		for (int i = 0; i < base.Parent.Children.Count; i++)
		{
			XUiView xUiView = base.Parent.Children[i].ViewComponent;
			if (xUiView.ID.EqualsCaseInsensitive("icon"))
			{
				((XUiV_Sprite)xUiView).SpriteName = _s;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		select(_isOver);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onPressed(XUiController _sender, int _mouseButton)
	{
		select(_bSelect: true);
		((XUiC_MapArea)base.xui.GetWindow("mapArea").Controller).OnWaypointEntryChosen(spriteName);
		XUiC_MapEnterWaypoint childByType = base.xui.GetWindow("mapAreaEnterWaypointName").Controller.GetChildByType<XUiC_MapEnterWaypoint>();
		int num = index / 10;
		int num2 = index % 10;
		Vector2i position = base.xui.GetWindow("mapAreaChooseWaypoint").Position + new Vector2i(52 * (num + 1), num2 * -43);
		childByType.Show(position);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void select(bool _bSelect)
	{
		XUiV_Sprite xUiV_Sprite = (XUiV_Sprite)viewComponent;
		if (xUiV_Sprite != null)
		{
			xUiV_Sprite.Color = (_bSelect ? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue) : new Color32(64, 64, 64, byte.MaxValue));
			xUiV_Sprite.SpriteName = (_bSelect ? "ui_game_select_row" : "menu_empty");
		}
	}

	public void Reset()
	{
		select(_bSelect: false);
	}
}
