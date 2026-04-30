using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class vp_Respawner : MonoBehaviour
{
	public enum SpawnMode
	{
		SamePosition,
		SpawnPoint
	}

	public enum ObstructionSolver
	{
		Wait,
		AdjustPlacement
	}

	public SpawnMode m_SpawnMode;

	public string SpawnPointTag = "";

	public ObstructionSolver m_ObstructionSolver;

	public float ObstructionRadius = 1f;

	public float MinRespawnTime = 3f;

	public float MaxRespawnTime = 3f;

	public float LastRespawnTime;

	public bool SpawnOnAwake;

	public AudioClip SpawnSound;

	public GameObject[] SpawnFXPrefabs;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_InitialPosition = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Quaternion m_InitialRotation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Placement Placement = new vp_Placement();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Transform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioSource m_Audio;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_IsInitialSpawnOnAwake;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Timer.Handle m_RespawnTimer = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static Dictionary<Collider, vp_Respawner> m_RespawnersByCollider;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static vp_Respawner m_GetRespawnerOfColliderResult;

	public static Dictionary<Collider, vp_Respawner> RespawnersByCollider
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_RespawnersByCollider == null)
			{
				m_RespawnersByCollider = new Dictionary<Collider, vp_Respawner>(100);
			}
			return m_RespawnersByCollider;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		m_Transform = base.transform;
		m_Audio = GetComponent<AudioSource>();
		Placement.Position = (m_InitialPosition = m_Transform.position);
		Placement.Rotation = (m_InitialRotation = m_Transform.rotation);
		if (m_SpawnMode == SpawnMode.SamePosition)
		{
			SpawnPointTag = "";
		}
		if (SpawnOnAwake)
		{
			m_IsInitialSpawnOnAwake = true;
			vp_Utility.Activate(base.gameObject, activate: false);
			PickSpawnPoint();
		}
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SpawnFX()
	{
		if (!m_IsInitialSpawnOnAwake)
		{
			if (m_Audio != null)
			{
				m_Audio.pitch = Time.timeScale;
				m_Audio.PlayOneShot(SpawnSound);
			}
			if (SpawnFXPrefabs != null && SpawnFXPrefabs.Length != 0)
			{
				GameObject[] spawnFXPrefabs = SpawnFXPrefabs;
				foreach (GameObject gameObject in spawnFXPrefabs)
				{
					if (gameObject != null)
					{
						vp_Utility.Instantiate(gameObject, m_Transform.position, m_Transform.rotation);
					}
				}
			}
		}
		m_IsInitialSpawnOnAwake = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Die()
	{
		vp_Timer.In(UnityEngine.Random.Range(MinRespawnTime, MaxRespawnTime), PickSpawnPoint, m_RespawnTimer);
	}

	public virtual void PickSpawnPoint()
	{
		if (this == null)
		{
			return;
		}
		if (m_SpawnMode == SpawnMode.SamePosition || vp_SpawnPoint.SpawnPoints.Count < 1)
		{
			Placement.Position = m_InitialPosition;
			Placement.Rotation = m_InitialRotation;
			if (Placement.IsObstructed(ObstructionRadius))
			{
				switch (m_ObstructionSolver)
				{
				case ObstructionSolver.Wait:
					vp_Timer.In(UnityEngine.Random.Range(MinRespawnTime, MaxRespawnTime), PickSpawnPoint, m_RespawnTimer);
					return;
				case ObstructionSolver.AdjustPlacement:
					if (!vp_Placement.AdjustPosition(Placement, ObstructionRadius))
					{
						vp_Timer.In(UnityEngine.Random.Range(MinRespawnTime, MaxRespawnTime), PickSpawnPoint, m_RespawnTimer);
						return;
					}
					break;
				}
			}
		}
		else
		{
			switch (m_ObstructionSolver)
			{
			case ObstructionSolver.Wait:
				Placement = vp_SpawnPoint.GetRandomPlacement(0f, SpawnPointTag);
				if (Placement == null)
				{
					Placement = new vp_Placement();
					m_SpawnMode = SpawnMode.SamePosition;
					PickSpawnPoint();
				}
				if (Placement.IsObstructed(ObstructionRadius))
				{
					vp_Timer.In(UnityEngine.Random.Range(MinRespawnTime, MaxRespawnTime), PickSpawnPoint, m_RespawnTimer);
					return;
				}
				break;
			case ObstructionSolver.AdjustPlacement:
				Placement = vp_SpawnPoint.GetRandomPlacement(ObstructionRadius, SpawnPointTag);
				if (Placement == null)
				{
					vp_Timer.In(UnityEngine.Random.Range(MinRespawnTime, MaxRespawnTime), PickSpawnPoint, m_RespawnTimer);
					return;
				}
				break;
			}
		}
		Respawn();
	}

	public virtual void PickSpawnPoint(Vector3 position, Quaternion rotation)
	{
		Placement.Position = position;
		Placement.Rotation = rotation;
		Respawn();
	}

	public virtual void Respawn()
	{
		LastRespawnTime = Time.time;
		vp_Utility.Activate(base.gameObject);
		SpawnFX();
		if (vp_Gameplay.isMaster)
		{
			vp_GlobalEvent<Transform, vp_Placement>.Send("Respawn", base.transform.root, Placement);
		}
		SendMessage("Reset");
		Placement.Position = m_InitialPosition;
		Placement.Rotation = m_InitialRotation;
	}

	public virtual void Reset()
	{
		if (Application.isPlaying)
		{
			m_Transform.position = Placement.Position;
			if (GetComponent<Rigidbody>() != null && !GetComponent<Rigidbody>().isKinematic)
			{
				GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
				GetComponent<Rigidbody>().velocity = Vector3.zero;
			}
		}
	}

	public static vp_Respawner GetRespawnerOfCollider(Collider col)
	{
		if (!RespawnersByCollider.TryGetValue(col, out m_GetRespawnerOfColliderResult))
		{
			m_GetRespawnerOfColliderResult = col.transform.root.GetComponentInChildren<vp_Respawner>();
			RespawnersByCollider.Add(col, m_GetRespawnerOfColliderResult);
		}
		return m_GetRespawnerOfColliderResult;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void NotifyLevelWasLoaded(Scene scene, LoadSceneMode mode)
	{
		RespawnersByCollider.Clear();
	}
}
