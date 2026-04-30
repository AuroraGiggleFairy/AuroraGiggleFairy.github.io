using UnityEngine.Scripting;

[Preserve]
public class BaseItemActionEntry
{
	public enum GamepadShortCut
	{
		DPadUp,
		DPadLeft,
		DPadRight,
		DPadDown,
		None,
		Max
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string ActionName { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string IconName { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool Enabled { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string SoundName { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string DisabledSound { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiController ItemController { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemActionEntry ParentItem { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemActionList ParentActionList { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public GamepadShortCut ShortCut { get; set; }

	public BaseItemActionEntry(XUiController itemController, string actionName, string spriteName, GamepadShortCut shortcut = GamepadShortCut.None, string soundName = "crafting/craft_click_craft", string disabledSoundName = "ui/ui_denied")
	{
		ItemController = itemController;
		ActionName = Localization.Get(actionName);
		IconName = spriteName;
		SoundName = soundName;
		DisabledSound = disabledSoundName;
		Enabled = true;
		ShortCut = shortcut;
	}

	public virtual void RefreshEnabled()
	{
		if (ItemController is XUiC_ItemStack)
		{
			Enabled = !((XUiC_ItemStack)ItemController).StackLock;
		}
	}

	public virtual void OnActivated()
	{
	}

	public virtual void OnDisabledActivate()
	{
	}

	public virtual void OnTimerCompleted()
	{
	}

	public virtual void DisableEvents()
	{
	}
}
