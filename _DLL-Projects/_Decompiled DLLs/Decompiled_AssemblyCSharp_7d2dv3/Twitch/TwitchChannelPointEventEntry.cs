using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Twitch;

public class TwitchChannelPointEventEntry : BaseTwitchEventEntry
{
	[Serializable]
	public class CreateCustomRewards
	{
		public List<CreateCustomReward> data = new List<CreateCustomReward>();
	}

	[Serializable]
	public class CreateCustomReward
	{
		public string broadcaster_id;

		public string title;

		public string background_color = "#F13030";

		public int cost;

		public int max_per_user_per_stream;

		public bool is_max_per_user_per_stream_enabled;

		public int max_per_stream;

		public bool is_max_per_stream_enabled;

		public int global_cooldown_seconds;

		public bool is_global_cooldown_enabled;
	}

	[Serializable]
	public class CreateCustomRewardResponses
	{
		public List<CreateCustomRewardResponse> data;
	}

	[Serializable]
	public class CreateCustomRewardResponse
	{
		public string id;

		public string title;
	}

	[Serializable]
	public class ErrorResponse
	{
		public string status;

		public string message;
	}

	public string ChannelPointTitle = "";

	public int Cost = 1000;

	public int MaxPerUserPerStream;

	public int MaxPerStream;

	public int GlobalCooldown;

	public string ChannelPointID = "";

	public bool AutoCreate = true;

	public override bool IsValid(int amount = -1, string name = "", TwitchSubEventEntry.SubTierTypes subTier = TwitchSubEventEntry.SubTierTypes.Any)
	{
		return ChannelPointTitle == name;
	}

	public CreateCustomReward SetupRewardEntry(string channelID)
	{
		return new CreateCustomReward
		{
			broadcaster_id = channelID,
			title = ChannelPointTitle,
			cost = Cost,
			is_max_per_user_per_stream_enabled = (MaxPerUserPerStream > 0),
			max_per_user_per_stream = MaxPerUserPerStream,
			is_max_per_stream_enabled = (MaxPerStream > 0),
			max_per_stream = MaxPerStream,
			is_global_cooldown_enabled = (GlobalCooldown > 0),
			global_cooldown_seconds = GlobalCooldown
		};
	}

	public static IEnumerator CreateCustomRewardPost(CreateCustomReward _rd, Action<string> _onSucess, Action<string> _onFail)
	{
		yield return new WaitUntil([PublicizedFrom(EAccessModifier.Internal)] () => TwitchManager.Current.Authentication != null && TwitchManager.Current.Authentication.oauth != "" && TwitchManager.Current.Authentication.userID != "");
		Log.Out("creating Custom reward on: " + TwitchManager.Current.Authentication.userID);
		string uri = "https://api.twitch.tv/helix/channel_points/custom_rewards?broadcaster_id=" + TwitchManager.Current.Authentication.userID;
		string bodyData = JsonUtility.ToJson(_rd);
		using UnityWebRequest req = UnityWebRequest.Put(uri, bodyData);
		req.method = "POST";
		req.SetRequestHeader("Authorization", "Bearer " + TwitchManager.Current.Authentication.oauth.Substring(6));
		req.SetRequestHeader("Client-Id", TwitchAuthentication.client_id);
		req.SetRequestHeader("Content-Type", "application/json");
		yield return req.SendWebRequest();
		if (req.result == UnityWebRequest.Result.Success)
		{
			Log.Out("sucessfully created Custom Channel Points Reward");
			_onSucess(req.downloadHandler.text);
			yield break;
		}
		ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(req.downloadHandler.text);
		if (errorResponse != null)
		{
			_onFail("response code: " + errorResponse.status + "\nmessage: " + errorResponse.message);
		}
		else
		{
			_onFail("Something went wrong. Please Try again.");
		}
	}

	public static IEnumerator DeleteCustomRewardsDelete(string id, Action<string> _onSucess, Action<string> _onFail)
	{
		string uri = $"https://api.twitch.tv/helix/channel_points/custom_rewards?broadcaster_id={TwitchManager.Current.Authentication.userID}&id={id}";
		using UnityWebRequest req = UnityWebRequest.Delete(uri);
		req.method = "DELETE";
		req.SetRequestHeader("Authorization", "Bearer " + TwitchManager.Current.Authentication.oauth.Substring(6));
		req.SetRequestHeader("Client-Id", TwitchAuthentication.client_id);
		yield return req.SendWebRequest();
		if (req.result == UnityWebRequest.Result.Success)
		{
			_onSucess("Success");
			yield break;
		}
		Debug.Log($"response code: {req.responseCode}");
		if (req.responseCode == 404)
		{
			_onSucess("Not Found");
			yield break;
		}
		ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(req.downloadHandler.text);
		if (errorResponse != null)
		{
			_onFail(errorResponse.message);
		}
		else
		{
			_onFail("Something went wrong. Please Try again.");
		}
	}
}
