using System.IO;
using DoomAutoPickup;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDoomAutoPickup : NetPackage
{
	private ItemStack[] _stacks = (ItemStack[])(object)new ItemStack[0];

	public override NetPackageDirection PackageDirection => (NetPackageDirection)2;

	public NetPackageDoomAutoPickup Setup(ItemStack[] stacks)
	{
		_stacks = (ItemStack[])(((object)stacks) ?? ((object)new ItemStack[0]));
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		int num = ((BinaryReader)(object)_br).ReadUInt16();
		_stacks = (ItemStack[])(object)new ItemStack[num];
		for (int i = 0; i < num; i++)
		{
			_stacks[i] = new ItemStack().Read((BinaryReader)(object)_br);
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		((NetPackage)this).write(_bw);
		((BinaryWriter)(object)_bw).Write((ushort)_stacks.Length);
		for (int i = 0; i < _stacks.Length; i++)
		{
			_stacks[i].Write((BinaryWriter)(object)_bw);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			AutoPickup.Give(((WorldBase)_world).GetPrimaryPlayer(), _stacks);
		}
	}

	public override int GetLength()
	{
		return 4 + _stacks.Length * 16;
	}
}
