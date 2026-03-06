using System.Collections.Generic;
using Discord.API;

namespace Discord.WebSocket;

internal class SocketCommandBaseData<TOption> : SocketEntity<ulong>, IApplicationCommandInteractionData, IDiscordInteractionData where TOption : IApplicationCommandInteractionDataOption
{
	internal readonly SocketResolvableData<ApplicationCommandInteractionData> ResolvableData;

	public string Name { get; private set; }

	public virtual IReadOnlyCollection<TOption> Options { get; internal set; }

	IReadOnlyCollection<IApplicationCommandInteractionDataOption> IApplicationCommandInteractionData.Options => (IReadOnlyCollection<IApplicationCommandInteractionDataOption>)Options;

	internal SocketCommandBaseData(DiscordSocketClient client, ApplicationCommandInteractionData model, ulong? guildId)
		: base(client, model.Id)
	{
		if (model.Resolved.IsSpecified)
		{
			ResolvableData = new SocketResolvableData<ApplicationCommandInteractionData>(client, guildId, model);
		}
	}

	internal static SocketCommandBaseData Create(DiscordSocketClient client, ApplicationCommandInteractionData model, ulong id, ulong? guildId)
	{
		SocketCommandBaseData socketCommandBaseData = new SocketCommandBaseData(client, model, guildId);
		socketCommandBaseData.Update(model);
		return socketCommandBaseData;
	}

	internal virtual void Update(ApplicationCommandInteractionData model)
	{
		Name = model.Name;
	}
}
internal class SocketCommandBaseData : SocketCommandBaseData<IApplicationCommandInteractionDataOption>
{
	internal SocketCommandBaseData(DiscordSocketClient client, ApplicationCommandInteractionData model, ulong? guildId)
		: base(client, model, guildId)
	{
	}
}
