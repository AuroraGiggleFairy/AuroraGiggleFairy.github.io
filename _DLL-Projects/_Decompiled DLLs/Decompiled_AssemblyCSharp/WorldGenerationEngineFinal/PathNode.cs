using Unity.Burst;

namespace WorldGenerationEngineFinal;

[BurstCompile(CompileSynchronously = true)]
public struct PathNode(Vector2i _position, float _travelledCost, float _totalCost, int _next)
{
	public Vector2i position = _position;

	public float travelledCost = _travelledCost;

	public float totalCost = _totalCost;

	public int pathNext = _next;

	public int listNext = -1;

	public void Set(Vector2i _position, float _travelledCost, float _totalCost, int _next)
	{
		position = _position;
		travelledCost = _travelledCost;
		totalCost = _totalCost;
		pathNext = _next;
	}
}
