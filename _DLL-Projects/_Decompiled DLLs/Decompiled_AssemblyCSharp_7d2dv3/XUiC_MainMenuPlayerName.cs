using UnityEngine.Scripting;

[Preserve]
public class XUiC_MainMenuPlayerName : XUiController
{
	[XuiXmlBinding("name")]
	public string PlayerName => GamePrefs.GetString(EnumGamePrefs.PlayerName) ?? "";
}
