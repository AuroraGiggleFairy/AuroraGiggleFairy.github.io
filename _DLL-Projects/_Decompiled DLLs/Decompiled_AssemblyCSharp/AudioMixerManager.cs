using System;
using DynamicMusic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerManager : MonoBehaviour
{
	[Serializable]
	public class SnapshotController
	{
		public AudioMixerSnapshot snapshot;

		public float transitionToTime = 1f;
	}

	public SnapshotController underwaterSnapshot;

	public SnapshotController stunnedSnapshot;

	public SnapshotController deafenedSnapshot;

	public SnapshotController defaultSnapshot;

	public bool bCameraWasUnderWater;

	public bool wasStunned;

	public bool wasDeafened;

	public void Update()
	{
		if (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			return;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (!(primaryPlayer != null))
		{
			return;
		}
		bool isUnderwaterCamera = primaryPlayer.IsUnderwaterCamera;
		if (primaryPlayer.isDeafened)
		{
			if (!wasDeafened)
			{
				transitionTo(deafenedSnapshot);
			}
		}
		else if (primaryPlayer.isStunned)
		{
			if (!wasStunned || wasDeafened)
			{
				transitionTo(stunnedSnapshot);
			}
		}
		else if (isUnderwaterCamera)
		{
			if (!bCameraWasUnderWater || wasStunned || wasDeafened)
			{
				transitionTo(underwaterSnapshot);
			}
		}
		else if (wasStunned || wasDeafened || bCameraWasUnderWater)
		{
			transitionTo(defaultSnapshot);
		}
		bCameraWasUnderWater = isUnderwaterCamera;
		wasStunned = primaryPlayer.isStunned;
		wasDeafened = primaryPlayer.isDeafened;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void transitionTo(SnapshotController _snapshot)
	{
		_snapshot.snapshot.TransitionTo(_snapshot.transitionToTime);
		if (GamePrefs.GetBool(EnumGamePrefs.OptionsDynamicMusicEnabled) && !GameManager.Instance.IsEditMode())
		{
			MixerController.Instance.OnSnapshotTransition();
		}
	}
}
