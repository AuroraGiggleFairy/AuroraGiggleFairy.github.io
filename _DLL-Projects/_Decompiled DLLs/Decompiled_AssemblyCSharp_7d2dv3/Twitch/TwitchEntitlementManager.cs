using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Platform;
using UnityEngine.Networking;

namespace Twitch;

public class TwitchEntitlementManager : IEntitlementValidator
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string FETCH_URL = "https://xjjvn6hovg33dqetux65clszte0vhysi.lambda-url.us-east-2.on.aws/?platform_id=";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string FULFILLMENT_URL = "https://ev2dltb7u2pdtwuaphayq5icdy0nmtsx.lambda-url.us-east-2.on.aws/?platform_id=";

	[PublicizedFrom(EAccessModifier.Private)]
	public static EnumDictionary<EntitlementSetEnum, string> EntitlementSetToTwitchMap = new EnumDictionary<EntitlementSetEnum, string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static EnumDictionary<EPlatformIdentifier, string> platformIdentifierToPrefix = new EnumDictionary<EPlatformIdentifier, string>
	{
		{
			EPlatformIdentifier.PSN,
			"p"
		},
		{
			EPlatformIdentifier.XBL,
			"x"
		},
		{
			EPlatformIdentifier.Steam,
			"s"
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Entitlement> entitlements = new List<Entitlement>();

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public string prefix;

	public string PlatformID
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return prefix + owner.User.PlatformUserId.ReadablePlatformUserIdentifier;
		}
	}

	public void Init(IPlatform _owner)
	{
		owner = _owner;
		if (!platformIdentifierToPrefix.TryGetValue(_owner.PlatformIdentifier, out prefix))
		{
			Log.Warning("could not get platform prefix in Twitch Entitlements Manager");
		}
	}

	public void Init()
	{
		TwitchDropAvailabilityManager.Instance.Updated -= OnDropsUpdated;
		TwitchDropAvailabilityManager.Instance.Updated += OnDropsUpdated;
		TwitchDropAvailabilityManager.Instance.RegisterSource("rfs://drops.xml");
		TwitchDropAvailabilityManager.Instance.UpdateAll(force: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDropsUpdated(TwitchDropAvailabilityManager mgr)
	{
		List<TwitchDropAvailabilityManager.TwitchDropEntry> list = new List<TwitchDropAvailabilityManager.TwitchDropEntry>();
		mgr.GetEntries(new List<string> { "rfs://drops.xml" }, list);
		EntitlementSetToTwitchMap.Clear();
		foreach (TwitchDropAvailabilityManager.TwitchDropEntry item in list)
		{
			if (item.IsAvailable(DateTime.Now))
			{
				EntitlementSetToTwitchMap.Add(item.EntitlementSet, item.BenefitId);
			}
		}
		FetchEntitlements();
	}

	public void FetchEntitlements()
	{
		ThreadManager.StartCoroutine(FetchEntitlementsCo());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator FetchEntitlementsCo()
	{
		UnityWebRequest request = UnityWebRequest.Get("https://xjjvn6hovg33dqetux65clszte0vhysi.lambda-url.us-east-2.on.aws/?platform_id=" + PlatformID);
		Log.Out("fetching Twitch entitlements for " + PlatformID);
		yield return request.SendWebRequest();
		if (request.result != UnityWebRequest.Result.Success)
		{
			Log.Warning("Failed to fetch Twitch entitlements: " + request.error);
			yield break;
		}
		JArray obj = (JArray)JObject.Parse(request.downloadHandler.text)["data"];
		entitlements.Clear();
		foreach (JToken item in obj)
		{
			string id = item.Value<string>("id");
			string benefit_id = item.Value<string>("benefit_id");
			string fulfillment_status = item.Value<string>("fulfillment_status");
			entitlements.Add(new Entitlement
			{
				id = id,
				benefit_id = benefit_id,
				fulfillment_status = fulfillment_status
			});
		}
		yield return FulfillEntitlementsCo(null);
		Log.Out("Successfully fetched Twitch entitlements");
	}

	public void FulfillEntitlements(Action onSuccess = null)
	{
		GameManager.Instance.StartCoroutine(FulfillEntitlementsCo(onSuccess));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator FulfillEntitlementsCo(Action onSuccess)
	{
		List<string> list = new List<string>();
		foreach (Entitlement entitlement2 in entitlements)
		{
			if (entitlement2.fulfillment_status == "CLAIMED")
			{
				list.Add(entitlement2.id);
			}
		}
		if (list.Count == 0)
		{
			yield break;
		}
		string s = JsonConvert.SerializeObject(new FulfillmentPayload
		{
			platform_id = PlatformID,
			entitlement_ids = list
		});
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		UnityWebRequest request = new UnityWebRequest("https://ev2dltb7u2pdtwuaphayq5icdy0nmtsx.lambda-url.us-east-2.on.aws/?platform_id=" + PlatformID, "POST")
		{
			uploadHandler = new UploadHandlerRaw(bytes),
			downloadHandler = new DownloadHandlerBuffer()
		};
		request.SetRequestHeader("Content-Type", "application/json");
		Log.Out("fulfilling Twitch entitlements for " + PlatformID);
		yield return request.SendWebRequest();
		if (request.result != UnityWebRequest.Result.Success)
		{
			Log.Warning("Failed to fulfill twitch entitlements: " + request.downloadHandler.text);
			yield break;
		}
		foreach (JToken item in (JArray)JObject.Parse(request.downloadHandler.text)["data"])
		{
			string id = item.Value<string>("id");
			string benefit_id = item.Value<string>("benefit_id");
			string fulfillment_status = item.Value<string>("fulfillment_status");
			Entitlement entitlement = entitlements.Find([PublicizedFrom(EAccessModifier.Internal)] (Entitlement e) => e.id == id);
			if (entitlement != null)
			{
				entitlement.fulfillment_status = fulfillment_status;
				entitlement.benefit_id = benefit_id;
				continue;
			}
			entitlements.Add(new Entitlement
			{
				id = id,
				benefit_id = benefit_id,
				fulfillment_status = fulfillment_status
			});
		}
		onSuccess?.Invoke();
	}

	public void SerializeEntitlements()
	{
		JsonConvert.SerializeObject(new EntitlementListWrapper
		{
			entitlements = entitlements
		}, Formatting.Indented);
	}

	public bool IsAvailableOnPlatform(EntitlementSetEnum _set)
	{
		return EntitlementSetToTwitchMap.ContainsKey(_set);
	}

	public bool HasEntitlement(EntitlementSetEnum _set)
	{
		if (EntitlementSetToTwitchMap.TryGetValue(_set, out var id))
		{
			return entitlements.Exists([PublicizedFrom(EAccessModifier.Internal)] (Entitlement e) => e.benefit_id == id);
		}
		return false;
	}

	public bool IsEntitlementPurchasable(EntitlementSetEnum _set)
	{
		return false;
	}

	public bool OpenStore(EntitlementSetEnum _set, Action<EntitlementSetEnum> _onDlcPurchased)
	{
		return false;
	}
}
