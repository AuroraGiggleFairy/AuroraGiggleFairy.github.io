using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureSignable : TEFeatureAbs, ITileEntitySignable, ITileEntity, ILockTarget, IFeatureSavedInPrefab
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockable lockFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockPickable lockpickFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public AuthoredText signText = new AuthoredText();

	[PublicizedFrom(EAccessModifier.Private)]
	public int fontSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lineCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lineWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lineSpacing;

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
		lockFeature = base.Parent.GetFeature<TEFeatureLockable>();
		lockpickFeature = base.Parent.GetFeature<TEFeatureLockPickable>();
		DynamicProperties props = _featureData.Props;
		props.ParseInt("LineCount", ref lineCount);
		props.ParseFloat("LineWidth", ref lineWidth);
		props.ParseInt("FontSize", ref fontSize);
		props.ParseFloat("LineSpacing", ref lineSpacing);
		if (lineWidth <= 0f || lineCount <= 0 || fontSize <= 0 || lineSpacing <= 0f)
		{
			Log.Error($"Block '{base.Parent.TeData.Block.GetBlockName()}' has TEFeatureSignable with missing or invalid properties: LineWidth={lineWidth}, LineCount={lineCount}, FontSize={fontSize}, LineSpacing={lineSpacing}");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
		if (_other.TryGetSelfOrFeature<TEFeatureSignable>(out var _typedTe))
		{
			signText = _typedTe.signText.Clone();
		}
	}

	public override void SetBlockEntityData(BlockEntityData _blockEntityData)
	{
		if (_blockEntityData == null || !_blockEntityData.bHasTransform || GameManager.IsDedicatedServer || lineWidth <= 0f || lineCount <= 0 || fontSize <= 0 || lineSpacing <= 0f)
		{
			return;
		}
		float maxWidthReal = lineWidth;
		TextMesh[] componentsInChildren = _blockEntityData.transform.GetComponentsInChildren<TextMesh>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			smartTextMesh = new SmartTextMesh[componentsInChildren.Length];
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].fontSize = fontSize;
				componentsInChildren[i].characterSize = 0.01f;
				componentsInChildren[i].lineSpacing = lineSpacing;
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
		if (lockpickFeature != null && lockpickFeature.NeedsLockpicking())
		{
			return string.Format(Localization.Get("tooltipLocked"), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		if (lockFeature != null && lockFeature.IsLocked() && !lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			return string.Format(Localization.Get("tooltipJammed"), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		return Localization.Get("useWorkstation");
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("edit", "pen", _enabled: true), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
		_addCallback(new BlockActivationCommand("report", "report", _enabled: true), TileEntityComposite.EBlockCommandOrder.Last, base.FeatureData);
	}

	public override bool AllowBlockActivationCommand(ITileEntityFeature _module, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		if (!base.AllowBlockActivationCommand(_module, _commandName, _world, _blockPos, _blockValue, _entityFocusing))
		{
			return false;
		}
		if (!Equals(_module))
		{
			return true;
		}
		if (CommandIs(_commandName, "edit"))
		{
			if (lockFeature != null && !GameManager.Instance.IsEditMode() && lockFeature.IsLocked())
			{
				return lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier);
			}
			return true;
		}
		if (CommandIs(_commandName, "report"))
		{
			PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
			bool num = PlatformManager.MultiPlatform.PlayerReporting != null && !string.IsNullOrEmpty(signText.Text) && !internalLocalUserIdentifier.Equals(signText.Author);
			bool flag = GameManager.Instance.persistentPlayers.GetPlayerData(signText.Author)?.PlatformData.Blocked[EBlockType.TextChat].IsBlocked() ?? false;
			if (num)
			{
				return !flag;
			}
			return false;
		}
		return true;
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (CommandIs(_commandName, "edit"))
		{
			_player.AimingGun = false;
			LockManager.Instance.LockRequestLocal(this, null, 0);
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

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.Read(_br, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeRead.Persistency)
		{
			if (base.Parent.UseLocalVersioning())
			{
				_br.ReadUInt16();
			}
			else
			{
				base.Parent.GetLegacyForkVersion();
			}
		}
		SetText(AuthoredText.FromStream(_br), _syncData: false);
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write((ushort)18);
		}
		if (GameManager.Instance.IsEditMode())
		{
			AuthoredText.ToStream(new AuthoredText(signText.Text, null), _bw);
		}
		else
		{
			AuthoredText.ToStream(signText, _bw);
		}
	}

	public override bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return LocalPlayerUI.GetUIForPrimaryPlayer() != null;
	}

	public override void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		ShowUI(this, _success);
	}

	public static void ShowUI(ITileEntitySignable _te, bool _lockGranted)
	{
		LocalPlayerUI uIForPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (!_lockGranted)
		{
			GameManager.ShowTooltip(uIForPrimaryPlayer.entityPlayer, Localization.Get("ttNoInteractItem"), string.Empty, "ui_denied");
			return;
		}
		((XUiWindowGroup)uIForPrimaryPlayer.windowManager.GetWindow("signMultiline")).Controller.GetChildByType<XUiC_SignWindow>().SetTileEntitySign(_te);
		uIForPrimaryPlayer.windowManager.Open("signMultiline", _bModal: true);
	}
}
