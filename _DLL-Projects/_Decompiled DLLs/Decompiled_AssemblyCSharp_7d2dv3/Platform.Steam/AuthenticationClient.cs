using System;
using Steamworks;

namespace Platform.Steam;

public class AuthenticationClient : IAuthenticationClient
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public HAuthTicket ticketHandle = HAuthTicket.Invalid;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool registeredDisconnectEvent;

	public void Init(IPlatform _owner)
	{
		owner = _owner;
	}

	public string GetAuthTicket()
	{
		if (!registeredDisconnectEvent)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.OnDisconnectFromServer += OnDisconnectFromServer;
			registeredDisconnectEvent = true;
		}
		byte[] array = new byte[1024];
		Log.Out("[Steamworks.NET] Auth.GetAuthTicket()");
		if (ticketHandle != HAuthTicket.Invalid)
		{
			SteamUser.CancelAuthTicket(ticketHandle);
			ticketHandle = HAuthTicket.Invalid;
		}
		SteamNetworkingIdentity pSteamNetworkingIdentity = new SteamNetworkingIdentity
		{
			m_eType = ESteamNetworkingIdentityType.k_ESteamNetworkingIdentityType_Invalid
		};
		ticketHandle = SteamUser.GetAuthSessionTicket(array, array.Length, out var _, ref pSteamNetworkingIdentity);
		return Convert.ToBase64String(array);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisconnectFromServer()
	{
		if (ticketHandle != HAuthTicket.Invalid)
		{
			SteamUser.CancelAuthTicket(ticketHandle);
			ticketHandle = HAuthTicket.Invalid;
		}
	}

	public void AuthenticateServer(ClientAuthenticateServerContext _context)
	{
		_context.Success();
	}

	public void Destroy()
	{
		OnDisconnectFromServer();
	}
}
