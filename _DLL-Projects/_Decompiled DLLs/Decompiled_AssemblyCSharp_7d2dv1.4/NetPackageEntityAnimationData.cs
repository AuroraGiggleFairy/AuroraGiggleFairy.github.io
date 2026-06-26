using System.Collections.Generic;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityAnimationData : NetPackage, IMemoryPoolableObject
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IList<AnimParamData> animationParameterData = new List<AnimParamData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CustomSampler getSampler = CustomSampler.Create("NetPackageEntityAnimationData.read");

	public NetPackageEntityAnimationData Setup(int _entityId, IList<AnimParamData> _animationParameterData)
	{
		entityId = _entityId;
		_animationParameterData.CopyTo(animationParameterData);
		return this;
	}

	public NetPackageEntityAnimationData Setup(int _entityId, Dictionary<int, AnimParamData> _animationParameterData)
	{
		entityId = _entityId;
		_animationParameterData.CopyValuesTo(animationParameterData);
		return this;
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write(animationParameterData.Count);
		for (int i = 0; i < animationParameterData.Count; i++)
		{
			animationParameterData[i].Write(_writer);
		}
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		int num = _reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			animationParameterData.Add(AnimParamData.CreateFromBinary(_reader));
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(entityId) as EntityAlive;
		if (entityAlive == null || !entityAlive.isEntityRemote)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAnimationData>().Setup(entityId, animationParameterData), _onlyClientsAttachedToAnEntity: false, -1, entityId, entityId);
		}
		if (entityAlive.emodel == null)
		{
			return;
		}
		AvatarController avatarController = entityAlive.emodel.avatarController;
		if (avatarController == null)
		{
			return;
		}
		for (int i = 0; i < animationParameterData.Count; i++)
		{
			int nameHash = animationParameterData[i].NameHash;
			switch (animationParameterData[i].ValueType)
			{
			case AnimParamData.ValueTypes.Bool:
				avatarController.UpdateBool(nameHash, animationParameterData[i].IntValue != 0);
				break;
			case AnimParamData.ValueTypes.Trigger:
				if (animationParameterData[i].IntValue != 0)
				{
					avatarController.TriggerEvent(nameHash);
				}
				else
				{
					avatarController.CancelEvent(nameHash);
				}
				break;
			case AnimParamData.ValueTypes.Float:
				avatarController.UpdateFloat(nameHash, animationParameterData[i].FloatValue);
				break;
			case AnimParamData.ValueTypes.Int:
				avatarController.UpdateInt(nameHash, animationParameterData[i].IntValue);
				break;
			case AnimParamData.ValueTypes.DataFloat:
				avatarController.SetDataFloat((AvatarController.DataTypes)nameHash, animationParameterData[i].FloatValue);
				break;
			}
		}
	}

	public override int GetLength()
	{
		return 0;
	}

	public void Reset()
	{
		entityId = 0;
		animationParameterData.Clear();
	}

	public void Cleanup()
	{
		Reset();
	}
}
