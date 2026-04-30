using Discord.API;

namespace Discord.WebSocket;

internal class SocketUserCommand : SocketCommandBase, IUserCommandInteraction, IApplicationCommandInteraction, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	public new SocketUserCommandData Data { get; }

	IUserCommandInteractionData IUserCommandInteraction.Data => Data;

	IDiscordInteractionData IDiscordInteraction.Data => Data;

	IApplicationCommandInteractionData IApplicationCommandInteraction.Data => Data;

	internal SocketUserCommand(DiscordSocketClient client, Interaction model, ISocketMessageChannel channel, SocketUser user)
		: base(client, model, channel, user)
	{
		ApplicationCommandInteractionData model2 = (model.Data.IsSpecified ? ((ApplicationCommandInteractionData)model.Data.Value) : null);
		ulong? guildId = model.GuildId.ToNullable();
		Data = SocketUserCommandData.Create(client, model2, model.Id, guildId);
	}

	internal new static SocketInteraction Create(DiscordSocketClient client, Interaction model, ISocketMessageChannel channel, SocketUser user)
	{
		SocketUserCommand socketUserCommand = new SocketUserCommand(client, model, channel, user);
		socketUserCommand.Update(model);
		return socketUserCommand;
	}
}
