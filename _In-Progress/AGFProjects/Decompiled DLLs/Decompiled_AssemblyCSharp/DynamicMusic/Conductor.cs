using System.Collections;
using System.Collections.Generic;
using DynamicMusic.Factories;
using MusicUtils.Enums;
using UniLinq;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public class Conductor : IUpdatable, ICleanable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ISectionSelector sectionSelector;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumDictionary<SectionType, ISection> sections;

	[PublicizedFrom(EAccessModifier.Private)]
	public ISection CurrentSection;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	public Dictionary<int, bool> PlayerEligibleForBloodmoonCache;

	public bool IsBloodmoonMusicEligible;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasMusicPlaying;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public SectionType CurrentSectionType
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsMusicPlaying
	{
		get
		{
			if (sections != null)
			{
				foreach (ISection value in sections.Values)
				{
					if (value.IsPlaying)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public Conductor()
	{
		if (!GameManager.IsDedicatedServer)
		{
			MixerController.Instance.Init();
		}
	}

	public void Init(bool ReadyImmediate = false)
	{
		world = GameManager.Instance.World;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PlayerEligibleForBloodmoonCache == null)
		{
			PlayerEligibleForBloodmoonCache = new Dictionary<int, bool>();
			ConnectionManager.OnClientDisconnected += OnClientDisconnected;
			Log.Out("Dynamic Music Initialized on Server");
		}
		if (GameManager.IsDedicatedServer)
		{
			Log.Out("Dynamic Music Initialized on Dedi");
			return;
		}
		LayeredContent.ReadyQueuesImmediate();
		sections = new EnumDictionary<SectionType, ISection>
		{
			{
				SectionType.Exploration,
				Factory.CreateSection<Adventure>(SectionType.Exploration)
			},
			{
				SectionType.Suspense,
				Factory.CreateSection<Adventure>(SectionType.Suspense)
			},
			{
				SectionType.Combat,
				Factory.CreateSection<Combat>(SectionType.Combat)
			},
			{
				SectionType.Bloodmoon,
				Factory.CreateSection<Bloodmoon>(SectionType.Bloodmoon)
			},
			{
				SectionType.HomeDay,
				Factory.CreateSection<Song>(SectionType.HomeDay)
			},
			{
				SectionType.HomeNight,
				Factory.CreateSection<Song>(SectionType.HomeNight)
			},
			{
				SectionType.TraderBob,
				Factory.CreateSection<Song>(SectionType.TraderBob)
			},
			{
				SectionType.TraderHugh,
				Factory.CreateSection<Song>(SectionType.TraderHugh)
			},
			{
				SectionType.TraderJen,
				Factory.CreateSection<Song>(SectionType.TraderJen)
			},
			{
				SectionType.TraderJoel,
				Factory.CreateSection<Song>(SectionType.TraderJoel)
			},
			{
				SectionType.TraderRekt,
				Factory.CreateSection<Song>(SectionType.TraderRekt)
			}
		};
		sectionSelector = Factory.CreateSectionSelector();
		wasMusicPlaying = false;
		Log.Out("Dynamic Music Initialized on Client");
	}

	public IEnumerator PreloadRoutine()
	{
		Log.Out("Begin DMS Conductor Preload Routine");
		foreach (KeyValuePair<SectionType, ISection> section2 in sections)
		{
			if (section2.Value is Section section)
			{
				yield return section.PreloadRoutine();
			}
		}
	}

	public void Update()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			lock (PlayerEligibleForBloodmoonCache)
			{
				foreach (EntityPlayer item in world.Players.list)
				{
					bool value2;
					if (item.bloodMoonParty != null && item.bloodMoonParty.partySpawner != null)
					{
						bool flag = item.bloodMoonParty.partySpawner.partyMembers.Max([PublicizedFrom(EAccessModifier.Internal)] (EntityPlayer p) => p.Progression.GetLevel()) > 1 && (!item.bloodMoonParty.partySpawner.IsDone || item.bloodMoonParty.BloodmoonZombiesRemain);
						if (item is EntityPlayerLocal)
						{
							IsBloodmoonMusicEligible = flag;
							continue;
						}
						if (!PlayerEligibleForBloodmoonCache.TryGetValue(item.entityId, out var value))
						{
							PlayerEligibleForBloodmoonCache.Add(item.entityId, flag);
						}
						if (value != flag)
						{
							PlayerEligibleForBloodmoonCache[item.entityId] = flag;
							NetPackageBloodmoonMusic package = NetPackageManager.GetPackage<NetPackageBloodmoonMusic>().Setup(flag);
							SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, item.entityId);
						}
					}
					else if (item is EntityPlayerLocal)
					{
						IsBloodmoonMusicEligible = false;
					}
					else if (PlayerEligibleForBloodmoonCache.TryGetValue(item.entityId, out value2) && value2)
					{
						PlayerEligibleForBloodmoonCache[item.entityId] = false;
						NetPackageBloodmoonMusic package2 = NetPackageManager.GetPackage<NetPackageBloodmoonMusic>().Setup(_isBloodmoonMusicEligible: false);
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package2, _onlyClientsAttachedToAnEntity: false, item.entityId);
					}
					else if (!PlayerEligibleForBloodmoonCache.ContainsKey(item.entityId))
					{
						PlayerEligibleForBloodmoonCache.Add(item.entityId, value: false);
						NetPackageBloodmoonMusic package3 = NetPackageManager.GetPackage<NetPackageBloodmoonMusic>().Setup(_isBloodmoonMusicEligible: false);
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package3, _onlyClientsAttachedToAnEntity: false, item.entityId);
					}
				}
			}
		}
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		if (wasMusicPlaying & !(wasMusicPlaying = IsMusicPlaying))
		{
			sectionSelector.Notify(MusicActionType.Stop);
			Log.Out("Notified SectionSelector that music stopped");
		}
		SectionType sectionType = sectionSelector.Select();
		if (CurrentSectionType != sectionType)
		{
			Log.Out($"SectionType change from {CurrentSectionType} to {sectionType}");
			if (CurrentSectionType == SectionType.None)
			{
				if (sections.TryGetValue(sectionType, out var value3))
				{
					value3.FadeIn();
					CurrentSection = value3;
					sectionSelector.Notify(MusicActionType.Play);
					Log.Out("Notified SectionSelector that music played");
				}
			}
			else
			{
				if (CurrentSection != null && CurrentSection.IsPlaying && !CurrentSection.IsPaused)
				{
					CurrentSection.FadeOut();
				}
				if (sections.TryGetValue(sectionType, out var value4))
				{
					value4.FadeIn();
					sectionSelector.Notify(MusicActionType.FadeIn);
					CurrentSection = value4;
				}
				else
				{
					CurrentSection = null;
				}
			}
		}
		CurrentSectionType = sectionType;
		MixerController.Instance.Update();
	}

	public void OnPauseGame()
	{
		if (!GameManager.IsDedicatedServer)
		{
			if (CurrentSection != null)
			{
				CurrentSection.Pause();
			}
			if (sectionSelector != null)
			{
				sectionSelector.Notify(MusicActionType.Pause);
			}
		}
	}

	public void OnUnPauseGame()
	{
		if (!GameManager.IsDedicatedServer)
		{
			if (CurrentSection != null)
			{
				CurrentSection.UnPause();
			}
			if (sectionSelector != null)
			{
				sectionSelector.Notify(MusicActionType.UnPause);
			}
		}
	}

	public void CleanUp()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		sectionSelector = null;
		if (sections != null)
		{
			foreach (ISection value in sections.Values)
			{
				value.CleanUp();
			}
			sections.Clear();
		}
		sections = null;
		CurrentSection = null;
		CurrentSectionType = SectionType.None;
		LayeredContent.ClearQueues();
	}

	public void OnWorldExit()
	{
		ConnectionManager.OnClientDisconnected -= OnClientDisconnected;
		if (PlayerEligibleForBloodmoonCache != null)
		{
			lock (PlayerEligibleForBloodmoonCache)
			{
				PlayerEligibleForBloodmoonCache.Clear();
				PlayerEligibleForBloodmoonCache = null;
			}
		}
	}

	public override string ToString()
	{
		return "Conductor:\n" + $"Current Section Type: {CurrentSectionType}\n";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnClientDisconnected(ClientInfo _ci)
	{
		if (PlayerEligibleForBloodmoonCache == null)
		{
			return;
		}
		lock (PlayerEligibleForBloodmoonCache)
		{
			if (!PlayerEligibleForBloodmoonCache.Remove(_ci.entityId))
			{
				Log.Warning($"DynamicMusic: {_ci.entityId} was not in Bloodmoon state cache on disconnect");
			}
			else
			{
				Log.Out($"DynamicMusic: {_ci.entityId} successfully removed from Bloodmoon state cache");
			}
		}
	}
}
