using System;
using UnityEngine;

public abstract class BlockShapeRotatedAbstract : BlockShape
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3[] vertices;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3 vecInternalOffset = new Vector3(-0.5f, -0.5f, -0.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] aabbVertices;

	[PublicizedFrom(EAccessModifier.Private)]
	public Bounds[][] cachedBounds = new Bounds[32][];

	[PublicizedFrom(EAccessModifier.Protected)]
	public float[] maxAABB_Y = new float[32];

	public BlockShapeRotatedAbstract()
	{
		IsRotatable = true;
	}

	public override void Init(Block _block)
	{
		base.Init(_block);
		createVertices();
		createBoundingBoxes();
		createAABBVertices();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void createBoundingBoxes()
	{
		if (vertices == null)
		{
			return;
		}
		for (int i = 0; i < vertices.Length; i++)
		{
			if (i == 0)
			{
				boundsArr[0] = new Bounds(vertices[0], Vector3.zero);
			}
			else
			{
				boundsArr[0].Encapsulate(vertices[i]);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createAABBVertices()
	{
		if (vertices != null)
		{
			aabbVertices = new Vector3[boundsArr.Length * 2];
			for (int i = 0; i < boundsArr.Length; i++)
			{
				aabbVertices[i * 2] = new Vector3(boundsArr[i].min.x, boundsArr[i].min.y, boundsArr[i].min.z);
				aabbVertices[i * 2 + 1] = new Vector3(boundsArr[i].max.x, boundsArr[i].max.y, boundsArr[i].max.z);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void createVertices();

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3[] rotateVertices(Vector3[] _vertices, Vector3 _drawPos, BlockValue _blockValue)
	{
		Quaternion rotation = GetRotation(_blockValue);
		Vector3[] array = MemoryPools.poolVector3.Alloc(_vertices.Length);
		Vector3 vector = vecInternalOffset;
		Vector3 vector2 = _drawPos - vecInternalOffset + GetRotationOffset(_blockValue);
		for (int i = 0; i < _vertices.Length; i++)
		{
			array[i] = rotation * (_vertices[i] + vector) + vector2;
		}
		return array;
	}

	public override int getFacesDrawnFullBitfield(BlockValue _blockValue)
	{
		return 0;
	}

	public override bool isRenderFace(BlockValue _blockValue, BlockFace _face, BlockValue _adjBlockValue)
	{
		return false;
	}

	public override bool IsRenderDecoration()
	{
		return true;
	}

	public override Bounds[] GetBounds(BlockValue _blockValue)
	{
		int rotation = _blockValue.rotation;
		if (rotation < cachedBounds.Length)
		{
			Bounds[] array = cachedBounds[rotation];
			if (array != null)
			{
				return array;
			}
		}
		if (aabbVertices == null)
		{
			createAABBVertices();
		}
		Vector3[] array2 = (_blockValue.Block.RotateVerticesOnCollisionCheck(_blockValue) ? rotateVertices(aabbVertices, Vector3.zero, _blockValue) : aabbVertices);
		maxAABB_Y[rotation] = 0f;
		for (int i = 0; i < boundsArr.Length; i++)
		{
			boundsArr[i] = new Bounds(array2[i * 2], Vector3.zero);
			boundsArr[i].Encapsulate(array2[i * 2 + 1]);
			maxAABB_Y[rotation] = Utils.FastMax(maxAABB_Y[rotation], boundsArr[i].max.y);
		}
		for (int j = 0; j < boundsArr.Length; j++)
		{
			Vector3 size = boundsArr[j].size;
			for (int k = 0; k < 3; k++)
			{
				size[k] = Math.Max(Math.Max(minBounds[k], 0.05f) - size[k], 0f);
			}
			boundsArr[j].Expand(size);
		}
		if (rotation < cachedBounds.Length)
		{
			Bounds[] array3 = new Bounds[boundsArr.Length];
			boundsArr.CopyTo(array3, 0);
			cachedBounds[rotation] = array3;
		}
		return boundsArr;
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
			else if (b <= 15)
			{
				b = ((!_bLeft) ? ((byte)((b < 15) ? ((uint)(b + 1)) : 12u)) : ((byte)((b > 12) ? ((uint)(b - 1)) : 15u)));
			}
			_blockValue.rotation = b;
		}
		return _blockValue;
	}

	public override float GetStepHeight(BlockValue _blockValue, BlockFace crossingFace)
	{
		GetBounds(_blockValue);
		return maxAABB_Y[_blockValue.rotation];
	}
}
