using UnityEngine.Scripting;

[Preserve]
public class XUiC_ShapeMaterialInfoWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture backgroundTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public Block blockData;

	[PublicizedFrom(EAccessModifier.Private)]
	public string materialName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string upgradeMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public string downgradeMaterial;

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("backgroundTexture");
		if (childById != null)
		{
			backgroundTexture = (XUiV_Texture)childById.ViewComponent;
			backgroundTexture.CreateMaterial();
		}
		XUiController childById2 = GetChildById("btnDowngrade");
		XUiController childById3 = GetChildById("btnUpgrade");
		if (childById2 != null)
		{
			childById2.OnPress += BtnDowngrade_OnPress;
		}
		if (childById3 != null)
		{
			childById3.OnPress += BtnUpgrade_OnPress;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnUpgrade_OnPress(XUiController _sender, int _mouseButton)
	{
		windowGroup.Controller.GetChildByType<XUiC_ShapesWindow>().UpgradeDowngradeShapes(blockData.UpgradeBlock);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDowngrade_OnPress(XUiController _sender, int _mouseButton)
	{
		windowGroup.Controller.GetChildByType<XUiC_ShapesWindow>().UpgradeDowngradeShapes(blockData.DowngradeBlock);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty && base.ViewComponent.IsVisible)
		{
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "hasmaterial":
			_value = (!string.IsNullOrEmpty(materialName)).ToString();
			return true;
		case "materialname":
			_value = materialName ?? "";
			return true;
		case "has_upgrade":
			_value = (!string.IsNullOrEmpty(upgradeMaterial)).ToString();
			return true;
		case "upgrade_material":
			_value = upgradeMaterial ?? "";
			return true;
		case "has_downgrade":
			_value = (!string.IsNullOrEmpty(downgradeMaterial)).ToString();
			return true;
		case "downgrade_material":
			_value = downgradeMaterial ?? "";
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public void SetShape(Block _newBlockData)
	{
		backgroundTexture.IsVisible = false;
		blockData = _newBlockData;
		materialName = null;
		upgradeMaterial = null;
		downgradeMaterial = null;
		if (_newBlockData != null && _newBlockData.GetAutoShapeType() != EAutoShapeType.None)
		{
			materialName = _newBlockData.blockMaterial.GetLocalizedMaterialName();
			if (!_newBlockData.UpgradeBlock.isair)
			{
				upgradeMaterial = _newBlockData.UpgradeBlock.Block.blockMaterial.GetLocalizedMaterialName();
			}
			if (!_newBlockData.DowngradeBlock.isair)
			{
				downgradeMaterial = _newBlockData.DowngradeBlock.Block.blockMaterial.GetLocalizedMaterialName();
			}
			if (backgroundTexture != null)
			{
				int sideTextureId = _newBlockData.GetSideTextureId(new BlockValue((uint)_newBlockData.blockID), BlockFace.Top, 0);
				if (sideTextureId != 0)
				{
					MeshDescription meshDescription = MeshDescription.meshes[0];
					UVRectTiling uVRectTiling = meshDescription.textureAtlas.uvMapping[sideTextureId];
					backgroundTexture.Texture = meshDescription.textureAtlas.diffuseTexture;
					if (meshDescription.bTextureArray)
					{
						backgroundTexture.Material.SetTexture("_BumpMap", meshDescription.textureAtlas.normalTexture);
						backgroundTexture.Material.SetFloat("_Index", uVRectTiling.index);
						backgroundTexture.Material.SetFloat("_Size", uVRectTiling.blockW);
					}
					else
					{
						backgroundTexture.UVRect = uVRectTiling.uv;
					}
					backgroundTexture.IsVisible = true;
				}
			}
		}
		RefreshBindings();
	}
}
