using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DeathBar : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string deathText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal LocalPlayer;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive Target { get; set; }

	public override void Init()
	{
		base.Init();
		IsDirty = true;
		viewComponent.IsVisible = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (LocalPlayer == null && base.xui != null && base.xui.playerUI != null && base.xui.playerUI.entityPlayer != null)
		{
			LocalPlayer = base.xui.playerUI.entityPlayer;
		}
		if (!(LocalPlayer == null))
		{
			if (LocalPlayer.IsAlive())
			{
				viewComponent.IsVisible = false;
			}
			else if (deathText != TwitchManager.DeathText)
			{
				deathText = TwitchManager.DeathText;
				RefreshBindings(_forceAll: true);
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RefreshBindings(_forceAll: true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (!(_bindingName == "death_text"))
		{
			if (_bindingName == "visible")
			{
				if (LocalPlayer == null)
				{
					_value = "false";
					return true;
				}
				if (LocalPlayer.IsAlive())
				{
					_value = "false";
					return true;
				}
				if (TwitchManager.DeathText == "")
				{
					_value = "false";
					return true;
				}
				_value = "true";
				return true;
			}
			return false;
		}
		if (LocalPlayer == null)
		{
			_value = "";
			return true;
		}
		_value = deathText;
		return true;
	}
}
