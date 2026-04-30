using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageConsoleCmdClient : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> lines;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bExecute;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageConsoleCmdClient Setup(List<string> _lines, bool _bExecute)
	{
		lines = _lines;
		bExecute = _bExecute;
		return this;
	}

	public NetPackageConsoleCmdClient Setup(string _line, bool _bExecute)
	{
		lines = new List<string> { _line };
		bExecute = _bExecute;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		int num = _br.ReadInt32();
		lines = new List<string>(num);
		for (int i = 0; i < num; i++)
		{
			lines.Add(_br.ReadString());
		}
		bExecute = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(lines.Count);
		for (int i = 0; i < lines.Count; i++)
		{
			_bw.Write(lines[i]);
		}
		_bw.Write(bExecute);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			if (bExecute)
			{
				GameManager.Instance.m_GUIConsole.AddLines(SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(lines[0], null));
			}
			else
			{
				GameManager.Instance.m_GUIConsole.AddLines(lines);
			}
		}
	}

	public override int GetLength()
	{
		return 40;
	}
}
