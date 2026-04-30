using System;
using System.Collections.Generic;
using UnityEngine;

public class LightViewer : MonoBehaviour
{
	public static bool IsEnabled;

	public static bool IsAllOff;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Light[] allLights;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Light, bool> lightsOn = new Dictionary<Light, bool>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> spheres = new List<GameObject>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject sourceSphereClear;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject sourceSphereInc;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject sourceSphereInvInc;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject sourceSphereColor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject sourceSphereInvColor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject sourceSphereNoShadowColor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject sourceSphereNoShadowInvColor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timeLastGathered;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float updateFrequency = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 prevCameraPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Light> noShadowList;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Light> shadowList;

	public static void SetEnabled(bool _on)
	{
		IsEnabled = _on;
		LightLOD.DebugViewDistance = (_on ? float.MaxValue : 0f);
	}

	public void OnDestroy()
	{
		Disable();
	}

	public void SetUpdateFrequency(float _updateFrequency)
	{
		updateFrequency = _updateFrequency;
	}

	public void Disable()
	{
		for (int i = 0; i < spheres.Count; i++)
		{
			UnityEngine.Object.DestroyImmediate(spheres[i]);
		}
		spheres.Clear();
		TurnOnAllLights();
		allLights = null;
	}

	public void TurnOnAllLights()
	{
		foreach (KeyValuePair<Light, bool> item in lightsOn)
		{
			if (item.Key != null)
			{
				item.Key.enabled = item.Value;
			}
		}
		lightsOn.Clear();
	}

	public void TurnOffAllLights()
	{
		if (allLights == null)
		{
			allLights = UnityEngine.Object.FindObjectsOfType<Light>();
		}
		lightsOn.Clear();
		for (int i = 0; i < allLights.Length; i++)
		{
			if (allLights[i].type != LightType.Directional)
			{
				lightsOn.Add(allLights[i], allLights[i].enabled);
				allLights[i].enabled = false;
			}
		}
	}

	public void Update()
	{
		if (lightsOn.Count > 0)
		{
			return;
		}
		if (!IsEnabled)
		{
			Disable();
			return;
		}
		if (sourceSphereClear == null)
		{
			sourceSphereClear = Resources.Load<GameObject>("Prefabs/LightViewerClear");
		}
		if (sourceSphereInc == null)
		{
			sourceSphereInc = Resources.Load<GameObject>("Prefabs/LightViewerInc");
		}
		if (sourceSphereInvInc == null)
		{
			sourceSphereInvInc = Resources.Load<GameObject>("Prefabs/LightViewerInvInc");
		}
		if (sourceSphereColor == null)
		{
			sourceSphereColor = Resources.Load<GameObject>("Prefabs/LightViewerColor");
		}
		if (sourceSphereInvColor == null)
		{
			sourceSphereInvColor = Resources.Load<GameObject>("Prefabs/LightViewerInvColor");
		}
		if (sourceSphereNoShadowColor == null)
		{
			sourceSphereNoShadowColor = Resources.Load<GameObject>("Prefabs/LightViewerNoShadowColor");
		}
		if (sourceSphereNoShadowInvColor == null)
		{
			sourceSphereNoShadowInvColor = Resources.Load<GameObject>("Prefabs/LightViewerNoShadowInvColor");
		}
		if (!((prevCameraPos - Camera.main.transform.position).magnitude > 1f) && !(Time.realtimeSinceStartup >= timeLastGathered + updateFrequency))
		{
			return;
		}
		allLights = UnityEngine.Object.FindObjectsOfType<Light>();
		for (int i = 0; i < spheres.Count; i++)
		{
			UnityEngine.Object.DestroyImmediate(spheres[i]);
		}
		spheres.Clear();
		if (noShadowList == null)
		{
			noShadowList = new List<Light>();
		}
		if (shadowList == null)
		{
			shadowList = new List<Light>();
		}
		for (int j = 0; j < allLights.Length; j++)
		{
			if (allLights[j].type != LightType.Directional)
			{
				if (allLights[j].shadows == LightShadows.None)
				{
					noShadowList.Add(allLights[j]);
				}
				else
				{
					shadowList.Add(allLights[j]);
				}
			}
		}
		for (int k = 0; k < noShadowList.Count; k++)
		{
			if (noShadowList[k].type != LightType.Directional)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(sourceSphereClear);
				gameObject.transform.position = noShadowList[k].transform.position;
				gameObject.transform.localScale = Vector3.one * noShadowList[k].range * 2f;
				gameObject.transform.parent = GameManager.Instance.gameObject.transform;
				gameObject.GetComponent<Renderer>().sortingOrder = k;
				spheres.Add(gameObject);
				if (noShadowList[k].enabled)
				{
					bool num = (Camera.main.transform.position - noShadowList[k].transform.position).magnitude < noShadowList[k].range;
					gameObject = UnityEngine.Object.Instantiate(num ? sourceSphereInvInc : sourceSphereInc);
					gameObject.transform.position = noShadowList[k].transform.position;
					gameObject.transform.localScale = Vector3.one * noShadowList[k].range * 2f;
					gameObject.transform.parent = GameManager.Instance.gameObject.transform;
					gameObject.GetComponent<Renderer>().sortingOrder = k + noShadowList.Count;
					spheres.Add(gameObject);
					gameObject = UnityEngine.Object.Instantiate(num ? sourceSphereNoShadowInvColor : sourceSphereNoShadowColor);
					gameObject.transform.position = noShadowList[k].transform.position;
					gameObject.transform.localScale = Vector3.one * noShadowList[k].range * 2f;
					gameObject.transform.parent = GameManager.Instance.gameObject.transform;
					gameObject.GetComponent<Renderer>().sortingOrder = k + noShadowList.Count * 2;
					spheres.Add(gameObject);
				}
			}
		}
		for (int l = 0; l < shadowList.Count; l++)
		{
			if (shadowList[l].type != LightType.Directional)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(sourceSphereClear);
				gameObject2.transform.position = shadowList[l].transform.position;
				gameObject2.transform.localScale = Vector3.one * shadowList[l].range * 2f;
				gameObject2.transform.parent = GameManager.Instance.gameObject.transform;
				gameObject2.GetComponent<Renderer>().sortingOrder = l + noShadowList.Count * 3;
				spheres.Add(gameObject2);
				if (shadowList[l].enabled)
				{
					bool num2 = (Camera.main.transform.position - shadowList[l].transform.position).magnitude < shadowList[l].range;
					gameObject2 = UnityEngine.Object.Instantiate(num2 ? sourceSphereInvInc : sourceSphereInc);
					gameObject2.transform.position = shadowList[l].transform.position;
					gameObject2.transform.localScale = Vector3.one * shadowList[l].range * 2f;
					gameObject2.transform.parent = GameManager.Instance.gameObject.transform;
					gameObject2.GetComponent<Renderer>().sortingOrder = l + shadowList.Count + noShadowList.Count * 3;
					spheres.Add(gameObject2);
					gameObject2 = UnityEngine.Object.Instantiate(num2 ? sourceSphereInvColor : sourceSphereColor);
					gameObject2.transform.position = shadowList[l].transform.position;
					gameObject2.transform.localScale = Vector3.one * shadowList[l].range * 2f;
					gameObject2.transform.parent = GameManager.Instance.gameObject.transform;
					gameObject2.GetComponent<Renderer>().sortingOrder = l + shadowList.Count * 2 + noShadowList.Count * 3;
					spheres.Add(gameObject2);
				}
			}
		}
		noShadowList.Clear();
		shadowList.Clear();
		prevCameraPos = Camera.main.transform.position;
		timeLastGathered = Time.realtimeSinceStartup;
	}
}
