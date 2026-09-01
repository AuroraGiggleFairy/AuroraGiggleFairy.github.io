using Newtonsoft.Json;

namespace SortingNearbyPlus
{
	public class ModSettings
	{
		[JsonProperty("InboxHorizontalRange")]
		public int HorizontalRange { get; set; } = 25;

		[JsonProperty("InboxVerticalRange")]
		public int VerticalRange { get; set; } = 25;

		[JsonProperty("BaseSiphoningProtection")]
		public bool LandClaimClamp { get; set; } = true;

		[JsonProperty("DistributionSuccessNoticeTime")]
		public float SuccessNoticeTime { get; set; } = 2f;

		[JsonProperty("DistributionBlockedNoticeTime")]
		public float BlockedNoticeTime { get; set; } = 3f;
	}
}
