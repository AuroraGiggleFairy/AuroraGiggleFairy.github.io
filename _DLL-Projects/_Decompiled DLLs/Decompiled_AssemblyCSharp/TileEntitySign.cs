using System.Collections.Generic;
using Platform;
using UnityEngine;

public class TileEntitySign : TileEntity, ILockable, ITileEntitySignable, ITileEntity
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int ver = 2;

	public int lineCharWidth = 19;

	public int lineCount = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlatformUserIdentifierAbs> allowedUserIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public string password;

	[PublicizedFrom(EAccessModifier.Private)]
	public AuthoredText signText;

	[PublicizedFrom(EAccessModifier.Private)]
	public TextMesh textMesh;

	[PublicizedFrom(EAccessModifier.Private)]
	public SmartTextMesh smartTextMesh;

	public new int EntityId
	{
		get
		{
			return entityId;
		}
		set
		{
			entityId = value;
		}
	}

	public TileEntitySign(Chunk _chunk)
		: base(_chunk)
	{
		allowedUserIds = new List<PlatformUserIdentifierAbs>();
		isLocked = true;
		ownerID = null;
		password = "";
		signText = new AuthoredText();
		PlatformUserManager.BlockedStateChanged += UserBlockedStateChanged;
	}

	public override void OnUnload(World world)
	{
		base.OnUnload(world);
		PlatformUserManager.BlockedStateChanged -= UserBlockedStateChanged;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		PlatformUserManager.BlockedStateChanged -= UserBlockedStateChanged;
	}

	public bool IsLocked()
	{
		return isLocked;
	}

	public void SetLocked(bool _isLocked)
	{
		isLocked = _isLocked;
		setModified();
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		ownerID = _userIdentifier;
		setModified();
	}

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		if ((_userIdentifier != null && _userIdentifier.Equals(ownerID)) || allowedUserIds.Contains(_userIdentifier))
		{
			return true;
		}
		return false;
	}

	public bool LocalPlayerIsOwner()
	{
		return IsOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier?.Equals(ownerID) ?? false;
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return ownerID;
	}

	public bool HasPassword()
	{
		return !string.IsNullOrEmpty(password);
	}

	public string GetPassword()
	{
		return password;
	}

	public List<PlatformUserIdentifierAbs> GetUsers()
	{
		return allowedUserIds;
	}

	public bool CheckPassword(string _password, PlatformUserIdentifierAbs _userIdentifier, out bool changed)
	{
		changed = false;
		if (_userIdentifier != null && _userIdentifier.Equals(ownerID))
		{
			if (Utils.HashString(_password) != password)
			{
				changed = true;
				password = Utils.HashString(_password);
				allowedUserIds.Clear();
				setModified();
			}
			return true;
		}
		if (Utils.HashString(_password) == password)
		{
			allowedUserIds.Add(_userIdentifier);
			setModified();
			return true;
		}
		return false;
	}

	public void SetBlockEntityData(BlockEntityData _blockEntityData)
	{
		if (_blockEntityData != null && _blockEntityData.bHasTransform && !GameManager.IsDedicatedServer)
		{
			textMesh = _blockEntityData.transform.GetComponentInChildren<TextMesh>();
			smartTextMesh = textMesh.transform.gameObject.AddComponent<SmartTextMesh>();
			float num = _blockEntityData.blockValue.Block.multiBlockPos.dim.x;
			smartTextMesh.MaxWidth = 0.48f * num;
			smartTextMesh.MaxLines = lineCount;
			smartTextMesh.ConvertNewLines = true;
			RefreshTextMesh(signText?.Text);
		}
	}

	public virtual void SetText(AuthoredText _authoredText, bool _syncData = true)
	{
		SetText(_authoredText?.Text, _syncData, _authoredText?.Author);
	}

	public void SetText(string _text, bool _syncData = true, PlatformUserIdentifierAbs _signingPlayer = null)
	{
		if (!GameManager.Instance)
		{
			return;
		}
		if (_signingPlayer == null)
		{
			_signingPlayer = PlatformManager.MultiPlatform.User.PlatformUserId;
		}
		if (GameManager.Instance.persistentPlayers?.GetPlayerData(_signingPlayer) == null)
		{
			_signingPlayer = null;
		}
		if (!(_text == signText.Text))
		{
			signText.Update(_text, _signingPlayer);
			GeneratedTextManager.GetDisplayText(signText, RefreshTextMesh, _runCallbackIfReadyNow: true, _checkBlockState: true, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.NotSupported);
			if (_syncData)
			{
				setModified();
			}
		}
	}

	public AuthoredText GetAuthoredText()
	{
		return signText;
	}

	public bool CanRenderString(string _text)
	{
		return smartTextMesh.CanRenderString(_text);
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		int num = _br.ReadInt32();
		isLocked = _br.ReadBoolean();
		ownerID = PlatformUserIdentifierAbs.FromStream(_br);
		if (num > 1)
		{
			SetText(AuthoredText.FromStream(_br), _syncData: false);
		}
		password = _br.ReadString();
		allowedUserIds = new List<PlatformUserIdentifierAbs>();
		int num2 = _br.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			allowedUserIds.Add(PlatformUserIdentifierAbs.FromStream(_br));
		}
		if (num <= 1)
		{
			SetText(_br.ReadString(), _syncData: false);
		}
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(2);
		_bw.Write(isLocked);
		ownerID.ToStream(_bw);
		AuthoredText.ToStream(signText, _bw);
		_bw.Write(password);
		_bw.Write(allowedUserIds.Count);
		for (int i = 0; i < allowedUserIds.Count; i++)
		{
			allowedUserIds[i].ToStream(_bw);
		}
	}

	public override TileEntity Clone()
	{
		return new TileEntitySign(chunk)
		{
			localChunkPos = base.localChunkPos,
			isLocked = isLocked,
			ownerID = ownerID,
			password = password,
			allowedUserIds = new List<PlatformUserIdentifierAbs>(allowedUserIds),
			signText = AuthoredText.Clone(signText)
		};
	}

	public override void CopyFrom(TileEntity _other)
	{
		TileEntitySign tileEntitySign = (TileEntitySign)_other;
		base.localChunkPos = tileEntitySign.localChunkPos;
		isLocked = tileEntitySign.isLocked;
		ownerID = tileEntitySign.ownerID;
		password = tileEntitySign.password;
		allowedUserIds = new List<PlatformUserIdentifierAbs>(tileEntitySign.allowedUserIds);
		signText = AuthoredText.Clone(tileEntitySign.signText);
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Sign;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UserBlockedStateChanged(IPlatformUserData userData, EBlockType blockType, EUserBlockState blockState)
	{
		if (userData.PrimaryId.Equals(signText.Author) && blockType == EBlockType.TextChat)
		{
			GeneratedTextManager.GetDisplayText(signText, RefreshTextMesh, _runCallbackIfReadyNow: true, _checkBlockState: true, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.NotSupported);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshTextMesh(string _text)
	{
		if (smartTextMesh != null && !GameManager.IsDedicatedServer)
		{
			smartTextMesh.UnwrappedText = _text;
		}
	}
}
