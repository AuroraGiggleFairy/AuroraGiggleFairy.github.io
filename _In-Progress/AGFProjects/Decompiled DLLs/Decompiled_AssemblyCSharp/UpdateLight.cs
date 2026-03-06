using System;
using System.Collections.Generic;
using UnityEngine;

public class UpdateLight : MonoBehaviour
{
	public bool IsDynamicObject;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float currentLit;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float targetLit;

	public float appliedLit = -1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> IgnoreNamedRenderersList;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Entity entity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Renderer> rendererList = new List<Renderer>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<MeshRenderer> meshRendererList = new List<MeshRenderer>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<SkinnedMeshRenderer> skinnedMeshRendererList = new List<SkinnedMeshRenderer>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static MaterialPropertyBlock props;

	public void AddRendererNameToIgnore(string _name)
	{
		if (IgnoreNamedRenderersList == null)
		{
			IgnoreNamedRenderersList = new List<string>();
		}
		IgnoreNamedRenderersList.Add(_name);
	}

	public void SetTintColorForItem(Vector3 _color)
	{
		Color color = new Color(_color.x, _color.y, _color.z, 1f);
		base.gameObject.GetComponentsInChildren(rendererList);
		for (int i = 0; i < rendererList.Count; i++)
		{
			Renderer renderer = rendererList[i];
			if ((bool)renderer && (IgnoreNamedRenderersList == null || !IgnoreNamedRenderersList.ContainsCaseInsensitive(renderer.gameObject.name)))
			{
				SetTintColor(renderer, color);
			}
		}
		rendererList.Clear();
	}

	public static void SetTintColor(Transform _t, Color _color)
	{
		_t.GetComponentsInChildren(rendererList);
		for (int i = 0; i < rendererList.Count; i++)
		{
			SetTintColor(rendererList[i], _color);
		}
		rendererList.Clear();
	}

	public static void SetTintColor(Renderer _r, Color _color)
	{
		Material[] materials = _r.materials;
		if (materials == null)
		{
			return;
		}
		foreach (Material material in materials)
		{
			if (material != null)
			{
				material.SetColor("_Color", _color);
				material.SetColor("TintColor", _color);
				material.SetVector("TintColor", _color);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (GameManager.IsDedicatedServer)
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
	}

	public void Reset()
	{
		appliedLit = -1f;
	}

	public virtual void ManagerFirstUpdate()
	{
		base.gameObject.TryGetComponent<Entity>(out entity);
		currentLit = 0.5f;
		appliedLit = -1f;
		UpdateLighting(1f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetFloatProp<T>(string varName, float value, List<T> _rends) where T : Renderer
	{
		_rends[0].GetPropertyBlock(props);
		props.SetFloat(varName, value);
		for (int i = 0; i < _rends.Count; i++)
		{
			_rends[i].SetPropertyBlock(props);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyLit(float _lit)
	{
		appliedLit = _lit;
		GameObject obj = base.gameObject;
		obj.GetComponentsInChildren(includeInactive: true, meshRendererList);
		obj.GetComponentsInChildren(includeInactive: true, skinnedMeshRendererList);
		if (props == null)
		{
			props = new MaterialPropertyBlock();
		}
		if (meshRendererList.Count > 0)
		{
			SetFloatProp("_MacroAO", _lit, meshRendererList);
			meshRendererList.Clear();
		}
		if (skinnedMeshRendererList.Count > 0)
		{
			SetFloatProp("_MacroAO", _lit, skinnedMeshRendererList);
			skinnedMeshRendererList.Clear();
		}
	}

	public void UpdateLighting(float _step)
	{
		if ((bool)entity)
		{
			targetLit = entity.GetLightBrightness();
		}
		else
		{
			targetLit = 1f;
			Vector3i blockPos = World.worldToBlockPos(base.transform.position + Origin.position);
			if ((uint)blockPos.y < 255u)
			{
				IChunk chunkFromWorldPos = GameManager.Instance.World.GetChunkFromWorldPos(blockPos);
				if (chunkFromWorldPos != null)
				{
					float v = (int)chunkFromWorldPos.GetLight(blockPos.x, blockPos.y, blockPos.z, Chunk.LIGHT_TYPE.SUN);
					float v2 = (int)chunkFromWorldPos.GetLight(blockPos.x, blockPos.y + 1, blockPos.z, Chunk.LIGHT_TYPE.SUN);
					targetLit = Utils.FastMax(v, v2);
					targetLit /= 15f;
				}
			}
		}
		currentLit = Mathf.MoveTowards(currentLit, targetLit, _step);
		if (currentLit != appliedLit)
		{
			ApplyLit(currentLit);
		}
	}
}
