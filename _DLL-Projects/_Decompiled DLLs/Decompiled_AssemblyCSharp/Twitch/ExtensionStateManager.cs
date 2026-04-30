using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Twitch;

public class ExtensionStateManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string userId;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastUpdate = Time.realtimeSinceStartup;

	[PublicizedFrom(EAccessModifier.Private)]
	public ExtensionConfigManager ecm;

	[PublicizedFrom(EAccessModifier.Private)]
	public ExtensionPubSubManager epm;

	[PublicizedFrom(EAccessModifier.Private)]
	public string jwt = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public long jwtRefreshTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool gettingJWT;

	public void Init()
	{
		userId = TwitchManager.Current.Authentication.userID;
		gettingJWT = true;
		GameManager.Instance.StartCoroutine(GetJWT(TwitchManager.Current.Authentication.oauth.Substring(6)));
		ecm = new ExtensionConfigManager();
		ecm.Init();
		epm = new ExtensionPubSubManager();
	}

	public void OnPartyChanged()
	{
		ecm?.OnPartyChanged();
	}

	public void PushUserBalance((string, int) userBalance)
	{
		epm?.PushUserBalance(userBalance);
	}

	public void PushViewerChatState(string id, bool hasChatted)
	{
		epm?.PushViewerChatState(id, hasChatted);
	}

	public bool CanUseBitCommands()
	{
		return ecm.CanUseBitCommands();
	}

	public void Update()
	{
		if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > jwtRefreshTime && !gettingJWT)
		{
			gettingJWT = true;
			GameManager.Instance.StartCoroutine(GetJWT(TwitchManager.Current.Authentication.oauth.Substring(6)));
		}
		if (jwt != string.Empty && Time.realtimeSinceStartup - lastUpdate >= 1f)
		{
			epm.Update(ecm.UpdatedConfig());
			lastUpdate = Time.realtimeSinceStartup;
		}
	}

	public void RetrieveJWT()
	{
		GameManager.Instance.StartCoroutine(GetJWT(TwitchManager.Current.Authentication.oauth.Substring(6)));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator GetJWT(string token)
	{
		using (UnityWebRequest req = UnityWebRequest.Get("https://2v3d0ewjcg.execute-api.us-east-1.amazonaws.com/prod/jwt/broadcaster"))
		{
			req.SetRequestHeader("Authorization", userId + " " + token);
			yield return req.SendWebRequest();
			if (req.result != UnityWebRequest.Result.Success)
			{
				Log.Warning($"Could not retrieve JWT: {req.result}");
			}
			else
			{
				try
				{
					JObject jObject = JObject.Parse(req.downloadHandler.text);
					if (jObject != null)
					{
						if (jObject.TryGetValue("token", out JToken value))
						{
							jwt = value.ToString();
							epm.SetJWT(jwt);
							Log.Out("received jwt");
						}
						else
						{
							Log.Warning("Could not parse JWT in message body");
						}
						if (jObject.TryGetValue("refreshTime", out JToken value2))
						{
							jwtRefreshTime = long.Parse(value2.ToString());
							Log.Out($"will refresh jwt at {jwtRefreshTime}");
						}
					}
				}
				catch (Exception ex)
				{
					Log.Warning(ex.Message);
				}
			}
		}
		gettingJWT = false;
	}

	public void Cleanup()
	{
		ecm.Cleanup();
		ecm = null;
		epm = null;
	}
}
