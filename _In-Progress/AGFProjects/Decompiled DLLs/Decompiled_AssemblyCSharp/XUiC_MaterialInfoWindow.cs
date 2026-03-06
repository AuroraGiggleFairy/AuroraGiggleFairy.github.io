using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MaterialInfoWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public BlockTextureData TextureData;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture textMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ushort> paintcostFormatter = new CachedStringFormatter<ushort>([PublicizedFrom(EAccessModifier.Internal)] (ushort _i) => _i.ToString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ushort> perklevelFormatter = new CachedStringFormatter<ushort>([PublicizedFrom(EAccessModifier.Internal)] (ushort _i) => _i.ToString());

	public override void Init()
	{
		base.Init();
		textMaterial = GetChildById("textMaterial").ViewComponent as XUiV_Texture;
		textMaterial.CreateMaterial();
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
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "grouptitle":
			value = ((TextureData != null) ? Localization.Get("xuiMaterialGroup") : "");
			return true;
		case "group":
			value = ((TextureData != null) ? Localization.Get(TextureData.Group) : "");
			return true;
		case "hasperklock":
			value = ((TextureData != null) ? (TextureData.LockedByPerk != "").ToString() : "false");
			return true;
		case "materialname":
			value = ((TextureData != null) ? Localization.Get(TextureData.Name) : "");
			return true;
		case "paintcosttitle":
			value = ((TextureData != null) ? Localization.Get("xuiPaintCost") : "");
			return true;
		case "paintcost":
			value = ((TextureData != null) ? paintcostFormatter.Format(TextureData.PaintCost) : "");
			return true;
		case "perk":
			value = "";
			if (TextureData != null && TextureData.LockedByPerk != "")
			{
				ProgressionValue progressionValue = base.xui.playerUI.entityPlayer.Progression.GetProgressionValue(TextureData.LockedByPerk);
				value = Localization.Get(progressionValue.ProgressionClass.NameKey);
			}
			return true;
		case "perklevel":
			value = ((TextureData != null) ? perklevelFormatter.Format(TextureData.RequiredLevel) : "");
			return true;
		case "requiredtitle":
			value = ((TextureData != null) ? Localization.Get("xuiRequired") : "");
			return true;
		case "paintunit":
			value = ((TextureData != null) ? Localization.Get("xuiPaintUnit") : "");
			return true;
		default:
			return false;
		}
	}

	public void SetMaterial(BlockTextureData newTexture)
	{
		TextureData = newTexture;
		textMaterial.IsVisible = false;
		if (TextureData != null)
		{
			textMaterial.IsVisible = true;
			MeshDescription meshDescription = MeshDescription.meshes[0];
			int textureID = TextureData.TextureID;
			Rect uVRect = ((textureID != 0) ? meshDescription.textureAtlas.uvMapping[textureID].uv : WorldConstants.uvRectZero);
			textMaterial.Texture = meshDescription.textureAtlas.diffuseTexture;
			if (meshDescription.bTextureArray)
			{
				textMaterial.Material.SetTexture("_BumpMap", meshDescription.textureAtlas.normalTexture);
				textMaterial.Material.SetFloat("_Index", meshDescription.textureAtlas.uvMapping[textureID].index);
				textMaterial.Material.SetFloat("_Size", meshDescription.textureAtlas.uvMapping[textureID].blockW);
			}
			else
			{
				textMaterial.UVRect = uVRect;
			}
		}
		RefreshBindings();
	}
}
