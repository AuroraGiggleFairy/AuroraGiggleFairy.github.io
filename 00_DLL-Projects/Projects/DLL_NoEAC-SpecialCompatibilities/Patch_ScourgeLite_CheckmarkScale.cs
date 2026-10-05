using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SpecialNoEACCompatibilities
{
	/// <summary>
	/// Every Scourge X and check uses the small tier-0 size. Up close, each sits
	/// on a tight dark circle with a colored ring. Zooming out shrinks that
	/// button with the map instead of letting it cover the town.
	/// </summary>
	[HarmonyPatch(typeof(XUiC_MapArea), nameof(XUiC_MapArea.Update))]
	public static class Patch_ScourgeLite_CheckmarkScale
	{
		const float SmallScale = 0.35f;

		static readonly HashSet<NavObjectMapSettings> Scaled = new HashSet<NavObjectMapSettings>();
		static bool _done;
		static bool _logged;

		static bool Prepare()
		{
			return ModManager.GetMod("POI_Scourge_Lite") != null;
		}

		internal static void Forget()
		{
			Scaled.Clear();
			_done = false;
		}

		static void Prefix()
		{
			if (_done)
			{
				return;
			}

			try
			{
				Apply();
			}
			catch (Exception ex)
			{
				Console.WriteLine("[NoEACCompatibilities] Scourge icon scale failed: " + ex.Message);
			}
		}

		static void Apply()
		{
			List<NavObjectClass> list = NavObjectClass.NavObjectClassList;
			if (list == null || list.Count == 0)
			{
				return;
			}

			bool waiting = false;
			int markers = 0;
			for (int i = 0; i < list.Count; i++)
			{
				NavObjectClass nav = list[i];
				string name = nav?.NavObjectClassName;
				if (!IsScourgeMark(name))
				{
					continue;
				}

				markers++;
				if (nav.MapSettings == null)
				{
					waiting = true;
					continue;
				}

				SetSmall(nav.MapSettings);
				SetSmall(nav.InactiveMapSettings);
			}

			if (markers > 0 && !waiting)
			{
				_done = true;
				if (!_logged)
				{
					_logged = true;
					Console.WriteLine("[NoEACCompatibilities] Scourge X and check icons use the small size on a tight circle.");
				}
			}
		}

		static void SetSmall(NavObjectMapSettings settings)
		{
			if (settings == null || !Scaled.Add(settings))
			{
				return;
			}

			settings.IconScale = SmallScale;
			settings.IconScaleVector = new Vector3(SmallScale, SmallScale, SmallScale);
		}

		internal static bool IsScourgeMark(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return false;
			}

			return name.StartsWith("scourge_marker_", StringComparison.Ordinal)
				|| name.StartsWith("scourge_uncleared_marker_", StringComparison.Ordinal);
		}
	}

	[HarmonyPatch]
	public static class Patch_ScourgeLite_IconBack
	{
		const string RingName = "ScourgeRing";
		const string FillName = "ScourgeFill";
		const int RingPixels = 2;
		const int FillPad = 2;
		const float FullSizeZoom = 1.5f;
		const float MinZoomFactor = 0.35f;
		static Texture2D _circle;

		static FieldInfo _navs;
		static FieldInfo _sprites;

		static bool Prepare()
		{
			if (ModManager.GetMod("POI_Scourge_Lite") == null)
			{
				return false;
			}

			_navs = AccessTools.Field(typeof(XUiC_MapArea), "keyToNavObject");
			_sprites = AccessTools.Field(typeof(XUiC_MapArea), "keyToNavSprite");
			return _navs != null && _sprites != null && AccessTools.Method(typeof(XUiC_MapArea), "updateNavObjectList") != null;
		}

		static MethodBase TargetMethod()
		{
			return AccessTools.Method(typeof(XUiC_MapArea), "updateNavObjectList");
		}

		static void Postfix(XUiC_MapArea __instance)
		{
			try
			{
				DictionarySave<int, NavObject> navs = _navs.GetValue(__instance) as DictionarySave<int, NavObject>;
				DictionarySave<int, GameObject> sprites = _sprites.GetValue(__instance) as DictionarySave<int, GameObject>;
				if (navs?.Dict == null || sprites?.Dict == null)
				{
					return;
				}

				foreach (KeyValuePair<int, NavObject> pair in navs.Dict)
				{
					string name = pair.Value?.NavObjectClass?.NavObjectClassName;
					if (!Patch_ScourgeLite_CheckmarkScale.IsScourgeMark(name))
					{
						continue;
					}

					if (!sprites.Dict.TryGetValue(pair.Key, out GameObject root) || root == null)
					{
						continue;
					}

					Transform frontTransform = root.transform.Find("Sprite");
					UISprite front = frontTransform != null ? frontTransform.GetComponent<UISprite>() : null;
					if (front == null)
					{
						continue;
					}

					PlaceBack(root.transform, front, __instance.zoomScale);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("[NoEACCompatibilities] Scourge icon back failed: " + ex.Message);
			}
		}

		static void PlaceBack(Transform root, UISprite front, float zoomScale)
		{
			DestroyChild(root, "ScourgeBack");
			DestroyChild(root, "ScourgeCircle");

			UITexture ring = GetCircle(root, RingName, front);
			UITexture fill = GetCircle(root, FillName, front);
			if (ring == null || fill == null)
			{
				return;
			}

			int raw = Mathf.Max(front.width, front.height);
			// zoomScale is about 0.7 zoomed in and about 6.15 zoomed out.
			float factor = Mathf.Clamp(FullSizeZoom / Mathf.Max(zoomScale, 0.01f), MinZoomFactor, 1f);
			int span = Mathf.Max(1, Mathf.RoundToInt(raw * factor));
			int innerFull = Mathf.CeilToInt(raw * 1.41421356f) + FillPad * 2;
			int outerFull = innerFull + RingPixels * 2;
			int inner = Mathf.Max(span, Mathf.RoundToInt(innerFull * factor));
			int outer = Mathf.Max(inner + 1, Mathf.RoundToInt(outerFull * factor));
			front.width = span;
			front.height = span;
			Vector3 center = front.transform.localPosition + front.transform.localRotation * Vector3.Scale(front.localCenter, front.transform.localScale);
			Color ringColor = front.color;
			ringColor.a = 1f;

			Style(ring, outer, ringColor, front.depth - 2, center);
			Style(fill, inner, new Color(0f, 0f, 0f, 1f), front.depth - 1, center);
		}

		static void DestroyChild(Transform root, string childName)
		{
			Transform child = root.Find(childName);
			if (child != null)
			{
				UnityEngine.Object.Destroy(child.gameObject);
			}
		}

		static UITexture GetCircle(Transform root, string childName, UISprite front)
		{
			Transform existing = root.Find(childName);
			if (existing == null)
			{
				GameObject circleObject = new GameObject(childName);
				circleObject.layer = front.gameObject.layer;
				circleObject.transform.SetParent(root, false);
				UITexture created = circleObject.AddComponent<UITexture>();
				created.mainTexture = CircleTexture();
				return created;
			}

			return existing.GetComponent<UITexture>();
		}

		static void Style(UITexture circle, int diameter, Color color, int depth, Vector3 center)
		{
			circle.pivot = UIWidget.Pivot.Center;
			circle.color = color;
			circle.depth = depth;
			circle.width = diameter;
			circle.height = diameter;
			circle.transform.localPosition = center;
			circle.transform.localRotation = Quaternion.identity;
			circle.transform.localScale = Vector3.one;
			circle.transform.SetAsFirstSibling();
		}

		static Texture2D CircleTexture()
		{
			if (_circle != null)
			{
				return _circle;
			}

			const int size = 64;
			Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
			texture.wrapMode = TextureWrapMode.Clamp;
			texture.filterMode = FilterMode.Bilinear;
			float radius = (size - 1) * 0.5f;
			float cx = radius;
			float cy = radius;
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					float distance = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
					float alpha = Mathf.Clamp01(radius - distance + 0.75f);
					texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
				}
			}

			texture.Apply(false, true);
			_circle = texture;
			return texture;
		}
	}

	[HarmonyPatch(typeof(NavObjectClass), nameof(NavObjectClass.Reset))]
	public static class Patch_ScourgeLite_CheckmarkScaleReset
	{
		static bool Prepare()
		{
			return ModManager.GetMod("POI_Scourge_Lite") != null;
		}

		static void Postfix()
		{
			Patch_ScourgeLite_CheckmarkScale.Forget();
		}
	}
}
