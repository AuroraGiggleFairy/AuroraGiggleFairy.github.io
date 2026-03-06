using System.IO;

public class Waypoint
{
	public Vector3i pos;

	public string icon;

	public AuthoredText name;

	public bool bTracked;

	public PlatformUserIdentifierAbs ownerId;

	public int lastKnownPositionEntityId = -1;

	public eLastKnownPositionEntityType lastKnownPositionEntityType;

	public long MapObjectKey;

	public bool hiddenOnCompass;

	public NavObject navObject;

	public bool bIsAutoWaypoint;

	public bool bUsingLocalizationId;

	public bool IsSaved = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int inviterEntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hiddenOnMap;

	public int InviterEntityId
	{
		get
		{
			return inviterEntityId;
		}
		set
		{
			inviterEntityId = value;
			PlatformUserIdentifierAbs primaryId = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(value).PrimaryId;
			name.Update(name.Text, primaryId);
		}
	}

	public bool HiddenOnMap
	{
		get
		{
			return hiddenOnMap;
		}
		set
		{
			hiddenOnMap = value;
			navObject.hiddenOnMap = hiddenOnMap;
		}
	}

	public Waypoint()
	{
		name = new AuthoredText();
	}

	public Waypoint Clone()
	{
		return new Waypoint
		{
			pos = pos,
			icon = icon,
			name = AuthoredText.Clone(name),
			bTracked = bTracked,
			ownerId = ownerId,
			lastKnownPositionEntityId = lastKnownPositionEntityId,
			lastKnownPositionEntityType = lastKnownPositionEntityType,
			navObject = navObject,
			hiddenOnCompass = hiddenOnCompass,
			bIsAutoWaypoint = bIsAutoWaypoint,
			bUsingLocalizationId = bUsingLocalizationId,
			IsSaved = IsSaved,
			inviterEntityId = inviterEntityId,
			hiddenOnMap = hiddenOnMap
		};
	}

	public void Read(BinaryReader _br, int version = 7)
	{
		pos = StreamUtils.ReadVector3i(_br);
		icon = _br.ReadString();
		name = AuthoredText.FromStream(_br);
		bTracked = _br.ReadBoolean();
		if (version > 2)
		{
			hiddenOnCompass = _br.ReadBoolean();
		}
		if (version > 1)
		{
			ownerId = PlatformUserIdentifierAbs.FromStream(_br);
			lastKnownPositionEntityId = _br.ReadInt32();
		}
		if (version > 3)
		{
			bIsAutoWaypoint = _br.ReadBoolean();
			bUsingLocalizationId = _br.ReadBoolean();
		}
		if (version > 4)
		{
			inviterEntityId = _br.ReadInt32();
		}
		if (version > 5)
		{
			hiddenOnMap = _br.ReadBoolean();
		}
		if (version > 6)
		{
			lastKnownPositionEntityType = (eLastKnownPositionEntityType)_br.ReadInt32();
		}
		else if (lastKnownPositionEntityId > -1)
		{
			lastKnownPositionEntityType = eLastKnownPositionEntityType.Vehicle;
		}
	}

	public void Write(BinaryWriter _bw)
	{
		StreamUtils.Write(_bw, pos);
		if (icon == null)
		{
			_bw.Write("");
		}
		else
		{
			_bw.Write(icon);
		}
		AuthoredText.ToStream(name, _bw);
		_bw.Write(bTracked);
		_bw.Write(hiddenOnCompass);
		ownerId.ToStream(_bw);
		_bw.Write(lastKnownPositionEntityId);
		_bw.Write(bIsAutoWaypoint);
		_bw.Write(bUsingLocalizationId);
		_bw.Write(inviterEntityId);
		_bw.Write(hiddenOnMap);
		_bw.Write((int)lastKnownPositionEntityType);
	}

	public override bool Equals(object obj)
	{
		if (obj is Waypoint waypoint)
		{
			if (waypoint.pos.Equals(pos) && waypoint.icon == icon && waypoint.name.Text == name.Text && object.Equals(waypoint.ownerId, ownerId))
			{
				return waypoint.lastKnownPositionEntityId == lastKnownPositionEntityId;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((((17 * 31 + pos.GetHashCode()) * 31 + icon.GetHashCode()) * 31 + name.Text.GetHashCode()) * 31 + (ownerId?.GetHashCode() ?? 0)) * 31 + lastKnownPositionEntityId.GetHashCode();
	}

	public bool CanBeViewedBy(PlatformUserIdentifierAbs _userIdentifier)
	{
		if (lastKnownPositionEntityId != -1)
		{
			return _userIdentifier?.Equals(ownerId) ?? false;
		}
		return true;
	}

	public override string ToString()
	{
		return "Waypoint name:" + name?.ToString() + " icon:" + icon + " Entity ID:" + lastKnownPositionEntityId;
	}
}
