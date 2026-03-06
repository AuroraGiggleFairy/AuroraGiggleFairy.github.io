using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class vp_SpawnPoint : MonoBehaviour
{
	public bool RandomDirection;

	public float Radius;

	public float GroundSnapThreshold = 2.5f;

	public bool LockGroundSnapToRadius = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static List<vp_SpawnPoint> m_MatchingSpawnPoints = new List<vp_SpawnPoint>(50);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static List<vp_SpawnPoint> m_SpawnPoints = null;

	public static List<vp_SpawnPoint> SpawnPoints
	{
		get
		{
			if (m_SpawnPoints == null)
			{
				m_SpawnPoints = new List<vp_SpawnPoint>(UnityEngine.Object.FindObjectsOfType(typeof(vp_SpawnPoint)) as vp_SpawnPoint[]);
			}
			return m_SpawnPoints;
		}
	}

	public static vp_Placement GetRandomPlacement()
	{
		return GetRandomPlacement(0f, null);
	}

	public static vp_Placement GetRandomPlacement(float physicsCheckRadius)
	{
		return GetRandomPlacement(physicsCheckRadius, null);
	}

	public static vp_Placement GetRandomPlacement(string tag)
	{
		return GetRandomPlacement(0f, tag);
	}

	public static vp_Placement GetRandomPlacement(float physicsCheckRadius, string tag)
	{
		if (SpawnPoints == null || SpawnPoints.Count < 1)
		{
			return null;
		}
		vp_SpawnPoint vp_SpawnPoint2 = null;
		if (string.IsNullOrEmpty(tag))
		{
			vp_SpawnPoint2 = GetRandomSpawnPoint();
		}
		else
		{
			vp_SpawnPoint2 = GetRandomSpawnPoint(tag);
			if (vp_SpawnPoint2 == null)
			{
				vp_SpawnPoint2 = GetRandomSpawnPoint();
				Debug.LogWarning("Warning (vp_SpawnPoint --> GetRandomPlacement) Could not find a spawnpoint tagged '" + tag + "'. Falling back to 'any random spawnpoint'.");
			}
		}
		if (vp_SpawnPoint2 == null)
		{
			Debug.LogError("Error (vp_SpawnPoint --> GetRandomPlacement) Could not find a spawnpoint" + ((!string.IsNullOrEmpty(tag)) ? (" tagged '" + tag + "'") : ".") + " Reverting to world origin.");
			return null;
		}
		vp_Placement vp_Placement2 = new vp_Placement();
		vp_Placement2.Position = vp_SpawnPoint2.transform.position;
		if (vp_SpawnPoint2.Radius > 0f)
		{
			Vector3 vector = UnityEngine.Random.insideUnitSphere * vp_SpawnPoint2.Radius;
			vp_Placement2.Position.x += vector.x;
			vp_Placement2.Position.z += vector.z;
		}
		if (physicsCheckRadius != 0f)
		{
			if (!vp_Placement.AdjustPosition(vp_Placement2, physicsCheckRadius))
			{
				return null;
			}
			vp_Placement.SnapToGround(vp_Placement2, physicsCheckRadius, vp_SpawnPoint2.GroundSnapThreshold);
		}
		if (vp_SpawnPoint2.RandomDirection)
		{
			vp_Placement2.Rotation = Quaternion.Euler(Vector3.up * UnityEngine.Random.Range(0f, 360f));
		}
		else
		{
			vp_Placement2.Rotation = vp_SpawnPoint2.transform.rotation;
		}
		return vp_Placement2;
	}

	public static vp_SpawnPoint GetRandomSpawnPoint()
	{
		if (SpawnPoints.Count < 1)
		{
			return null;
		}
		return SpawnPoints[UnityEngine.Random.Range(0, SpawnPoints.Count)];
	}

	public static vp_SpawnPoint GetRandomSpawnPoint(string tag)
	{
		m_MatchingSpawnPoints.Clear();
		for (int i = 0; i < SpawnPoints.Count; i++)
		{
			if (m_SpawnPoints[i].tag == tag)
			{
				m_MatchingSpawnPoints.Add(m_SpawnPoints[i]);
			}
		}
		if (m_MatchingSpawnPoints.Count < 1)
		{
			return null;
		}
		if (m_MatchingSpawnPoints.Count == 1)
		{
			return m_MatchingSpawnPoints[0];
		}
		return m_MatchingSpawnPoints[UnityEngine.Random.Range(0, m_MatchingSpawnPoints.Count)];
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
	public virtual void NotifyLevelWasLoaded(Scene scene, LoadSceneMode mode)
	{
		m_SpawnPoints = null;
	}
}
