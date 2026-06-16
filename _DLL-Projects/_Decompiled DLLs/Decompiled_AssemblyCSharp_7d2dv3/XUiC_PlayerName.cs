using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayerName : XUiController
{
	[XuiBindComponent("playerName", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Rect rect;

	[XuiBindComponent("name", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label lblName;

	[XuiBindComponent("nameCrossplay", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label lblNameCrossplay;

	[XuiBindComponent("iconCrossplay", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite sprIconCrossplay;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerData playerData;

	public Color Color
	{
		get
		{
			return lblName.Color;
		}
		set
		{
			lblName.Color = value;
			lblNameCrossplay.Color = value;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiV_Rect xUiV_Rect = rect;
		bool isNavigatable = (rect.IsSnappable = false);
		xUiV_Rect.IsNavigatable = isNavigatable;
		base.OnPress += PlayerName_OnPress;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		base.OnPress -= PlayerName_OnPress;
	}

	public void SetGenericName(string _name)
	{
		UpdatePlayerData(null, _showCrossplay: false, _name);
	}

	public void UpdatePlayerData(PlayerData _playerData, bool _showCrossplay, string _displayName = null)
	{
		playerData = _playerData;
		bool flag = false;
		if (_displayName != null)
		{
			XUiV_Label xUiV_Label = lblName;
			string text = (lblNameCrossplay.Text = _displayName);
			xUiV_Label.Text = text;
			flag = true;
		}
		else if (playerData != null)
		{
			GeneratedTextManager.GetDisplayText(playerData.PlayerName, [PublicizedFrom(EAccessModifier.Private)] (string _name) =>
			{
				XUiV_Label xUiV_Label2 = lblName;
				string text3 = (lblNameCrossplay.Text = _name);
				xUiV_Label2.Text = text3;
			}, _runCallbackIfReadyNow: true, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString);
			flag = true;
		}
		rect.EventOnPress = flag && CanShowProfile();
		XUiV_Rect xUiV_Rect = rect;
		bool isNavigatable = (rect.IsSnappable = flag);
		xUiV_Rect.IsNavigatable = isNavigatable;
		if (_showCrossplay && playerData != null && playerData.PlayGroup != EPlayGroup.Unknown)
		{
			sprIconCrossplay.SpriteName = PlatformManager.NativePlatform.Utils.GetCrossplayPlayerIcon(playerData.PlayGroup, _fetchGenericIcons: true, playerData.NativeId.PlatformIdentifier);
			sprIconCrossplay.UIAtlas = "SymbolAtlas";
			sprIconCrossplay.IsVisible = true;
			lblName.IsVisible = false;
			lblNameCrossplay.IsVisible = true;
		}
		else
		{
			sprIconCrossplay.IsVisible = false;
			lblName.IsVisible = true;
			lblNameCrossplay.IsVisible = false;
		}
		RefreshBindings();
	}

	public void ClearPlayerData()
	{
		playerData = null;
		lblName.Text = string.Empty;
		lblNameCrossplay.Text = string.Empty;
		sprIconCrossplay.IsVisible = false;
	}

	public bool CanShowProfile()
	{
		if (playerData == null)
		{
			return false;
		}
		if (playerData.NativeId != null && PlatformManager.MultiPlatform.User.CanShowProfile(playerData.NativeId))
		{
			return true;
		}
		if (playerData.PrimaryId != null && PlatformManager.MultiPlatform.User.CanShowProfile(playerData.PrimaryId))
		{
			return true;
		}
		return false;
	}

	public void ShowProfile()
	{
		if (playerData != null)
		{
			if (playerData.NativeId != null && PlatformManager.MultiPlatform.User.CanShowProfile(playerData.NativeId))
			{
				PlatformManager.MultiPlatform.User.ShowProfile(playerData.NativeId);
			}
			else if (playerData.PrimaryId != null && PlatformManager.MultiPlatform.User.CanShowProfile(playerData.PrimaryId))
			{
				PlatformManager.MultiPlatform.User.ShowProfile(playerData.PrimaryId);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerName_OnPress(XUiController _sender, int _mousebutton)
	{
		ShowProfile();
	}
}
