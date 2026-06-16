using UnityEngine;

public class GUIUtils
{
	public class IntRect
	{
		public int x;

		public int y;

		public int width;

		public int height;

		public IntRect(int _x, int _y, int _width, int _height)
		{
			x = _x;
			y = _y;
			width = _width;
			height = _height;
		}

		public IntRect(float _x, float _y, float _width, float _height)
		{
			x = (int)_x;
			y = (int)_y;
			width = (int)_width;
			height = (int)_height;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool clippingEnabled;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Rect clippingBounds;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Material lineMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Camera lineCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float lineHalfWidth;

	public static void DrawFilledRect(Rect _rect, Color _colFill, bool _bDrawBorder, Color _colBorder)
	{
		if (Event.current.type == EventType.Repaint)
		{
			GUI.color = _colFill;
			GUI.DrawTexture(_rect, Texture2D.whiteTexture);
			GUI.color = Color.white;
			if (_bDrawBorder)
			{
				_rect.width -= 1f;
				_rect.height -= 1f;
				DrawRect(_rect, _colBorder);
			}
		}
	}

	public static void DrawFilledRect(Rect _rect, Texture2D _tex, bool _bDrawBorder, Color _colBorder)
	{
		if (Event.current.type == EventType.Repaint)
		{
			GUI.DrawTexture(_rect, _tex);
			if (_bDrawBorder)
			{
				DrawRect(_rect, _colBorder);
			}
		}
	}

	public static void DrawRect(Rect _rect, Color _col)
	{
		DrawLine(new Vector2(_rect.x, _rect.y), new Vector2(_rect.x, _rect.y + _rect.height), _col);
		DrawLine(new Vector2(_rect.x, _rect.y + _rect.height), new Vector2(_rect.x + _rect.width, _rect.y + _rect.height), _col);
		DrawLine(new Vector2(_rect.x + _rect.width, _rect.y + _rect.height), new Vector2(_rect.x + _rect.width, _rect.y), _col);
		DrawLine(new Vector2(_rect.x + _rect.width, _rect.y), new Vector2(_rect.x, _rect.y), _col);
	}

	public static void DrawArrow(Vector2 pos, Vector2 dir, float size, Color col)
	{
		Vector2 vector = new Vector2(dir.y, 0f - dir.x);
		dir *= size;
		vector *= size;
		Vector2 pointA = new Vector2(pos.x + dir.x, pos.y - dir.y);
		Vector2 vector2 = new Vector2(pos.x + dir.x / 3f, pos.y - dir.y / 3f);
		Vector2 vector3 = new Vector2(vector2.x + vector.x, vector2.y - vector.y);
		Vector2 vector4 = new Vector2(vector2.x + (0f - vector.x), vector2.y - (0f - vector.y));
		DrawLine(pointA, vector3, col);
		DrawLine(pointA, vector4, col);
		DrawLine(vector3, vector4, col);
		vector3 += new Vector2((0f - vector.x) / 2f, vector.y / 2f);
		vector4 += new Vector2(vector.x / 2f, (0f - vector.y) / 2f);
		Vector2 vector5 = new Vector2(vector3.x - dir.x, vector3.y + dir.y);
		Vector2 pointB = new Vector2(vector4.x - dir.x, vector4.y + dir.y);
		DrawLine(vector3, vector5, col);
		DrawLine(vector4, pointB, col);
		DrawLine(vector5, pointB, col);
	}

	public static void DrawTriangle(Vector2 pos, Vector2 dir, float size, Color col)
	{
		Vector2 vector = new Vector2(dir.y, 0f - dir.x);
		dir *= size;
		vector *= size;
		Vector2 pointA = new Vector2(pos.x + dir.x / 2f, pos.y - dir.y / 2f);
		Vector2 vector2 = new Vector2(pos.x - dir.x / 2f, pos.y + dir.y / 2f);
		Vector2 vector3 = new Vector2(vector2.x + vector.x, vector2.y - vector.y);
		Vector2 pointB = new Vector2(vector2.x + (0f - vector.x), vector2.y - (0f - vector.y));
		DrawLine(pointA, vector3, col);
		DrawLine(pointA, pointB, col);
		DrawLine(vector3, pointB, col);
	}

	public static void DrawTriangleWide(Vector3 pos, Vector3 facing, Vector3 perpDir, float size, Color _color)
	{
		Vector3 vector = Vector3.Cross(facing, perpDir);
		facing *= size;
		vector *= size;
		Vector3 pointA = pos + facing;
		Vector3 vector2 = pos + vector;
		Vector3 pointB = pos - vector;
		DrawLineWide(pointA, vector2, _color, _color);
		DrawLineWide(pointA, pointB, _color, _color);
		DrawLineWide(vector2, pointB, _color, _color);
	}

	public static void DrawRectWide(Vector3 pos, Vector3 facing, Vector3 perpDir, float size, Color _color)
	{
		Vector3 vector = Vector3.Cross(facing, perpDir);
		facing *= size;
		vector *= size;
		Vector3 vector2 = pos + vector / 2f;
		Vector3 vector3 = pos - vector / 2f;
		Vector3 vector4 = vector2 + facing;
		Vector3 pointB = vector3 + facing;
		DrawLineWide(vector2, vector3, _color, _color);
		DrawLineWide(vector2, vector4, _color, _color);
		DrawLineWide(vector3, pointB, _color, _color);
		DrawLineWide(vector4, pointB, _color, _color);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool clip_test(float p, float q, ref float u1, ref float u2)
	{
		bool result = true;
		if ((double)p < 0.0)
		{
			float num = q / p;
			if (num > u2)
			{
				result = false;
			}
			else if (num > u1)
			{
				u1 = num;
			}
		}
		else if ((double)p > 0.0)
		{
			float num = q / p;
			if (num < u1)
			{
				result = false;
			}
			else if (num < u2)
			{
				u2 = num;
			}
		}
		else if ((double)q < 0.0)
		{
			result = false;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool segment_rect_intersection(Rect bounds, ref Vector2 p1, ref Vector2 p2)
	{
		float u = 0f;
		float u2 = 1f;
		float num = p2.x - p1.x;
		if (clip_test(0f - num, p1.x - bounds.xMin, ref u, ref u2) && clip_test(num, bounds.xMax - p1.x, ref u, ref u2))
		{
			float num2 = p2.y - p1.y;
			if (clip_test(0f - num2, p1.y - bounds.yMin, ref u, ref u2) && clip_test(num2, bounds.yMax - p1.y, ref u, ref u2))
			{
				if ((double)u2 < 1.0)
				{
					p2.x = p1.x + u2 * num;
					p2.y = p1.y + u2 * num2;
				}
				if ((double)u > 0.0)
				{
					p1.x += u * num;
					p1.y += u * num2;
				}
				return true;
			}
		}
		return false;
	}

	public static void BeginGroup(Rect position)
	{
		clippingEnabled = true;
		clippingBounds = new Rect(0f, 0f, position.width, position.height);
		GUI.BeginGroup(position);
	}

	public static void EndGroup()
	{
		GUI.EndGroup();
		clippingBounds = new Rect(0f, 0f, Screen.width, Screen.height);
		clippingEnabled = false;
	}

	public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color)
	{
		if (!clippingEnabled || segment_rect_intersection(clippingBounds, ref pointA, ref pointB))
		{
			if (!lineMaterial)
			{
				createLineMaterial();
			}
			lineMaterial.SetPass(0);
			GL.Begin(1);
			GL.Color(color);
			GL.Vertex3(pointA.x, pointA.y, 0f);
			GL.Vertex3(pointB.x, pointB.y, 0f);
			GL.End();
		}
	}

	public static IntRect RectIntersection(IntRect r1, IntRect r2)
	{
		int num = r1.x;
		int num2 = r1.y;
		int x = r2.x;
		int y = r2.y;
		long num3 = num;
		num3 += r1.width;
		long num4 = num2;
		num4 += r1.height;
		long num5 = x;
		num5 += r2.width;
		long num6 = y;
		num6 += r2.height;
		if (num < x)
		{
			num = x;
		}
		if (num2 < y)
		{
			num2 = y;
		}
		if (num3 > num5)
		{
			num3 = num5;
		}
		if (num4 > num6)
		{
			num4 = num6;
		}
		num3 -= num;
		num4 -= num2;
		if (num3 < int.MinValue)
		{
			num3 = 2147483647L;
		}
		if (num4 < int.MinValue)
		{
			num4 = 2147483647L;
		}
		return new IntRect(num, num2, (int)num3, (int)num4);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void createLineMaterial()
	{
		lineMaterial = new Material(Resources.Load("Shaders/DrawLine", typeof(Shader)) as Shader);
	}

	public static void DrawLine(Vector3 pointA, Vector3 pointB, Color color)
	{
		if (!lineMaterial)
		{
			createLineMaterial();
		}
		GL.PushMatrix();
		lineMaterial.SetPass(0);
		GL.Begin(1);
		GL.Color(color);
		GL.Vertex(pointA);
		GL.Vertex(pointB);
		GL.End();
		GL.PopMatrix();
	}

	public static void SetupLines(Camera _camera, float _lineWidth)
	{
		lineCamera = _camera;
		lineHalfWidth = _lineWidth * 0.5f;
	}

	public static void DrawLineWide(Vector3 pointA, Vector3 pointB, Color color)
	{
		DrawLineWide(pointA, pointB, color, color);
	}

	public static void DrawLineWide(Vector3 pointA, Vector3 pointB, Color colorA, Color colorB)
	{
		if (!lineMaterial)
		{
			createLineMaterial();
		}
		Vector3 vector = lineCamera.WorldToScreenPoint(pointA);
		Vector3 vector2 = lineCamera.WorldToScreenPoint(pointB);
		Vector3 vector3 = Vector3.Cross((vector2 - vector).normalized, Vector3.forward) * lineHalfWidth;
		GL.PushMatrix();
		lineMaterial.SetPass(0);
		GL.Begin(7);
		GL.Color(colorA);
		GL.Vertex(lineCamera.ScreenToWorldPoint(vector - vector3));
		GL.Vertex(lineCamera.ScreenToWorldPoint(vector + vector3));
		GL.Color(colorB);
		GL.Vertex(lineCamera.ScreenToWorldPoint(vector2 + vector3));
		GL.Vertex(lineCamera.ScreenToWorldPoint(vector2 - vector3));
		GL.End();
		GL.PopMatrix();
	}
}
