using UnityEngine.Scripting;

[Preserve]
public class NetPackageNetMetrics : NetPackage
{
	public bool enable;

	public float duration;

	public bool loop;

	public string content;

	public string csv;

	public static NetPackageNetMetrics SetupClient(string content, string csv)
	{
		NetPackageNetMetrics package = NetPackageManager.GetPackage<NetPackageNetMetrics>();
		package.content = content;
		package.csv = csv;
		return package;
	}

	public static NetPackageNetMetrics SetupServer(bool enable, float duration, bool loop)
	{
		NetPackageNetMetrics package = NetPackageManager.GetPackage<NetPackageNetMetrics>();
		package.enable = enable;
		package.duration = duration;
		package.loop = loop;
		return package;
	}

	public override void read(PooledBinaryReader _reader)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			content = _reader.ReadString();
			csv = _reader.ReadString();
		}
		else
		{
			enable = _reader.ReadBoolean();
			duration = _reader.ReadSingle();
			loop = _reader.ReadBoolean();
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			_writer.Write(enable);
			_writer.Write(duration);
			_writer.Write(loop);
		}
		else
		{
			_writer.Write(content);
			_writer.Write(csv);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Out("RECEIVED STATS BACK");
			Log.Out(content);
			Log.Out(csv);
			GameManager.Instance.netpackageMetrics.AppendClientCSV(csv);
		}
		else
		{
			Log.Out("REQUESTED TO RECORD STATS");
			if (enable)
			{
				NetPackageMetrics.Instance.RecordForPeriod(duration);
			}
		}
	}

	public override int GetLength()
	{
		return 0;
	}
}
