using System;

public struct Vector2F : IEquatable<Vector2F>
{
	public float Y;

	public float X;

	public static readonly Vector2F Zero;

	public double Length => Math.Sqrt(X * X + Y * Y);

	public Vector2F(double angle)
	{
		X = (float)Math.Sin(angle);
		Y = (float)Math.Cos(angle);
	}

	public Vector2F(float x, float y)
	{
		X = x;
		Y = y;
	}

	public static double Lengthsquared(Vector2F a)
	{
		return a.X * a.X + a.Y * a.Y;
	}

	public static Vector2F operator -(Vector2F a, Vector2F b)
	{
		return new Vector2F(a.X - b.X, a.Y - b.Y);
	}

	public static bool operator ==(Vector2F a, Vector2F b)
	{
		if (a.X == b.X)
		{
			return a.Y == b.Y;
		}
		return false;
	}

	public static bool operator !=(Vector2F a, Vector2F b)
	{
		if (a.X == b.X)
		{
			return a.Y != b.Y;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return X.GetHashCode() * Y.GetHashCode();
	}

	public double Lengthsquared()
	{
		return X * X + Y * Y;
	}

	public override bool Equals(object obj)
	{
		return this == (Vector2F)obj;
	}

	public bool Equals(Vector2F other)
	{
		if (other.X == X)
		{
			return other.Y == Y;
		}
		return false;
	}

	public override string ToString()
	{
		return X.ToCultureInvariantString() + ", " + Y.ToCultureInvariantString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static Vector2F()
	{
		Zero = new Vector2F(0f, 0f);
	}
}
