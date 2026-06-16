using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace Twitch;

public class ExtensionManager
{
	public const string API_STAGE = "prod";

	public const string EXTENSION_ID = "k6ji189bf7i4ge8il4iczzw7kpgmjt";

	public static string Version = "2.0.2";

	[PublicizedFrom(EAccessModifier.Private)]
	public ExtensionStateManager extensionStateManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public ExtensionCommandPoller extensionCommandPoller;

	public void Init()
	{
		extensionStateManager = new ExtensionStateManager();
		extensionCommandPoller = new ExtensionCommandPoller();
		extensionStateManager.Init();
		extensionCommandPoller.Init();
	}

	public void OnPartyChanged()
	{
		extensionStateManager?.OnPartyChanged();
	}

	public void TwitchEnabledChanged(EntityPlayer _ep)
	{
		EntityPlayerLocal localPlayer = TwitchManager.Current.LocalPlayer;
		if (_ep != localPlayer && localPlayer.Party != null && localPlayer.Party.ContainsMember(_ep))
		{
			extensionStateManager.OnPartyChanged();
		}
	}

	public void PushUserBalance((string, int) userBalance)
	{
		extensionStateManager?.PushUserBalance(userBalance);
	}

	public void PushViewerChatState(string id, bool hasChatted)
	{
		extensionStateManager?.PushViewerChatState(id, hasChatted);
	}

	public bool CanUseBitCommands()
	{
		return extensionStateManager.CanUseBitCommands();
	}

	public void Update()
	{
		extensionStateManager.Update();
		extensionCommandPoller.Update();
	}

	public bool HasCommand()
	{
		return extensionCommandPoller.HasCommand();
	}

	public ExtensionAction GetCommand()
	{
		return extensionCommandPoller.GetCommand();
	}

	public void RetrieveJWT()
	{
		extensionStateManager.RetrieveJWT();
	}

	public void Cleanup()
	{
		extensionCommandPoller.Cleanup();
		extensionStateManager.Cleanup();
		extensionCommandPoller = null;
		extensionStateManager = null;
	}

	public static void CheckExtensionInstalled(Action<bool> _cb)
	{
		GameManager.Instance.StartCoroutine(CheckExtensionInstall(_cb));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator CheckExtensionInstall(Action<bool> _cb)
	{
		using (UnityWebRequest req = UnityWebRequest.Get("https://api.twitch.tv/helix/users/extensions?user_id=" + TwitchManager.Current.Authentication.userID))
		{
			req.SetRequestHeader("Authorization", "Bearer " + TwitchManager.Current.Authentication.oauth.Substring(6));
			req.SetRequestHeader("Client-Id", TwitchAuthentication.client_id);
			yield return req.SendWebRequest();
			if (req.result != UnityWebRequest.Result.Success)
			{
				Log.Warning("InBeta Check Failed: " + req.downloadHandler.text);
			}
			else
			{
				try
				{
					JObject jObject = JObject.Parse(req.downloadHandler.text);
					foreach (JToken item in jObject["data"]["panel"].ToObject<JObject>().Values())
					{
						JObject jObject2 = item.ToObject<JObject>();
						if (jObject2.TryGetValue("id", out JToken value) && value.ToString() == "k6ji189bf7i4ge8il4iczzw7kpgmjt" && jObject2["active"].ToString() == bool.TrueString)
						{
							_cb(obj: true);
							yield break;
						}
					}
					foreach (JToken item2 in jObject["data"]["overlay"].ToObject<JObject>().Values())
					{
						JObject jObject3 = item2.ToObject<JObject>();
						if (jObject3.TryGetValue("version", out JToken value2))
						{
							Version = value2.ToString();
						}
						if (jObject3.TryGetValue("id", out JToken value3) && value3.ToString() == "k6ji189bf7i4ge8il4iczzw7kpgmjt" && jObject3["active"].ToString() == bool.TrueString)
						{
							_cb(obj: true);
							yield break;
						}
					}
				}
				catch (Exception)
				{
					Log.Warning("could not read extension check data");
				}
			}
		}
		_cb(obj: false);
	}
}
