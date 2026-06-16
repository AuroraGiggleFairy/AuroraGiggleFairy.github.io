using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageLockResponse : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool locking;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool success;

	[PublicizedFrom(EAccessModifier.Private)]
	public string errorMsg = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public ILockTarget[] targets;

	[PublicizedFrom(EAccessModifier.Private)]
	public ILockContext context;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort channel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isForceUnlocked;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageLockResponse Setup(bool _success, string _errorMsg, ReadOnlySpan<ILockTarget> _targets, ILockContext _context, ushort _channel = 0)
	{
		if (_targets == null || _targets.Length == 0)
		{
			Log.Error("[NetPackageLockResponse] No lock targets supplied.");
			return null;
		}
		locking = true;
		success = _success;
		errorMsg = _errorMsg ?? string.Empty;
		targets = new ILockTarget[_targets.Length];
		for (int i = 0; i < _targets.Length; i++)
		{
			targets[i] = _targets[i];
		}
		context = _context;
		channel = _channel;
		return this;
	}

	public NetPackageLockResponse Setup(bool _success, string _errorMsg, bool _isForceUnlocked)
	{
		locking = false;
		success = _success;
		errorMsg = _errorMsg ?? string.Empty;
		isForceUnlocked = _isForceUnlocked;
		targets = null;
		context = null;
		channel = 0;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		locking = _br.ReadBoolean();
		success = _br.ReadBoolean();
		errorMsg = _br.ReadString();
		isForceUnlocked = _br.ReadBoolean();
		channel = _br.ReadUInt16();
		int num = _br.ReadInt32();
		if (num > 0)
		{
			targets = new ILockTarget[num];
			for (int i = 0; i < num; i++)
			{
				targets[i] = ILockTarget.ReadIdentifyingInfo(_br);
			}
		}
		string text = _br.ReadString();
		if (string.IsNullOrEmpty(text))
		{
			context = null;
			return;
		}
		Type type = Type.GetType(text);
		if (type == null)
		{
			Log.Error("[NetPackageLockResponse] Unknown context type: " + text);
			context = null;
		}
		else
		{
			context = (ILockContext)Activator.CreateInstance(type);
			context.Read(_br);
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(locking);
		_bw.Write(success);
		_bw.Write(errorMsg);
		_bw.Write(isForceUnlocked);
		_bw.Write(channel);
		ILockTarget[] array = targets;
		_bw.Write((array != null) ? array.Length : 0);
		if (targets != null)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				ILockTarget.WriteIdentifyingInfo(targets[i], _bw);
			}
		}
		if (context == null)
		{
			_bw.Write(string.Empty);
			return;
		}
		_bw.Write(context.GetType().FullName);
		context.Write(_bw);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!locking)
		{
			LockManager.Instance.UnlockResponse(success, errorMsg, isForceUnlocked);
		}
		else
		{
			LockManager.Instance.LockResponse(success, errorMsg, targets.AsSpan(), context, channel);
		}
	}

	public override int GetLength()
	{
		return 0;
	}
}
