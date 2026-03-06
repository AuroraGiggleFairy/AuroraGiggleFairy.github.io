using UnityEngine.Scripting;

[Preserve]
public class XUiC_PoweredSpotlightWindowGroup : XUiC_PoweredGenericWindowGroup
{
	public static string ID = "powerspotlight";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeader;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CameraWindow cameraWindowPreview;

	public override void Init()
	{
		base.Init();
		XUiController childByType = GetChildByType<XUiC_WindowNonPagingHeader>();
		if (childByType != null)
		{
			nonPagingHeader = (XUiC_WindowNonPagingHeader)childByType;
		}
		childByType = GetChildById("windowPowerCameraControlPreview");
		if (childByType != null)
		{
			cameraWindowPreview = (XUiC_CameraWindow)childByType;
			cameraWindowPreview.Owner = this;
			cameraWindowPreview.UseEdgeDetection = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setupWindowTileEntities()
	{
		base.setupWindowTileEntities();
		if (cameraWindowPreview != null)
		{
			cameraWindowPreview.TileEntity = tileEntity;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (nonPagingHeader != null)
		{
			string text = "";
			_ = GameManager.Instance.World;
			text = tileEntity.GetChunk().GetBlock(tileEntity.localChunkPos).Block.GetLocalizedBlockName();
			nonPagingHeader.SetHeader(text);
		}
		base.TileEntity.Destroyed += TileEntity_Destroyed;
	}

	public override void OnClose()
	{
		base.TileEntity.Destroyed -= TileEntity_Destroyed;
		base.OnClose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TileEntity_Destroyed(ITileEntity te)
	{
		if (base.TileEntity == te)
		{
			if (GameManager.Instance != null)
			{
				base.xui.playerUI.windowManager.Close("powerspotlight");
				base.xui.playerUI.windowManager.Close("powercamera");
			}
		}
		else
		{
			te.Destroyed -= TileEntity_Destroyed;
		}
	}
}
