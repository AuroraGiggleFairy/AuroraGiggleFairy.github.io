using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_InGameTimeControls : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly float[] skipHours = new float[4] { 0f, 4f, 12f, 22f };

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt cbxDay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt cbxSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastNonZeroSpeed = 1;

	public static bool HasWorld
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameManager.Instance.World != null;
		}
	}

	public static int CurrentSpeed
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameStats.GetInt(EnumGameStats.TimeOfDayIncPerSec);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "is_paused")
		{
			_value = (CurrentSpeed == 0).ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public override void Init()
	{
		base.Init();
		GameStats.OnChangedDelegates += onGameStatsChanged;
		cbxDay = GetChildById("cbxDay")?.GetChildByType<XUiC_ComboBoxInt>();
		if (cbxDay != null)
		{
			cbxDay.OnValueChanged += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, long _, long _newValue) =>
			{
				setDay((int)_newValue);
			};
		}
		cbxTime = GetChildById("cbxTime")?.GetChildByType<XUiC_ComboBoxFloat>();
		if (cbxTime != null)
		{
			cbxTime.OnValueChanged += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, double _, double _newValue) =>
			{
				setTime((float)_newValue);
			};
			cbxTime.CustomValueFormatter = [PublicizedFrom(EAccessModifier.Internal)] (double _value) =>
			{
				int num = (int)_value;
				int num2 = (int)((_value - (double)num) * 60.0);
				return $"{num:00}:{num2:00}";
			};
		}
		cbxSpeed = GetChildById("cbxSpeed")?.GetChildByType<XUiC_ComboBoxInt>();
		if (cbxSpeed != null)
		{
			cbxSpeed.OnValueChanged += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, long _, long _newValue) =>
			{
				setSpeed((int)_newValue);
			};
		}
		if (GetChildById("btnTimeSkipBack")?.ViewComponent is XUiV_Button xUiV_Button)
		{
			xUiV_Button.Controller.OnPress += btnTimeSkipBackPressed;
		}
		if (GetChildById("btnTimeSlower")?.ViewComponent is XUiV_Button xUiV_Button2)
		{
			xUiV_Button2.Controller.OnPress += btnTimeSlowerPressed;
		}
		if (GetChildById("btnTimePlayPause")?.ViewComponent is XUiV_Button xUiV_Button3)
		{
			xUiV_Button3.Controller.OnPress += btnTimePlayPausePressed;
		}
		if (GetChildById("btnTimeFaster")?.ViewComponent is XUiV_Button xUiV_Button4)
		{
			xUiV_Button4.Controller.OnPress += btnTimeFasterPressed;
		}
		if (GetChildById("btnTimeSkipForward")?.ViewComponent is XUiV_Button xUiV_Button5)
		{
			xUiV_Button5.Controller.OnPress += btnTimeSkipForwardPressed;
		}
	}

	public override void Cleanup()
	{
		GameStats.OnChangedDelegates -= onGameStatsChanged;
		base.Cleanup();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onGameStatsChanged(EnumGameStats _stat, object _value)
	{
		if (_stat == EnumGameStats.TimeOfDayIncPerSec)
		{
			int num = (int)_value;
			if (num > 0)
			{
				lastNonZeroSpeed = num;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setDay(int _day)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			ulong num = GameManager.Instance.World.worldTime % 24000;
			GameManager.Instance.World.SetTimeJump(num + (ulong)((long)(_day - 1) * 24000L), _isSeek: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setTime(float _dayTime)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			ulong num = GameManager.Instance.World.worldTime / 24000;
			int num2 = Mathf.Clamp((int)(_dayTime * 1000f), 0, 23999);
			GameManager.Instance.World.SetTimeJump(num * 24000 + (ulong)num2, _isSeek: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setSpeed(int _value)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			GameStats.Set(EnumGameStats.TimeOfDayIncPerSec, _value);
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnTimeSkipBackPressed(XUiController _sender, int _mouseButton)
	{
		float num = (float)(GameManager.Instance.World.worldTime % 24000) / 1000f - 0.050000004f;
		for (int num2 = skipHours.Length - 1; num2 >= 0; num2--)
		{
			float num3 = skipHours[num2];
			if (num3 < num)
			{
				setTime(num3);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnTimeSlowerPressed(XUiController _sender, int _mouseButton)
	{
		setSpeed((int)MathUtils.ToPreviousPowerOfTwo((uint)CurrentSpeed));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnTimePlayPausePressed(XUiController _sender, int _mouseButton)
	{
		setSpeed((CurrentSpeed == 0) ? lastNonZeroSpeed : 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnTimeFasterPressed(XUiController _sender, int _mouseButton)
	{
		setSpeed((int)MathUtils.ToNextPowerOfTwo((uint)CurrentSpeed));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnTimeSkipForwardPressed(XUiController _sender, int _mouseButton)
	{
		float num = (float)(GameManager.Instance.World.worldTime % 24000) / 1000f + 0.050000004f;
		for (int i = 0; i < skipHours.Length; i++)
		{
			float num2 = skipHours[i];
			if (num2 > num)
			{
				setTime(num2);
				break;
			}
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (cbxDay != null)
		{
			cbxDay.Value = (HasWorld ? ((long)GameManager.Instance.World.worldTime / 24000L + 1) : 0);
		}
		if (cbxTime != null)
		{
			cbxTime.Value = (HasWorld ? ((float)(GameManager.Instance.World.worldTime % 24000) / 1000f) : 0f);
		}
		if (cbxSpeed != null)
		{
			cbxSpeed.Value = CurrentSpeed;
		}
		RefreshBindings(_forceAll: true);
	}
}
