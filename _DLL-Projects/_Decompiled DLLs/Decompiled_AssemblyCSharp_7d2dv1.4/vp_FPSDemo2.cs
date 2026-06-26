using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_FPSDemo2 : MonoBehaviour
{
	public GameObject Player;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_FPSDemoManager m_Demo;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_FPCamera m_FPSCamera;

	public Texture ImageLeftArrow;

	public Texture ImageRightArrow;

	public Texture ImageRightPointer;

	public Texture ImageLeftPointer;

	public Texture ImageCheckmark;

	public Texture ImagePresetDialogs;

	public Texture ImageShooter;

	public Texture ImageAllParams;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int m_ExamplesCurrentSel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float m_GoAgainButtonAlpha;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float m_WASDInfoClickTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_SlimePos = new Vector3(115.3f, 113.3f, -94.5f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_WetRoofPos = new Vector3(115.3f, 113.3f, -86.5f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_FallDeflectPos = new Vector3(106.6f, 116.8f, -97.1f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_BlownAwayPos = new Vector3(132f, 122.18f, -100.6f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_ActionPos = new Vector3(127f, 122.18f, -97.6f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_HeadBumpPos = new Vector3(106.4f, 102.4f, -99.89f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_WallBouncePos = new Vector3(114.2f, 104.6f, -91.9f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_ExplorePos = new Vector3(134.0023f, 107.64261f, -109.5f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_OverViewPos = new Vector3(135f, 105.8f, -70.7f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_OutroPos = new Vector3(135f, 205.8f, -70.7f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_OutroAngle = new Vector2(-19.3f, 241.7f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_SlimeAngle = new Vector2(0f, 180f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_WetRoofAngle = new Vector2(30f, 230f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_FallDeflectAngle = new Vector2(25f, 180f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_BlownAwayAngle = new Vector2(0f, -90f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_ActionAngle = new Vector2(0f, 180f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_HeadBumpAngle = new Vector2(0f, 180f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_WallBounceAngle = new Vector2(0f, 130f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_ExploreAngle = new Vector2(30f, 40f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_OverViewAngle = new Vector2(-16.5f, 215f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Timer.Handle m_ForceTimer = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Timer.Handle m_GoAgainTimer = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Timer.Handle m_HeadBumpTimer1 = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Timer.Handle m_HeadBumpTimer2 = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Timer.Handle m_ActionTimer1 = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Timer.Handle m_ActionTimer2 = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Timer.Handle m_ActionTimer3 = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Timer.Handle m_ActionTimer4 = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Timer.Handle m_ActionTimer5 = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float m_OutroStartTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_RunForward;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_Jump;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int m_LookPoint;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] m_LookPoints = new Vector3[9];

	public AudioClip m_ExplosionSound;

	public List<AudioClip> FallImpactSounds;

	public GameObject m_ExplosionFX;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		m_FPSCamera = (vp_FPCamera)UnityEngine.Object.FindObjectOfType(typeof(vp_FPCamera));
		m_Demo = new vp_FPSDemoManager(Player);
		m_Demo.CurrentFullScreenFadeTime = Time.time;
		m_Demo.DrawCrosshair = false;
		m_Demo.Input.MouseCursorZones = new Rect[3];
		m_Demo.Input.MouseCursorZones[0] = new Rect((float)Screen.width * 0.5f - 370f, 40f, 80f, 80f);
		m_Demo.Input.MouseCursorZones[1] = new Rect((float)Screen.width * 0.5f + 290f, 40f, 80f, 80f);
		m_Demo.Input.MouseCursorZones[2] = new Rect(0f, 0f, 150f, Screen.height);
		vp_Utility.LockCursor = false;
		m_LookPoints[1] = new Vector3(129.3f, 122f, -186f);
		m_LookPoints[2] = new Vector3(129.3f, 85f, -186f);
		m_LookPoints[3] = new Vector3(147f, 85f, -186f);
		m_LookPoints[4] = new Vector3(12f, 85f, -214f);
		m_LookPoints[5] = new Vector3(129f, 122f, -118f);
		m_LookPoints[6] = new Vector3(125.175f, 106.1071f, -97.58212f);
		m_LookPoints[7] = new Vector3(119.6f, 104.2f, -89.1f);
		m_LookPoints[8] = new Vector3(129f, 112f, -150f);
		m_Demo.PlayerEventHandler.SetWeapon.Disallow(10000000f);
		m_Demo.PlayerEventHandler.SetPrevWeapon.Try = [PublicizedFrom(EAccessModifier.Internal)] () => false;
		m_Demo.PlayerEventHandler.SetNextWeapon.Try = [PublicizedFrom(EAccessModifier.Internal)] () => false;
		m_Demo.PlayerEventHandler.FallImpact.Register(this, "FallImpact", 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		m_Demo.Update();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoPhysics()
	{
		m_Demo.DrawBoxes("part iii: physics", "Ultimate FPS features a cool, tweakable MOTOR and PHYSICS simulation.\nAll motion is forwarded to the camera and weapon for some CRAZY MOVES that you won't see in an everyday FPS. Click these buttons for some quick examples ...", null, ImageRightArrow);
		if (m_Demo.FirstFrame)
		{
			m_Demo.DrawCrosshair = true;
			m_Demo.Teleport(m_SlimePos, m_SlimeAngle);
			m_Demo.FirstFrame = false;
			m_Demo.ButtonSelection = 0;
			m_Demo.Camera.SnapSprings();
			m_Demo.RefreshDefaultState();
			m_Demo.Input.MouseCursorForced = true;
			m_Demo.Teleport(m_SlimePos, m_SlimeAngle);
			m_Demo.WeaponHandler.SetWeapon(1);
			m_ExamplesCurrentSel = -1;
			m_RunForward = false;
			m_LookPoint = 0;
			m_Demo.LockControls();
		}
		if (m_Demo.ShowGUI && !m_GoAgainTimer.Active && m_Demo.ButtonSelection != 3)
		{
			GUI.color = new Color(1f, 1f, 1f, m_GoAgainButtonAlpha);
			m_GoAgainButtonAlpha = Mathf.Lerp(0f, 1f, m_GoAgainButtonAlpha + Time.deltaTime);
			if (GUI.Button(new Rect(Screen.width / 2 - 60, 210f, 120f, 30f), "Go again!"))
			{
				m_GoAgainButtonAlpha = 0f;
				m_ExamplesCurrentSel = -1;
			}
			GUI.color = new Color(1f, 1f, 1f, 1f * m_Demo.GlobalAlpha);
		}
		if (m_Demo.ButtonSelection != m_ExamplesCurrentSel)
		{
			m_WASDInfoClickTime = Time.time;
			m_Demo.Controller.Stop();
			m_Jump = false;
			m_LookPoint = 0;
			m_GoAgainButtonAlpha = 0f;
			m_ForceTimer.Cancel();
			m_Demo.Controller.Stop();
			m_Demo.PlayerEventHandler.RefreshActivityStates();
			m_Demo.Input.MouseCursorForced = true;
			m_Demo.Controller.PhysicsSlopeSlidiness = 0.15f;
			m_Demo.Controller.MotorAirSpeed = 0.7f;
			m_Demo.Controller.MotorAcceleration = 0.18f;
			m_Demo.Controller.PhysicsWallBounce = 0f;
			m_HeadBumpTimer1.Cancel();
			m_HeadBumpTimer2.Cancel();
			m_ActionTimer1.Cancel();
			m_ActionTimer2.Cancel();
			m_ActionTimer3.Cancel();
			m_ActionTimer4.Cancel();
			m_ActionTimer5.Cancel();
			m_GoAgainTimer.Cancel();
			m_ForceTimer.Cancel();
			vp_Utility.LockCursor = true;
			m_Demo.Camera.SnapSprings();
			if (m_Demo.WeaponHandler.CurrentWeapon != null)
			{
				m_Demo.Camera.SnapSprings();
			}
			m_Demo.PlayerEventHandler.Platform.Set(null);
			switch (m_Demo.ButtonSelection)
			{
			case 0:
				vp_Timer.In(29f, [PublicizedFrom(EAccessModifier.Internal)] () =>
				{
				}, m_GoAgainTimer);
				m_Demo.Teleport(m_SlimePos, m_SlimeAngle);
				break;
			case 1:
				vp_Timer.In(5f, [PublicizedFrom(EAccessModifier.Internal)] () =>
				{
				}, m_GoAgainTimer);
				m_Demo.Teleport(m_WetRoofPos, m_WetRoofAngle);
				m_Demo.Controller.PhysicsSlopeSlidiness = 1f;
				break;
			case 2:
				m_Demo.Controller.MotorAirSpeed = 0f;
				m_Demo.Teleport(m_ActionPos, m_ActionAngle);
				m_Demo.SnapLookAt(m_LookPoints[1]);
				m_LookPoint = 1;
				m_RunForward = true;
				vp_Timer.In(1.75f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					m_LookPoint = 2;
					m_Jump = true;
					m_Demo.LookDamping = 1f;
				}, m_ActionTimer1);
				vp_Timer.In(2.25f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					m_LookPoint = 3;
					m_Jump = false;
					m_Demo.LookDamping = 1f;
				}, m_ActionTimer2);
				vp_Timer.In(3.5f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					m_LookPoint = 4;
					m_Demo.Controller.MotorAcceleration = 0f;
					m_Demo.LookDamping = 3f;
				}, m_ActionTimer3);
				vp_Timer.In(5f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					m_LookPoint = 5;
					m_RunForward = false;
					m_Demo.Controller.MotorAcceleration = 0.18f;
					m_Demo.LookDamping = 1f;
				}, m_ActionTimer4);
				vp_Timer.In(9f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					m_LookPoint = 8;
				}, m_ActionTimer5);
				vp_Timer.In(11f, [PublicizedFrom(EAccessModifier.Internal)] () =>
				{
				}, m_GoAgainTimer);
				break;
			case 4:
				m_Demo.Teleport(m_HeadBumpPos, m_HeadBumpAngle);
				vp_Timer.In(1f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					m_Jump = true;
				}, m_HeadBumpTimer1);
				vp_Timer.In(1.25f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					m_Jump = false;
				}, m_HeadBumpTimer2);
				vp_Timer.In(2f, [PublicizedFrom(EAccessModifier.Internal)] () =>
				{
				}, m_GoAgainTimer);
				break;
			case 5:
				m_Demo.Teleport(m_WallBouncePos, m_WallBounceAngle);
				m_LookPoint = 6;
				m_Demo.LookDamping = 0f;
				vp_Timer.In(1f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					m_LookPoint = 7;
					m_Demo.LookDamping = 3f;
					m_Demo.Controller.PhysicsWallBounce = 0f;
					UnityEngine.Object.Instantiate(m_ExplosionFX, m_Demo.Controller.transform.position + new Vector3(3f, 0f, 0f), Quaternion.identity);
					m_Demo.PlayerEventHandler.CameraBombShake.Send(0.3f);
					m_Demo.Controller.AddForce(Vector3.right * 3f);
					if (m_Demo.WeaponHandler.CurrentWeapon != null)
					{
						m_Demo.WeaponHandler.CurrentWeapon.GetComponent<AudioSource>().PlayOneShot(m_ExplosionSound);
					}
				}, m_ForceTimer);
				vp_Timer.In(5f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					m_Demo.Controller.PhysicsWallBounce = 0f;
					m_LookPoint = 6;
					m_Demo.LookDamping = 0.5f;
					m_Demo.Teleport(m_WallBouncePos, m_WallBounceAngle);
				}, m_GoAgainTimer);
				break;
			case 6:
				vp_Timer.In(5f, [PublicizedFrom(EAccessModifier.Internal)] () =>
				{
				}, m_GoAgainTimer);
				m_Demo.Teleport(m_FallDeflectPos, m_FallDeflectAngle);
				break;
			case 7:
				vp_Timer.In(7f, [PublicizedFrom(EAccessModifier.Internal)] () =>
				{
				}, m_GoAgainTimer);
				vp_Timer.In(1f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					UnityEngine.Object.Instantiate(m_ExplosionFX, m_Demo.Controller.transform.position + new Vector3(-3f, 0f, 0f), Quaternion.identity);
					m_Demo.PlayerEventHandler.CameraBombShake.Send(0.5f);
					m_Demo.Controller.AddForce(Vector3.forward * 0.55f);
					if (m_Demo.WeaponHandler.CurrentWeapon != null)
					{
						m_Demo.WeaponHandler.CurrentWeapon.GetComponent<AudioSource>().PlayOneShot(m_ExplosionSound);
					}
				}, m_ForceTimer);
				m_Demo.Teleport(m_BlownAwayPos, m_BlownAwayAngle);
				break;
			case 3:
				m_Demo.Input.MouseCursorForced = false;
				m_Demo.WeaponHandler.SetWeapon(2);
				m_Demo.Teleport(m_ExplorePos, m_ExploreAngle);
				m_Demo.Input.AllowGameplayInput = true;
				break;
			}
			m_Demo.LastInputTime = Time.time;
			m_ExamplesCurrentSel = m_Demo.ButtonSelection;
		}
		if (m_Demo.ButtonSelection != 2 && m_Demo.ButtonSelection != 3)
		{
			m_Demo.LockControls();
			m_Demo.Input.AllowGameplayInput = false;
		}
		else if (m_Demo.ButtonSelection != 3)
		{
			m_Demo.LockControls();
			m_Demo.Input.AllowGameplayInput = false;
		}
		if (m_Demo.ButtonSelection != 3 && m_Demo.WeaponHandler.CurrentWeaponIndex != 1)
		{
			m_Demo.WeaponHandler.SetWeapon(1);
		}
		switch (m_Demo.ButtonSelection)
		{
		case 0:
			m_Demo.Camera.Angle = m_SlimeAngle;
			break;
		case 2:
		{
			Vector2 o = m_Demo.PlayerEventHandler.InputMoveVector.Get();
			o.y = (m_RunForward ? 1f : 0f);
			m_Demo.PlayerEventHandler.InputMoveVector.Set(o);
			m_Demo.PlayerEventHandler.Jump.Active = m_Jump;
			if (m_Demo.Controller.StateEnabled("Run") != m_RunForward)
			{
				m_Demo.Controller.SetState("Run", m_RunForward, recursive: true);
			}
			m_Demo.SmoothLookAt(m_LookPoints[m_LookPoint]);
			break;
		}
		case 4:
			m_Demo.PlayerEventHandler.Jump.Active = m_Jump;
			break;
		case 5:
			m_Demo.SmoothLookAt(m_LookPoints[m_LookPoint]);
			break;
		case 3:
			if (m_Demo.ShowGUI && vp_Utility.LockCursor)
			{
				GUI.color = new Color(1f, 1f, 1f, m_Demo.ClosingDown ? m_Demo.GlobalAlpha : 1f);
				GUI.Label(new Rect(Screen.width / 2 - 200, 140f, 400f, 20f), "(Press ENTER to reenable menu)", m_Demo.CenterStyle);
				GUI.color = new Color(0f, 0f, 0f, 1f * (1f - (Time.time - m_WASDInfoClickTime) * 0.05f));
				GUI.Label(new Rect(Screen.width / 2 - 200, 170f, 400f, 20f), "(Use WASD to move around freely)", m_Demo.CenterStyle);
				GUI.color = new Color(1f, 1f, 1f, 1f * m_Demo.GlobalAlpha);
			}
			break;
		}
		if (m_Demo.ShowGUI)
		{
			m_ExamplesCurrentSel = m_Demo.ButtonSelection;
			string[] strings = new string[8] { "Mud... or Slime", "Wet Roof", "Action Hero", "Moving Platforms", "Head Bumps", "Wall Deflection", "Fall Deflection", "Blown Away" };
			m_Demo.ButtonSelection = m_Demo.ToggleColumn(140, 150, m_Demo.ButtonSelection, strings, center: false, arrow: true, ImageRightPointer, ImageLeftPointer);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoPresets()
	{
		if (m_Demo.FirstFrame)
		{
			m_GoAgainTimer.Cancel();
			m_Demo.FirstFrame = false;
			m_Demo.DrawCrosshair = false;
			m_Demo.FreezePlayer(m_OverViewPos, m_OverViewAngle, freezeCamera: true);
			m_Demo.WeaponHandler.CancelTimers();
			m_Demo.WeaponHandler.SetWeapon(0);
			m_Demo.Input.MouseCursorZones[0] = new Rect((float)Screen.width * 0.5f - 370f, 40f, 80f, 80f);
			m_Demo.Input.MouseCursorZones[1] = new Rect((float)Screen.width * 0.5f + 290f, 40f, 80f, 80f);
			m_Demo.Input.MouseCursorForced = true;
		}
		m_Demo.DrawBoxes("states & presets", "You may easily design custom movement STATES (like running, crouching or proning).\nWhen happy with your tweaks, save them to PRESET FILES, and the STATE MANAGER\nwill blend smoothly between them at runtime.", ImageLeftArrow, ImageRightArrow, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			m_Demo.LoadLevel(0);
		});
		m_Demo.DrawImage(ImagePresetDialogs);
		m_Demo.ForceCameraShake();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoOutro()
	{
		if (m_Demo.FirstFrame)
		{
			m_Demo.FirstFrame = false;
			m_Demo.DrawCrosshair = false;
			m_Demo.FreezePlayer(m_OutroPos, m_OutroAngle, freezeCamera: true);
			m_Demo.Input.MouseCursorForced = true;
			m_OutroStartTime = Time.time;
			m_Demo.PlayerEventHandler.Platform.Set(null);
		}
		m_FPSCamera.Angle = new Vector2(m_OutroAngle.x, m_OutroAngle.y + Mathf.Cos((Time.time - m_OutroStartTime + 50f) * 0.03f) * 20f);
		m_Demo.DrawBoxes("WHAT YOU GET", "• An in-depth 100+ page MANUAL with many tutorials to get you started EASILY.\n• Tons of scripts, art & sound FX. • Full, well commented C# SOURCE CODE.\n• A FANTASTIC starting point (or upgrade) for any FPS project.\nBest part? It can be yours in a minute! GET IT NOW on visionpunk.com ...", ImageLeftArrow, ImageCheckmark, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			m_Demo.LoadLevel(0);
		});
		m_Demo.DrawImage(ImageAllParams);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FallImpact(float f)
	{
		if (f > 0.2f)
		{
			vp_AudioUtility.PlayRandomSound(Player.GetComponent<AudioSource>(), FallImpactSounds);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGUI()
	{
		m_Demo.OnGUI();
		switch (m_Demo.CurrentScreen)
		{
		case 1:
			DemoPhysics();
			break;
		case 2:
			DemoOutro();
			break;
		}
	}
}
