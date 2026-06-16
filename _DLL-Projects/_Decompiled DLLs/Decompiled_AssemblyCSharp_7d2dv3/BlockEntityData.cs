using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockEntityData
{
	[PublicizedFrom(EAccessModifier.Private)]
	public MaterialPropertyBlock matPropBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Renderer> renderers;

	public BlockValue blockValue;

	public Vector3i pos;

	public Transform transform;

	public bool bHasTransform;

	public bool bRenderingOn;

	public bool bNeedsTemperature;

	public BlockEntityData()
	{
	}

	public BlockEntityData(BlockValue _blockValue, Vector3i _pos)
	{
		pos = _pos;
		blockValue = _blockValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void getRenderers()
	{
		if (matPropBlock == null)
		{
			matPropBlock = new MaterialPropertyBlock();
		}
		if (renderers != null)
		{
			renderers.Clear();
		}
		else
		{
			renderers = new List<Renderer>();
		}
		transform.GetComponentsInChildren(includeInactive: true, renderers);
	}

	public List<Renderer> GetRenderers()
	{
		return renderers;
	}

	public void Cleanup()
	{
		if (renderers != null)
		{
			renderers.Clear();
		}
	}

	public void SetMaterialColor(string name, Color value)
	{
		getRenderers();
		if (renderers == null || GameManager.IsDedicatedServer)
		{
			return;
		}
		for (int i = 0; i < renderers.Count; i++)
		{
			if (renderers[i] != null)
			{
				renderers[i].GetPropertyBlock(matPropBlock);
				matPropBlock.SetColor(name, value);
				renderers[i].SetPropertyBlock(matPropBlock);
			}
		}
	}

	public void SetMaterialValue(string name, float value)
	{
		getRenderers();
		if (renderers == null || GameManager.IsDedicatedServer)
		{
			return;
		}
		for (int i = 0; i < renderers.Count; i++)
		{
			if (renderers[i] != null)
			{
				renderers[i].GetPropertyBlock(matPropBlock);
				matPropBlock.SetFloat(name, value);
				renderers[i].SetPropertyBlock(matPropBlock);
			}
		}
	}

	public void SetMaterialColor(Color color)
	{
		getRenderers();
		for (int i = 0; i < renderers.Count; i++)
		{
			if (renderers[i] != null)
			{
				renderers[i].GetPropertyBlock(matPropBlock);
				matPropBlock.SetColor("_Color", color);
				renderers[i].SetPropertyBlock(matPropBlock);
			}
		}
	}

	public void UpdateTemperature()
	{
	}

	public override string ToString()
	{
		BlockValue blockValue = this.blockValue;
		return "EntityBlockCreationData " + blockValue.ToString();
	}
}
