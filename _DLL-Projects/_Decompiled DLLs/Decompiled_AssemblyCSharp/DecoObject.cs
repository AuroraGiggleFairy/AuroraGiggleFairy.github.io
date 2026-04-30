using System;
using System.IO;
using UnityEngine;

public class DecoObject : IEquatable<DecoObject>
{
	public Vector3i pos;

	public float realYPos;

	public BlockValue bv;

	public DecoState state;

	public GameObjectPool.AsyncItem asyncItem;

	public GameObject go;

	public void Init(Vector3i _pos, float _realYPos, BlockValue _bv, DecoState _state)
	{
		pos = _pos;
		realYPos = _realYPos;
		bv = _bv;
		state = _state;
		asyncItem = null;
		go = null;
	}

	public string GetModelName()
	{
		Block block = bv.Block;
		if (block == null)
		{
			Log.Error($"DecoObject '{bv}', no block!");
			return null;
		}
		string text = block.Properties.Values["Model"];
		if (string.IsNullOrEmpty(text))
		{
			Log.Error("DecoObject block '" + block.GetBlockName() + "', no model!");
			return null;
		}
		return GameIO.GetFilenameFromPathWithoutExtension(text);
	}

	public void CreateGameObject(DecoChunk _decoChunk, Transform _parent)
	{
		string modelName = GetModelName();
		if (modelName != null)
		{
			GameObject objectForType = GameObjectPool.Instance.GetObjectForType(modelName);
			CreateGameObjectCallback(objectForType, _parent, _isAsync: false);
		}
	}

	public void CreateGameObjectCallback(GameObject _obj, Transform _parent, bool _isAsync)
	{
		go = _obj;
		if (_isAsync && asyncItem == null)
		{
			Destroy();
			return;
		}
		asyncItem = null;
		Block block = bv.Block;
		if (!(block.shape is BlockShapeDistantDeco blockShapeDistantDeco))
		{
			Log.Error("Block '{0}' needs a deco shape assigned but has not!", block.GetBlockName());
			return;
		}
		Transform transform = go.transform;
		transform.SetParent(_parent, worldPositionStays: false);
		float y = blockShapeDistantDeco.modelOffset.y;
		transform.position = new Vector3((float)pos.x + DecoManager.cDecoMiddleOffset.x, realYPos + y, (float)pos.z + DecoManager.cDecoMiddleOffset.z) - Origin.position;
		int num = bv.rotation;
		if (!blockShapeDistantDeco.Has45DegreeRotations)
		{
			num &= 3;
		}
		transform.localRotation = BlockShapeNew.GetRotationStatic(num);
		go.SetActive(value: true);
		BlockEntityData blockEntityData = new BlockEntityData();
		blockEntityData.transform = transform;
		blockShapeDistantDeco.OnBlockEntityTransformAfterActivated(null, pos, bv, blockEntityData);
	}

	public void Destroy()
	{
		asyncItem = null;
		if ((bool)go)
		{
			GameObjectPool.Instance.PoolObjectAsync(go);
			go = null;
		}
	}

	public void Write(BinaryWriter _bw, NameIdMapping _blockMap = null)
	{
		_bw.Write(GameUtils.Vector3iToUInt64(pos));
		_bw.Write(realYPos);
		_bw.Write(bv.rawData);
		_bw.Write((byte)state);
		Block block = bv.Block;
		if (block == null)
		{
			Log.Error($"Writing DecoObject '{bv}', no block!");
		}
		else
		{
			_blockMap?.AddMapping(block.blockID, block.GetBlockName());
		}
	}

	public void Read(BinaryReader _br)
	{
		pos = GameUtils.UInt64ToVector3i(_br.ReadUInt64());
		realYPos = _br.ReadSingle();
		bv = new BlockValue(_br.ReadUInt32());
		state = (DecoState)_br.ReadByte();
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(pos.GetHashCode(), state, bv.GetHashCode());
	}

	public override bool Equals(object _obj)
	{
		if (_obj == null)
		{
			return false;
		}
		if (this == _obj)
		{
			return true;
		}
		if (_obj is DecoObject other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(DecoObject _other)
	{
		if (_other == null)
		{
			return false;
		}
		if (this == _other)
		{
			return true;
		}
		if (_other.pos.Equals(pos) && _other.state == state)
		{
			return _other.bv.Equals(bv);
		}
		return false;
	}
}
