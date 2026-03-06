using System.Collections.Immutable;
using System.Linq;
using Discord.API;

namespace Discord.WebSocket;

internal class SocketSlashCommandData : SocketCommandBaseData<SocketSlashCommandDataOption>, IDiscordInteractionData
{
	internal SocketSlashCommandData(DiscordSocketClient client, ApplicationCommandInteractionData model, ulong? guildId)
		: base(client, model, guildId)
	{
	}

	internal static SocketSlashCommandData Create(DiscordSocketClient client, ApplicationCommandInteractionData model, ulong? guildId)
	{
		SocketSlashCommandData socketSlashCommandData = new SocketSlashCommandData(client, model, guildId);
		socketSlashCommandData.Update(model);
		return socketSlashCommandData;
	}

	internal override void Update(ApplicationCommandInteractionData model)
	{
		base.Update(model);
		Options = (model.Options.IsSpecified ? model.Options.Value.Select((ApplicationCommandInteractionDataOption x) => new SocketSlashCommandDataOption(this, x)).ToImmutableArray() : System.Collections.Immutable.ImmutableArray.Create<SocketSlashCommandDataOption>());
	}
}
