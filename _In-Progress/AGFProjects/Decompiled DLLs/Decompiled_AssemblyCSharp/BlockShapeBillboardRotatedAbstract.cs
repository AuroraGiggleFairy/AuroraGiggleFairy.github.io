public abstract class BlockShapeBillboardRotatedAbstract : BlockShapeRotatedAbstract
{
	public BlockShapeBillboardRotatedAbstract()
	{
		IsSolidCube = false;
		IsSolidSpace = false;
		IsRotatable = true;
		LightOpacity = 0;
	}

	public override void Init(Block _block)
	{
		base.Init(_block);
		_block.IsDecoration = true;
	}

	public override bool IsRenderDecoration()
	{
		return true;
	}

	public override bool isRenderFace(BlockValue _blockValue, BlockFace _face, BlockValue _adjBlockValue)
	{
		return false;
	}

	public override int getFacesDrawnFullBitfield(BlockValue _blockValue)
	{
		return 0;
	}

	public override BlockValue RotateY(bool _bLeft, BlockValue _blockValue, int _rotCount)
	{
		for (int i = 0; i < _rotCount; i++)
		{
			byte b = _blockValue.rotation;
			if (b <= 3)
			{
				b = ((!_bLeft) ? ((byte)((b < 3) ? ((uint)(b + 1)) : 0u)) : ((byte)((b > 0) ? ((uint)(b - 1)) : 3u)));
			}
			else if (b <= 7)
			{
				b = ((!_bLeft) ? ((byte)((b < 7) ? ((uint)(b + 1)) : 4u)) : ((byte)((b > 4) ? ((uint)(b - 1)) : 7u)));
			}
			else if (b <= 11)
			{
				b = ((!_bLeft) ? ((byte)((b < 11) ? ((uint)(b + 1)) : 8u)) : ((byte)((b > 8) ? ((uint)(b - 1)) : 11u)));
			}
			_blockValue.rotation = b;
		}
		return _blockValue;
	}
}
