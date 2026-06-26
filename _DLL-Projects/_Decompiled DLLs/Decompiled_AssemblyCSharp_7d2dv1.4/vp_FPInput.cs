using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_FPInput : vp_Component
{
	public Vector2 MouseLookSensitivity = new Vector2(5f, 5f);

	public int MouseLookSmoothSteps = 10;

	public float MouseLookSmoothWeight = 0.5f;

	public bool MouseLookAcceleration;

	public float MouseLookAccelerationThreshold = 0.4f;

	public bool MouseLookInvert;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 m_MouseLookSmoothMove = Vector2.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 m_MouseLookRawMove = Vector2.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<Vector2> m_MouseLookSmoothBuffer = new List<Vector2>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_LastMouseLookFrame = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 m_CurrentMouseLook = Vector2.zero;

	public Rect[] MouseCursorZones;

	public bool MouseCursorForced;

	public bool MouseCursorBlocksMouseLook = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 m_MousePos = Vector2.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 m_MoveVector = Vector2.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_AllowGameplayInput = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPPlayerEventHandler m_FPPlayer;

	public Vector2 MousePos => m_MousePos;

	public bool AllowGameplayInput
	{
		get
		{
			return m_AllowGameplayInput;
		}
		set
		{
			m_AllowGameplayInput = value;
		}
	}

	public vp_FPPlayerEventHandler FPPlayer
	{
		get
		{
			if (m_FPPlayer == null)
			{
				m_FPPlayer = base.transform.root.GetComponentInChildren<vp_FPPlayerEventHandler>();
			}
			return m_FPPlayer;
		}
	}

	public virtual Vector2 OnValue_InputMoveVector
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return m_MoveVector;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			m_MoveVector = ((value != Vector2.zero) ? value.normalized : value);
		}
	}

	public virtual float OnValue_InputClimbVector
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return vp_Input.GetAxisRaw("Vertical");
		}
	}

	public virtual bool OnValue_InputAllowGameplay
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return m_AllowGameplayInput;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			m_AllowGameplayInput = value;
		}
	}

	public virtual bool OnValue_Pause
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return vp_TimeUtility.Paused;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			vp_TimeUtility.Paused = !vp_Gameplay.isMultiplayer && value;
		}
	}

	public virtual Vector2 OnValue_InputSmoothLook
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return GetMouseLook();
		}
	}

	public virtual Vector2 OnValue_InputRawLook
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return GetMouseLookRaw();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEnable()
	{
		if (FPPlayer != null)
		{
			FPPlayer.Register(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDisable()
	{
		if (FPPlayer != null)
		{
			FPPlayer.Unregister(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		UpdateCursorLock();
		UpdatePause();
		if (!FPPlayer.Pause.Get() && m_AllowGameplayInput)
		{
			InputInteract();
			InputMove();
			InputRun();
			InputJump();
			InputCrouch();
			InputAttack();
			InputReload();
			InputSetWeapon();
			InputCamera();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputInteract()
	{
		if (vp_Input.GetButtonDown("Interact"))
		{
			FPPlayer.Interact.TryStart();
		}
		else
		{
			FPPlayer.Interact.TryStop();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputMove()
	{
		FPPlayer.InputMoveVector.Set(new Vector2(vp_Input.GetAxisRaw("Horizontal"), vp_Input.GetAxisRaw("Vertical")));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputRun()
	{
		if (vp_Input.GetButton("Run"))
		{
			FPPlayer.Run.TryStart();
		}
		else
		{
			FPPlayer.Run.TryStop();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputJump()
	{
		if (vp_Input.GetButton("Jump"))
		{
			FPPlayer.Jump.TryStart();
		}
		else
		{
			FPPlayer.Jump.Stop();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputCrouch()
	{
		if (vp_Input.GetButton("Crouch"))
		{
			FPPlayer.Crouch.TryStart();
		}
		else
		{
			FPPlayer.Crouch.TryStop();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputCamera()
	{
		if (vp_Input.GetButton("Zoom"))
		{
			FPPlayer.Zoom.TryStart();
		}
		else
		{
			FPPlayer.Zoom.TryStop();
		}
		if (vp_Input.GetButtonDown("Toggle3rdPerson"))
		{
			FPPlayer.CameraToggle3rdPerson.Send();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputAttack()
	{
		if (vp_Utility.LockCursor)
		{
			if (vp_Input.GetButton("Attack"))
			{
				FPPlayer.Attack.TryStart();
			}
			else
			{
				FPPlayer.Attack.TryStop();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputReload()
	{
		if (vp_Input.GetButtonDown("Reload"))
		{
			FPPlayer.Reload.TryStart();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputSetWeapon()
	{
		if (vp_Input.GetButtonDown("SetPrevWeapon"))
		{
			FPPlayer.SetPrevWeapon.Try();
		}
		if (vp_Input.GetButtonDown("SetNextWeapon"))
		{
			FPPlayer.SetNextWeapon.Try();
		}
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			FPPlayer.SetWeapon.TryStart(1);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			FPPlayer.SetWeapon.TryStart(2);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			FPPlayer.SetWeapon.TryStart(3);
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			FPPlayer.SetWeapon.TryStart(4);
		}
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			FPPlayer.SetWeapon.TryStart(5);
		}
		if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			FPPlayer.SetWeapon.TryStart(6);
		}
		if (Input.GetKeyDown(KeyCode.Alpha7))
		{
			FPPlayer.SetWeapon.TryStart(7);
		}
		if (Input.GetKeyDown(KeyCode.Alpha8))
		{
			FPPlayer.SetWeapon.TryStart(8);
		}
		if (Input.GetKeyDown(KeyCode.Alpha9))
		{
			FPPlayer.SetWeapon.TryStart(9);
		}
		if (Input.GetKeyDown(KeyCode.Alpha0))
		{
			FPPlayer.SetWeapon.TryStart(10);
		}
		if (vp_Input.GetButtonDown("ClearWeapon"))
		{
			FPPlayer.SetWeapon.TryStart(0);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdatePause()
	{
		if (vp_Input.GetButtonDown("Pause"))
		{
			FPPlayer.Pause.Set(!FPPlayer.Pause.Get());
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateCursorLock()
	{
		m_MousePos.x = Input.mousePosition.x;
		m_MousePos.y = (float)Screen.height - Input.mousePosition.y;
		if (MouseCursorForced)
		{
			vp_Utility.LockCursor = false;
			return;
		}
		if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
		{
			if (MouseCursorZones.Length != 0)
			{
				Rect[] mouseCursorZones = MouseCursorZones;
				int num = 0;
				while (num < mouseCursorZones.Length)
				{
					Rect rect = mouseCursorZones[num];
					if (!rect.Contains(m_MousePos))
					{
						num++;
						continue;
					}
					goto IL_0083;
				}
			}
			vp_Utility.LockCursor = true;
		}
		goto IL_009b;
		IL_0083:
		vp_Utility.LockCursor = false;
		goto IL_009b;
		IL_009b:
		if (vp_Input.GetButtonUp("Accept1") || vp_Input.GetButtonUp("Accept2") || vp_Input.GetButtonUp("Menu"))
		{
			vp_Utility.LockCursor = !vp_Utility.LockCursor;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector2 GetMouseLook()
	{
		if (MouseCursorBlocksMouseLook && !vp_Utility.LockCursor)
		{
			return Vector2.zero;
		}
		if (m_LastMouseLookFrame == Time.frameCount)
		{
			return m_CurrentMouseLook;
		}
		m_LastMouseLookFrame = Time.frameCount;
		m_MouseLookSmoothMove.x = vp_Input.GetAxisRaw("Mouse X") * Time.timeScale;
		m_MouseLookSmoothMove.y = vp_Input.GetAxisRaw("Mouse Y") * Time.timeScale;
		MouseLookSmoothSteps = Mathf.Clamp(MouseLookSmoothSteps, 1, 20);
		MouseLookSmoothWeight = Mathf.Clamp01(MouseLookSmoothWeight);
		while (m_MouseLookSmoothBuffer.Count > MouseLookSmoothSteps)
		{
			m_MouseLookSmoothBuffer.RemoveAt(0);
		}
		m_MouseLookSmoothBuffer.Add(m_MouseLookSmoothMove);
		float num = 1f;
		Vector2 zero = Vector2.zero;
		float num2 = 0f;
		for (int num3 = m_MouseLookSmoothBuffer.Count - 1; num3 > 0; num3--)
		{
			zero += m_MouseLookSmoothBuffer[num3] * num;
			num2 += 1f * num;
			num *= MouseLookSmoothWeight / base.Delta;
		}
		num2 = Mathf.Max(1f, num2);
		m_CurrentMouseLook = vp_MathUtility.NaNSafeVector2(zero / num2);
		float num4 = 0f;
		float num5 = Mathf.Abs(m_CurrentMouseLook.x);
		float num6 = Mathf.Abs(m_CurrentMouseLook.y);
		if (MouseLookAcceleration)
		{
			num4 = Mathf.Sqrt(num5 * num5 + num6 * num6) / base.Delta;
			num4 = ((num4 <= MouseLookAccelerationThreshold) ? 0f : num4);
		}
		m_CurrentMouseLook.x *= MouseLookSensitivity.x + num4;
		m_CurrentMouseLook.y *= MouseLookSensitivity.y + num4;
		m_CurrentMouseLook.y = (MouseLookInvert ? m_CurrentMouseLook.y : (0f - m_CurrentMouseLook.y));
		return m_CurrentMouseLook;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector2 GetMouseLookRaw()
	{
		if (MouseCursorBlocksMouseLook && !vp_Utility.LockCursor)
		{
			return Vector2.zero;
		}
		m_MouseLookRawMove.x = vp_Input.GetAxisRaw("Mouse X");
		m_MouseLookRawMove.y = vp_Input.GetAxisRaw("Mouse Y");
		return m_MouseLookRawMove;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnMessage_InputGetButton(string button)
	{
		return vp_Input.GetButton(button);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnMessage_InputGetButtonUp(string button)
	{
		return vp_Input.GetButtonUp(button);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnMessage_InputGetButtonDown(string button)
	{
		return vp_Input.GetButtonDown(button);
	}
}
