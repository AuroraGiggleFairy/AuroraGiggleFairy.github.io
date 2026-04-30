using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockParticle : Block
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string particleName;

	[PublicizedFrom(EAccessModifier.Private)]
	public int particleId;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Light> dediLights;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 offset;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform hierarchyParentT = new GameObject("BlockParticleLights").transform;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<int, List<Light>> particleLights;

	public BlockParticle()
	{
		base.IsNotifyOnLoadUnload = true;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("ParticleName"))
		{
			particleName = base.Properties.Values["ParticleName"];
			ParticleEffect.LoadAsset(particleName);
		}
		if (base.Properties.Values.ContainsKey("ParticleOffset"))
		{
			offset = StringParsers.ParseVector3(base.Properties.Values["ParticleOffset"]);
		}
		if (!GameManager.IsDedicatedServer || particleName == null || particleName.Length <= 0)
		{
			return;
		}
		if (particleLights == null)
		{
			particleLights = new Dictionary<int, List<Light>>();
		}
		particleId = ParticleEffect.ToId(particleName);
		if (particleLights.ContainsKey(particleId))
		{
			return;
		}
		particleLights.Add(particleId, new List<Light>());
		Transform dynamicTransform = ParticleEffect.GetDynamicTransform(particleId);
		if (!(dynamicTransform != null))
		{
			return;
		}
		Light[] componentsInChildren = dynamicTransform.GetComponentsInChildren<Light>();
		if (componentsInChildren != null)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				particleLights[particleId].Add(componentsInChildren[i]);
			}
		}
	}

	public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		removeParticles(_world, _blockPos.x, _blockPos.y, _blockPos.z, _blockValue);
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!_chunk.NeedsDecoration)
		{
			checkParticles(_world, _chunk.ClrIdx, _blockPos, _blockValue);
		}
	}

	public override void OnNeighborBlockChange(WorldBase world, int _clrIdx, Vector3i _myBlockPos, BlockValue _myBlockValue, Vector3i _blockPosThatChanged, BlockValue _newNeighborBlockValue, BlockValue _oldNeighborBlockValue)
	{
		Transform blockParticleEffect;
		if (_myBlockPos == _blockPosThatChanged + Vector3i.up && _newNeighborBlockValue.Block.shape.IsTerrain() && _myBlockValue.Block.IsTerrainDecoration && particleName != null && (blockParticleEffect = world.GetGameManager().GetBlockParticleEffect(_myBlockPos)) != null)
		{
			float num = 0f;
			if (_myBlockPos.y > 0)
			{
				sbyte density = world.GetDensity(_clrIdx, _myBlockPos.x, _myBlockPos.y, _myBlockPos.z);
				sbyte density2 = world.GetDensity(_clrIdx, _myBlockPos.x, _myBlockPos.y - 1, _myBlockPos.z);
				num = MarchingCubes.GetDecorationOffsetY(density, density2);
			}
			blockParticleEffect.localPosition = new Vector3(_myBlockPos.x, (float)_myBlockPos.y + num, _myBlockPos.z) + getParticleOffset(_myBlockValue);
		}
	}

	public override void OnBlockValueChanged(WorldBase world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		Transform blockParticleEffect;
		if (_oldBlockValue.rotation != _newBlockValue.rotation && particleName != null && (blockParticleEffect = world.GetGameManager().GetBlockParticleEffect(_blockPos)) != null)
		{
			Vector3 particleOffset = getParticleOffset(_oldBlockValue);
			Vector3 particleOffset2 = getParticleOffset(_newBlockValue);
			blockParticleEffect.localPosition -= particleOffset;
			blockParticleEffect.localPosition += particleOffset2;
			blockParticleEffect.localRotation = shape.GetRotation(_newBlockValue);
		}
	}

	public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		if (particleName == null)
		{
			return;
		}
		if (GameManager.IsDedicatedServer && particleLights.ContainsKey(particleId))
		{
			List<Light> list = particleLights[particleId];
			if (list.Count > 0)
			{
				Vector3 vector = default(Vector3);
				vector.x = _blockPos.x;
				vector.y = _blockPos.y;
				vector.z = _blockPos.z;
				dediLights = new List<Light>();
				for (int i = 0; i < list.Count; i++)
				{
					Light light = Object.Instantiate(list[i]);
					Transform transform = light.transform;
					transform.position = vector + getParticleOffset(_blockValue) - Origin.position;
					transform.parent = hierarchyParentT;
					dediLights.Add(light);
					LightManager.RegisterLight(light);
				}
			}
		}
		checkParticles(_world, _clrIdx, _blockPos, _blockValue);
	}

	public override void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
		removeParticles(_world, _blockPos.x, _blockPos.y, _blockPos.z, _blockValue);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector3 getParticleOffset(BlockValue _blockValue)
	{
		return shape.GetRotation(_blockValue) * (offset - new Vector3(0.5f, 0.5f, 0.5f)) + new Vector3(0.5f, 0.5f, 0.5f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void checkParticles(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (particleName != null && !_world.GetGameManager().HasBlockParticleEffect(_blockPos))
		{
			addParticles(_world, _clrIdx, _blockPos.x, _blockPos.y, _blockPos.z, _blockValue);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void addParticles(WorldBase _world, int _clrIdx, int _x, int _y, int _z, BlockValue _blockValue)
	{
		if (particleName != null && !(particleName == ""))
		{
			float num = 0f;
			if (_y > 0 && _blockValue.Block.IsTerrainDecoration && _world.GetBlock(_x, _y - 1, _z).Block.shape.IsTerrain())
			{
				sbyte density = _world.GetDensity(_clrIdx, _x, _y, _z);
				sbyte density2 = _world.GetDensity(_clrIdx, _x, _y - 1, _z);
				num = MarchingCubes.GetDecorationOffsetY(density, density2);
			}
			_world.GetGameManager().SpawnBlockParticleEffect(new Vector3i(_x, _y, _z), new ParticleEffect(particleName, new Vector3(_x, (float)_y + num, _z) + getParticleOffset(_blockValue), shape.GetRotation(_blockValue), 1f, Color.white));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void removeParticles(WorldBase _world, int _x, int _y, int _z, BlockValue _blockValue)
	{
		if (GameManager.IsDedicatedServer && dediLights != null)
		{
			for (int i = 0; i < dediLights.Count; i++)
			{
				LightManager.UnRegisterLight(dediLights[i].transform.position + Origin.position, dediLights[i].range);
				Object.Destroy(dediLights[i]);
			}
			dediLights.Clear();
		}
		_world.GetGameManager().RemoveBlockParticleEffect(new Vector3i(_x, _y, _z));
	}
}
