using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_GameEventMenu : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_GameEventsList gameEventsList;

	[PublicizedFrom(EAccessModifier.Private)]
	public string categoryDisplay = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> cbxTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, EntityPlayer> PlayerList = new Dictionary<string, EntityPlayer>();

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		categoryList = (XUiC_CategoryList)GetChildById("categories");
		categoryList.CategoryChanged += CategoryList_CategoryChanged;
		gameEventsList = (XUiC_GameEventsList)GetChildById("gameevents");
		gameEventsList.SelectionChanged += EntitiesList_SelectionChanged;
		cbxTarget = (XUiC_ComboBoxList<string>)GetChildById("cbxTarget");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		gameEventsList.Category = _categoryEntry.CategoryName;
		categoryDisplay = _categoryEntry.CategoryDisplayName;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EntitiesList_SelectionChanged(XUiC_ListEntry<XUiC_GameEventsList.GameEventEntry> _previousEntry, XUiC_ListEntry<XUiC_GameEventsList.GameEventEntry> _newEntry)
	{
		if (_newEntry != null)
		{
			gameEventsList.ClearSelection();
			if (_newEntry.GetEntry() != null)
			{
				XUiC_GameEventsList.GameEventEntry entry = _newEntry.GetEntry();
				BtnSpawns_OnPress(entry.name);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSpawns_OnPress(string _name)
	{
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		EntityPlayer entityPlayer2 = PlayerList[cbxTarget.Value];
		if (entityPlayer2 == entityPlayer || !entityPlayer2.IsAdmin)
		{
			GameEventManager.Current.HandleAction(_name, entityPlayer, entityPlayer2, twitchActivated: false);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		categoryList.SetupCategoriesBasedOnGameEventCategories(GameEventManager.Current.CategoryList);
		categoryList.SetCategoryToFirst();
		cbxTarget.Elements.Clear();
		PlayerList.Clear();
		int selectedIndex = 0;
		for (int i = 0; i < GameManager.Instance.World.Players.list.Count; i++)
		{
			EntityPlayer entityPlayer = GameManager.Instance.World.Players.list[i];
			cbxTarget.Elements.Add(entityPlayer.EntityName);
			PlayerList.Add(entityPlayer.EntityName, entityPlayer);
			if (entityPlayer is EntityPlayerLocal)
			{
				selectedIndex = i;
			}
		}
		cbxTarget.SelectedIndex = selectedIndex;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "headertitle")
		{
			if (categoryDisplay == "")
			{
				value = "Game Events";
			}
			else
			{
				value = $"Game Events - {categoryDisplay}";
			}
			return true;
		}
		return false;
	}
}
