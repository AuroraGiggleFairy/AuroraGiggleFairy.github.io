namespace WorldGenerationEngineFinal;

public class TownshipShared
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	public int NextId;

	public readonly Vector2i[] dir4way = new Vector2i[4]
	{
		new Vector2i(0, 1),
		new Vector2i(1, 0),
		new Vector2i(0, -1),
		new Vector2i(-1, 0)
	};

	public TownshipShared(WorldBuilder _worldBuilder)
	{
		worldBuilder = _worldBuilder;
	}
}
