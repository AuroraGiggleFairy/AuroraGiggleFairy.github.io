using System;
using Audio;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
	public string soundName;

	public float duration = -1f;

	public bool playOnDemand;

	public float startDelay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Entity attachedEntity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public RootTransformRefEntity refEntity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool queuedForPlaying;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPlaying;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float startTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 playPos;

	public void Play()
	{
		if (!refEntity)
		{
			refEntity = RootTransformRefEntity.AddIfEntity(base.transform);
			if ((bool)refEntity)
			{
				attachedEntity = refEntity.RootTransform.GetComponent<Entity>();
			}
		}
		if ((bool)attachedEntity)
		{
			Manager.Play(attachedEntity, soundName);
			isPlaying = true;
			queuedForPlaying = false;
		}
		else if ((bool)refEntity)
		{
			Vector3 position = refEntity.transform.position;
			if (position == Vector3.zero)
			{
				queuedForPlaying = true;
			}
			else
			{
				PlayAtPos(position);
			}
		}
		else
		{
			Vector3 position2 = base.transform.position;
			if (position2 == Vector3.zero)
			{
				queuedForPlaying = true;
			}
			else
			{
				PlayAtPos(position2);
			}
		}
		if (isPlaying && duration > 0f)
		{
			startTime = Time.time;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayAtPos(Vector3 _pos)
	{
		playPos = _pos + Origin.position;
		Manager.Play(playPos, soundName);
		isPlaying = true;
		queuedForPlaying = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (startDelay > 0f)
		{
			startDelay -= Time.deltaTime;
			return;
		}
		if (queuedForPlaying)
		{
			Play();
		}
		if (isPlaying && duration > 0f && Time.time > startTime + duration)
		{
			StopAudio();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StopAudio()
	{
		if (isPlaying)
		{
			if ((bool)attachedEntity)
			{
				Manager.Stop(attachedEntity.entityId, soundName);
			}
			else
			{
				Manager.Stop(playPos, soundName);
			}
			isPlaying = false;
		}
	}

	public void OnEnable()
	{
		if (!playOnDemand)
		{
			queuedForPlaying = true;
		}
	}

	public void OnDisable()
	{
		StopAudio();
	}

	public void OnDestroy()
	{
		StopAudio();
	}
}
