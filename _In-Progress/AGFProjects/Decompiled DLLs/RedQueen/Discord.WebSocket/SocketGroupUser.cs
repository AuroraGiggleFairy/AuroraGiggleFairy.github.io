using System;
using System.Diagnostics;
using Discord.API;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class SocketGroupUser : SocketUser, IGroupUser, IUser, ISnowflakeEntity, IEntity<ulong>, IMentionable, IPresence, IVoiceState
{
	public SocketGroupChannel Channel { get; }

	internal override SocketGlobalUser GlobalUser { get; set; }

	public override bool IsBot
	{
		get
		{
			return GlobalUser.IsBot;
		}
		internal set
		{
			GlobalUser.IsBot = value;
		}
	}

	public override string Username
	{
		get
		{
			return GlobalUser.Username;
		}
		internal set
		{
			GlobalUser.Username = value;
		}
	}

	public override ushort DiscriminatorValue
	{
		get
		{
			return GlobalUser.DiscriminatorValue;
		}
		internal set
		{
			GlobalUser.DiscriminatorValue = value;
		}
	}

	public override string AvatarId
	{
		get
		{
			return GlobalUser.AvatarId;
		}
		internal set
		{
			GlobalUser.AvatarId = value;
		}
	}

	internal override SocketPresence Presence
	{
		get
		{
			return GlobalUser.Presence;
		}
		set
		{
			GlobalUser.Presence = value;
		}
	}

	public override bool IsWebhook => false;

	private string DebuggerDisplay => string.Format("{0}#{1} ({2}{3}, Group)", Username, base.Discriminator, base.Id, IsBot ? ", Bot" : "");

	bool IVoiceState.IsDeafened => false;

	bool IVoiceState.IsMuted => false;

	bool IVoiceState.IsSelfDeafened => false;

	bool IVoiceState.IsSelfMuted => false;

	bool IVoiceState.IsSuppressed => false;

	IVoiceChannel IVoiceState.VoiceChannel => null;

	string IVoiceState.VoiceSessionId => null;

	bool IVoiceState.IsStreaming => false;

	bool IVoiceState.IsVideoing => false;

	DateTimeOffset? IVoiceState.RequestToSpeakTimestamp => null;

	internal SocketGroupUser(SocketGroupChannel channel, SocketGlobalUser globalUser)
		: base(channel.Discord, globalUser.Id)
	{
		Channel = channel;
		GlobalUser = globalUser;
	}

	internal static SocketGroupUser Create(SocketGroupChannel channel, ClientState state, User model)
	{
		SocketGroupUser socketGroupUser = new SocketGroupUser(channel, channel.Discord.GetOrCreateUser(state, model));
		socketGroupUser.Update(state, model);
		return socketGroupUser;
	}

	internal new SocketGroupUser Clone()
	{
		return MemberwiseClone() as SocketGroupUser;
	}
}
