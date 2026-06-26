using UnityEngine.Scripting;

[Preserve]
public class NetPackageWaypoint : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Waypoint waypoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumWaypointInviteMode inviteMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public int inviterEntityId;

	public NetPackageWaypoint Setup(Waypoint _waypoint, EnumWaypointInviteMode _inviteMode, int _inviterEntityId)
	{
		waypoint = _waypoint;
		waypoint.InviterEntityId = _inviterEntityId;
		inviteMode = _inviteMode;
		inviterEntityId = _inviterEntityId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		waypoint = new Waypoint();
		waypoint.Read(_br);
		inviteMode = (EnumWaypointInviteMode)_br.ReadByte();
		inviterEntityId = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		waypoint.Write(_bw);
		_bw.Write((byte)inviteMode);
		_bw.Write(inviterEntityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (ValidEntityIdForSender(inviterEntityId))
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				GameManager.Instance.WaypointInviteServer(waypoint, inviteMode, inviterEntityId);
			}
			else
			{
				GameManager.Instance.WaypointInviteClient(waypoint, inviteMode, inviterEntityId);
			}
		}
	}

	public override int GetLength()
	{
		return 4;
	}
}
