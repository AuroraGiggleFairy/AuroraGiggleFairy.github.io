using UnityEngine;

public class RenderTextureSystem
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cSceneParentName = "3D RenderTextures";

	public GameObject ParentGO;

	public GameObject CameraGO;

	public GameObject TargetGO;

	public GameObject LightGO;

	public RenderTexture RenderTex;

	[PublicizedFrom(EAccessModifier.Private)]
	public Camera cam;

	public void Create(string _name, GameObject _target, Vector3 _targetRelPos, Vector3 _lightSourceRelPos, Vector2i _renderTexSize, bool _isAA, bool _orthographic = false, float _orthoSize = 1f)
	{
		GameObject gameObject = GameObject.Find("3D RenderTextures");
		if (gameObject == null)
		{
			gameObject = new GameObject("3D RenderTextures");
			gameObject.layer = 11;
		}
		ParentGO = new GameObject(_name);
		ParentGO.transform.parent = gameObject.transform;
		ParentGO.layer = 11;
		TargetGO = _target;
		TargetGO.transform.parent = ParentGO.transform;
		TargetGO.transform.localPosition = _targetRelPos;
		TargetGO.layer = 11;
		CameraGO = new GameObject("Camera");
		CameraGO.transform.parent = ParentGO.transform;
		CameraGO.transform.LookAt(_target.transform);
		CameraGO.layer = 11;
		cam = CameraGO.AddComponent<Camera>();
		cam.nearClipPlane = 0.01f;
		cam.farClipPlane = 20f;
		cam.cullingMask = 2048;
		cam.renderingPath = RenderingPath.Forward;
		cam.clearFlags = CameraClearFlags.Color;
		if (_orthographic)
		{
			cam.orthographic = _orthographic;
			cam.orthographicSize = _orthoSize;
			Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
			Renderer[] componentsInChildren = TargetGO.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				bounds.Encapsulate(renderer.bounds);
			}
			TargetGO.transform.localPosition = _targetRelPos - bounds.center / 2f;
			if (bounds.size.y > bounds.size.x && bounds.size.y > bounds.size.z)
			{
				cam.orthographicSize = bounds.size.magnitude / 2f * _orthoSize;
			}
			else
			{
				cam.orthographicSize = bounds.size.magnitude / 3f * _orthoSize;
			}
		}
		int num = ((!_isAA) ? 1 : 2);
		RenderTex = new RenderTexture(_renderTexSize.x * num, _renderTexSize.y * num, 24);
		RenderTex.autoGenerateMips = _isAA;
		RenderTex.useMipMap = _isAA;
		cam.targetTexture = RenderTex;
		LightGO = new GameObject("Light");
		LightGO.transform.parent = ParentGO.transform;
		LightGO.transform.localPosition = _lightSourceRelPos;
		LightGO.layer = 11;
		Light light = LightGO.AddComponent<Light>();
		light.type = LightType.Point;
		light.intensity = 1.5f;
		light.bounceIntensity = 0f;
		light.range = _targetRelPos.magnitude * 10f;
		light.cullingMask = 2048;
	}

	public void SetTarget(GameObject _target, Vector3 _targetRelPos, bool _orthographic, float _orthoSize = 1f)
	{
		if (TargetGO != null)
		{
			Object.Destroy(TargetGO);
		}
		TargetGO = Object.Instantiate(_target);
		TargetGO.transform.parent = ParentGO.transform;
		TargetGO.transform.localPosition = Vector3.zero;
		recursiveLayerSetup(TargetGO, 11);
		if (_orthographic)
		{
			cam.orthographic = _orthographic;
			cam.orthographicSize = _orthoSize;
			Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
			Renderer[] componentsInChildren = TargetGO.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				bounds.Encapsulate(renderer.bounds);
			}
			TargetGO.transform.localPosition = _targetRelPos - bounds.center / 2f;
			if (bounds.size.y > bounds.size.x && bounds.size.y > bounds.size.z)
			{
				cam.orthographicSize = bounds.size.magnitude / 2f * _orthoSize;
			}
			else
			{
				cam.orthographicSize = bounds.size.magnitude / 3f * _orthoSize;
			}
		}
	}

	public void SetTargetNoCopy(GameObject _target, Vector3 _targetRelPos, bool _orthographic, float _orthoSize = 1f)
	{
		if (TargetGO != null)
		{
			Object.Destroy(TargetGO);
		}
		Light component = LightGO.GetComponent<Light>();
		component.range = 14f;
		component.intensity = 2f;
		TargetGO = _target;
		TargetGO.transform.parent = ParentGO.transform;
		TargetGO.transform.localPosition = Vector3.zero;
		recursiveLayerSetup(_target, 11);
		if (_orthographic)
		{
			cam.orthographic = _orthographic;
			cam.orthographicSize = _orthoSize;
			cam.backgroundColor = new Color32(0, 0, 0, 0);
			Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
			Renderer[] componentsInChildren = TargetGO.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				bounds.Encapsulate(renderer.bounds);
			}
			TargetGO.transform.localPosition = _targetRelPos - bounds.center / 2f;
		}
	}

	public void SetOrtho(bool enabled, float _orthoSize)
	{
		cam.orthographic = enabled;
		cam.orthographicSize = _orthoSize;
	}

	public void RotateTarget(float _amount)
	{
		if (TargetGO != null)
		{
			Vector3 eulerAngles = TargetGO.transform.localRotation.eulerAngles;
			TargetGO.transform.localRotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y + _amount, eulerAngles.z);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void recursiveLayerSetup(GameObject _go, int _layer)
	{
		Transform transform = _go.transform;
		transform.gameObject.layer = _layer;
		foreach (Transform item in transform)
		{
			item.gameObject.layer = _layer;
			recursiveLayerSetup(item.gameObject, _layer);
		}
	}

	public void Create(string _name, Camera _existingCamera, Vector2i _renderTexSize)
	{
		GameObject gameObject = GameObject.Find("3D RenderTextures");
		if (gameObject == null)
		{
			gameObject = new GameObject("3D RenderTextures");
			gameObject.layer = 11;
		}
		ParentGO = new GameObject(_name);
		ParentGO.transform.parent = gameObject.transform;
		ParentGO.layer = 11;
		RenderTex = new RenderTexture(_renderTexSize.x, _renderTexSize.y, 24);
		_existingCamera.targetTexture = RenderTex;
	}

	public void SetEnabled(bool _b)
	{
		ParentGO.SetActive(_b);
	}

	public void Cleanup()
	{
		if (ParentGO != null)
		{
			Object.Destroy(ParentGO);
		}
		if (RenderTex != null)
		{
			Object.Destroy(RenderTex);
		}
	}
}
