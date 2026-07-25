using HarmonyLib;
using UnityEngine;

namespace Harmony;

[HarmonyPatch(typeof(RenderDisplacedCube))]
[HarmonyPatch("disableAllComponents")]
public class disableAllComponents
{
	public static void Postfix(Transform _transform)
	{
		_transform.FindInChilds("arrowHelper")?.gameObject.SetActive(value: true);
	}
}
