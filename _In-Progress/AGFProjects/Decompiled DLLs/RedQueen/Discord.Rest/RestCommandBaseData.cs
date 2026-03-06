using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal class RestCommandBaseData<TOption> : RestEntity<ulong>, IApplicationCommandInteractionData, IDiscordInteractionData where TOption : IApplicationCommandInteractionDataOption
{
	internal RestResolvableData<ApplicationCommandInteractionData> ResolvableData;

	public string Name { get; private set; }

	public virtual IReadOnlyCollection<TOption> Options { get; internal set; }

	IReadOnlyCollection<IApplicationCommandInteractionDataOption> IApplicationCommandInteractionData.Options => (IReadOnlyCollection<IApplicationCommandInteractionDataOption>)Options;

	internal RestCommandBaseData(BaseDiscordClient client, ApplicationCommandInteractionData model)
		: base(client, model.Id)
	{
	}

	internal static async Task<RestCommandBaseData> CreateAsync(DiscordRestClient client, ApplicationCommandInteractionData model, RestGuild guild, IRestMessageChannel channel, bool doApiCall)
	{
		RestCommandBaseData entity = new RestCommandBaseData(client, model);
		await entity.UpdateAsync(client, model, guild, channel, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
		return entity;
	}

	internal virtual async Task UpdateAsync(DiscordRestClient client, ApplicationCommandInteractionData model, RestGuild guild, IRestMessageChannel channel, bool doApiCall)
	{
		Name = model.Name;
		if (model.Resolved.IsSpecified && ResolvableData == null)
		{
			ResolvableData = new RestResolvableData<ApplicationCommandInteractionData>();
			await ResolvableData.PopulateAsync(client, guild, channel, model, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
		}
	}
}
internal class RestCommandBaseData : RestCommandBaseData<IApplicationCommandInteractionDataOption>
{
	internal RestCommandBaseData(DiscordRestClient client, ApplicationCommandInteractionData model)
		: base((BaseDiscordClient)client, model)
	{
	}
}
