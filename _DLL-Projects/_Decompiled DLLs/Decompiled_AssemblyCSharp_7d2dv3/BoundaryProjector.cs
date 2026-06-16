using System;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryProjector : MonoBehaviour
{
	public class ProjectorEntry
	{
		public ProjectorEffectData EffectData;

		public Projector Projector;
	}

	public class ProjectorEffectData
	{
		public bool AutoRotate;

		public float RotationSpeed;

		public bool IsGlowing;

		public float targetRadius = -1f;
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<ProjectorEntry> ProjectorList = new List<ProjectorEntry>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static Vector3 invalidPos = new Vector3(-999f, -999f, -999f);

	public Vector3 targetPos = invalidPos;

	public bool IsInitialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		Projector[] componentsInChildren = base.transform.GetComponentsInChildren<Projector>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			ProjectorList.Add(new ProjectorEntry
			{
				Projector = componentsInChildren[i],
				EffectData = new ProjectorEffectData()
			});
		}
		SetupProjectors();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetupProjectors()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		for (int i = 0; i < ProjectorList.Count; i++)
		{
			if (ProjectorList[i] == null || !ProjectorList[i].Projector.gameObject.activeSelf)
			{
				continue;
			}
			ProjectorEntry projectorEntry = ProjectorList[i];
			if (projectorEntry.EffectData.AutoRotate)
			{
				Vector3 eulerAngles = projectorEntry.Projector.transform.localRotation.eulerAngles;
				projectorEntry.Projector.transform.localRotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y + Time.deltaTime * projectorEntry.EffectData.RotationSpeed, eulerAngles.z);
			}
			if (projectorEntry.EffectData.targetRadius != -1f)
			{
				projectorEntry.Projector.orthographicSize = Mathf.Lerp(projectorEntry.Projector.orthographicSize, projectorEntry.EffectData.targetRadius, Time.deltaTime);
				if (projectorEntry.Projector.orthographicSize == projectorEntry.EffectData.targetRadius)
				{
					projectorEntry.EffectData.targetRadius = -1f;
				}
			}
			if (projectorEntry.EffectData.IsGlowing)
			{
				Color color = projectorEntry.Projector.material.color;
				float num = Mathf.PingPong(Time.time, 0.25f);
				projectorEntry.Projector.material.color = new Color(color.r, color.g, color.b, 0.5f + num * 2f);
			}
		}
		if (targetPos != invalidPos)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, targetPos, Time.deltaTime);
			if (base.transform.position == targetPos)
			{
				targetPos = invalidPos;
			}
		}
	}

	public void SetRadius(int projectorID, float size)
	{
		if (projectorID < ProjectorList.Count && ProjectorList[projectorID] != null)
		{
			if (ProjectorList[projectorID].Projector.orthographicSize == -1f || size == 0f)
			{
				ProjectorList[projectorID].Projector.orthographicSize = size;
				ProjectorList[projectorID].EffectData.targetRadius = -1f;
			}
			else
			{
				ProjectorList[projectorID].EffectData.targetRadius = size;
			}
		}
	}

	public void SetAlpha(int projectorID, float alpha)
	{
		if (projectorID < ProjectorList.Count && ProjectorList[projectorID] != null)
		{
			Color color = ProjectorList[projectorID].Projector.material.color;
			ProjectorList[projectorID].Projector.material.color = new Color(color.r, color.g, color.b, alpha);
		}
	}

	public void SetGlow(int projectorID, bool isGlowing)
	{
		if (projectorID < ProjectorList.Count && ProjectorList[projectorID] != null)
		{
			_ = ProjectorList[projectorID].Projector.material.color;
			ProjectorList[projectorID].EffectData.IsGlowing = isGlowing;
		}
	}

	public void SetAutoRotate(int projectorID, bool autoRotate, float rotateSpeed)
	{
		if (projectorID < ProjectorList.Count && ProjectorList[projectorID] != null)
		{
			ProjectorList[projectorID].EffectData.AutoRotate = autoRotate;
			ProjectorList[projectorID].EffectData.RotationSpeed = rotateSpeed;
		}
	}

	public void SetMoveToPosition(Vector3 vNew)
	{
		if (base.transform.position.y == -999f)
		{
			base.transform.position = vNew;
		}
		else
		{
			targetPos = vNew;
		}
	}
}
