using UnityEngine;

public class PropReference : MonoBehaviour
{
	public int ChunkX;

	public int ChunkZ;

	public int PropId;

	public Vector2i ChunkPos
	{
		get
		{
			return new Vector2i(ChunkX, ChunkZ);
		}
		set
		{
			ChunkX = value.x;
			ChunkZ = value.y;
		}
	}

	public PropRef PropRef
	{
		get
		{
			return new PropRef
			{
				ChunkPos = new Vector2i(ChunkX, ChunkZ),
				PropId = PropId
			};
		}
		set
		{
			ChunkX = value.ChunkPos.x;
			ChunkZ = value.ChunkPos.y;
			PropId = value.PropId;
		}
	}

	public override string ToString()
	{
		return PropRef.ToString();
	}
}
