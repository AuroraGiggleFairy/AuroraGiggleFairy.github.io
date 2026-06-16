using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageNavObject : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string navObjectClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool usingLocalizationId;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isAdd;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useOverrideColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color overrideColor;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageNavObject Setup(string _navObjectClass, string _displayName, Vector3 _position, bool _isAdd, bool _usingLocalizationId, int _entityId = -1)
	{
		navObjectClass = _navObjectClass;
		name = _displayName;
		position = _position;
		isAdd = _isAdd;
		usingLocalizationId = _usingLocalizationId;
		entityId = _entityId;
		return this;
	}

	public NetPackageNavObject Setup(int _entityId)
	{
		navObjectClass = "";
		name = "";
		position = Vector3.zero;
		isAdd = false;
		usingLocalizationId = false;
		entityId = _entityId;
		return this;
	}

	public NetPackageNavObject Setup(string _navObjectClass, string _displayName, Vector3 _position, bool _isAdd, Color _overrideColor, bool _usingLocalizationId)
	{
		navObjectClass = _navObjectClass;
		name = _displayName;
		position = _position;
		isAdd = _isAdd;
		usingLocalizationId = _usingLocalizationId;
		useOverrideColor = true;
		overrideColor = _overrideColor;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		navObjectClass = _br.ReadString();
		name = _br.ReadString();
		position = StreamUtils.ReadVector3(_br);
		isAdd = _br.ReadBoolean();
		useOverrideColor = _br.ReadBoolean();
		overrideColor = StreamUtils.ReadColor32(_br);
		usingLocalizationId = _br.ReadBoolean();
		entityId = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(navObjectClass);
		_bw.Write(name ?? "");
		StreamUtils.Write(_bw, position);
		_bw.Write(isAdd);
		_bw.Write(useOverrideColor);
		StreamUtils.WriteColor32(_bw, overrideColor);
		_bw.Write(usingLocalizationId);
		_bw.Write(entityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || !_world.IsRemote())
		{
			return;
		}
		if (isAdd)
		{
			NavObject navObject = ((entityId == -1) ? null : NavObjectManager.Instance.GetNavObjectByEntityID(entityId));
			if (navObject == null || navObject.TrackType != NavObject.TrackTypes.Entity)
			{
				NavObject navObject2 = NavObjectManager.Instance.RegisterNavObject(navObjectClass, position);
				navObject2.name = name;
				navObject2.usingLocalizationId = usingLocalizationId;
				navObject2.EntityID = entityId;
				if (useOverrideColor)
				{
					navObject2.UseOverrideColor = true;
					navObject2.OverrideColor = overrideColor;
				}
			}
		}
		else if (entityId != -1)
		{
			NavObjectManager.Instance.UnRegisterNavObjectByEntityID(entityId);
		}
		else
		{
			NavObjectManager.Instance.UnRegisterNavObjectByPosition(position, navObjectClass);
		}
	}

	public override int GetLength()
	{
		return 30;
	}
}
