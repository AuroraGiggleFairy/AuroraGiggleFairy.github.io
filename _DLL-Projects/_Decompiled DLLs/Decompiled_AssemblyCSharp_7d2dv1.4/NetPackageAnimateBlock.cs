using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageAnimateBlock : NetPackage
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3i blockPosition;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string animParamater;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int animType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int animationInteger;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool animationBool;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string animationTrigger;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageAnimateBlock Setup(Vector3i _blockPosition, string _animParamater, int _animationInteger = 0)
	{
		blockPosition = _blockPosition;
		animParamater = _animParamater;
		animationInteger = _animationInteger;
		animType = 0;
		return this;
	}

	public NetPackageAnimateBlock Setup(Vector3i _blockPosition, string _animParamater, bool _animationBool = false)
	{
		blockPosition = _blockPosition;
		animParamater = _animParamater;
		animationBool = _animationBool;
		animType = 1;
		return this;
	}

	public NetPackageAnimateBlock Setup(Vector3i _blockPosition, string _animParamater)
	{
		blockPosition = _blockPosition;
		animParamater = _animParamater;
		animType = 2;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		blockPosition = StreamUtils.ReadVector3i(_reader);
		animParamater = _reader.ReadString();
		animType = _reader.ReadInt32();
		animationInteger = _reader.ReadInt32();
		animationBool = _reader.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		StreamUtils.Write(_writer, blockPosition);
		_writer.Write(animParamater);
		_writer.Write(animType);
		_writer.Write(animationInteger);
		_writer.Write(animationBool);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		Chunk chunk = (Chunk)_world.GetChunkFromWorldPos(blockPosition);
		if (chunk == null)
		{
			return;
		}
		BlockEntityData blockEntity = _world.ChunkClusters[chunk.ClrIdx].GetBlockEntity(blockPosition);
		if (blockEntity != null)
		{
			if (blockEntity.transform == null)
			{
				GameManager.Instance.StartCoroutine(WaitForBEDTransform(blockEntity));
			}
			else
			{
				AnimateBlock(blockEntity);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator WaitForBEDTransform(BlockEntityData bed)
	{
		for (int frames = 0; frames < 10; frames++)
		{
			yield return 0;
			if (bed == null)
			{
				break;
			}
			if (bed.transform != null)
			{
				AnimateBlock(bed);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AnimateBlock(BlockEntityData bed)
	{
		Animator[] componentsInChildren = bed.transform.GetComponentsInChildren<Animator>();
		if (componentsInChildren == null)
		{
			return;
		}
		for (int num = componentsInChildren.Length - 1; num >= 0; num--)
		{
			Animator animator = componentsInChildren[num];
			animator.enabled = true;
			switch (animType)
			{
			case 0:
				animator.SetInteger(animParamater, animationInteger);
				break;
			case 1:
				animator.SetBool(animParamater, animationBool);
				break;
			case 2:
				animator.SetTrigger(animParamater);
				break;
			}
		}
	}

	public override int GetLength()
	{
		return 0;
	}
}
