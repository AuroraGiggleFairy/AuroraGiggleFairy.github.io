using System;
using UnityEngine;

public class UpdateLightOnChunkMesh : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTimeLightBrightnessChecked;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float cTimeBrightnessUpdate = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public byte lastSunLight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public byte lastBlockLight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Color lastSunMoonLight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshFilter meshFilter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshRenderer meshRenderer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public VoxelMesh chunkMesh;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		if (GameManager.IsDedicatedServer)
		{
			base.enabled = false;
		}
		else
		{
			Reset();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		if (!GameManager.IsDedicatedServer)
		{
			checkLight();
		}
	}

	public void SetChunkMesh(VoxelMesh _chunkMesh)
	{
		chunkMesh = _chunkMesh;
	}

	public void Reset()
	{
		chunkMesh = null;
		lastTimeLightBrightnessChecked = 0f;
		lastSunLight = (lastBlockLight = byte.MaxValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (!GameManager.IsDedicatedServer && !(Time.time - lastTimeLightBrightnessChecked < cTimeBrightnessUpdate))
		{
			lastTimeLightBrightnessChecked = Time.time;
			checkLight();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkLight()
	{
		if (chunkMesh == null)
		{
			return;
		}
		GameManager instance = GameManager.Instance;
		if (!instance || !instance.gameStateManager.IsGameStarted())
		{
			return;
		}
		World world = instance.World;
		if (world == null)
		{
			return;
		}
		if (meshFilter == null)
		{
			meshFilter = base.transform.GetComponent<MeshFilter>();
		}
		if (meshRenderer == null)
		{
			meshRenderer = base.transform.GetComponent<MeshRenderer>();
		}
		if (!(meshFilter == null) && !(meshRenderer == null))
		{
			world.GetSunAndBlockColors(World.worldToBlockPos(base.transform.position + Origin.position), out var sunLight, out var blockLight);
			world.GetSunAndBlockColors(World.worldToBlockPos(base.transform.position + Vector3.up + Origin.position), out var sunLight2, out var blockLight2);
			byte b = Utils.FastMax(sunLight, sunLight2);
			byte b2 = Utils.FastMax(blockLight, blockLight2);
			Color value = (world.IsDaytime() ? world.m_WorldEnvironment.GetSunLightColor() : world.m_WorldEnvironment.GetMoonLightColor());
			value.a = 1f;
			if (b != lastSunLight || b2 != lastBlockLight || !value.Equals(lastSunMoonLight))
			{
				lastSunLight = b;
				lastBlockLight = b2;
				lastSunMoonLight = value;
				meshFilter.mesh.colors = chunkMesh.UpdateColors(b, b2);
				meshRenderer.material.SetColor("_SunMoonlight", value);
			}
		}
	}
}
