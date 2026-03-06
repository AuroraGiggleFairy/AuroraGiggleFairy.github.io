using UnityEngine;
using UnityEngine.Scripting;

namespace RaycastPathing;

[Preserve]
public class FloodFillNode : RaycastNode
{
	[PublicizedFrom(EAccessModifier.Private)]
	public FloodFillNodeScore score;

	public float G
	{
		get
		{
			return score.G;
		}
		set
		{
			score.G = value;
		}
	}

	public float Heuristic
	{
		get
		{
			return score.H;
		}
		set
		{
			score.H = value;
		}
	}

	public float F => score.F;

	public FloodFillNode(Vector3 pos, float scale = 1f, int depth = 0)
		: base(pos, scale, depth)
	{
		score = new FloodFillNodeScore();
	}

	public FloodFillNode(Vector3 min, Vector3 max, float scale = 1f, int depth = 0)
		: base(min, max, scale, depth)
	{
		score = new FloodFillNodeScore();
	}
}
