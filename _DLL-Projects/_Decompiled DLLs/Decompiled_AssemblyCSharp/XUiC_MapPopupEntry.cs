using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapPopupEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		XUiV_Sprite xUiV_Sprite = (XUiV_Sprite)GetChildById("background").ViewComponent;
		if (xUiV_Sprite != null)
		{
			xUiV_Sprite.Color = (_isOver ? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue) : new Color32(64, 64, 64, byte.MaxValue));
			xUiV_Sprite.SpriteName = (_isOver ? "ui_game_select_row" : "menu_empty");
		}
		base.OnHovered(_isOver);
	}
}
