using Discord.API;

namespace Discord.WebSocket;

internal class SocketMessageCommand : SocketCommandBase, IMessageCommandInteraction, IApplicationCommandInteraction, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	public new SocketMessageCommandData Data { get; }

	IMessageCommandInteractionData IMessageCommandInteraction.Data => Data;

	IDiscordInteractionData IDiscordInteraction.Data => Data;

	IApplicationCommandInteractionData IApplicationCommandInteraction.Data => Data;

	internal SocketMessageCommand(DiscordSocketClient client, Interaction model, ISocketMessageChannel channel, SocketUser user)
		: base(client, model, channel, user)
	{
		ApplicationCommandInteractionData model2 = (model.Data.IsSpecified ? ((ApplicationCommandInteractionData)model.Data.Value) : null);
		ulong? guildId = model.GuildId.ToNullable();
		Data = SocketMessageCommandData.Create(client, model2, model.Id, guildId);
	}

	internal new static SocketInteraction Create(DiscordSocketClient client, Interaction model, ISocketMessageChannel channel, SocketUser user)
	{
		SocketMessageCommand socketMessageCommand = new SocketMessageCommand(client, model, channel, user);
		socketMessageCommand.Update(model);
		return socketMessageCommand;
	}
}
