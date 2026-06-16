using System;

public interface IBlockAccess
{
	BlockValue GetBlock(int x, int y, int z);

	BlockValue GetBlock(Vector3i pos)
	{
		return DefaultGetBlock(this, pos);
	}

	BlockValue GetBlock(BlockValueRef bvRef)
	{
		return DefaultGetBlock(this, bvRef);
	}

	PropValue GetProp(int chunkX, int chunkZ, int propId);

	PropValue GetProp(long chunkKey, int propId);

	PropValue GetProp(Vector2i chunkPos, int propId)
	{
		return DefaultGetProp(this, chunkPos, propId);
	}

	PropValue GetProp(PropRef propRef)
	{
		return DefaultGetProp(this, propRef);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	static BlockValue DefaultGetBlock(IBlockAccess iba, Vector3i pos)
	{
		return iba.GetBlock(pos.x, pos.y, pos.z);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	static BlockValue DefaultGetBlock(IBlockAccess iba, BlockValueRef bvRef)
	{
		return bvRef.Type switch
		{
			BlockValueRefType.None => BlockValue.Air, 
			BlockValueRefType.Block => iba.GetBlock(bvRef.BlockPosition), 
			BlockValueRefType.Prop => iba.GetProp(bvRef.PropReference).blockValue, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	static PropValue DefaultGetProp(IBlockAccess iba, Vector2i chunkPos, int propId)
	{
		return iba.GetProp(chunkPos.x, chunkPos.y, propId);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	static PropValue DefaultGetProp(IBlockAccess iba, PropRef propRef)
	{
		return iba.GetProp(propRef.ChunkPos, propRef.PropId);
	}
}
