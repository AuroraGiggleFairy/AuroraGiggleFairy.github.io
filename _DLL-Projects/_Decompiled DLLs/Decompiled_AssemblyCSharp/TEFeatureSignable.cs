using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureSignable : TEFeatureAbs, ITileEntitySignable, ITileEntity
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ILockable lockFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public AuthoredText signText = new AuthoredText();

	[PublicizedFrom(EAccessModifier.Private)]
	public int fontSize = 132;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lineCount = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lineWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public string displayText;

	[PublicizedFrom(EAccessModifier.Private)]
	public SmartTextMesh[] smartTextMesh;

	public TEFeatureSignable()
	{
		PlatformUserManager.BlockedStateChanged += UserBlockedStateChanged;
	}

	public override void OnUnload(World _world)
	{
		PlatformUserManager.BlockedStateChanged -= UserBlockedStateChanged;
	}

	public override void OnDestroy()
	{
		PlatformUserManager.BlockedStateChanged -= UserBlockedStateChanged;
	}

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		lockFeature = base.Parent.GetFeature<ILockable>();
		DynamicProperties props = _featureData.Props;
		props.ParseInt("LineCount", ref lineCount);
		props.ParseFloat("LineWidth", ref lineWidth);
		props.ParseInt("FontSize", ref fontSize);
	}

	public override void SetBlockEntityData(BlockEntityData _blockEntityData)
	{
		if (_blockEntityData == null || !_blockEntityData.bHasTransform || GameManager.IsDedicatedServer)
		{
			return;
		}
		float num = 0.8f * (float)(base.Parent.TeData.Block.multiBlockPos?.dim.x ?? 1);
		float maxWidthReal = ((lineWidth > 0f) ? lineWidth : num);
		TextMesh[] componentsInChildren = _blockEntityData.transform.GetComponentsInChildren<TextMesh>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			smartTextMesh = new SmartTextMesh[componentsInChildren.Length];
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].fontSize = fontSize;
				smartTextMesh[i] = componentsInChildren[i].gameObject.AddComponent<SmartTextMesh>();
				smartTextMesh[i].MaxWidthReal = maxWidthReal;
				smartTextMesh[i].MaxLines = lineCount;
				smartTextMesh[i].ConvertNewLines = true;
			}
			RefreshTextMesh(displayText ?? signText?.Text);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UserBlockedStateChanged(IPlatformUserData _userData, EBlockType _blockType, EUserBlockState _blockState)
	{
		if (_userData.PrimaryId.Equals(signText.Author) && _blockType == EBlockType.TextChat)
		{
			GeneratedTextManager.GetDisplayText(signText, RefreshTextMesh, _runCallbackIfReadyNow: true, _checkBlockState: true, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.NotSupported);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshTextMesh(string _text)
	{
		displayText = _text;
		if (!GameManager.IsDedicatedServer && smartTextMesh != null && !(displayText == smartTextMesh[0].UnwrappedText))
		{
			for (int i = 0; i < smartTextMesh.Length; i++)
			{
				smartTextMesh[i].UnwrappedText = displayText;
			}
		}
	}

	public override void UpgradeDowngradeFrom(TileEntityComposite _other)
	{
		base.UpgradeDowngradeFrom(_other);
		ITileEntitySignable feature = _other.GetFeature<ITileEntitySignable>();
		if (feature != null)
		{
			signText = feature.GetAuthoredText();
			if (signText != null)
			{
				GeneratedTextManager.GetDisplayText(signText, RefreshTextMesh, _runCallbackIfReadyNow: true, _checkBlockState: true, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.NotSupported);
			}
		}
	}

	public virtual void SetText(AuthoredText _authoredText, bool _syncData = true)
	{
		SetText(_authoredText?.Text, _syncData, _authoredText?.Author);
	}

	public virtual void SetText(string _text, bool _syncData = true, PlatformUserIdentifierAbs _signingPlayer = null)
	{
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
				SetModified();
			}
		}
	}

	public virtual AuthoredText GetAuthoredText()
	{
		return signText;
	}

	public virtual bool CanRenderString(string _text)
	{
		if (smartTextMesh != null && smartTextMesh.Length != 0)
		{
			return smartTextMesh[0].CanRenderString(_text);
		}
		return false;
	}

	public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		base.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
		return Localization.Get("useWorkstation");
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("edit", "pen", _enabled: true), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
		_addCallback(new BlockActivationCommand("report", "report", _enabled: true), TileEntityComposite.EBlockCommandOrder.Last, base.FeatureData);
	}

	public override void UpdateBlockActivationCommands(ref BlockActivationCommand _command, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		base.UpdateBlockActivationCommands(ref _command, _commandName, _world, _blockPos, _blockValue, _entityFocusing);
		if (CommandIs(_commandName, "edit"))
		{
			_command.enabled = lockFeature == null || GameManager.Instance.IsEditMode() || !lockFeature.IsLocked() || lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier);
		}
		else if (CommandIs(_commandName, "report"))
		{
			PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
			bool flag = PlatformManager.MultiPlatform.PlayerReporting != null && !string.IsNullOrEmpty(signText.Text) && !internalLocalUserIdentifier.Equals(signText.Author);
			bool flag2 = GameManager.Instance.persistentPlayers.GetPlayerData(signText.Author)?.PlatformData.Blocked[EBlockType.TextChat].IsBlocked() ?? false;
			_command.enabled = flag && !flag2;
		}
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (CommandIs(_commandName, "edit"))
		{
			_player.AimingGun = false;
			Vector3i blockPos = base.Parent.ToWorldPos();
			_world.GetGameManager().TELockServer(0, blockPos, base.Parent.EntityId, _player.entityId, "sign");
			return true;
		}
		if (CommandIs(_commandName, "report"))
		{
			GeneratedTextManager.GetDisplayText(signText, [PublicizedFrom(EAccessModifier.Private)] (string _filtered) =>
			{
				ThreadManager.AddSingleTaskMainThread("OpenReportWindow", [PublicizedFrom(EAccessModifier.Internal)] (object _) =>
				{
					XUiC_ReportPlayer.Open(GameManager.Instance.persistentPlayers.GetPlayerData(signText.Author)?.PlayerData, EnumReportCategory.VerbalAbuse, string.Format(Localization.Get("xuiReportOffensiveTextMessage"), _filtered));
				});
			}, _runCallbackIfReadyNow: true, _checkBlockState: false);
			return true;
		}
		return false;
	}

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode, int _readVersion)
	{
		base.Read(_br, _eStreamMode, _readVersion);
		SetText(AuthoredText.FromStream(_br), _syncData: false);
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		AuthoredText.ToStream(signText, _bw);
	}
}
