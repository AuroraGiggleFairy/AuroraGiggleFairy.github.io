using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CustomCharacterWindow : XUiController
{
	public string lockedSprite = "ui_game_symbol_lock";

	public string unlockedSprite = "ui_game_symbol_lock";

	public Color lockedColor;

	public Color unlockedColor;

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			switch (name)
			{
			case "locked_sprite":
				lockedSprite = value;
				break;
			case "unlocked_sprite":
				unlockedSprite = value;
				break;
			case "locked_color":
				lockedColor = StringParsers.ParseColor32(value);
				break;
			case "unlocked_color":
				unlockedColor = StringParsers.ParseColor32(value);
				break;
			default:
				return false;
			}
			return true;
		}
		return flag;
	}
}
