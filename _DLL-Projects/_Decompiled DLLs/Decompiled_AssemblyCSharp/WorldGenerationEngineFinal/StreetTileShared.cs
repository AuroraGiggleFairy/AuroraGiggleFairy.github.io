using System.Collections.Generic;

namespace WorldGenerationEngineFinal;

public class StreetTileShared
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	public readonly FastTags<TagGroup.Poi> traderTag = FastTags<TagGroup.Poi>.Parse("trader");

	public readonly FastTags<TagGroup.Poi> wildernessTag = FastTags<TagGroup.Poi>.Parse("wilderness");

	public readonly string[] RoadShapes = new string[5] { "rwg_tile_straight", "rwg_tile_t", "rwg_tile_intersection", "rwg_tile_cap", "rwg_tile_corner" };

	public readonly string[] RoadShapesDistrict = new string[5] { "rwg_tile_{0}straight", "rwg_tile_{0}t", "rwg_tile_{0}intersection", "rwg_tile_{0}cap", "rwg_tile_{0}corner" };

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] RoadShapeExits = new int[5] { 5, 7, 15, 4, 6 };

	public readonly List<int> RoadShapeExitCounts = new List<int> { 2, 3, 4, 1, 2 };

	public readonly List<int[]> RoadShapeExitsPerRotation = new List<int[]>();

	public readonly Vector2i[] dir4way = new Vector2i[4]
	{
		new Vector2i(0, 1),
		new Vector2i(1, 0),
		new Vector2i(0, -1),
		new Vector2i(-1, 0)
	};

	public readonly Vector2i[] dir8way = new Vector2i[8]
	{
		new Vector2i(0, 1),
		new Vector2i(1, 1),
		new Vector2i(1, 0),
		new Vector2i(1, -1),
		new Vector2i(0, -1),
		new Vector2i(-1, -1),
		new Vector2i(-1, 0),
		new Vector2i(-1, 1)
	};

	public readonly Vector2i[] dir9way = new Vector2i[9]
	{
		new Vector2i(0, 1),
		new Vector2i(1, 1),
		new Vector2i(1, 0),
		new Vector2i(1, -1),
		new Vector2i(0, 0),
		new Vector2i(0, -1),
		new Vector2i(-1, -1),
		new Vector2i(-1, 0),
		new Vector2i(-1, 1)
	};

	public StreetTileShared(WorldBuilder _worldBuilder)
	{
		worldBuilder = _worldBuilder;
		for (int i = 0; i < RoadShapeExits.Length; i++)
		{
			int num = RoadShapeExits[i];
			int[] array = new int[4];
			for (int j = 0; j < 4; j++)
			{
				array[j] = num;
				num = ((num << 1) & 0xF) | (num >> 3);
			}
			RoadShapeExitsPerRotation.Add(array);
		}
	}
}
