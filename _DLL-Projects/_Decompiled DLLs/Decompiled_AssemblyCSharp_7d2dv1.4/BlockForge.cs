using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockForge : BlockWorkstation
{
	public BlockForge()
	{
		CraftingParticleLightIntensity = 1.6f;
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		MaterialUpdate(_world, _blockPos, _blockValue);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void checkParticles(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.checkParticles(_world, _clrIdx, _blockPos, _blockValue);
		if (!_blockValue.ischild)
		{
			MaterialUpdate(_world, _blockPos, _blockValue);
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return Localization.Get("useForge");
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
