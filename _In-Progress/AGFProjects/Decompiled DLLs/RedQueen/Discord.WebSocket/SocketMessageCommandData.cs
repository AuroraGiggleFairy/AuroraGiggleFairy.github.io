using System;
using System.Collections.Generic;
using System.Linq;
using Discord.API;

namespace Discord.WebSocket;

internal class SocketMessageCommandData : SocketCommandBaseData, IMessageCommandInteractionData, IApplicationCommandInteractionData, IDiscordInteractionData
{
	public SocketMessage Message => ResolvableData?.Messages.FirstOrDefault().Value;

	public override IReadOnlyCollection<IApplicationCommandInteractionDataOption> Options
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	IMessage IMessageCommandInteractionData.Message => Message;

	internal SocketMessageCommandData(DiscordSocketClient client, ApplicationCommandInteractionData model, ulong? guildId)
		: base(client, model, guildId)
	{
	}

	internal new static SocketMessageCommandData Create(DiscordSocketClient client, ApplicationCommandInteractionData model, ulong id, ulong? guildId)
	{
		SocketMessageCommandData socketMessageCommandData = new SocketMessageCommandData(client, model, guildId);
		socketMessageCommandData.Update(model);
		return socketMessageCommandData;
	}
}
