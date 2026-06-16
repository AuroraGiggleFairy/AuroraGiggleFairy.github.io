using System;
using mumblelib;
using UnityEngine;

public class MumblePositionalAudio : SingletonMonoBehaviour<MumblePositionalAudio>
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float updateInterval = 0.02f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ILinkFile mumbleLink;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool initErrorLoggedOnce;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool contextCleared;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastUpdateTime;

	public static void Init()
	{
		if (Application.isPlaying && !(SingletonMonoBehaviour<MumblePositionalAudio>.Instance != null))
		{
			new GameObject("MumbleLink").AddComponent<MumblePositionalAudio>().IsPersistant = true;
		}
	}

	public static void Destroy()
	{
		if (!(SingletonMonoBehaviour<MumblePositionalAudio>.Instance == null))
		{
			UnityEngine.Object.Destroy(SingletonMonoBehaviour<MumblePositionalAudio>.Instance.gameObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setCommonValues()
	{
		if (mumbleLink != null && !(player == null))
		{
			mumbleLink.Name = "7 Days To Die";
			mumbleLink.Description = "7 Days To Die Positional Audio";
			mumbleLink.UIVersion = 2u;
			string text = player.entityId.ToString();
			Log.Out("[Mumble] Setting Mumble ID to " + text);
			mumbleLink.Identity = text;
			string text2 = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient ? GamePrefs.GetString(EnumGamePrefs.GameGuidClient) : player.world.Guid);
			Log.Out("[Mumble] Setting context to " + text2);
			mumbleLink.Context = text2;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initShm()
	{
		mumbleLink = LinkFileManager.Open();
		setCommonValues();
		Log.Out("[Mumble] Shared Memory initialized");
	}

	public void ReinitShm()
	{
		if (mumbleLink == null)
		{
			initShm();
		}
		else
		{
			setCommonValues();
		}
	}

	public void SetPlayer(EntityPlayerLocal player)
	{
		this.player = player;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (player == null)
		{
			if (mumbleLink != null)
			{
				if (!contextCleared)
				{
					contextCleared = true;
					mumbleLink.Context = "";
					mumbleLink.Tick();
				}
				else
				{
					mumbleLink.Dispose();
					mumbleLink = null;
				}
			}
			return;
		}
		if (mumbleLink == null)
		{
			try
			{
				initShm();
				initErrorLoggedOnce = false;
				contextCleared = false;
			}
			catch (Exception e)
			{
				if (!initErrorLoggedOnce)
				{
					initErrorLoggedOnce = true;
					Log.Error("[Mumble] Error initializing Mumble link:");
					Log.Exception(e);
				}
				return;
			}
		}
		float unscaledTime = Time.unscaledTime;
		if (!(unscaledTime - lastUpdateTime < 0.02f))
		{
			lastUpdateTime = unscaledTime;
			if (mumbleLink.UIVersion == 0)
			{
				Log.Warning("[Mumble] Mumble disconnected, reinit");
				ReinitShm();
			}
			ILinkFile linkFile = mumbleLink;
			Vector3 avatarPosition = (mumbleLink.CameraPosition = player.position);
			linkFile.AvatarPosition = avatarPosition;
			ILinkFile linkFile2 = mumbleLink;
			avatarPosition = (mumbleLink.CameraForward = player.cameraTransform.forward);
			linkFile2.AvatarForward = avatarPosition;
			mumbleLink.Tick();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void singletonDestroy()
	{
		if (mumbleLink != null)
		{
			if (!contextCleared)
			{
				contextCleared = true;
				mumbleLink.Context = "";
				mumbleLink.Tick();
			}
			mumbleLink.Dispose();
			mumbleLink = null;
			Log.Out("[Mumble] Shared Memory disposed");
		}
		Log.Out("[Mumble] Link destroyed");
	}

	public void printUiVersion()
	{
		if (mumbleLink == null)
		{
			Log.Out("[Mumble] MumbleLink == null!");
		}
		else
		{
			Log.Out($"[Mumble] UiVersion = {mumbleLink.UIVersion}");
		}
	}
}
