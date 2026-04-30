public class WorkstationData
{
	public string WorkstationName;

	public string WorkstationIcon;

	public string CraftIcon;

	public string CraftActionName = "";

	public string WorkstationWindow = "";

	public string OpenSound;

	public string CloseSound;

	public string CraftSound;

	public string CraftCompleteSound;

	public WorkstationData(string blockName, DynamicProperties properties)
	{
		if (properties.Values.ContainsKey("WorkstationName"))
		{
			WorkstationName = properties.Values["WorkstationName"];
		}
		else
		{
			WorkstationName = blockName;
		}
		if (properties.Values.ContainsKey("WorkstationIcon"))
		{
			WorkstationIcon = properties.Values["WorkstationIcon"];
		}
		else
		{
			WorkstationIcon = "ui_game_symbol_hammer";
		}
		if (properties.Values.ContainsKey("CraftActionName"))
		{
			CraftActionName = Localization.Get(properties.Values["CraftActionName"]);
		}
		else
		{
			CraftActionName = Localization.Get("lblContextActionCraft");
		}
		if (properties.Values.ContainsKey("CraftIcon"))
		{
			CraftIcon = properties.Values["CraftIcon"];
		}
		else
		{
			CraftIcon = "ui_game_symbol_hammer";
		}
		if (properties.Values.ContainsKey("OpenSound"))
		{
			OpenSound = properties.Values["OpenSound"];
		}
		else
		{
			OpenSound = "open_workbench";
		}
		if (properties.Values.ContainsKey("CloseSound"))
		{
			CloseSound = properties.Values["CloseSound"];
		}
		else
		{
			CloseSound = "close_workbench";
		}
		if (properties.Values.ContainsKey("CraftSound"))
		{
			CraftSound = properties.Values["CraftSound"];
		}
		else
		{
			CraftSound = "craft_click_craft";
		}
		if (properties.Values.ContainsKey("CraftCompleteSound"))
		{
			CraftCompleteSound = properties.Values["CraftCompleteSound"];
		}
		else
		{
			CraftCompleteSound = "craft_complete_item";
		}
		if (properties.Values.ContainsKey("WorkstationWindow"))
		{
			WorkstationWindow = properties.Values["WorkstationWindow"];
		}
		else
		{
			WorkstationWindow = "";
		}
	}
}
