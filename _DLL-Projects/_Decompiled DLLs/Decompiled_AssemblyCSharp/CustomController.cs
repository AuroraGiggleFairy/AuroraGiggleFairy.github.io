using System;
using UnityEngine;

public class CustomController : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float m_BoxWidth = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_Velocity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_Forward;

	public float Speed = 0.1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public World m_WorldData;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CollidesWithX(Vector3 position, float movement, out float newVelocity)
	{
		float num = Speed * (float)Math.Sign(movement);
		Vector3 vector = new Vector3(position.x + num + m_BoxWidth, position.y, position.z);
		if (!m_WorldData.GetBlock((int)vector.x, (int)vector.y, (int)vector.z).Equals(BlockValue.Air))
		{
			newVelocity = 0f;
			return true;
		}
		newVelocity = movement;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CollidesWithY(Vector3 position, float movement, out float newVelocity)
	{
		float num = Speed * (float)Math.Sign(movement);
		Vector3 vector = new Vector3(position.x, position.y + num + m_BoxWidth, position.z);
		Log.Out("Checking " + vector.x.ToCultureInvariantString() + ", " + vector.z.ToCultureInvariantString() + ", " + vector.y.ToCultureInvariantString());
		BlockValue block = m_WorldData.GetBlock((int)vector.x, (int)vector.z, (int)vector.y);
		if (!block.Equals(BlockValue.Air))
		{
			string[] obj = new string[8] { "Block ", null, null, null, null, null, null, null };
			BlockValue blockValue = block;
			obj[1] = blockValue.ToString();
			obj[2] = " hit at ";
			obj[3] = vector.x.ToCultureInvariantString();
			obj[4] = ", ";
			obj[5] = vector.z.ToCultureInvariantString();
			obj[6] = ", ";
			obj[7] = vector.y.ToCultureInvariantString();
			Log.Out(string.Concat(obj));
			newVelocity = 0f;
			return true;
		}
		newVelocity = movement;
		return false;
	}
}
