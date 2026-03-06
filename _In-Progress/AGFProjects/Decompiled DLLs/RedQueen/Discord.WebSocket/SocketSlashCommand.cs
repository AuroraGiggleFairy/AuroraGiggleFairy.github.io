using Discord.API;

namespace Discord.WebSocket;

internal class SocketSlashCommand : SocketCommandBase, ISlashCommandInteraction, IApplicationCommandInteraction, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	public new SocketSlashCommandData Data { get; }

	IApplicationCommandInteractionData ISlashCommandInteraction.Data => Data;

	IDiscordInteractionData IDiscordInteraction.Data => Data;

	IApplicationCommandInteractionData IApplicationCommandInteraction.Data => Data;

	internal SocketSlashCommand(DiscordSocketClient client, Interaction model, ISocketMessageChannel channel, SocketUser user)
		: base(client, model, channel, user)
	{
		ApplicationCommandInteractionData model2 = (model.Data.IsSpecified ? ((ApplicationCommandInteractionData)model.Data.Value) : null);
		ulong? guildId = model.GuildId.ToNullable();
		Data = SocketSlashCommandData.Create(client, model2, guildId);
	}

	internal new static SocketInteraction Create(DiscordSocketClient client, Interaction model, ISocketMessageChannel channel, SocketUser user)
	{
		SocketSlashCommand socketSlashCommand = new SocketSlashCommand(client, model, channel, user);
		socketSlashCommand.Update(model);
		return socketSlashCommand;
	}
}
