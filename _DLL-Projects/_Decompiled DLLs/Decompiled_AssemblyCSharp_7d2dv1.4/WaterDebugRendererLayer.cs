using System;
using Unity.Mathematics;
using UnityEngine;

public class WaterDebugRendererLayer : IMemoryPoolableObject
{
	public const int dimX = 16;

	public const int dimY = 16;

	public const int dimZ = 16;

	public const int elementsPerLayer = 4096;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float SCALE_CUTOFF = 0.01f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float RENDER_SCALE = 0.9f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float4 waterColor = new float4(0f, 0f, 1f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float4 overfullColor = new float4(1f, 0f, 1f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public MaterialPropertyBlock materialProperties;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 layerOrigin;

	[PublicizedFrom(EAccessModifier.Private)]
	public Bounds bounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public Matrix4x4[] transforms;

	[PublicizedFrom(EAccessModifier.Private)]
	public float4[] colors;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool transformsHaveChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] normalizedMass;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool massesHaveChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public ComputeBuffer transformsBuffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public ComputeBuffer massBuffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public ComputeBuffer colorBuffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalWater;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsInitialized
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitializeData()
	{
		transforms = new Matrix4x4[4096];
		colors = new float4[4096];
		normalizedMass = new float[4096];
		RegenerateTransforms();
		Origin.OriginChanged = (Action<Vector3>)Delegate.Combine(Origin.OriginChanged, new Action<Vector3>(OnOriginChanged));
		IsInitialized = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RegenerateTransforms()
	{
		Vector3 vector = Vector3.one * 0.9f;
		Vector3 vector2 = (Vector3.one - vector) * 0.5f;
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					int num = CoordToIndex(i, j, k);
					transforms[num] = Matrix4x4.TRS(layerOrigin + new Vector3(i, j, k) - Origin.position + vector2 + Vector3.one * 0.5f, Quaternion.identity, vector);
				}
			}
		}
		Vector3 vector3 = layerOrigin - Origin.position;
		Vector3 vector4 = layerOrigin - Origin.position + new Vector3(16f, 16f, 16f);
		bounds = new Bounds((vector4 + vector3) / 2f, vector4 - vector3);
		transformsHaveChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnOriginChanged(Vector3 _origin)
	{
		if (IsInitialized)
		{
			RegenerateTransforms();
		}
	}

	public void SetLayerOrigin(Vector3 _origin)
	{
		layerOrigin = _origin;
		if (IsInitialized)
		{
			RegenerateTransforms();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CoordToIndex(int _x, int _y, int _z)
	{
		return _x + 16 * _y + 256 * _z;
	}

	public void SetWater(int _x, int _y, int _z, float mass)
	{
		float num = mass / 19500f;
		if (!IsInitialized)
		{
			if (num < 0.01f)
			{
				return;
			}
			InitializeData();
		}
		int num2 = CoordToIndex(_x, _y, _z);
		if (normalizedMass[num2] < 0.01f && num > 0.01f)
		{
			totalWater++;
		}
		else if (normalizedMass[num2] > 0.01f && num < 0.01f)
		{
			totalWater--;
		}
		normalizedMass[num2] = num;
		float s = math.max((mass - 19500f) / 65535f, 0f);
		colors[num2] = math.lerp(waterColor, overfullColor, s);
		massesHaveChanged = true;
	}

	public void Draw()
	{
		if (totalWater != 0)
		{
			if (materialProperties == null)
			{
				materialProperties = new MaterialPropertyBlock();
				materialProperties.SetFloat("_ScaleCutoff", 0.01f);
			}
			if (transformsBuffer == null)
			{
				transformsBuffer = new ComputeBuffer(4096, 64);
				materialProperties.SetBuffer("_Transforms", transformsBuffer);
			}
			if (colorBuffer == null)
			{
				colorBuffer = new ComputeBuffer(4096, 16);
				materialProperties.SetBuffer("_Colors", colorBuffer);
			}
			if (transformsHaveChanged)
			{
				transformsBuffer.SetData(transforms);
				transformsHaveChanged = false;
			}
			if (massBuffer == null)
			{
				massBuffer = new ComputeBuffer(4096, 4);
				materialProperties.SetBuffer("_Scales", massBuffer);
			}
			if (massesHaveChanged)
			{
				massBuffer.SetData(normalizedMass);
				colorBuffer.SetData(colors);
				massesHaveChanged = false;
			}
			Graphics.DrawMeshInstancedProcedural(WaterDebugAssets.CubeMesh, 0, WaterDebugAssets.DebugMaterial, bounds, 4096, materialProperties);
		}
	}

	public void Reset()
	{
		if (IsInitialized)
		{
			Origin.OriginChanged = (Action<Vector3>)Delegate.Remove(Origin.OriginChanged, new Action<Vector3>(OnOriginChanged));
			transforms = null;
			normalizedMass = null;
			IsInitialized = false;
		}
		totalWater = 0;
		transformsHaveChanged = false;
		massesHaveChanged = false;
	}

	public void Cleanup()
	{
		if (transformsBuffer != null)
		{
			transformsBuffer.Dispose();
			transformsBuffer = null;
		}
		if (massBuffer != null)
		{
			massBuffer.Dispose();
			massBuffer = null;
		}
		if (colorBuffer != null)
		{
			colorBuffer.Dispose();
			colorBuffer = null;
		}
	}
}
