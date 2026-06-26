using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class vp_DamageHandler : MonoBehaviour
{
	public float MaxHealth = 1f;

	public GameObject[] DeathSpawnObjects;

	public float MinDeathDelay;

	public float MaxDeathDelay;

	public float CurrentHealth;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_InstaKill;

	public AudioClip DeathSound;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioSource m_Audio;

	public float ImpactDamageThreshold = 10f;

	public float ImpactDamageMultiplier;

	[HideInInspector]
	public bool Respawns;

	[HideInInspector]
	public float MinRespawnTime = -99999f;

	[HideInInspector]
	public float MaxRespawnTime = -99999f;

	[HideInInspector]
	public float RespawnCheckRadius = -99999f;

	[HideInInspector]
	public AudioClip RespawnSound;

	[HideInInspector]
	public GameObject DeathEffect;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static Dictionary<Collider, vp_DamageHandler> m_DamageHandlersByCollider;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static vp_DamageHandler m_GetDamageHandlerOfColliderResult;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_StartPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Quaternion m_StartRotation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Transform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Respawner m_Respawner;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Source;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_OriginalSource;

	public static Dictionary<Collider, vp_DamageHandler> DamageHandlersByCollider
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_DamageHandlersByCollider == null)
			{
				m_DamageHandlersByCollider = new Dictionary<Collider, vp_DamageHandler>(100);
			}
			return m_DamageHandlersByCollider;
		}
	}

	public Transform Transform
	{
		get
		{
			if (m_Transform == null)
			{
				m_Transform = base.transform;
			}
			return m_Transform;
		}
	}

	public vp_Respawner Respawner
	{
		get
		{
			if (m_Respawner == null)
			{
				m_Respawner = GetComponent<vp_Respawner>();
			}
			return m_Respawner;
		}
	}

	public Transform Source
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_Source == null)
			{
				m_Source = Transform;
			}
			return m_Source;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			m_Source = value;
		}
	}

	public Transform OriginalSource
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_OriginalSource == null)
			{
				m_OriginalSource = Transform;
			}
			return m_OriginalSource;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			m_OriginalSource = value;
		}
	}

	[Obsolete("This property will be removed in an upcoming release.")]
	public Transform Sender
	{
		get
		{
			return Source;
		}
		set
		{
			Source = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		m_Audio = GetComponent<AudioSource>();
		CurrentHealth = MaxHealth;
		CheckForObsoleteParams();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		SceneManager.sceneLoaded += NotifyLevelWasLoaded;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		SceneManager.sceneLoaded -= NotifyLevelWasLoaded;
	}

	public virtual void Damage(float damage)
	{
		Damage(new vp_DamageInfo(damage, null));
	}

	public virtual void Damage(vp_DamageInfo damageInfo)
	{
		if (!base.enabled || !vp_Utility.IsActive(base.gameObject) || CurrentHealth <= 0f)
		{
			return;
		}
		if (damageInfo != null)
		{
			if (damageInfo.Source != null)
			{
				Source = damageInfo.Source;
			}
			if (damageInfo.OriginalSource != null)
			{
				OriginalSource = damageInfo.OriginalSource;
			}
		}
		CurrentHealth = Mathf.Min(CurrentHealth - damageInfo.Damage, MaxHealth);
		if (!vp_Gameplay.isMaster)
		{
			return;
		}
		if (vp_Gameplay.isMultiplayer && damageInfo.Source != null)
		{
			vp_GlobalEvent<Transform, Transform, float>.Send("Damage", Transform.root, damageInfo.OriginalSource, damageInfo.Damage, vp_GlobalEventMode.REQUIRE_LISTENER);
		}
		if (!(CurrentHealth <= 0f))
		{
			return;
		}
		if (m_InstaKill)
		{
			SendMessage("Die");
			return;
		}
		vp_Timer.In(UnityEngine.Random.Range(MinDeathDelay, MaxDeathDelay), [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			SendMessage("Die");
		});
	}

	public virtual void DieBySources(Transform[] sourceAndOriginalSource)
	{
		if (sourceAndOriginalSource.Length != 2)
		{
			Debug.LogWarning("Warning (" + this?.ToString() + ") 'DieBySources' argument must contain 2 transforms.");
			return;
		}
		Source = sourceAndOriginalSource[0];
		OriginalSource = sourceAndOriginalSource[1];
		Die();
	}

	public virtual void DieBySource(Transform source)
	{
		Transform originalSource = (Source = source);
		OriginalSource = originalSource;
		Die();
	}

	public virtual void Die()
	{
		if (!base.enabled || !vp_Utility.IsActive(base.gameObject))
		{
			return;
		}
		if (m_Audio != null)
		{
			m_Audio.pitch = Time.timeScale;
			m_Audio.PlayOneShot(DeathSound);
		}
		GameObject[] deathSpawnObjects = DeathSpawnObjects;
		foreach (GameObject gameObject in deathSpawnObjects)
		{
			if (gameObject != null)
			{
				GameObject gameObject2 = (GameObject)vp_Utility.Instantiate(gameObject, Transform.position, Transform.rotation);
				if (Source != null && gameObject2 != null)
				{
					vp_TargetEvent<Transform>.Send(gameObject2.transform, "SetSource", OriginalSource);
				}
			}
		}
		if (Respawner == null)
		{
			vp_Utility.Destroy(base.gameObject);
		}
		else
		{
			RemoveBulletHoles();
			vp_Utility.Activate(base.gameObject, activate: false);
		}
		m_InstaKill = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Reset()
	{
		CurrentHealth = MaxHealth;
		Source = null;
		OriginalSource = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void RemoveBulletHoles()
	{
		vp_HitscanBullet[] componentsInChildren = GetComponentsInChildren<vp_HitscanBullet>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			vp_Utility.Destroy(componentsInChildren[i].gameObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnCollisionEnter(Collision collision)
	{
		float num = collision.relativeVelocity.sqrMagnitude * 0.1f;
		float num2 = ((num > ImpactDamageThreshold) ? (num * ImpactDamageMultiplier) : 0f);
		if (!(num2 <= 0f))
		{
			if (CurrentHealth - num2 <= 0f)
			{
				m_InstaKill = true;
			}
			Damage(num2);
		}
	}

	public static vp_DamageHandler GetDamageHandlerOfCollider(Collider col)
	{
		if (!DamageHandlersByCollider.TryGetValue(col, out m_GetDamageHandlerOfColliderResult))
		{
			m_GetDamageHandlerOfColliderResult = col.transform.root.GetComponentInChildren<vp_DamageHandler>();
			DamageHandlersByCollider.Add(col, m_GetDamageHandlerOfColliderResult);
		}
		return m_GetDamageHandlerOfColliderResult;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void NotifyLevelWasLoaded(Scene scene, LoadSceneMode mode)
	{
		DamageHandlersByCollider.Clear();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Respawn()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Reactivate()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckForObsoleteParams()
	{
		if (DeathEffect != null)
		{
			Debug.LogWarning(this?.ToString() + "'DeathEffect' is obsolete! Please use the 'DeathSpawnObjects' array instead.");
		}
		string text = "";
		if (Respawns)
		{
			text += "Respawns, ";
		}
		if (MinRespawnTime != -99999f)
		{
			text += "MinRespawnTime, ";
		}
		if (MaxRespawnTime != -99999f)
		{
			text += "MaxRespawnTime, ";
		}
		if (RespawnCheckRadius != -99999f)
		{
			text += "RespawnCheckRadius, ";
		}
		if (RespawnSound != null)
		{
			text += "RespawnSound, ";
		}
		if (text != "")
		{
			text = text.Remove(text.LastIndexOf(", "));
			Debug.LogWarning(string.Format("Warning + (" + this?.ToString() + ") The following parameters are obsolete: \"{0}\". Creating a temp vp_Respawner component. To remove this warning, see the UFPS menu -> Wizards -> Convert Old DamageHandlers.", text));
			CreateTempRespawner();
		}
	}

	public bool CreateTempRespawner()
	{
		if ((bool)GetComponent<vp_Respawner>() || (bool)GetComponent<vp_PlayerRespawner>())
		{
			DisableOldParams();
			return false;
		}
		CreateRespawnerForDamageHandler(this);
		DisableOldParams();
		return true;
	}

	public static int GenerateRespawnersForAllDamageHandlers()
	{
		if (UnityEngine.Object.FindObjectsOfType(typeof(vp_PlayerDamageHandler)) is vp_PlayerDamageHandler[] array && array.Length != 0)
		{
			vp_PlayerDamageHandler[] array2 = array;
			foreach (vp_PlayerDamageHandler vp_PlayerDamageHandler2 in array2)
			{
				if (!(vp_PlayerDamageHandler2.transform.GetComponent<vp_FPPlayerEventHandler>() == null))
				{
					vp_FPPlayerDamageHandler obj = vp_PlayerDamageHandler2.gameObject.AddComponent<vp_FPPlayerDamageHandler>();
					obj.AllowFallDamage = vp_PlayerDamageHandler2.AllowFallDamage;
					obj.DeathEffect = vp_PlayerDamageHandler2.DeathEffect;
					obj.DeathSound = vp_PlayerDamageHandler2.DeathSound;
					obj.DeathSpawnObjects = vp_PlayerDamageHandler2.DeathSpawnObjects;
					obj.FallImpactPitch = vp_PlayerDamageHandler2.FallImpactPitch;
					obj.FallImpactSounds = vp_PlayerDamageHandler2.FallImpactSounds;
					obj.FallImpactThreshold = vp_PlayerDamageHandler2.FallImpactThreshold;
					obj.ImpactDamageMultiplier = vp_PlayerDamageHandler2.ImpactDamageMultiplier;
					obj.ImpactDamageThreshold = vp_PlayerDamageHandler2.ImpactDamageThreshold;
					obj.m_Audio = vp_PlayerDamageHandler2.m_Audio;
					obj.CurrentHealth = vp_PlayerDamageHandler2.CurrentHealth;
					obj.m_StartPosition = vp_PlayerDamageHandler2.m_StartPosition;
					obj.m_StartRotation = vp_PlayerDamageHandler2.m_StartRotation;
					obj.MaxDeathDelay = vp_PlayerDamageHandler2.MaxDeathDelay;
					obj.MaxHealth = vp_PlayerDamageHandler2.MaxHealth;
					obj.MaxRespawnTime = vp_PlayerDamageHandler2.MaxRespawnTime;
					obj.MinDeathDelay = vp_PlayerDamageHandler2.MinDeathDelay;
					obj.MinRespawnTime = vp_PlayerDamageHandler2.MinRespawnTime;
					obj.RespawnCheckRadius = vp_PlayerDamageHandler2.RespawnCheckRadius;
					obj.Respawns = vp_PlayerDamageHandler2.Respawns;
					obj.RespawnSound = vp_PlayerDamageHandler2.RespawnSound;
					UnityEngine.Object.DestroyImmediate(vp_PlayerDamageHandler2);
				}
			}
		}
		vp_DamageHandler[] obj2 = UnityEngine.Object.FindObjectsOfType(typeof(vp_DamageHandler)) as vp_DamageHandler[];
		vp_DamageHandler[] array3 = UnityEngine.Object.FindObjectsOfType(typeof(vp_FPPlayerDamageHandler)) as vp_DamageHandler[];
		int num = 0;
		vp_DamageHandler[] array4 = obj2;
		for (int i = 0; i < array4.Length; i++)
		{
			if (array4[i].CreateTempRespawner())
			{
				num++;
			}
		}
		array4 = array3;
		for (int i = 0; i < array4.Length; i++)
		{
			if (array4[i].CreateTempRespawner())
			{
				num++;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DisableOldParams()
	{
		Respawns = false;
		MinRespawnTime = -99999f;
		MaxRespawnTime = -99999f;
		RespawnCheckRadius = -99999f;
		RespawnSound = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateRespawnerForDamageHandler(vp_DamageHandler damageHandler)
	{
		if ((bool)damageHandler.gameObject.GetComponent<vp_Respawner>() || (bool)damageHandler.gameObject.GetComponent<vp_PlayerRespawner>())
		{
			return;
		}
		vp_Respawner vp_Respawner2 = null;
		vp_Respawner2 = ((!(damageHandler is vp_FPPlayerDamageHandler)) ? damageHandler.gameObject.AddComponent<vp_Respawner>() : damageHandler.gameObject.AddComponent<vp_PlayerRespawner>());
		if (!(vp_Respawner2 == null))
		{
			if (damageHandler.MinRespawnTime != -99999f)
			{
				vp_Respawner2.MinRespawnTime = damageHandler.MinRespawnTime;
			}
			if (damageHandler.MaxRespawnTime != -99999f)
			{
				vp_Respawner2.MaxRespawnTime = damageHandler.MaxRespawnTime;
			}
			if (damageHandler.RespawnCheckRadius != -99999f)
			{
				vp_Respawner2.ObstructionRadius = damageHandler.RespawnCheckRadius;
			}
			if (damageHandler.RespawnSound != null)
			{
				vp_Respawner2.SpawnSound = damageHandler.RespawnSound;
			}
		}
	}
}
