using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Steamworks;

namespace Platform.Steam;

public class SteamQueryPortReader
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class RulesRequest
	{
		public uint Ip;

		public ushort Port;

		public GameServerInfo GameInfo;

		public GameServerInfo GameInfoClone;

		public bool DataErrors;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ISteamMatchmakingRulesResponse matchmakingRulesResponse;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Queue<RulesRequest> rulesRequests = new Queue<RulesRequest>();

	[PublicizedFrom(EAccessModifier.Private)]
	public RulesRequest currentRulesRequest;

	[PublicizedFrom(EAccessModifier.Private)]
	public HServerQuery rulesRequestHandle = HServerQuery.Invalid;

	[method: PublicizedFrom(EAccessModifier.Private)]
	public event GameServerDetailsCallback GameServerDetailsEvent;

	public void Init(IPlatform _owner)
	{
		if (!GameManager.IsDedicatedServer && matchmakingRulesResponse == null)
		{
			matchmakingRulesResponse = new ISteamMatchmakingRulesResponse(RulesResponded, RulesFailedToRespond, RulesRefreshComplete);
		}
	}

	public void Disconnect()
	{
		if (rulesRequestHandle != HServerQuery.Invalid)
		{
			SteamMatchmakingServers.CancelServerQuery(rulesRequestHandle);
			rulesRequestHandle = HServerQuery.Invalid;
		}
		this.GameServerDetailsEvent = null;
	}

	public void RegisterGameServerCallbacks(GameServerDetailsCallback _details)
	{
		this.GameServerDetailsEvent = _details;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RunGameServerDetailsEvent(GameServerInfo _info, bool _success)
	{
		this.GameServerDetailsEvent?.Invoke(_info, _success);
	}

	public void GetGameServerInfo(GameServerInfo _gameInfo)
	{
		if (_gameInfo.IsLobby)
		{
			RunGameServerDetailsEvent(_gameInfo, _success: true);
			return;
		}
		if (_gameInfo.IsNoResponse)
		{
			RunGameServerDetailsEvent(_gameInfo, _success: true);
		}
		string text = _gameInfo.GetValue(GameInfoString.IP);
		if (!long.TryParse(text.Replace(".", ""), out var _))
		{
			try
			{
				IPHostEntry hostEntry = Dns.GetHostEntry(text);
				if (hostEntry.AddressList.Length == 0)
				{
					Log.Out("Steamworks.NET] No valid IP for server found");
					RunGameServerDetailsEvent(_gameInfo, _success: false);
					return;
				}
				text = hostEntry.AddressList[0].ToString();
			}
			catch (SocketException ex)
			{
				Log.Out("Steamworks.NET] No such hostname: \"" + text + "\": " + ex);
				RunGameServerDetailsEvent(_gameInfo, _success: false);
				return;
			}
		}
		RulesRequest item = new RulesRequest
		{
			GameInfo = _gameInfo,
			Ip = NetworkUtils.ToInt(text),
			Port = (ushort)_gameInfo.GetValue(GameInfoInt.Port)
		};
		rulesRequests.Enqueue(item);
		if (rulesRequestHandle == HServerQuery.Invalid)
		{
			StartNextRulesRequest();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartNextRulesRequest()
	{
		currentRulesRequest = null;
		rulesRequestHandle = HServerQuery.Invalid;
		if (rulesRequests.Count > 0)
		{
			currentRulesRequest = rulesRequests.Dequeue();
			currentRulesRequest.GameInfoClone = new GameServerInfo(currentRulesRequest.GameInfo);
			rulesRequestHandle = SteamMatchmakingServers.ServerRules(currentRulesRequest.Ip, currentRulesRequest.Port, matchmakingRulesResponse);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RulesFailedToRespond()
	{
		RunGameServerDetailsEvent(currentRulesRequest.GameInfo, _success: false);
		StartNextRulesRequest();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RulesRefreshComplete()
	{
		if (!currentRulesRequest.DataErrors && currentRulesRequest.GameInfoClone.GetValue(GameInfoString.GameName).Length > 0)
		{
			currentRulesRequest.GameInfo.Merge(currentRulesRequest.GameInfoClone, currentRulesRequest.GameInfo.IsLAN ? EServerRelationType.LAN : EServerRelationType.Internet);
			RunGameServerDetailsEvent(currentRulesRequest.GameInfo, _success: true);
		}
		else
		{
			if (currentRulesRequest.DataErrors)
			{
				currentRulesRequest.GameInfo.SetValue(GameInfoString.ServerDescription, Localization.Get("xuiServerBrowserFailedRetrievingData"));
			}
			RunGameServerDetailsEvent(currentRulesRequest.GameInfo, _success: false);
		}
		StartNextRulesRequest();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RulesResponded(string _rule, string _value)
	{
		RulesRequest rulesRequest = currentRulesRequest;
		if (!rulesRequest.DataErrors && !_rule.EqualsCaseInsensitive("gameinfo") && !_rule.EqualsCaseInsensitive("ping") && (!rulesRequest.GameInfoClone.IsLAN || !_rule.EqualsCaseInsensitive("ip")) && !rulesRequest.GameInfoClone.ParseAny(_rule, _value))
		{
			rulesRequest.DataErrors = true;
		}
	}
}
