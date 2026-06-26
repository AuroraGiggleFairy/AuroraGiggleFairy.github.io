namespace WorldGenerationEngineFinal;

public class PathNode
{
	public Vector2i position;

	public float pathCost;

	public PathNode next;

	public PathNode nextListElem;

	public const int stepSize = 10;

	public static readonly Vector2i offset = Vector2i.one * 5;

	public PathNode(Vector2i position, float pathCost, PathNode next)
	{
		this.position = position;
		this.pathCost = pathCost;
		this.next = next;
	}

	public PathNode()
	{
	}

	public void Set(Vector2i position, float pathCost, PathNode next)
	{
		this.position = position;
		this.pathCost = pathCost;
		this.next = next;
	}

	public void Reset()
	{
		next = null;
		nextListElem = null;
	}
}
