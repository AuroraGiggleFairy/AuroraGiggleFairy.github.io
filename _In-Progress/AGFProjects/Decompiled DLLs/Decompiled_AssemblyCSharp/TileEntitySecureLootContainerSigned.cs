using Platform;
using UnityEngine;

public class TileEntitySecureLootContainerSigned : TileEntitySecureLootContainer, ITileEntitySignable, ITileEntity
{
	[PublicizedFrom(EAccessModifier.Private)]
	public new const int ver = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public AuthoredText signText;

	public int lineCount = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public SmartTextMesh[] smartTextMesh;

	public TileEntitySecureLootContainerSigned(Chunk _chunk)
		: base(_chunk)
	{
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

	public void SetBlockEntityData(BlockEntityData _blockEntityData)
	{
		if (_blockEntityData == null || !_blockEntityData.bHasTransform || GameManager.IsDedicatedServer)
		{
			return;
		}
		float num = _blockEntityData.blockValue.Block.multiBlockPos?.dim.x ?? 1;
		TextMesh[] componentsInChildren = _blockEntityData.transform.GetComponentsInChildren<TextMesh>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			smartTextMesh = new SmartTextMesh[componentsInChildren.Length];
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				smartTextMesh[i] = componentsInChildren[i].gameObject.AddComponent<SmartTextMesh>();
				smartTextMesh[i].MaxWidth = 0.4f * num;
				smartTextMesh[i].MaxLines = lineCount;
				smartTextMesh[i].ConvertNewLines = true;
				smartTextMesh[i].SeperatedLinesMode = false;
			}
			RefreshTextMesh(signText?.Text);
		}
	}

	public void SetText(AuthoredText _authoredText, bool _syncData = true)
	{
		SetText(_authoredText?.Text, _syncData, _authoredText?.Author);
	}

	public void SetText(string _text, bool _syncData = true, PlatformUserIdentifierAbs _signingPlayer = null)
	{
		if (_signingPlayer == null)
		{
			_signingPlayer = PlatformManager.MultiPlatform.User.PlatformUserId;
		}
		if (GameManager.Instance.persistentPlayers.GetPlayerData(_signingPlayer) == null)
		{
			_signingPlayer = null;
			_text = "";
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
		if (smartTextMesh.Length != 0)
		{
			return smartTextMesh[0].CanRenderString(_text);
		}
		return false;
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.SecureLootSigned;
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		int num = _br.ReadInt32();
		_br.ReadBoolean();
		_br.ReadBoolean();
		PlatformUserIdentifierAbs.FromStream(_br);
		if (num > 1)
		{
			SetText(AuthoredText.FromStream(_br), _syncData: false);
		}
		_br.ReadString();
		int num2 = _br.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			PlatformUserIdentifierAbs.FromStream(_br);
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
		_bw.Write(bPlayerPlaced);
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
		if (smartTextMesh != null && _text != smartTextMesh[0].UnwrappedText && !GameManager.IsDedicatedServer)
		{
			for (int i = 0; i < smartTextMesh.Length; i++)
			{
				smartTextMesh[i].UnwrappedText = _text;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	int ITileEntity.get_EntityId()
	{
		return base.EntityId;
	}
}
