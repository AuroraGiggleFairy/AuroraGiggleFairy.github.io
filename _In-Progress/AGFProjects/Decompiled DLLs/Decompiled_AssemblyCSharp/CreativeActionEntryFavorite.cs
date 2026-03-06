using UnityEngine.Scripting;

[Preserve]
public class CreativeActionEntryFavorite : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ushort stackID;

	public CreativeActionEntryFavorite(XUiController _controller, int _stackID)
		: base(_controller, "lblContextActionFavorite", "server_favorite", GamepadShortCut.DPadRight)
	{
		stackID = (ushort)_stackID;
	}

	public override void OnActivated()
	{
		EntityPlayer entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
		if (entityPlayer.favoriteCreativeStacks.Contains(stackID))
		{
			entityPlayer.favoriteCreativeStacks.Remove(stackID);
		}
		else
		{
			entityPlayer.favoriteCreativeStacks.Add(stackID);
		}
		base.ItemController.WindowGroup.Controller.GetChildByType<XUiC_Creative2Window>()?.RefreshView();
	}
}
