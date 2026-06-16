using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockForge : BlockWorkstation
{
	public BlockForge()
	{
		CraftingParticleLightIntensity = 1.6f;
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
		MaterialUpdate(_world, _blockPos, _blockValue);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void checkParticles(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.checkParticles(_world, _blockPos, _blockValue);
		if (!_blockValue.ischild)
		{
			MaterialUpdate(_world, _blockPos, _blockValue);
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (XUiM_Recipes.DisableSmelter)
		{
			TileEntityWorkstation tileEntityWorkstation = (TileEntityWorkstation)_world.GetTileEntity(_blockPos);
			if (tileEntityWorkstation.InputSlotCount > 0 && !tileEntityWorkstation.InputIsEmpty())
			{
				return Localization.Get("useForgeMaterials");
			}
		}
		return _blockValue.Block.GetLocalizedBlockName() + "\n" + Localization.Get("useWorkstation");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MaterialUpdate(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		Chunk chunk = (Chunk)_world.GetChunkFromWorldPos(_blockPos);
		if (chunk == null)
		{
			return;
		}
		BlockEntityData blockEntity = chunk.GetBlockEntity(_blockPos);
		if (blockEntity == null || !blockEntity.bHasTransform)
		{
			return;
		}
		Renderer[] componentsInChildren = blockEntity.transform.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
		Renderer[] array = componentsInChildren;
		if (array.Length == 0)
		{
			return;
		}
		Material material = array[0].material;
		if ((bool)material)
		{
			float value = ((_blockValue.meta != 0) ? 20 : 0);
			material.SetFloat("_EmissionMultiply", value);
			for (int i = 1; i < array.Length; i++)
			{
				array[i].material = material;
			}
		}
	}
}
