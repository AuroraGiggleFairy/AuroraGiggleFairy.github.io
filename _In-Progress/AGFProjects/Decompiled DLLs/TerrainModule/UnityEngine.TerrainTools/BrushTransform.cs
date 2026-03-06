using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.TerrainTools;

[MovedFrom("UnityEngine.Experimental.TerrainAPI")]
public struct BrushTransform
{
	public Vector2 brushOrigin { get; }

	public Vector2 brushU { get; }

	public Vector2 brushV { get; }

	public Vector2 targetOrigin { get; }

	public Vector2 targetX { get; }

	public Vector2 targetY { get; }

	public BrushTransform(Vector2 brushOrigin, Vector2 brushU, Vector2 brushV)
	{
		float num = brushU.x * brushV.y - brushU.y * brushV.x;
		float num2 = (Mathf.Approximately(num, 0f) ? 1f : (1f / num));
		Vector2 vector = new Vector2(brushV.y, 0f - brushU.y) * num2;
		Vector2 vector2 = new Vector2(0f - brushV.x, brushU.x) * num2;
		Vector2 vector3 = (0f - brushOrigin.x) * vector - brushOrigin.y * vector2;
		this.brushOrigin = brushOrigin;
		this.brushU = brushU;
		this.brushV = brushV;
		targetOrigin = vector3;
		targetX = vector;
		targetY = vector2;
	}

	public Rect GetBrushXYBounds()
	{
		Vector2 vector = brushOrigin + brushU;
		Vector2 vector2 = brushOrigin + brushV;
		Vector2 vector3 = brushOrigin + brushU + brushV;
		float xmin = Mathf.Min(Mathf.Min(brushOrigin.x, vector.x), Mathf.Min(vector2.x, vector3.x));
		float xmax = Mathf.Max(Mathf.Max(brushOrigin.x, vector.x), Mathf.Max(vector2.x, vector3.x));
		float ymin = Mathf.Min(Mathf.Min(brushOrigin.y, vector.y), Mathf.Min(vector2.y, vector3.y));
		float ymax = Mathf.Max(Mathf.Max(brushOrigin.y, vector.y), Mathf.Max(vector2.y, vector3.y));
		return Rect.MinMaxRect(xmin, ymin, xmax, ymax);
	}

	public static BrushTransform FromRect(Rect brushRect)
	{
		Vector2 min = brushRect.min;
		Vector2 vector = new Vector2(brushRect.width, 0f);
		Vector2 vector2 = new Vector2(0f, brushRect.height);
		return new BrushTransform(min, vector, vector2);
	}

	public Vector2 ToBrushUV(Vector2 targetXY)
	{
		return targetXY.x * targetX + targetXY.y * targetY + targetOrigin;
	}

	public Vector2 FromBrushUV(Vector2 brushUV)
	{
		return brushUV.x * brushU + brushUV.y * brushV + brushOrigin;
	}
}
