using System.ComponentModel;
using BhvrAnalyticsServices.Interfaces;
using Newtonsoft.Json;
using Platform;

namespace Services.Analytics.Events;

[JsonObject(MemberSerialization.OptIn)]
public abstract class BaseEventData : IAnalyticsEventData
{
	public abstract string EventType { get; }

	[JsonProperty(PropertyName = "provider_user_id")]
	[Description("The unique player identifier based on the provider_id (anonymized)")]
	public string ProviderUserId => PlatformManager.NativePlatform?.User?.PlatformUserId?.ReadablePlatformUserIdentifier;

	[JsonProperty(PropertyName = "session_build")]
	[Description("The name of the build used for the game session initiated (patch/CL number)")]
	public string SessionBuild
	{
		get
		{
			int build = Constants.cVersionInformation.Build;
			return build.ToString();
		}
	}

	[JsonProperty(PropertyName = "game_version")]
	[Description("The name of the game version (Major.Minor.Hotfix)")]
	public string GameVersion => Constants.cVersionInformation.ShortString;

	[JsonProperty(PropertyName = "easy_anti_cheat_enabled")]
	[Description("TRUE if the player has EAC activated")]
	public bool EasyAntiCheatEnabled => GamePrefs.GetBool(EnumGamePrefs.EACEnabled);

	[PublicizedFrom(EAccessModifier.Protected)]
	public BaseEventData()
	{
	}
}
