using System.IO;
using UnityEngine;

public class BuffValue
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum BuffFlags : byte
	{
		None = 0,
		Started = 1,
		Finished = 2,
		Remove = 4,
		Update = 8,
		Invalid = 0x10,
		Paused = 0x20
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BuffClass cachedBuff;

	[PublicizedFrom(EAccessModifier.Private)]
	public string buffName;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte stackEffectMultiplier;

	[PublicizedFrom(EAccessModifier.Private)]
	public uint durationTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int instigatorId;

	[PublicizedFrom(EAccessModifier.Private)]
	public BuffFlags buffFlags;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort timeSinceLastUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i instigatorPos;

	public BuffClass BuffClass
	{
		get
		{
			if (cachedBuff == null && !BuffManager.Buffs.TryGetValue(buffName, out cachedBuff))
			{
				Log.Error("Buff Class not found for '{0}'", buffName);
			}
			return cachedBuff;
		}
	}

	public bool Remove
	{
		get
		{
			return (buffFlags & BuffFlags.Remove) != 0;
		}
		set
		{
			if (value)
			{
				buffFlags |= BuffFlags.Remove;
			}
			else
			{
				buffFlags &= (BuffFlags)251;
			}
		}
	}

	public bool Finished
	{
		get
		{
			return (buffFlags & BuffFlags.Finished) != 0;
		}
		set
		{
			if (value)
			{
				buffFlags |= BuffFlags.Finished;
			}
			else
			{
				buffFlags &= (BuffFlags)253;
			}
		}
	}

	public bool Started
	{
		get
		{
			return (buffFlags & BuffFlags.Started) != 0;
		}
		set
		{
			if (value)
			{
				buffFlags |= BuffFlags.Started;
			}
			else
			{
				buffFlags &= (BuffFlags)254;
			}
		}
	}

	public bool Invalid
	{
		get
		{
			return (buffFlags & BuffFlags.Invalid) != 0;
		}
		set
		{
			if (value)
			{
				buffFlags |= BuffFlags.Invalid;
			}
			else
			{
				buffFlags &= (BuffFlags)239;
			}
		}
	}

	public bool Update
	{
		get
		{
			return (buffFlags & BuffFlags.Update) != 0;
		}
		set
		{
			if (value)
			{
				buffFlags |= BuffFlags.Update;
			}
			else
			{
				buffFlags &= (BuffFlags)247;
			}
		}
	}

	public bool Paused
	{
		get
		{
			return (buffFlags & BuffFlags.Paused) != 0;
		}
		set
		{
			if (value)
			{
				buffFlags |= BuffFlags.Paused;
			}
			else
			{
				buffFlags &= (BuffFlags)223;
			}
		}
	}

	public int StackEffectMultiplier
	{
		get
		{
			return stackEffectMultiplier;
		}
		set
		{
			stackEffectMultiplier = (byte)Mathf.Clamp(value, 0, 255);
		}
	}

	public float DurationInSeconds => (float)durationTicks / 20f;

	public uint DurationInTicks
	{
		get
		{
			return durationTicks;
		}
		set
		{
			if (value == 0)
			{
				durationTicks = 0u;
				timeSinceLastUpdate = 0;
				return;
			}
			timeSinceLastUpdate += (ushort)(value - durationTicks);
			durationTicks = value;
			if (timeSinceLastUpdate == Mathf.FloorToInt(BuffClass.UpdateRate * 20f))
			{
				Update = true;
				timeSinceLastUpdate = 0;
			}
		}
	}

	public string BuffName => buffName;

	public int InstigatorId => instigatorId;

	public Vector3i InstigatorPos => instigatorPos;

	public BuffValue()
	{
	}

	public BuffValue(string _buffEffectGroupId, Vector3i _instigatorPos, int _instigatorId = -1, BuffClass _buffClass = null)
	{
		buffName = _buffEffectGroupId;
		stackEffectMultiplier = 1;
		durationTicks = 0u;
		instigatorId = _instigatorId;
		buffFlags = BuffFlags.None;
		timeSinceLastUpdate = 0;
		instigatorPos = _instigatorPos;
		if (_buffClass == null)
		{
			cacheBuffClassPointer();
		}
		else
		{
			cachedBuff = _buffClass;
		}
	}

	public void ClearBuffClassLink()
	{
		cachedBuff = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cacheBuffClassPointer()
	{
		if (BuffManager.Buffs.ContainsKey(buffName))
		{
			cachedBuff = BuffManager.Buffs[buffName];
		}
		else
		{
			Remove = true;
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(buffName);
		_bw.Write(stackEffectMultiplier);
		_bw.Write(durationTicks);
		_bw.Write(instigatorId);
		_bw.Write((byte)buffFlags);
		_bw.Write(timeSinceLastUpdate);
		StreamUtils.Write(_bw, instigatorPos);
	}

	public void Read(BinaryReader _br, int _version)
	{
		if (_version < 2)
		{
			int num = _br.ReadInt32();
			foreach (string key in BuffManager.Buffs.Keys)
			{
				if (key.GetHashCode() == num)
				{
					buffName = BuffManager.Buffs[key].Name;
					break;
				}
			}
		}
		else
		{
			buffName = _br.ReadString().ToLower();
		}
		stackEffectMultiplier = _br.ReadByte();
		durationTicks = _br.ReadUInt32();
		instigatorId = _br.ReadInt32();
		buffFlags = (BuffFlags)_br.ReadByte();
		if (_version == 0)
		{
			timeSinceLastUpdate = _br.ReadByte();
		}
		else
		{
			timeSinceLastUpdate = _br.ReadUInt16();
		}
		if (_version >= 3)
		{
			instigatorPos = StreamUtils.ReadVector3i(_br);
		}
		cacheBuffClassPointer();
	}
}
