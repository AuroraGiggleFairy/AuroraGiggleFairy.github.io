using System.Collections.Generic;
using UnityEngine;

namespace MapPlus
{
	/// <summary>
	/// Resolves the map-cursor name and POI outline. XML can bind {mapplusname}
	/// or use a label named poiName.
	/// </summary>
	public static class MapPlusHover
	{
		public const string BindingName = "mapplusname";
		public const string BindingNameAlt = "agfmapplusname";
		public const string LabelId = "poiName";
		public const string BoundsId = "poiBounds";
		public const string BoundsOuterId = "poiBoundsOuter";
		public const string BoundsFillId = "poiBoundsFill";
		public const string UnexploredLocKey = "xuiMapPlusUnexplored";
		public const string NotVisitedLocKey = "xuiMapPlusNotVisited";
		public const string TierFormatKey = "xuiMapPlusPoiTier";
		const int MinBoundsPixels = 8;
		const int BoundsOutset = 3;
		const string TierColorTag = "[FF0000]";
		static readonly Color32 PoiOrange = new Color32(255, 153, 0, 255);
		static readonly Color32 PoiOrangeFill = new Color32(255, 153, 0, 50);

		static readonly List<PrefabInstance> PrefabBuffer = new List<PrefabInstance>(8);

		static XUiC_MapArea BoundMap;
		static XUiV_Label BoundLabel;
		static XUiV_Sprite BoundBorder;
		static XUiV_Sprite BoundOuter;
		static XUiV_Sprite BoundFill;
		static XUiView BoundMapView;
		static bool BoundsVisible;

		public static string CurrentDisplayName { get; private set; } = string.Empty;

		public static bool TryHandleBinding(string bindingName, ref string value)
		{
			if (string.IsNullOrEmpty(bindingName))
			{
				return false;
			}

			if (!bindingName.Equals(BindingName) && !bindingName.Equals(BindingNameAlt))
			{
				return false;
			}

			value = CurrentDisplayName ?? string.Empty;
			return true;
		}

		public static void RefreshFromMap(XUiC_MapArea mapArea, Vector3 worldPos, bool mouseOverMap)
		{
			try
			{
				if (!mouseOverMap)
				{
					if (string.IsNullOrEmpty(CurrentDisplayName) && !BoundsVisible)
					{
						return;
					}

					Hide(mapArea);
					return;
				}

				BindViews(mapArea);
				PrefabInstance prefab = null;
				string next = FormatDisplay(Resolve(mapArea, worldPos, out prefab), prefab);
				if (next != CurrentDisplayName)
				{
					CurrentDisplayName = next;
					ApplyLabel(next);
					mapArea.RefreshBindings();
				}

				ApplyBounds(mapArea, prefab);
			}
			catch
			{
			}
		}

		public static void Hide(XUiC_MapArea mapArea)
		{
			CurrentDisplayName = string.Empty;
			BindViews(mapArea);
			ApplyLabel(string.Empty);
			ApplyBounds(mapArea, null);
			ClearViews();
		}

		static string Resolve(XUiC_MapArea mapArea, Vector3 worldPos, out PrefabInstance prefab)
		{
			prefab = null;
			if (!IsChunkExplored(mapArea, worldPos))
			{
				return Localization.Get(UnexploredLocKey);
			}

			prefab = FindNameablePrefabAt(worldPos);
			if (prefab != null && !MapPlusVisits.HasVisited(prefab))
			{
				return Localization.Get(NotVisitedLocKey);
			}

			if (prefab != null)
			{
				string poiName = prefab.prefab?.LocalizedName;
				if (!string.IsNullOrEmpty(poiName))
				{
					return poiName;
				}
			}

			return GetBiomeName(worldPos);
		}

		static string FormatDisplay(string name, PrefabInstance prefab)
		{
			if (string.IsNullOrEmpty(name) || IsPlaceholderName(name))
			{
				return name ?? string.Empty;
			}

			if (XUiC_Location.ShowLocation != XUiC_Location.ShowLocationInfoTypes.Yes)
			{
				return name;
			}

			int tier = prefab?.prefab != null ? prefab.prefab.DifficultyTier : 0;
			if (tier <= 0)
			{
				return name;
			}

			return name + " " + TierColorTag + FormatTierMarker(tier) + "[-]";
		}

		static string FormatTierMarker(int tier)
		{
			string format = Localization.Get(TierFormatKey);
			if (string.IsNullOrEmpty(format) || format.IndexOf("{0}") < 0)
			{
				format = "(T{0})";
			}

			return string.Format(format, tier);
		}

		static bool IsPlaceholderName(string displayName)
		{
			return displayName == Localization.Get(UnexploredLocKey) || displayName == Localization.Get(NotVisitedLocKey);
		}

		static string GetBiomeName(Vector3 worldPos)
		{
			World world = GameManager.Instance?.World;
			if (world == null)
			{
				return string.Empty;
			}

			int x = Utils.Fastfloor(worldPos.x);
			int z = Utils.Fastfloor(worldPos.z);
			BiomeDefinition biome = world.GetBiomeInWorld(x, z) ?? world.GetBiome(x, z);
			if (biome == null)
			{
				return string.Empty;
			}

			if (!string.IsNullOrEmpty(biome.LocalizedName))
			{
				return biome.LocalizedName;
			}

			string localized = BiomeDefinition.LocalizedBiomeName(biome.m_BiomeType);
			if (!string.IsNullOrEmpty(localized))
			{
				return localized;
			}

			return string.IsNullOrEmpty(biome.m_sBiomeName) ? string.Empty : biome.m_sBiomeName;
		}

		static bool IsChunkExplored(XUiC_MapArea mapArea, Vector3 worldPos)
		{
			EntityPlayerLocal player = mapArea?.xui?.playerUI?.entityPlayer;
			IMapChunkDatabase mapDatabase = player?.ChunkObserver?.mapDatabase;
			if (mapDatabase == null)
			{
				return false;
			}

			int chunkX = World.toChunkXZ(Utils.Fastfloor(worldPos.x));
			int chunkZ = World.toChunkXZ(Utils.Fastfloor(worldPos.z));
			return mapDatabase.Contains(WorldChunkCache.MakeChunkKey(chunkX, chunkZ));
		}

		static PrefabInstance FindNameablePrefabAt(Vector3 worldPos)
		{
			DynamicPrefabDecorator decorator = GameManager.Instance?.GetDynamicPrefabDecorator();
			if (decorator == null)
			{
				return null;
			}

			int x = Utils.Fastfloor(worldPos.x);
			int z = Utils.Fastfloor(worldPos.z);
			PrefabBuffer.Clear();
			decorator.GetPrefabsAtXZ(x, x, z, z, PrefabBuffer);

			PrefabInstance bestVisited = null;
			int bestVisitedArea = int.MaxValue;
			PrefabInstance best = null;
			int bestArea = int.MaxValue;
			for (int i = 0; i < PrefabBuffer.Count; i++)
			{
				PrefabInstance candidate = PrefabBuffer[i];
				if (!MapPlusVisits.IsNameablePrefab(candidate))
				{
					continue;
				}

				int area = candidate.boundingBoxSize.x * candidate.boundingBoxSize.z;
				if (area <= 0)
				{
					continue;
				}

				if (area < bestArea)
				{
					best = candidate;
					bestArea = area;
				}

				if (MapPlusVisits.HasVisited(candidate) && area < bestVisitedArea)
				{
					bestVisited = candidate;
					bestVisitedArea = area;
				}
			}

			return bestVisited ?? best;
		}

		static void BindViews(XUiC_MapArea mapArea)
		{
			if (mapArea == null || BoundMap == mapArea)
			{
				return;
			}

			BoundMap = mapArea;
			BoundLabel = mapArea.GetChildById(LabelId)?.ViewComponent as XUiV_Label;
			BoundBorder = mapArea.GetChildById(BoundsId)?.ViewComponent as XUiV_Sprite;
			BoundOuter = mapArea.GetChildById(BoundsOuterId)?.ViewComponent as XUiV_Sprite;
			BoundFill = mapArea.GetChildById(BoundsFillId)?.ViewComponent as XUiV_Sprite;
			BoundMapView = mapArea.GetChildById("mapView")?.ViewComponent;
		}

		static void ClearViews()
		{
			BoundMap = null;
			BoundLabel = null;
			BoundBorder = null;
			BoundOuter = null;
			BoundFill = null;
			BoundMapView = null;
		}

		static void ApplyLabel(string text)
		{
			if (BoundLabel == null)
			{
				return;
			}

			BoundLabel.Text = text ?? string.Empty;
			BoundLabel.Color = PoiOrange;
			BoundLabel.IsVisible = !string.IsNullOrEmpty(text);
		}

		static void ApplyBounds(XUiC_MapArea mapArea, PrefabInstance prefab)
		{
			if (BoundBorder == null && BoundOuter == null && BoundFill == null)
			{
				return;
			}

			if (prefab == null || mapArea == null)
			{
				if (!BoundsVisible)
				{
					return;
				}

				SetSpriteVisible(BoundBorder, visible: false);
				SetSpriteVisible(BoundOuter, visible: false);
				SetSpriteVisible(BoundFill, visible: false);
				BoundsVisible = false;
				return;
			}

			Vector3 minWorld = new Vector3(prefab.boundingBoxPosition.x, 0f, prefab.boundingBoxPosition.z);
			Vector3 maxWorld = new Vector3(
				prefab.boundingBoxPosition.x + prefab.boundingBoxSize.x,
				0f,
				prefab.boundingBoxPosition.z + prefab.boundingBoxSize.z);
			Vector3 minScreen = mapArea.worldPosToScreenPos(minWorld);
			Vector3 maxScreen = mapArea.worldPosToScreenPos(maxWorld);

			int x = Mathf.RoundToInt(Mathf.Min(minScreen.x, maxScreen.x));
			int y = Mathf.RoundToInt(Mathf.Max(minScreen.y, maxScreen.y));
			int width = Mathf.Max(MinBoundsPixels, Mathf.RoundToInt(Mathf.Abs(maxScreen.x - minScreen.x)));
			int height = Mathf.Max(MinBoundsPixels, Mathf.RoundToInt(Mathf.Abs(maxScreen.y - minScreen.y)));
			ClampToMap(ref x, ref y, ref width, ref height);
			if (width < 2 || height < 2)
			{
				SetSpriteVisible(BoundBorder, visible: false);
				SetSpriteVisible(BoundOuter, visible: false);
				SetSpriteVisible(BoundFill, visible: false);
				BoundsVisible = false;
				return;
			}

			int outerX = x - BoundsOutset;
			int outerY = y + BoundsOutset;
			int outerW = width + (BoundsOutset * 2);
			int outerH = height + (BoundsOutset * 2);
			ClampToMap(ref outerX, ref outerY, ref outerW, ref outerH);

			SetSpriteRect(BoundFill, x, y, width, height, PoiOrangeFill);
			SetSpriteRect(BoundBorder, x, y, width, height, PoiOrange);
			SetSpriteRect(BoundOuter, outerX, outerY, outerW, outerH, PoiOrange);
			BoundsVisible = true;
		}

		static void ClampToMap(ref int x, ref int y, ref int width, ref int height)
		{
			int mapW = BoundMapView != null ? BoundMapView.Size.x : 712;
			int mapH = BoundMapView != null ? BoundMapView.Size.y : 712;
			int right = x + width;
			int bottom = y - height;
			if (x < 0)
			{
				width += x;
				x = 0;
			}

			if (y > 0)
			{
				height -= y;
				y = 0;
			}

			if (right > mapW)
			{
				width -= right - mapW;
			}

			if (bottom < -mapH)
			{
				height -= -mapH - bottom;
			}
		}

		static void SetSpriteVisible(XUiV_Sprite sprite, bool visible)
		{
			if (sprite != null)
			{
				sprite.IsVisible = visible;
			}
		}

		static void SetSpriteRect(XUiV_Sprite sprite, int x, int y, int width, int height, Color32 color)
		{
			if (sprite == null)
			{
				return;
			}

			sprite.Position = new Vector2i(x, y);
			sprite.Size = new Vector2i(width, height);
			sprite.Color = color;
			sprite.IsVisible = true;
		}
	}
}
