using System;
using System.Collections;
using UnityEngine;

public class ReflectiveWater : MonoBehaviour
{
	public enum WaterMode
	{
		Simple,
		Reflective,
		Refractive
	}

	public WaterMode m_WaterMode = WaterMode.Refractive;

	public bool m_DisablePixelLights = true;

	public int m_TextureSize = 256;

	public float m_ClipPlaneOffset = 0.07f;

	public LayerMask m_ReflectLayers = -1;

	public LayerMask m_RefractLayers = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Hashtable m_ReflectionCameras = new Hashtable();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Hashtable m_RefractionCameras = new Hashtable();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture m_ReflectionTexture;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture m_RefractionTexture;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public WaterMode m_HardwareWaterSupport = WaterMode.Refractive;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int m_OldReflectionTextureSize;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_InsideWater;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public GameManager gameManager;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		gameManager = (GameManager)UnityEngine.Object.FindObjectOfType(typeof(GameManager));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnPreRender()
	{
		if (!GamePrefs.GetBool(EnumGamePrefs.OptionsGfxWaterQuality))
		{
			return;
		}
		Camera current = Camera.current;
		if ((bool)current && gameManager.World != null && !(gameManager.World.GetPrimaryPlayer() == null) && !s_InsideWater)
		{
			s_InsideWater = true;
			WaterMode waterMode = m_WaterMode;
			CreateWaterObjects(current, out var reflectionCamera, out var _);
			Vector3 position = gameManager.World.GetPrimaryPlayer().transform.position;
			position.y = 60f;
			Vector3 up = Vector3.up;
			int pixelLightCount = QualitySettings.pixelLightCount;
			if (m_DisablePixelLights)
			{
				QualitySettings.pixelLightCount = 0;
			}
			UpdateCameraModes(current, reflectionCamera);
			if (waterMode >= WaterMode.Reflective)
			{
				float w = 0f - Vector3.Dot(up, position) - m_ClipPlaneOffset;
				Vector4 plane = new Vector4(up.x, up.y, up.z, w);
				Matrix4x4 reflectionMat = Matrix4x4.zero;
				CalculateReflectionMatrix(ref reflectionMat, plane);
				Vector3 position2 = current.transform.position;
				Vector3 position3 = reflectionMat.MultiplyPoint(position2);
				reflectionCamera.worldToCameraMatrix = current.worldToCameraMatrix * reflectionMat;
				Vector4 clipPlane = CameraSpacePlane(reflectionCamera, position, up, 1f);
				Matrix4x4 projection = current.projectionMatrix;
				CalculateObliqueMatrix(ref projection, clipPlane);
				reflectionCamera.projectionMatrix = projection;
				reflectionCamera.cullingMask = -17 & m_ReflectLayers.value;
				reflectionCamera.targetTexture = m_ReflectionTexture;
				GL.invertCulling = true;
				reflectionCamera.transform.position = position3;
				reflectionCamera.Render();
				GL.invertCulling = false;
				Shader.SetGlobalTexture("_ReflectionTex", m_ReflectionTexture);
			}
			if (m_DisablePixelLights)
			{
				QualitySettings.pixelLightCount = pixelLightCount;
			}
			s_InsideWater = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnDisable()
	{
		if ((bool)m_ReflectionTexture)
		{
			UnityEngine.Object.DestroyImmediate(m_ReflectionTexture);
			m_ReflectionTexture = null;
		}
		if ((bool)m_RefractionTexture)
		{
			UnityEngine.Object.DestroyImmediate(m_RefractionTexture);
			m_RefractionTexture = null;
		}
		foreach (DictionaryEntry reflectionCamera in m_ReflectionCameras)
		{
			UnityEngine.Object.DestroyImmediate(((Camera)reflectionCamera.Value).gameObject);
		}
		m_ReflectionCameras.Clear();
		foreach (DictionaryEntry refractionCamera in m_RefractionCameras)
		{
			UnityEngine.Object.DestroyImmediate(((Camera)refractionCamera.Value).gameObject);
		}
		m_RefractionCameras.Clear();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (gameManager.gameStateManager.IsGameStarted() && GameStats.GetInt(EnumGameStats.GameState) == 1 && MeshDescription.meshes.Length >= 1 && !(MeshDescription.meshes[1].material == null) && MeshDescription.meshes[1].material.HasProperty("WaveSpeed") && MeshDescription.meshes[1].material.HasProperty("_WaveScale"))
		{
			Vector4 vector = MeshDescription.meshes[1].material.GetVector("WaveSpeed");
			float num = MeshDescription.meshes[1].material.GetFloat("_WaveScale");
			Vector4 value = new Vector4(num, num, num * 0.4f, num * 0.45f);
			double num2 = (double)Time.timeSinceLevelLoad / 200.0;
			Vector4 value2 = new Vector4((float)Math.IEEERemainder((double)(vector.x * value.x) * num2, 1.0), (float)Math.IEEERemainder((double)(vector.y * value.y) * num2, 1.0), (float)Math.IEEERemainder((double)(vector.z * value.z) * num2, 1.0), (float)Math.IEEERemainder((double)(vector.w * value.w) * num2, 1.0));
			MeshDescription.meshes[1].material.SetVector("_WaveOffset", value2);
			MeshDescription.meshes[1].material.SetVector("_WaveScale4", value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateCameraModes(Camera src, Camera dest)
	{
		if (dest == null)
		{
			return;
		}
		dest.clearFlags = src.clearFlags;
		dest.backgroundColor = src.backgroundColor;
		if (src.clearFlags == CameraClearFlags.Skybox)
		{
			Skybox skybox = src.GetComponent(typeof(Skybox)) as Skybox;
			Skybox skybox2 = dest.GetComponent(typeof(Skybox)) as Skybox;
			if (!skybox || !skybox.material)
			{
				skybox2.enabled = false;
			}
			else
			{
				skybox2.enabled = true;
				skybox2.material = skybox.material;
			}
		}
		dest.farClipPlane = src.farClipPlane;
		dest.nearClipPlane = src.nearClipPlane;
		dest.orthographic = src.orthographic;
		dest.fieldOfView = src.fieldOfView;
		dest.aspect = src.aspect;
		dest.orthographicSize = src.orthographicSize;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateWaterObjects(Camera currentCamera, out Camera reflectionCamera, out Camera refractionCamera)
	{
		WaterMode waterMode = GetWaterMode();
		reflectionCamera = null;
		refractionCamera = null;
		if (waterMode < WaterMode.Reflective)
		{
			return;
		}
		if (!m_ReflectionTexture || m_OldReflectionTextureSize != m_TextureSize)
		{
			if ((bool)m_ReflectionTexture)
			{
				UnityEngine.Object.DestroyImmediate(m_ReflectionTexture);
			}
			m_ReflectionTexture = new RenderTexture(m_TextureSize, m_TextureSize, 16);
			m_ReflectionTexture.name = "__WaterReflection" + GetInstanceID();
			m_ReflectionTexture.isPowerOfTwo = true;
			m_ReflectionTexture.hideFlags = HideFlags.DontSave;
			m_OldReflectionTextureSize = m_TextureSize;
		}
		reflectionCamera = m_ReflectionCameras[currentCamera] as Camera;
		if (!reflectionCamera)
		{
			GameObject gameObject = new GameObject("Water Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
			reflectionCamera = gameObject.GetComponent<Camera>();
			reflectionCamera.enabled = false;
			reflectionCamera.transform.position = base.transform.position;
			reflectionCamera.transform.rotation = base.transform.rotation;
			reflectionCamera.gameObject.AddComponent<FlareLayer>();
			gameObject.hideFlags = HideFlags.DontSave;
			m_ReflectionCameras[currentCamera] = reflectionCamera;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterMode GetWaterMode()
	{
		if (m_HardwareWaterSupport < m_WaterMode)
		{
			return m_HardwareWaterSupport;
		}
		return m_WaterMode;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterMode FindHardwareWaterSupport()
	{
		if (!GetComponent<Renderer>())
		{
			return WaterMode.Simple;
		}
		Material sharedMaterial = GetComponent<Renderer>().sharedMaterial;
		if (!sharedMaterial)
		{
			return WaterMode.Simple;
		}
		string text = sharedMaterial.GetTag("WATERMODE", searchFallbacks: false);
		if (text == "Refractive")
		{
			return WaterMode.Refractive;
		}
		if (text == "Reflective")
		{
			return WaterMode.Reflective;
		}
		return WaterMode.Simple;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float sgn(float a)
	{
		if (a > 0f)
		{
			return 1f;
		}
		if (a < 0f)
		{
			return -1f;
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 point = pos + normal * m_ClipPlaneOffset;
		Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
		Vector3 lhs = worldToCameraMatrix.MultiplyPoint(point);
		Vector3 rhs = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
		return new Vector4(rhs.x, rhs.y, rhs.z, 0f - Vector3.Dot(lhs, rhs));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CalculateObliqueMatrix(ref Matrix4x4 projection, Vector4 clipPlane)
	{
		Vector4 b = projection.inverse * new Vector4(sgn(clipPlane.x), sgn(clipPlane.y), 1f, 1f);
		Vector4 vector = clipPlane * (2f / Vector4.Dot(clipPlane, b));
		projection[2] = vector.x - projection[3];
		projection[6] = vector.y - projection[7];
		projection[10] = vector.z - projection[11];
		projection[14] = vector.w - projection[15];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
	{
		reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
		reflectionMat.m01 = -2f * plane[0] * plane[1];
		reflectionMat.m02 = -2f * plane[0] * plane[2];
		reflectionMat.m03 = -2f * plane[3] * plane[0];
		reflectionMat.m10 = -2f * plane[1] * plane[0];
		reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
		reflectionMat.m12 = -2f * plane[1] * plane[2];
		reflectionMat.m13 = -2f * plane[3] * plane[1];
		reflectionMat.m20 = -2f * plane[2] * plane[0];
		reflectionMat.m21 = -2f * plane[2] * plane[1];
		reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
		reflectionMat.m23 = -2f * plane[3] * plane[2];
		reflectionMat.m30 = 0f;
		reflectionMat.m31 = 0f;
		reflectionMat.m32 = 0f;
		reflectionMat.m33 = 1f;
	}
}
