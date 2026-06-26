using System;
using UnityEngine;

public class vp_FPSDemo1 : MonoBehaviour
{
	public GameObject Player;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_FPSDemoManager m_Demo;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int m_ExamplesCurrentSel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Timer.Handle m_ChrashingAirplaneRestoreTimer = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Timer.Handle m_WeaponSwitchTimer = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_WeaponLayerToggle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_MouseLookPos = new Vector3(-8.093015f, 20.08f, 3.416737f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_OverviewPos = new Vector3(1.246535f, 32.08f, 21.43753f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_StartPos = new Vector3(-18.14881f, 20.08f, -24.16859f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_WeaponLayerPos = new Vector3(-19.43989f, 16.08f, 2.10474f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_ForcesPos = new Vector3(-8.093015f, 20.08f, 3.416737f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_MechPos = new Vector3(0.02941191f, 1.08f, -93.50691f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_DrunkPos = new Vector3(18.48685f, 21.08f, 24.05441f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_SniperPos = new Vector3(0.8841875f, 33.08f, 21.3446f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_OldSchoolPos = new Vector3(25.88745f, 0.08f, 23.08822f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_AstronautPos = new Vector3(20f, 20f, 16f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_UnFreezePosition = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_MouseLookAngle = new Vector2(0f, 33.10683f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_OverviewAngle = new Vector2(28.89369f, 224f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_PerspectiveAngle = new Vector2(27f, 223f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_StartAngle = new Vector2(0f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_WeaponLayerAngle = new Vector2(0f, -90f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_ForcesAngle = new Vector2(0f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_MechAngle = new Vector3(0f, 180f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_DrunkAngle = new Vector3(0f, -90f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_SniperAngle = new Vector2(20f, 180f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_OldSchoolAngle = new Vector2(0f, 180f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_AstronautAngle = new Vector2(0f, 269.5f);

	public Texture m_ImageEditorPreview;

	public Texture m_ImageEditorPreviewShow;

	public Texture m_ImageCameraMouse;

	public Texture m_ImageWeaponPosition;

	public Texture m_ImageWeaponPerspective;

	public Texture m_ImageWeaponPivot;

	public Texture m_ImageEditorScreenshot;

	public Texture m_ImageLeftArrow;

	public Texture m_ImageRightArrow;

	public Texture m_ImageCheckmark;

	public Texture m_ImageLeftPointer;

	public Texture m_ImageRightPointer;

	public Texture m_ImageUpPointer;

	public Texture m_ImageCrosshair;

	public Texture m_ImageFullScreen;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AudioSource m_AudioSource;

	public AudioClip m_StompSound;

	public AudioClip m_EarthquakeSound;

	public AudioClip m_ExplosionSound;

	public GameObject m_ArtilleryFX;

	public TextAsset ArtilleryCamera;

	public TextAsset ArtilleryController;

	public TextAsset ArtilleryInput;

	public TextAsset AstronautCamera;

	public TextAsset AstronautController;

	public TextAsset AstronautInput;

	public TextAsset CowboyCamera;

	public TextAsset CowboyController;

	public TextAsset CowboyWeapon;

	public TextAsset CowboyShooter;

	public TextAsset CowboyInput;

	public TextAsset CrouchController;

	public TextAsset CrouchInput;

	public TextAsset DefaultCamera;

	public TextAsset DefaultWeapon;

	public TextAsset DefaultInput;

	public TextAsset DrunkCamera;

	public TextAsset DrunkController;

	public TextAsset DrunkInput;

	public TextAsset ImmobileCamera;

	public TextAsset ImmobileController;

	public TextAsset ImmobileInput;

	public TextAsset MaceCamera;

	public TextAsset MaceWeapon;

	public TextAsset MaceInput;

	public TextAsset MafiaCamera;

	public TextAsset MafiaWeapon;

	public TextAsset MafiaShooter;

	public TextAsset MafiaInput;

	public TextAsset MechCamera;

	public TextAsset MechController;

	public TextAsset MechWeapon;

	public TextAsset MechShooter;

	public TextAsset MechInput;

	public TextAsset ModernCamera;

	public TextAsset ModernController;

	public TextAsset ModernWeapon;

	public TextAsset ModernShooter;

	public TextAsset ModernInput;

	public TextAsset MouseLowSensCamera;

	public TextAsset MouseLowSensInput;

	public TextAsset MouseRawUnityCamera;

	public TextAsset MouseRawUnityInput;

	public TextAsset MouseSmoothingCamera;

	public TextAsset MouseSmoothingInput;

	public TextAsset OldSchoolCamera;

	public TextAsset OldSchoolController;

	public TextAsset OldSchoolWeapon;

	public TextAsset OldSchoolShooter;

	public TextAsset OldSchoolInput;

	public TextAsset Persp1999Camera;

	public TextAsset Persp1999Weapon;

	public TextAsset Persp1999Input;

	public TextAsset PerspModernCamera;

	public TextAsset PerspModernWeapon;

	public TextAsset PerspModernInput;

	public TextAsset PerspOldCamera;

	public TextAsset PerspOldWeapon;

	public TextAsset PerspOldInput;

	public TextAsset PivotChestWeapon;

	public TextAsset PivotElbowWeapon;

	public TextAsset PivotMuzzleWeapon;

	public TextAsset PivotWristWeapon;

	public TextAsset SmackController;

	public TextAsset SniperCamera;

	public TextAsset SniperWeapon;

	public TextAsset SniperShooter;

	public TextAsset SniperInput;

	public TextAsset StompingCamera;

	public TextAsset StompingInput;

	public TextAsset SystemOFFCamera;

	public TextAsset SystemOFFController;

	public TextAsset SystemOFFShooter;

	public TextAsset SystemOFFWeapon;

	public TextAsset SystemOFFWeaponGlideIn;

	public TextAsset SystemOFFInput;

	public TextAsset TurretCamera;

	public TextAsset TurretWeapon;

	public TextAsset TurretShooter;

	public TextAsset TurretInput;

	public TextAsset WallFacingCamera;

	public TextAsset WallFacingWeapon;

	public TextAsset WallFacingInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		m_Demo = new vp_FPSDemoManager(Player);
		m_Demo.PlayerEventHandler.Register(this);
		m_Demo.CurrentFullScreenFadeTime = Time.time;
		m_Demo.DrawCrosshair = false;
		m_Demo.Input.MouseCursorZones = new Rect[3];
		m_Demo.Input.MouseCursorZones[0] = new Rect((float)Screen.width * 0.5f - 370f, 40f, 80f, 80f);
		m_Demo.Input.MouseCursorZones[1] = new Rect((float)Screen.width * 0.5f + 290f, 40f, 80f, 80f);
		m_Demo.Input.MouseCursorZones[2] = new Rect(0f, 0f, 150f, Screen.height);
		vp_Utility.LockCursor = false;
		m_Demo.Camera.RenderingFieldOfView = 20f;
		m_Demo.Camera.SnapZoom();
		m_Demo.Camera.PositionOffset = new Vector3(0f, 1.75f, 0.1f);
		m_AudioSource = m_Demo.Camera.gameObject.AddComponent<AudioSource>();
		m_Demo.PlayerEventHandler.SetWeapon.Disallow(10000000f);
		m_Demo.PlayerEventHandler.SetPrevWeapon.Try = [PublicizedFrom(EAccessModifier.Internal)] () => false;
		m_Demo.PlayerEventHandler.SetNextWeapon.Try = [PublicizedFrom(EAccessModifier.Internal)] () => false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		if (m_Demo.PlayerEventHandler != null)
		{
			m_Demo.PlayerEventHandler.Unregister(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		m_Demo.Update();
		if (m_Demo.CurrentScreen == 1 && m_Demo.WeaponHandler.CurrentWeapon != null)
		{
			m_Demo.WeaponHandler.SetWeapon(0);
		}
		if (m_Demo.CurrentScreen == 2)
		{
			if (Input.GetKeyDown(KeyCode.Backspace))
			{
				m_Demo.ButtonSelection = 0;
			}
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				m_Demo.ButtonSelection = 1;
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				m_Demo.ButtonSelection = 2;
			}
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				m_Demo.ButtonSelection = 3;
			}
			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				m_Demo.ButtonSelection = 4;
			}
			if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				m_Demo.ButtonSelection = 5;
			}
			if (Input.GetKeyDown(KeyCode.Alpha6))
			{
				m_Demo.ButtonSelection = 6;
			}
			if (Input.GetKeyDown(KeyCode.Alpha7))
			{
				m_Demo.ButtonSelection = 7;
			}
			if (Input.GetKeyDown(KeyCode.Alpha8))
			{
				m_Demo.ButtonSelection = 8;
			}
			if (Input.GetKeyDown(KeyCode.Alpha9))
			{
				m_Demo.ButtonSelection = 9;
			}
			if (Input.GetKeyDown(KeyCode.Alpha0))
			{
				m_Demo.ButtonSelection = 10;
			}
			if (Input.GetKeyDown(KeyCode.Q))
			{
				m_Demo.ButtonSelection--;
				if (m_Demo.ButtonSelection < 1)
				{
					m_Demo.ButtonSelection = 10;
				}
			}
			if (Input.GetKeyDown(KeyCode.E))
			{
				m_Demo.ButtonSelection++;
				if (m_Demo.ButtonSelection > 10)
				{
					m_Demo.ButtonSelection = 1;
				}
			}
		}
		m_Demo.Input.MouseCursorBlocksMouseLook = false;
		if (m_Demo.CurrentScreen != 3 && m_ChrashingAirplaneRestoreTimer.Active)
		{
			m_ChrashingAirplaneRestoreTimer.Cancel();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoIntro()
	{
		m_Demo.DrawBoxes("part ii: under the hood", "Ultimate FPS features a NEXT-GEN first person camera system with ultra smooth PROCEDURAL ANIMATION of player movements. Camera and weapons are manipulated using over 100 parameters, allowing for a vast range of super-lifelike behaviors.", null, m_ImageRightArrow);
		if (m_Demo.FirstFrame)
		{
			m_Demo.DrawCrosshair = false;
			m_Demo.FirstFrame = false;
			m_Demo.Camera.RenderingFieldOfView = 20f;
			m_Demo.Camera.SnapZoom();
			m_Demo.WeaponHandler.SetWeapon(0);
			m_Demo.FreezePlayer(m_OverviewPos, m_OverviewAngle, freezeCamera: true);
			m_Demo.LastInputTime -= 20f;
			m_Demo.RefreshDefaultState();
			m_Demo.Input.MouseCursorForced = true;
		}
		m_Demo.Input.MouseCursorForced = true;
		m_Demo.ForceCameraShake();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetWeapon(int i, string state = null, bool drawCrosshair = true, bool wieldMotion = true)
	{
		m_Demo.DrawCrosshair = drawCrosshair;
		if (m_Demo.WeaponHandler.CurrentWeaponIndex != i)
		{
			if (m_Demo.WeaponHandler.CurrentWeapon != null)
			{
				if (m_ExamplesCurrentSel == 0)
				{
					((vp_FPWeapon)m_Demo.WeaponHandler.CurrentWeapon).SnapToExit();
				}
				else if (wieldMotion)
				{
					m_Demo.WeaponHandler.CurrentWeapon.Wield(isWielding: false);
				}
			}
			vp_Timer.In(wieldMotion ? 0.2f : 0f, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				m_Demo.WeaponHandler.SetWeapon(i);
				if (m_Demo.WeaponHandler.CurrentWeapon != null && wieldMotion)
				{
					m_Demo.WeaponHandler.CurrentWeapon.Wield();
				}
				if (state != null)
				{
					m_Demo.PlayerEventHandler.ResetActivityStates();
					m_Demo.PlayerEventHandler.SetState(state);
				}
			}, m_WeaponSwitchTimer);
		}
		else if (state != null)
		{
			m_Demo.PlayerEventHandler.SetState(state);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoExamples()
	{
		m_Demo.DrawBoxes("examples", "Try MOVING, JUMPING and STRAFING with the demo presets on the left.\nNote that NO ANIMATIONS are used in this demo. Instead, the camera and weapons are manipulated using realtime SPRING PHYSICS, SINUS BOB and NOISE SHAKING.\nCombining this with traditional animations (e.g. reload) can be very powerful!", m_ImageLeftArrow, m_ImageRightArrow);
		if (m_Demo.FirstFrame)
		{
			m_AudioSource.Stop();
			m_Demo.DrawCrosshair = true;
			m_Demo.Teleport(m_StartPos, m_StartAngle);
			m_Demo.FirstFrame = false;
			m_UnFreezePosition = m_Demo.Controller.transform.position;
			m_Demo.ButtonSelection = 0;
			m_Demo.WeaponHandler.SetWeapon(3);
			m_Demo.PlayerEventHandler.SetState("Freeze", setActive: false);
			m_Demo.PlayerEventHandler.SetState("SystemOFF");
			if (m_Demo.WeaponHandler.CurrentWeapon != null)
			{
				((vp_FPWeapon)m_Demo.WeaponHandler.CurrentWeapon).SnapZoom();
			}
			m_Demo.Camera.SnapZoom();
			m_Demo.Camera.SnapSprings();
			m_Demo.Input.MouseCursorForced = true;
		}
		if (m_Demo.ButtonSelection != m_ExamplesCurrentSel)
		{
			vp_Utility.LockCursor = true;
			m_Demo.ResetState();
			m_Demo.PlayerEventHandler.Attack.Stop(0.5f);
			m_Demo.Camera.BobStepCallback = null;
			m_Demo.Camera.SnapSprings();
			if (m_ExamplesCurrentSel == 9 && m_Demo.WeaponHandler.CurrentWeapon != null)
			{
				((vp_FPWeapon)m_Demo.WeaponHandler.CurrentWeapon).SnapZoom();
				((vp_FPWeapon)m_Demo.WeaponHandler.CurrentWeapon).SnapSprings();
				((vp_FPWeapon)m_Demo.WeaponHandler.CurrentWeapon).SnapPivot();
			}
			switch (m_Demo.ButtonSelection)
			{
			case 0:
				m_Demo.PlayerEventHandler.Attack.Stop(10000000f);
				m_Demo.DrawCrosshair = true;
				m_Demo.Controller.Stop();
				if (m_Demo.WeaponHandler.CurrentWeaponIndex == 5)
				{
					m_Demo.WeaponHandler.SetWeapon(1);
					m_Demo.PlayerEventHandler.SetState("SystemOFF");
					break;
				}
				m_Demo.Camera.SnapZoom();
				m_Demo.PlayerEventHandler.SetState("SystemOFF");
				if (m_Demo.WeaponHandler.CurrentWeapon != null)
				{
					m_Demo.WeaponHandler.CurrentWeapon.SnapSprings();
					((vp_FPWeapon)m_Demo.WeaponHandler.CurrentWeapon).SnapZoom();
				}
				break;
			case 1:
				SetWeapon(3, "MafiaBoss");
				break;
			case 2:
				SetWeapon(1, "ModernShooter");
				break;
			case 3:
				SetWeapon(4, "Barbarian");
				break;
			case 4:
				SetWeapon(2, "SniperBreath");
				m_Demo.Controller.Stop();
				m_Demo.Teleport(m_SniperPos, m_SniperAngle);
				break;
			case 5:
				SetWeapon(0, "Astronaut", drawCrosshair: false);
				m_Demo.Controller.Stop();
				m_Demo.Teleport(m_AstronautPos, m_AstronautAngle);
				break;
			case 6:
				SetWeapon(5, "MechOrDino", drawCrosshair: true, wieldMotion: false);
				m_UnFreezePosition = m_DrunkPos;
				m_Demo.Controller.Stop();
				m_Demo.Teleport(m_MechPos, m_MechAngle);
				m_Demo.Camera.BobStepCallback = [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					m_Demo.Camera.AddForce2(new Vector3(0f, -1f, 0f));
					if (m_Demo.WeaponHandler.CurrentWeapon != null)
					{
						((vp_FPWeapon)m_Demo.WeaponHandler.CurrentWeapon).AddForce(new Vector3(0f, 0f, 0f), new Vector3(-0.3f, 0f, 0f));
					}
					m_AudioSource.pitch = Time.timeScale;
					m_AudioSource.PlayOneShot(m_StompSound);
				};
				break;
			case 7:
				SetWeapon(3, "TankTurret", drawCrosshair: true, wieldMotion: false);
				m_Demo.FreezePlayer(m_OverviewPos, m_OverviewAngle);
				m_Demo.Controller.Stop();
				break;
			case 8:
				m_Demo.Controller.Stop();
				SetWeapon(0, "DrunkPerson", drawCrosshair: false);
				m_Demo.Controller.Stop();
				m_Demo.Teleport(m_DrunkPos, m_DrunkAngle);
				m_Demo.Camera.StopSprings();
				m_Demo.Camera.Refresh();
				break;
			case 9:
				SetWeapon(1, "OldSchool");
				m_Demo.Controller.Stop();
				m_Demo.Teleport(m_OldSchoolPos, m_OldSchoolAngle);
				m_Demo.Camera.SnapSprings();
				m_Demo.Camera.SnapZoom();
				vp_Timer.In(0.3f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					if (m_Demo.WeaponHandler.CurrentWeapon != null)
					{
						vp_Shooter componentInChildren = m_Demo.WeaponHandler.CurrentWeapon.GetComponentInChildren<vp_Shooter>();
						componentInChildren.MuzzleFlashPosition = new Vector3(0.0025736f, -0.0813138f, 1.662671f);
						componentInChildren.Refresh();
					}
				});
				break;
			case 10:
				SetWeapon(2, "CrazyCowboy");
				m_Demo.Teleport(m_StartPos, m_StartAngle);
				m_Demo.Controller.Stop();
				break;
			}
			m_ExamplesCurrentSel = m_Demo.ButtonSelection;
		}
		if (m_Demo.ShowGUI)
		{
			m_ExamplesCurrentSel = m_Demo.ButtonSelection;
			string[] strings = new string[11]
			{
				"System OFF", "Mafia Boss", "Modern Shooter", "Barbarian", "Sniper Breath", "Astronaut", "Mech... or Dino?", "Tank Turret", "Drunk Person", "Old School",
				"Crazy Cowboy"
			};
			m_Demo.ButtonSelection = m_Demo.ToggleColumn(140, 150, m_Demo.ButtonSelection, strings, center: false, arrow: true, m_ImageRightPointer, m_ImageLeftPointer);
		}
		if (m_Demo.ShowGUI && vp_Utility.LockCursor)
		{
			GUI.color = new Color(1f, 1f, 1f, m_Demo.ClosingDown ? m_Demo.GlobalAlpha : 1f);
			GUI.Label(new Rect(Screen.width / 2 - 200, 140f, 400f, 20f), "(Press ENTER to reenable menu)", m_Demo.CenterStyle);
			GUI.color = new Color(1f, 1f, 1f, 1f * m_Demo.GlobalAlpha);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoForces()
	{
		m_Demo.DrawBoxes("external forces", "The camera and weapon are mounted on 8 positional and angular SPRINGS.\nEXTERNAL FORCES can be applied to these in various ways, creating unique movement patterns every time. This is useful for shockwaves, explosion knockback and earthquakes.", m_ImageLeftArrow, m_ImageRightArrow);
		if (m_Demo.FirstFrame)
		{
			m_Demo.DrawCrosshair = false;
			m_Demo.ResetState();
			m_Demo.Camera.Load(StompingCamera);
			m_Demo.Input.Load(StompingInput);
			m_Demo.WeaponHandler.SetWeapon(1);
			m_Demo.Controller.Load(SmackController);
			m_Demo.Camera.SnapZoom();
			m_Demo.FirstFrame = false;
			m_Demo.Teleport(m_ForcesPos, m_ForcesAngle);
			m_Demo.ButtonColumnArrowY = -100f;
			m_Demo.Input.MouseCursorForced = true;
		}
		if (!m_Demo.ShowGUI)
		{
			return;
		}
		m_Demo.ButtonSelection = -1;
		string[] strings = new string[4] { "Earthquake", "Boss Stomp", "Incoming Artillery", "Crashing Airplane" };
		m_Demo.ButtonSelection = m_Demo.ButtonColumn(150, m_Demo.ButtonSelection, strings, m_ImageRightPointer);
		if (m_Demo.ButtonSelection != -1)
		{
			switch (m_Demo.ButtonSelection)
			{
			case 0:
				m_Demo.Camera.Load(StompingCamera);
				m_Demo.Input.Load(StompingInput);
				m_Demo.Controller.Load(SmackController);
				m_Demo.PlayerEventHandler.CameraEarthQuake.TryStart(new Vector3(0.2f, 0.2f, 10f));
				m_Demo.ButtonColumnArrowFadeoutTime = Time.time + 9f;
				m_AudioSource.Stop();
				m_AudioSource.pitch = Time.timeScale;
				m_AudioSource.PlayOneShot(m_EarthquakeSound);
				break;
			case 1:
				m_Demo.PlayerEventHandler.CameraEarthQuake.Stop();
				m_Demo.Camera.Load(ArtilleryCamera);
				m_Demo.Input.Load(ArtilleryInput);
				m_Demo.Controller.Load(SmackController);
				m_Demo.PlayerEventHandler.CameraGroundStomp.Send(1f);
				m_Demo.ButtonColumnArrowFadeoutTime = Time.time;
				m_AudioSource.Stop();
				m_AudioSource.pitch = Time.timeScale;
				m_AudioSource.PlayOneShot(m_StompSound);
				break;
			case 2:
			{
				m_Demo.PlayerEventHandler.CameraEarthQuake.Stop();
				m_Demo.Camera.Load(ArtilleryCamera);
				m_Demo.Input.Load(ArtilleryInput);
				m_Demo.Controller.Load(ArtilleryController);
				m_Demo.PlayerEventHandler.CameraBombShake.Send(1f);
				m_Demo.Controller.AddForce(UnityEngine.Random.Range(-1.5f, 1.5f), 0.5f, UnityEngine.Random.Range(-1.5f, -0.5f));
				m_Demo.ButtonColumnArrowFadeoutTime = Time.time + 1f;
				m_AudioSource.Stop();
				m_AudioSource.pitch = Time.timeScale;
				m_AudioSource.PlayOneShot(m_ExplosionSound);
				Vector3 position = m_Demo.Controller.transform.TransformPoint(Vector3.forward * UnityEngine.Random.Range(1, 2));
				position.y = m_Demo.Controller.transform.position.y + 1f;
				UnityEngine.Object.Instantiate(m_ArtilleryFX, position, Quaternion.identity);
				break;
			}
			case 3:
				m_Demo.Camera.Load(StompingCamera);
				m_Demo.Input.Load(StompingInput);
				m_Demo.Controller.Load(SmackController);
				m_Demo.PlayerEventHandler.CameraEarthQuake.TryStart(new Vector3(0.25f, 0.2f, 10f));
				m_Demo.ButtonColumnArrowFadeoutTime = Time.time + 9f;
				m_AudioSource.Stop();
				m_AudioSource.pitch = Time.timeScale;
				m_AudioSource.PlayOneShot(m_EarthquakeSound);
				m_Demo.Camera.RenderingFieldOfView = 80f;
				m_Demo.Camera.RotationEarthQuakeFactor = 6.5f;
				m_Demo.Camera.Zoom();
				vp_Timer.In(9f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					m_Demo.Camera.RenderingFieldOfView = 60f;
					m_Demo.Camera.RotationEarthQuakeFactor = 0f;
					m_Demo.Camera.Zoom();
				}, m_ChrashingAirplaneRestoreTimer);
				break;
			}
			m_Demo.LastInputTime = Time.time;
		}
		m_Demo.DrawEditorPreview(m_ImageWeaponPosition, m_ImageEditorPreview, m_ImageEditorScreenshot);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoMouseInput()
	{
		m_Demo.DrawBoxes("mouse input", "Any good FPS should offer configurable MOUSE SMOOTHING and ACCELERATION.\n• Smoothing interpolates mouse input over several frames to reduce jittering.\n • Acceleration + low mouse sensitivity allows high precision without loss of turn speed.\n• Click the below buttons to compare some example setups.", m_ImageLeftArrow, m_ImageRightArrow);
		if (m_Demo.FirstFrame)
		{
			m_Demo.ResetState();
			m_AudioSource.Stop();
			m_Demo.DrawCrosshair = true;
			m_Demo.FreezePlayer(m_MouseLookPos, m_MouseLookAngle);
			m_Demo.FirstFrame = false;
			m_Demo.WeaponHandler.SetWeapon(0);
			m_Demo.Input.MouseCursorForced = true;
			m_Demo.Camera.Load(MouseRawUnityCamera);
			m_Demo.Input.Load(MouseRawUnityInput);
		}
		if (!m_Demo.ShowGUI)
		{
			return;
		}
		int buttonSelection = m_Demo.ButtonSelection;
		bool arrow = m_Demo.ButtonSelection != 2;
		string[] strings = new string[3] { "Raw Mouse Input", "Mouse Smoothing", "Low Sens. + Acceleration" };
		m_Demo.ButtonSelection = m_Demo.ToggleColumn(200, 150, m_Demo.ButtonSelection, strings, center: true, arrow, m_ImageRightPointer, m_ImageLeftPointer);
		if (m_Demo.ButtonSelection != buttonSelection)
		{
			switch (m_Demo.ButtonSelection)
			{
			case 0:
				m_Demo.PlayerEventHandler.ResetActivityStates();
				m_Demo.Camera.Load(MouseRawUnityCamera);
				m_Demo.Input.Load(MouseRawUnityInput);
				break;
			case 1:
				m_Demo.PlayerEventHandler.ResetActivityStates();
				m_Demo.Camera.Load(MouseSmoothingCamera);
				m_Demo.Input.Load(MouseSmoothingInput);
				break;
			case 2:
				m_Demo.PlayerEventHandler.ResetActivityStates();
				m_Demo.Camera.Load(MouseLowSensCamera);
				m_Demo.Input.Load(MouseLowSensInput);
				break;
			}
			m_Demo.LastInputTime = Time.time;
		}
		arrow = true;
		if (m_Demo.ButtonSelection != 2)
		{
			GUI.enabled = false;
			arrow = false;
		}
		m_Demo.Input.MouseLookAcceleration = m_Demo.ButtonToggle(new Rect(Screen.width / 2 + 110, 215f, 90f, 40f), "Acceleration", m_Demo.Input.MouseLookAcceleration, arrow, m_ImageUpPointer);
		GUI.color = new Color(1f, 1f, 1f, 1f * m_Demo.GlobalAlpha);
		GUI.enabled = true;
		m_Demo.DrawEditorPreview(m_ImageCameraMouse, m_ImageEditorPreview, m_ImageEditorScreenshot);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoWeaponPerspective()
	{
		m_Demo.DrawBoxes("weapon perspective", "Proper WEAPON PERSPECTIVE is crucial to the final impression of your game!\nThe weapon has its own separate Field of View for full perspective control,\nalong with dynamic position and rotation offset.", m_ImageLeftArrow, m_ImageRightArrow);
		if (m_Demo.FirstFrame)
		{
			m_Demo.ResetState();
			m_Demo.Camera.Load(PerspOldCamera);
			m_Demo.Input.Load(PerspOldInput);
			m_Demo.Camera.SnapZoom();
			m_Demo.FirstFrame = false;
			m_Demo.FreezePlayer(m_OverviewPos, m_PerspectiveAngle, freezeCamera: true);
			m_Demo.Input.MouseCursorForced = true;
			m_Demo.WeaponHandler.SetWeapon(3);
			m_Demo.SetWeaponPreset(PerspOldWeapon);
			if (m_Demo.WeaponHandler.CurrentWeapon != null)
			{
				m_Demo.WeaponHandler.CurrentWeapon.SetState("WeaponPersp");
			}
			m_Demo.WeaponHandler.SetWeaponLayer(10);
			if (m_Demo.WeaponHandler.CurrentWeapon != null)
			{
				((vp_FPWeapon)m_Demo.WeaponHandler.CurrentWeapon).SnapZoom();
				m_Demo.WeaponHandler.CurrentWeapon.SnapSprings();
				((vp_FPWeapon)m_Demo.WeaponHandler.CurrentWeapon).SnapPivot();
			}
		}
		if (!m_Demo.ShowGUI)
		{
			return;
		}
		int buttonSelection = m_Demo.ButtonSelection;
		string[] strings = new string[3] { "Old School", "1999 Internet Café", "Modern Shooter" };
		m_Demo.ButtonSelection = m_Demo.ToggleColumn(200, 150, m_Demo.ButtonSelection, strings, center: true, arrow: true, m_ImageRightPointer, m_ImageLeftPointer);
		if (m_Demo.ButtonSelection != buttonSelection)
		{
			switch (m_Demo.ButtonSelection)
			{
			case 0:
				m_Demo.SetWeaponPreset(PerspOldWeapon);
				break;
			case 1:
				m_Demo.SetWeaponPreset(Persp1999Weapon);
				break;
			case 2:
				m_Demo.SetWeaponPreset(PerspModernWeapon);
				break;
			}
			m_Demo.LastInputTime = Time.time;
		}
		m_Demo.DrawEditorPreview(m_ImageWeaponPerspective, m_ImageEditorPreview, m_ImageEditorScreenshot);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoWeaponLayer()
	{
		m_Demo.DrawBoxes("weapon camera", "\nThe weapon can be rendered by a SEPARATE CAMERA so that it never sticks through walls or other geometry. Try toggling the weapon camera ON and OFF below.", m_ImageLeftArrow, m_ImageRightArrow);
		if (m_Demo.FirstFrame)
		{
			m_Demo.ResetState();
			m_Demo.DrawCrosshair = true;
			m_Demo.Camera.Load(WallFacingCamera);
			m_Demo.Input.Load(WallFacingInput);
			m_Demo.WeaponHandler.SetWeapon(3);
			m_Demo.SetWeaponPreset(WallFacingWeapon);
			m_Demo.Camera.SnapZoom();
			m_WeaponLayerToggle = false;
			m_Demo.FirstFrame = false;
			m_Demo.FreezePlayer(m_WeaponLayerPos, m_WeaponLayerAngle);
			int weaponLayer = (m_WeaponLayerToggle ? 10 : 0);
			m_Demo.WeaponHandler.SetWeaponLayer(weaponLayer);
			m_Demo.Input.MouseCursorForced = true;
		}
		if (m_Demo.ShowGUI)
		{
			bool weaponLayerToggle = m_WeaponLayerToggle;
			m_WeaponLayerToggle = m_Demo.ButtonToggle(new Rect(Screen.width / 2 - 45, 180f, 100f, 40f), "Weapon Camera", m_WeaponLayerToggle, arrow: true, m_ImageUpPointer);
			if (weaponLayerToggle != m_WeaponLayerToggle)
			{
				m_Demo.FreezePlayer(m_WeaponLayerPos, m_WeaponLayerAngle);
				int weaponLayer2 = (m_WeaponLayerToggle ? 10 : 0);
				m_Demo.WeaponHandler.SetWeaponLayer(weaponLayer2);
				m_Demo.LastInputTime = Time.time;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoPivot()
	{
		m_Demo.DrawBoxes("weapon pivot", "The PIVOT POINT of the weapon model greatly affects movement pattern.\nManipulating it at runtime can be quite useful, and easy with Ultimate FPS!\nClick the examples below and move the camera around.", m_ImageLeftArrow, m_ImageRightArrow, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			m_Demo.LoadLevel(2);
		});
		if (m_Demo.FirstFrame)
		{
			m_Demo.ResetState();
			m_Demo.DrawCrosshair = false;
			m_Demo.Camera.Load(DefaultCamera);
			m_Demo.Input.Load(DefaultInput);
			m_Demo.Controller.Load(ImmobileController);
			m_Demo.FirstFrame = false;
			m_Demo.FreezePlayer(m_OverviewPos, m_OverviewAngle);
			m_Demo.WeaponHandler.SetWeapon(1);
			m_Demo.SetWeaponPreset(DefaultWeapon);
			m_Demo.SetWeaponPreset(PivotMuzzleWeapon);
			if (m_Demo.WeaponHandler.CurrentWeapon != null)
			{
				((vp_FPWeapon)m_Demo.WeaponHandler.CurrentWeapon).SetPivotVisible(visible: true);
			}
			m_Demo.Input.MouseCursorForced = true;
			m_Demo.WeaponHandler.SetWeaponLayer(10);
		}
		if (!m_Demo.ShowGUI)
		{
			return;
		}
		int buttonSelection = m_Demo.ButtonSelection;
		string[] strings = new string[4] { "Muzzle", "Grip", "Chest", "Elbow (Uzi Style)" };
		m_Demo.ButtonSelection = m_Demo.ToggleColumn(200, 150, m_Demo.ButtonSelection, strings, center: true, arrow: true, m_ImageRightPointer, m_ImageLeftPointer);
		if (m_Demo.ButtonSelection != buttonSelection)
		{
			switch (m_Demo.ButtonSelection)
			{
			case 0:
				m_Demo.SetWeaponPreset(PivotMuzzleWeapon);
				break;
			case 1:
				m_Demo.SetWeaponPreset(PivotWristWeapon);
				break;
			case 2:
				m_Demo.SetWeaponPreset(PivotChestWeapon);
				break;
			case 3:
				m_Demo.SetWeaponPreset(PivotElbowWeapon);
				break;
			}
			m_Demo.LastInputTime = Time.time;
		}
		m_Demo.DrawEditorPreview(m_ImageWeaponPivot, m_ImageEditorPreview, m_ImageEditorScreenshot);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGUI()
	{
		m_Demo.OnGUI();
		switch (m_Demo.CurrentScreen)
		{
		case 1:
			DemoIntro();
			break;
		case 2:
			DemoExamples();
			break;
		case 3:
			DemoForces();
			break;
		case 4:
			DemoMouseInput();
			break;
		case 5:
			DemoWeaponPerspective();
			break;
		case 6:
			DemoWeaponLayer();
			break;
		case 7:
			DemoPivot();
			break;
		}
	}
}
