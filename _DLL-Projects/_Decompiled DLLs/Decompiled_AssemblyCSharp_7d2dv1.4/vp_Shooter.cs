using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class vp_Shooter : vp_Component
{
	public delegate void NetworkFunc();

	public delegate Vector3 FirePositionFunc();

	public delegate Quaternion FireRotationFunc();

	public delegate int FireSeedFunc();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public CharacterController m_CharacterController;

	public GameObject m_ProjectileSpawnPoint;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject m_ProjectileDefaultSpawnpoint;

	public GameObject ProjectilePrefab;

	public float ProjectileScale = 1f;

	public float ProjectileFiringRate = 0.3f;

	public float ProjectileSpawnDelay;

	public int ProjectileCount = 1;

	public float ProjectileSpread;

	public bool ProjectileSourceIsRoot = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_NextAllowedFireTime;

	public Vector3 MuzzleFlashPosition = Vector3.zero;

	public Vector3 MuzzleFlashScale = Vector3.one;

	public float MuzzleFlashFadeSpeed = 0.075f;

	public GameObject MuzzleFlashPrefab;

	public float MuzzleFlashDelay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject m_MuzzleFlash;

	public Transform m_MuzzleFlashSpawnPoint;

	public GameObject ShellPrefab;

	public float ShellScale = 1f;

	public Vector3 ShellEjectDirection = new Vector3(1f, 1f, 1f);

	public Vector3 ShellEjectPosition = new Vector3(1f, 0f, 1f);

	public float ShellEjectVelocity = 0.2f;

	public float ShellEjectDelay;

	public float ShellEjectSpin;

	public Transform m_ShellEjectSpawnPoint;

	public AudioClip SoundFire;

	public float SoundFireDelay;

	public Vector2 SoundFirePitch = new Vector2(1f, 1f);

	public NetworkFunc m_SendFireEventToNetworkFunc;

	public FirePositionFunc GetFirePosition;

	public FireRotationFunc GetFireRotation;

	public FireSeedFunc GetFireSeed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_CurrentFirePosition = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Quaternion m_CurrentFireRotation = Quaternion.identity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_CurrentFireSeed;

	public Vector3 FirePosition = Vector3.zero;

	public GameObject ProjectileSpawnPoint => m_ProjectileSpawnPoint;

	public GameObject MuzzleFlash
	{
		get
		{
			if (m_MuzzleFlash == null && MuzzleFlashPrefab != null && ProjectileSpawnPoint != null)
			{
				m_MuzzleFlash = (GameObject)vp_Utility.Instantiate(MuzzleFlashPrefab, ProjectileSpawnPoint.transform.position, ProjectileSpawnPoint.transform.rotation);
				m_MuzzleFlash.name = base.transform.name + "MuzzleFlash";
				m_MuzzleFlash.transform.parent = ProjectileSpawnPoint.transform;
			}
			return m_MuzzleFlash;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		if (m_ProjectileSpawnPoint == null)
		{
			m_ProjectileSpawnPoint = base.gameObject;
		}
		m_ProjectileDefaultSpawnpoint = m_ProjectileSpawnPoint;
		if (GetFirePosition == null)
		{
			GetFirePosition = [PublicizedFrom(EAccessModifier.Private)] () => FirePosition;
		}
		if (GetFireRotation == null)
		{
			GetFireRotation = [PublicizedFrom(EAccessModifier.Private)] () => m_ProjectileSpawnPoint.transform.rotation;
		}
		if (GetFireSeed == null)
		{
			GetFireSeed = [PublicizedFrom(EAccessModifier.Internal)] () => UnityEngine.Random.Range(0, 100);
		}
		m_CharacterController = m_ProjectileSpawnPoint.transform.root.GetComponentInChildren<CharacterController>();
		m_NextAllowedFireTime = Time.time;
		ProjectileSpawnDelay = Mathf.Min(ProjectileSpawnDelay, ProjectileFiringRate - 0.1f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		base.Audio.playOnAwake = false;
		base.Audio.dopplerLevel = 0f;
		RefreshDefaultState();
		Refresh();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void LateUpdate()
	{
		FirePosition = m_ProjectileSpawnPoint.transform.position;
	}

	public virtual bool CanFire()
	{
		if (Time.time < m_NextAllowedFireTime)
		{
			return false;
		}
		return true;
	}

	public virtual bool TryFire()
	{
		if (Time.time < m_NextAllowedFireTime)
		{
			return false;
		}
		Fire();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Fire()
	{
		m_NextAllowedFireTime = Time.time + ProjectileFiringRate;
		if (SoundFireDelay == 0f)
		{
			PlayFireSound();
		}
		else
		{
			vp_Timer.In(SoundFireDelay, PlayFireSound);
		}
		if (ProjectileSpawnDelay == 0f)
		{
			SpawnProjectiles();
		}
		else
		{
			vp_Timer.In(ProjectileSpawnDelay, [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				SpawnProjectiles();
			});
		}
		if (ShellEjectDelay == 0f)
		{
			EjectShell();
		}
		else
		{
			vp_Timer.In(ShellEjectDelay, EjectShell);
		}
		if (MuzzleFlashDelay == 0f)
		{
			ShowMuzzleFlash();
		}
		else
		{
			vp_Timer.In(MuzzleFlashDelay, ShowMuzzleFlash);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void PlayFireSound()
	{
		if (!(base.Audio == null))
		{
			base.Audio.pitch = UnityEngine.Random.Range(SoundFirePitch.x, SoundFirePitch.y) * Time.timeScale;
			base.Audio.clip = SoundFire;
			base.Audio.Play();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SpawnProjectiles()
	{
		if (!(ProjectilePrefab == null))
		{
			if (m_SendFireEventToNetworkFunc != null)
			{
				m_SendFireEventToNetworkFunc();
			}
			m_CurrentFirePosition = GetFirePosition();
			m_CurrentFireRotation = GetFireRotation();
			m_CurrentFireSeed = GetFireSeed();
			for (int i = 0; i < ProjectileCount; i++)
			{
				GameObject gameObject = null;
				gameObject = (GameObject)vp_Utility.Instantiate(ProjectilePrefab, m_CurrentFirePosition, m_CurrentFireRotation);
				gameObject.SendMessage("SetSource", ProjectileSourceIsRoot ? base.Root : base.Transform, SendMessageOptions.DontRequireReceiver);
				gameObject.transform.localScale = new Vector3(ProjectileScale, ProjectileScale, ProjectileScale);
				SetSpread(m_CurrentFireSeed * (i + 1), gameObject.transform);
			}
		}
	}

	public void SetSpread(int seed, Transform target)
	{
		UnityEngine.Random.InitState(seed);
		target.Rotate(0f, 0f, UnityEngine.Random.Range(0, 360));
		target.Rotate(0f, UnityEngine.Random.Range(0f - ProjectileSpread, ProjectileSpread), 0f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void ShowMuzzleFlash()
	{
		if (!(MuzzleFlash == null))
		{
			if (m_MuzzleFlashSpawnPoint != null && ProjectileSpawnPoint != null)
			{
				MuzzleFlash.transform.position = m_MuzzleFlashSpawnPoint.transform.position;
				MuzzleFlash.transform.rotation = ProjectileSpawnPoint.transform.rotation;
			}
			MuzzleFlash.SendMessage("Shoot", SendMessageOptions.DontRequireReceiver);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void EjectShell()
	{
		if (ShellPrefab == null)
		{
			return;
		}
		GameObject gameObject = null;
		gameObject = (GameObject)vp_Utility.Instantiate(ShellPrefab, (m_ShellEjectSpawnPoint == null) ? (FirePosition + m_ProjectileSpawnPoint.transform.TransformDirection(ShellEjectPosition)) : m_ShellEjectSpawnPoint.transform.position, m_ProjectileSpawnPoint.transform.rotation);
		gameObject.transform.localScale = new Vector3(ShellScale, ShellScale, ShellScale);
		vp_Layer.Set(gameObject.gameObject, 29);
		if ((bool)gameObject.GetComponent<Rigidbody>())
		{
			Vector3 force = ((m_ShellEjectSpawnPoint == null) ? (base.transform.TransformDirection(ShellEjectDirection).normalized * ShellEjectVelocity) : (m_ShellEjectSpawnPoint.transform.forward.normalized * ShellEjectVelocity));
			gameObject.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
		}
		if ((bool)m_CharacterController)
		{
			Vector3 velocity = m_CharacterController.velocity;
			gameObject.GetComponent<Rigidbody>().AddForce(velocity, ForceMode.VelocityChange);
		}
		if (ShellEjectSpin > 0f)
		{
			if (UnityEngine.Random.value > 0.5f)
			{
				gameObject.GetComponent<Rigidbody>().AddRelativeTorque(-UnityEngine.Random.rotation.eulerAngles * ShellEjectSpin);
			}
			else
			{
				gameObject.GetComponent<Rigidbody>().AddRelativeTorque(UnityEngine.Random.rotation.eulerAngles * ShellEjectSpin);
			}
		}
	}

	public virtual void DisableFiring(float seconds = 10000000f)
	{
		m_NextAllowedFireTime = Time.time + seconds;
	}

	public virtual void EnableFiring()
	{
		m_NextAllowedFireTime = Time.time;
	}

	public override void Refresh()
	{
		if (MuzzleFlash != null)
		{
			if (m_MuzzleFlashSpawnPoint == null)
			{
				if (ProjectileSpawnPoint == m_ProjectileDefaultSpawnpoint)
				{
					m_MuzzleFlashSpawnPoint = vp_Utility.GetTransformByNameInChildren(ProjectileSpawnPoint.transform, "muzzle");
				}
				else
				{
					m_MuzzleFlashSpawnPoint = vp_Utility.GetTransformByNameInChildren(base.Transform, "muzzle");
				}
			}
			if (m_MuzzleFlashSpawnPoint != null)
			{
				if (ProjectileSpawnPoint == m_ProjectileDefaultSpawnpoint)
				{
					m_MuzzleFlash.transform.parent = ProjectileSpawnPoint.transform.parent.parent.parent;
				}
				else
				{
					m_MuzzleFlash.transform.parent = ProjectileSpawnPoint.transform;
				}
			}
			else
			{
				m_MuzzleFlash.transform.parent = ProjectileSpawnPoint.transform;
				MuzzleFlash.transform.localPosition = MuzzleFlashPosition;
				MuzzleFlash.transform.rotation = ProjectileSpawnPoint.transform.rotation;
			}
			MuzzleFlash.transform.localScale = MuzzleFlashScale;
			MuzzleFlash.SendMessage("SetFadeSpeed", MuzzleFlashFadeSpeed, SendMessageOptions.DontRequireReceiver);
		}
		if (ShellPrefab != null && m_ShellEjectSpawnPoint == null && ProjectileSpawnPoint != null)
		{
			if (ProjectileSpawnPoint == m_ProjectileDefaultSpawnpoint)
			{
				m_ShellEjectSpawnPoint = vp_Utility.GetTransformByNameInChildren(ProjectileSpawnPoint.transform, "shell");
			}
			else
			{
				m_ShellEjectSpawnPoint = vp_Utility.GetTransformByNameInChildren(base.Transform, "shell");
			}
		}
	}

	public override void Activate()
	{
		base.Activate();
		if (MuzzleFlash != null)
		{
			vp_Utility.Activate(MuzzleFlash);
		}
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if (MuzzleFlash != null)
		{
			vp_Utility.Activate(MuzzleFlash, activate: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void DrawProjectileDebugInfo(int projectileIndex)
	{
		GameObject gameObject = vp_3DUtility.DebugPointer();
		gameObject.transform.rotation = GetFireRotation();
		gameObject.transform.position = GetFirePosition();
		GameObject gameObject2 = vp_3DUtility.DebugBall();
		if (Physics.Linecast(gameObject.transform.position, gameObject.transform.position + gameObject.transform.forward * 1000f, out var hitInfo, 1084850176) && !hitInfo.collider.isTrigger && base.Root.InverseTransformPoint(hitInfo.point).z > 0f)
		{
			gameObject2.transform.position = hitInfo.point;
		}
	}
}
