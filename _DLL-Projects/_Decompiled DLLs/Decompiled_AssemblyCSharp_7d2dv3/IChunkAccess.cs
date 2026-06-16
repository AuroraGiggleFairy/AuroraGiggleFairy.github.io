using System;

public interface IChunkAccess
{
	IChunk GetChunkSync(int chunkX, int chunkZ);

	IChunk GetChunkSync(long chunkKey);

	IChunk GetChunkSync(int chunkX, int chunkY, int chunkZ)
	{
		return DefaultGetChunkSync(this, chunkX, chunkY, chunkZ);
	}

	IChunk GetChunkSync(Vector2i chunkPos)
	{
		return DefaultGetChunkSync(this, chunkPos);
	}

	IChunk GetChunkSync(PropRef propRef)
	{
		return DefaultGetChunkSync(this, propRef);
	}

	IChunk GetChunkSync(BlockValueRef bvRef)
	{
		return DefaultGetChunkSync(this, bvRef);
	}

	IChunk GetChunkFromWorldPos(int x, int z)
	{
		return DefaultGetChunkFromWorldPos(this, x, z);
	}

	IChunk GetChunkFromWorldPos(int x, int y, int z)
	{
		return DefaultGetChunkFromWorldPos(this, x, y, z);
	}

	IChunk GetChunkFromWorldPos(Vector3i blockPos)
	{
		return DefaultGetChunkFromWorldPos(this, blockPos);
	}

	bool GetChunkFromWorldPos(int x, int z, ref IChunk chunk)
	{
		return DefaultGetChunkFromWorldPos(this, x, z, ref chunk);
	}

	bool GetChunkFromWorldPos(Vector3i blockPos, ref IChunk chunk)
	{
		return DefaultGetChunkFromWorldPos(this, blockPos, ref chunk);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	static IChunk DefaultGetChunkSync(IChunkAccess ica, int chunkX, int chunkY, int chunkZ)
	{
		return ica.GetChunkSync(chunkX, chunkZ);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	static IChunk DefaultGetChunkSync(IChunkAccess ica, Vector2i chunkPos)
	{
		return ica.GetChunkSync(chunkPos.x, chunkPos.y);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	static IChunk DefaultGetChunkSync(IChunkAccess ica, PropRef propRef)
	{
		return ica.GetChunkSync(propRef.ChunkPos);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	static IChunk DefaultGetChunkSync(IChunkAccess ica, BlockValueRef bvRef)
	{
		return bvRef.Type switch
		{
			BlockValueRefType.None => null, 
			BlockValueRefType.Block => ica.GetChunkFromWorldPos(bvRef.BlockPosition), 
			BlockValueRefType.Prop => ica.GetChunkSync(bvRef.PropReference), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	static IChunk DefaultGetChunkFromWorldPos(IChunkAccess ica, int x, int z)
	{
		return ica.GetChunkSync(World.toChunkXZ(x), World.toChunkXZ(z));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	static IChunk DefaultGetChunkFromWorldPos(IChunkAccess ica, int x, int y, int z)
	{
		return ica.GetChunkFromWorldPos(x, z);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	static IChunk DefaultGetChunkFromWorldPos(IChunkAccess ica, Vector3i blockPos)
	{
		return ica.GetChunkFromWorldPos(blockPos.x, blockPos.y, blockPos.z);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	static bool DefaultGetChunkFromWorldPos(IChunkAccess ica, int x, int z, ref IChunk chunk)
	{
		int num = World.toChunkXZ(x);
		int num2 = World.toChunkXZ(z);
		if (chunk == null || chunk.X != num || chunk.Z != num2)
		{
			chunk = ica.GetChunkSync(num, num2);
		}
		return chunk != null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	static bool DefaultGetChunkFromWorldPos(IChunkAccess ica, Vector3i blockPos, ref IChunk chunk)
	{
		return ica.GetChunkFromWorldPos(blockPos.x, blockPos.z, ref chunk);
	}
}
