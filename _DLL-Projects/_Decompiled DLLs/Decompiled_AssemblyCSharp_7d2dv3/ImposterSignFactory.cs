using UnityEngine;

public static class ImposterSignFactory
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string CanvasPrefabPath = "@:Entities/Crafting/sign_canvas_imposter_Prefab.prefab";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string DecalPrefabPath = "@:Entities/Crafting/sign_decal_imposter_Prefab.prefab";

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform canvasPrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform decalPrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform GetPrefab(bool isDecal)
	{
		if (isDecal)
		{
			if (decalPrefab == null)
			{
				decalPrefab = DataLoader.LoadAsset<Transform>("@:Entities/Crafting/sign_decal_imposter_Prefab.prefab");
			}
			return decalPrefab;
		}
		if (canvasPrefab == null)
		{
			canvasPrefab = DataLoader.LoadAsset<Transform>("@:Entities/Crafting/sign_canvas_imposter_Prefab.prefab");
		}
		return canvasPrefab;
	}

	public static SignCanvas Create(GameObject _parent, ImposterCanvas _data)
	{
		if (!_data.State.SignId.IsValid)
		{
			return null;
		}
		string text = (_data.IsDecal ? "@:Entities/Crafting/sign_decal_imposter_Prefab.prefab" : "@:Entities/Crafting/sign_canvas_imposter_Prefab.prefab");
		Transform prefab = GetPrefab(_data.IsDecal);
		if (prefab == null)
		{
			Log.Error("[ImposterSignFactory] Failed to load prefab: " + text);
			return null;
		}
		GameObject gameObject = Object.Instantiate(prefab.gameObject, _parent.transform);
		gameObject.name = $"sign_{_data.WorldPosition.x:F0}_{_data.WorldPosition.y:F0}_{_data.WorldPosition.z:F0}";
		gameObject.transform.localPosition = _data.WorldPosition;
		gameObject.transform.localRotation = _data.WorldRotation;
		gameObject.transform.localScale = new Vector3(_data.Size.x, _data.Size.y, 1f);
		SignCanvas canvas = gameObject.GetComponent<SignCanvas>();
		canvas.State = _data.State.Clone();
		canvas.CanvasAspect = _data.CanvasAspect;
		canvas.Initialize(PatchRenderingData);
		_data.Canvas = canvas;
		return canvas;
		[PublicizedFrom(EAccessModifier.Internal)]
		void PatchRenderingData(MaterialPropertyBlock mpb)
		{
			canvas.PatchRenderingData(mpb);
			Vector4 value = ComputeAspectST(_data.CanvasAspect);
			mpb.SetVector(SignShaderIDs._AtlasArray_ST, value);
		}
	}

	public static void Destroy(ImposterCanvas _data)
	{
		if (!(_data?.Canvas == null))
		{
			_data.Canvas.Cleanup();
			_data.Canvas = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector4 ComputeAspectST(float canvasAspect)
	{
		float num = ((canvasAspect >= 1f) ? 2f : (2f * canvasAspect));
		float num2 = ((canvasAspect >= 1f) ? (2f / canvasAspect) : 2f);
		return new Vector4(num, num2, (0f - num) * 0.5f, (0f - num2) * 0.5f);
	}
}
