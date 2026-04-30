using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public static class WorldBuilderConstants
{
	public static readonly Color32 forestCol = new Color32(0, 64, 0, byte.MaxValue);

	public static readonly Color32 burntForestCol = new Color32(186, 0, byte.MaxValue, byte.MaxValue);

	public static readonly Color32 desertCol = new Color32(byte.MaxValue, 228, 119, byte.MaxValue);

	public static readonly Color32 snowCol = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static readonly Color32 wastelandCol = new Color32(byte.MaxValue, 168, 0, byte.MaxValue);

	public static readonly List<Color32> biomeColorList = new List<Color32> { forestCol, burntForestCol, desertCol, snowCol, wastelandCol };

	public const int ForestBiomeWeightDefault = 13;

	public const int BurntForestBiomeWeightDefault = 18;

	public const int DesertBiomeWeightDefault = 22;

	public const int SnowBiomeWeightDefault = 23;

	public const int WastelandBiomeWeightDefault = 24;

	public static readonly int[] BiomeWeightDefaults = new int[5] { 13, 18, 22, 23, 24 };
}
