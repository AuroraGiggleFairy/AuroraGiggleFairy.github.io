using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public struct DynamicMusicSystemPassArbiter : IPassArbiter, IGamePrefsChangedListener, IPauseable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public byte BoolContainer = 0;

	public bool WillAllowPass => BoolContainer.Equals(224);

	public bool DoesPlayerExist
	{
		set
		{
			SetBoolContainer(value, 64);
		}
	}

	public bool IsGameUnPaused
	{
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			SetBoolContainer(value, 32);
		}
	}

	public bool IsDynamicMusicEnabled
	{
		set
		{
			SetBoolContainer(value, 128);
		}
	}

	public DynamicMusicSystemPassArbiter(bool _enabled)
	{
		IsDynamicMusicEnabled = _enabled;
		GamePrefs.AddChangeListener(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetBoolContainer(bool _value, byte _place)
	{
		if (_value)
		{
			BoolContainer |= _place;
		}
		else
		{
			BoolContainer &= (byte)(~_place);
		}
	}

	public void OnGamePrefChanged(EnumGamePrefs _enum)
	{
		if (_enum == EnumGamePrefs.OptionsDynamicMusicEnabled)
		{
			IsDynamicMusicEnabled = GamePrefs.GetBool(_enum);
		}
	}

	public void OnPause()
	{
		IsGameUnPaused = false;
	}

	public void OnUnPause()
	{
		IsGameUnPaused = true;
	}
}
