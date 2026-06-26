using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapSubPopupList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] sprites = new string[10] { "ui_game_symbol_map_cave", "ui_game_symbol_map_cabin", "ui_game_symbol_map_campsite", "ui_game_symbol_map_city", "ui_game_symbol_map_fortress", "ui_game_symbol_map_civil", "ui_game_symbol_map_house", "ui_game_symbol_map_town", "ui_game_symbol_map_trader", "ui_game_symbol_x" };

	public override void Init()
	{
		base.Init();
		for (int i = 0; i < children.Count; i++)
		{
			XUiController xUiController = children[i].Children[0];
			if (xUiController is XUiC_MapSubPopupEntry)
			{
				XUiC_MapSubPopupEntry obj = (XUiC_MapSubPopupEntry)xUiController;
				obj.SetIndex(i);
				obj.SetSpriteName(sprites[i % sprites.Length]);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void ResetList()
	{
		for (int i = 0; i < children.Count; i++)
		{
			XUiController xUiController = children[i].Children[0];
			if (xUiController is XUiC_MapSubPopupEntry)
			{
				((XUiC_MapSubPopupEntry)xUiController).Reset();
			}
		}
		children[0].SelectCursorElement(_withDelay: true);
	}
}
