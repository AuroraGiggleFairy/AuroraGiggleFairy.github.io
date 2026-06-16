using UnityEngine.Scripting;

namespace RaycastPathing;

[Preserve]
public class FloodFillNodeScore
{
	public float G;

	public float H;

	public float F => G + H;
}
