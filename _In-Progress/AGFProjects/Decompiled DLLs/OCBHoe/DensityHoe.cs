using System.Reflection;
using HarmonyLib;
using UnityEngine;

public class DensityHoe : IModApi
{
	[HarmonyPatch(typeof(RenderDisplacedCube))]
	[HarmonyPatch("update0")]
	public class RenderDisplacedCube_Update0
	{
		private static readonly MethodInfo MethodDestroyPreview = AccessTools.Method(typeof(RenderDisplacedCube), "DestroyPreview");

		public static bool Prefix(ref World _world, ref EntityAlive _player, ref WorldRayHitInfo _hitInfo, RenderDisplacedCube __instance, ref Bounds ___localPos, ref Vector3 ___multiDim, ref float ___lastTimeFocusTransformMoved, ref Transform ___transformFocusCubePrefab, ref Transform ___transformWireframeCube, ref Material ___previewMaterial)
		{
			MethodDestroyPreview.Invoke(__instance, null);
			Object.DestroyImmediate(___previewMaterial);
			int clrIdx = _hitInfo.hit.clrIdx;
			Vector3i blockPos = _hitInfo.hit.blockPos;
			BlockValue blockValue = _hitInfo.hit.blockValue;
			if (GetHoeActionRangeSq(_player?.inventory?.holdingItemItemValue?.ItemClass?.Actions) < _hitInfo.hit.distanceSq)
			{
				return true;
			}
			___lastTimeFocusTransformMoved = Time.time;
			sbyte density = _world.GetDensity(clrIdx, blockPos);
			if (___transformWireframeCube != null)
			{
				float y = Mathf.Min(1.75f, Mathf.Max(1.1f, 1.25f * (float)density / -50f));
				___transformWireframeCube.position = blockPos - Origin.position - new Vector3(0.05f, 0.25f, 0.05f);
				___transformWireframeCube.localScale = new Vector3(1.1f, y, 1.1f);
				___transformWireframeCube.rotation = blockValue.Block.shape.GetRotation(blockValue);
			}
			if (___transformFocusCubePrefab != null)
			{
				___transformFocusCubePrefab.localPosition = new Vector3(0.5f, 0.5f, 0.5f);
				___transformFocusCubePrefab.localScale = new Vector3(1f, 1f, 1f);
				___transformFocusCubePrefab.parent = ___transformWireframeCube;
			}
			___localPos = new Bounds(new Vector3(0.5f, 0.5f, 0.5f), Vector3.one);
			___multiDim = Vector3i.one;
			if (GameUtils.IsBlockOrTerrain(_hitInfo.tag) && blockValue.Block.shape.IsTerrain() && density < 0)
			{
				___transformWireframeCube?.gameObject.SetActive(value: true);
				___transformFocusCubePrefab?.gameObject.SetActive(value: true);
				Renderer[] array = ___transformFocusCubePrefab?.GetComponentsInChildren<Renderer>();
				for (int i = 0; i < array.Length; i++)
				{
					array[i].material.SetColor("_Color", Color.green);
				}
			}
			else
			{
				___transformWireframeCube?.gameObject.SetActive(value: false);
				___transformFocusCubePrefab?.gameObject.SetActive(value: false);
			}
			return false;
		}

		private static float GetHoeActionRangeSq(ItemAction[] actions)
		{
			float num = 0f;
			for (int i = 0; i < actions.Length; i++)
			{
				if (actions[i] is ItemActionDensityHoe itemActionDensityHoe)
				{
					num = Mathf.Max(num, itemActionDensityHoe.GetBlockRange());
				}
			}
			return num * num;
		}
	}

	public void InitMod(Mod mod)
	{
		Log.Out("OCB Harmony Patch: " + GetType().ToString());
		new Harmony(GetType().ToString()).PatchAll(Assembly.GetExecutingAssembly());
	}
}
