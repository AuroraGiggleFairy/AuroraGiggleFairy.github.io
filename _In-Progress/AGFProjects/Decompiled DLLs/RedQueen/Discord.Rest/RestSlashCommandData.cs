using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal class RestSlashCommandData : RestCommandBaseData<RestSlashCommandDataOption>, IDiscordInteractionData
{
	internal RestSlashCommandData(DiscordRestClient client, ApplicationCommandInteractionData model)
		: base((BaseDiscordClient)client, model)
	{
	}

	internal new static async Task<RestSlashCommandData> CreateAsync(DiscordRestClient client, ApplicationCommandInteractionData model, RestGuild guild, IRestMessageChannel channel, bool doApiCall)
	{
		RestSlashCommandData entity = new RestSlashCommandData(client, model);
		await entity.UpdateAsync(client, model, guild, channel, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
		return entity;
	}

	internal override async Task UpdateAsync(DiscordRestClient client, ApplicationCommandInteractionData model, RestGuild guild, IRestMessageChannel channel, bool doApiCall)
	{
		await base.UpdateAsync(client, model, guild, channel, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
		Options = (model.Options.IsSpecified ? model.Options.Value.Select((ApplicationCommandInteractionDataOption x) => new RestSlashCommandDataOption(this, x)).ToImmutableArray() : System.Collections.Immutable.ImmutableArray.Create<RestSlashCommandDataOption>());
	}
}
