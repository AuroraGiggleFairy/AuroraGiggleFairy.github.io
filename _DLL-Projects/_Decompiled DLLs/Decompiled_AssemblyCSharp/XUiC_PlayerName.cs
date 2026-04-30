using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayerName : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Rect rect;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblNameCrossplay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite sprIconCrossplay;

	public PlayerData PlayerData;

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
		rect = (XUiV_Rect)GetChildById("playerName").ViewComponent;
		lblName = (XUiV_Label)GetChildById("name").ViewComponent;
		lblNameCrossplay = (XUiV_Label)GetChildById("nameCrossplay").ViewComponent;
		sprIconCrossplay = (XUiV_Sprite)GetChildById("iconCrossplay").ViewComponent;
		rect.IsNavigatable = false;
		rect.IsSnappable = false;
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
		PlayerData = _playerData;
		bool flag = false;
		if (_displayName != null)
		{
			XUiV_Label xUiV_Label = lblName;
			string text = (lblNameCrossplay.Text = _displayName);
			xUiV_Label.Text = text;
			flag = true;
		}
		else if (PlayerData != null)
		{
			GeneratedTextManager.GetDisplayText(PlayerData.PlayerName, [PublicizedFrom(EAccessModifier.Private)] (string name) =>
			{
				XUiV_Label xUiV_Label2 = lblName;
				string text3 = (lblNameCrossplay.Text = name);
				xUiV_Label2.Text = text3;
			}, _runCallbackIfReadyNow: true, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString);
			flag = true;
		}
		rect.EventOnPress = flag && CanShowProfile();
		rect.IsSnappable = flag;
		rect.IsNavigatable = flag;
		if (_showCrossplay && PlayerData != null && PlayerData.PlayGroup != EPlayGroup.Unknown)
		{
			sprIconCrossplay.SpriteName = PlatformManager.NativePlatform.Utils.GetCrossplayPlayerIcon(PlayerData.PlayGroup, _fetchGenericIcons: true, PlayerData.NativeId.PlatformIdentifier);
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
		PlayerData = null;
		lblName.Text = string.Empty;
		lblNameCrossplay.Text = string.Empty;
		sprIconCrossplay.IsVisible = false;
	}

	public bool CanShowProfile()
	{
		if (PlayerData == null)
		{
			return false;
		}
		if (PlayerData.NativeId != null && PlatformManager.MultiPlatform.User.CanShowProfile(PlayerData.NativeId))
		{
			return true;
		}
		if (PlayerData.PrimaryId != null && PlatformManager.MultiPlatform.User.CanShowProfile(PlayerData.PrimaryId))
		{
			return true;
		}
		return false;
	}

	public void ShowProfile()
	{
		if (PlayerData != null)
		{
			if (PlayerData.NativeId != null && PlatformManager.MultiPlatform.User.CanShowProfile(PlayerData.NativeId))
			{
				PlatformManager.MultiPlatform.User.ShowProfile(PlayerData.NativeId);
			}
			else if (PlayerData.PrimaryId != null && PlatformManager.MultiPlatform.User.CanShowProfile(PlayerData.PrimaryId))
			{
				PlatformManager.MultiPlatform.User.ShowProfile(PlayerData.PrimaryId);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerName_OnPress(XUiController _sender, int _mousebutton)
	{
		ShowProfile();
	}
}
