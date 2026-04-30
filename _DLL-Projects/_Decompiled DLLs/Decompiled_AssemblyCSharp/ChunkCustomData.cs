using System.IO;

public class ChunkCustomData
{
	public delegate void TriggerWriteData();

	public string key;

	public ulong expiresInWorldTime;

	public bool isSavedToNetwork;

	public byte[] data;

	public TriggerWriteData TriggerWriteDataDelegate;

	public ChunkCustomData()
	{
	}

	public ChunkCustomData(string _key, ulong _expiresInWorldTime, bool _isSavedToNetwork)
	{
		key = _key;
		expiresInWorldTime = _expiresInWorldTime;
		isSavedToNetwork = _isSavedToNetwork;
	}

	public void Read(BinaryReader _br)
	{
		key = _br.ReadString();
		expiresInWorldTime = _br.ReadUInt64();
		isSavedToNetwork = _br.ReadBoolean();
		int num = _br.ReadUInt16();
		if (num > 0)
		{
			data = _br.ReadBytes(num);
		}
		else
		{
			data = null;
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(key);
		_bw.Write(expiresInWorldTime);
		_bw.Write(isSavedToNetwork);
		if (TriggerWriteDataDelegate != null)
		{
			TriggerWriteDataDelegate();
		}
		_bw.Write((ushort)((data != null) ? ((uint)data.Length) : 0u));
		if (data != null && data.Length != 0)
		{
			_bw.Write(data);
		}
	}

	public virtual void OnRemove(Chunk chunk)
	{
	}
}
