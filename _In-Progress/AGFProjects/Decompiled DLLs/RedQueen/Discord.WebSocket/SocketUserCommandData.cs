using System;
using System.Collections.Generic;
using System.Linq;
using Discord.API;

namespace Discord.WebSocket;

internal class SocketUserCommandData : SocketCommandBaseData, IUserCommandInteractionData, IApplicationCommandInteractionData, IDiscordInteractionData
{
	public SocketUser Member => (SocketUser)(((object)ResolvableData.GuildMembers.Values.FirstOrDefault()) ?? ((object)ResolvableData.Users.Values.FirstOrDefault()));

	public override IReadOnlyCollection<IApplicationCommandInteractionDataOption> Options
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	IUser IUserCommandInteractionData.User => Member;

	internal SocketUserCommandData(DiscordSocketClient client, ApplicationCommandInteractionData model, ulong? guildId)
		: base(client, model, guildId)
	{
	}

	internal new static SocketUserCommandData Create(DiscordSocketClient client, ApplicationCommandInteractionData model, ulong id, ulong? guildId)
	{
		SocketUserCommandData socketUserCommandData = new SocketUserCommandData(client, model, guildId);
		socketUserCommandData.Update(model);
		return socketUserCommandData;
	}
}
