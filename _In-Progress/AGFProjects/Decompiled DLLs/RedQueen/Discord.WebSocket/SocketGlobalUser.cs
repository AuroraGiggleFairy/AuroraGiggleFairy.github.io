using System;
using System.Diagnostics;
using Discord.API;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class SocketGlobalUser : SocketUser
{
	private readonly object _lockObj = new object();

	private ushort _references;

	public override bool IsBot { get; internal set; }

	public override string Username { get; internal set; }

	public override ushort DiscriminatorValue { get; internal set; }

	public override string AvatarId { get; internal set; }

	internal override SocketPresence Presence { get; set; }

	public override bool IsWebhook => false;

	internal override SocketGlobalUser GlobalUser
	{
		get
		{
			return this;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	private string DebuggerDisplay => string.Format("{0}#{1} ({2}{3}, Global)", Username, base.Discriminator, base.Id, IsBot ? ", Bot" : "");

	private SocketGlobalUser(DiscordSocketClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal static SocketGlobalUser Create(DiscordSocketClient discord, ClientState state, User model)
	{
		SocketGlobalUser socketGlobalUser = new SocketGlobalUser(discord, model.Id);
		socketGlobalUser.Update(state, model);
		return socketGlobalUser;
	}

	internal void AddRef()
	{
		checked
		{
			lock (_lockObj)
			{
				_references = (ushort)(unchecked((uint)_references) + 1u);
			}
		}
	}

	internal void RemoveRef(DiscordSocketClient discord)
	{
		lock (_lockObj)
		{
			if (--_references <= 0)
			{
				discord.RemoveUser(base.Id);
			}
		}
	}

	internal new SocketGlobalUser Clone()
	{
		return MemberwiseClone() as SocketGlobalUser;
	}
}
