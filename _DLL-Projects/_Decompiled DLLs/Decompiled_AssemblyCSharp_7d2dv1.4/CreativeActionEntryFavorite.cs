using UnityEngine.Scripting;

[Preserve]
public class CreativeActionEntryFavorite : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ushort StackID;

	public CreativeActionEntryFavorite(XUiController controller, int stackID)
		: base(controller, "lblContextActionFavorite", "server_favorite", GamepadShortCut.DPadRight)
	{
		StackID = (ushort)stackID;
	}

	public override void OnActivated()
	{
		EntityPlayer entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
		if (entityPlayer.favoriteCreativeStacks.Contains(StackID))
		{
			entityPlayer.favoriteCreativeStacks.Remove(StackID);
		}
		else
		{
			entityPlayer.favoriteCreativeStacks.Add(StackID);
		}
		base.ItemController.WindowGroup.Controller.GetChildByType<XUiC_Creative2Window>()?.RefreshView();
	}
}
