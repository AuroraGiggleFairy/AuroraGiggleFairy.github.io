using System.Runtime.CompilerServices;
using Discord.API;

namespace Discord.Rest;

[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
internal struct InteractionProperties
{
	public InteractionType Type { get; }

	public ApplicationCommandType? CommandType { get; }

	public string Name { get; }

	public string CustomId { get; }

	public ulong? GuildId { get; }

	public ulong? ChannelId { get; }

	internal InteractionProperties(Interaction model)
	{
		Name = string.Empty;
		CustomId = string.Empty;
		Type = model.Type;
		CommandType = null;
		if (model.GuildId.IsSpecified)
		{
			GuildId = model.GuildId.Value;
		}
		else
		{
			GuildId = null;
		}
		if (model.ChannelId.IsSpecified)
		{
			ChannelId = model.ChannelId.Value;
		}
		else
		{
			ChannelId = null;
		}
		switch (Type)
		{
		case InteractionType.ApplicationCommand:
		{
			ApplicationCommandInteractionData applicationCommandInteractionData = (ApplicationCommandInteractionData)(IDiscordInteractionData)model.Data;
			CommandType = applicationCommandInteractionData.Type;
			Name = applicationCommandInteractionData.Name;
			break;
		}
		case InteractionType.MessageComponent:
		{
			MessageComponentInteractionData messageComponentInteractionData = (MessageComponentInteractionData)(IDiscordInteractionData)model.Data;
			CustomId = messageComponentInteractionData.CustomId;
			break;
		}
		case InteractionType.ModalSubmit:
		{
			ModalInteractionData modalInteractionData = (ModalInteractionData)(IDiscordInteractionData)model.Data;
			CustomId = modalInteractionData.CustomId;
			break;
		}
		case InteractionType.ApplicationCommandAutocomplete:
			break;
		}
	}
}
