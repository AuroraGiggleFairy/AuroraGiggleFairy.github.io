using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageBlockLimitTracking : NetPackage
{
	public List<int> amounts;

	public NetPackageBlockLimitTracking()
	{
		amounts = new List<int>();
	}

	public NetPackageBlockLimitTracking Setup(List<int> _amounts)
	{
		amounts = _amounts;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		amounts.Clear();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			amounts.Add(_br.ReadInt32());
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(amounts.Count);
		for (int i = 0; i < amounts.Count; i++)
		{
			_bw.Write(amounts[i]);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Warning("Server should not receive a NetPackageBlockLimitTracking");
		}
		else
		{
			BlockLimitTracker.instance.UpdateClientAmounts(amounts);
		}
	}

	public override int GetLength()
	{
		return 4 + amounts.Count * 4;
	}
}
