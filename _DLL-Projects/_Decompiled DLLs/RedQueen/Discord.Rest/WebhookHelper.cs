using System;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;

namespace Discord.Rest;

internal static class WebhookHelper
{
	public static async Task<Webhook> ModifyAsync(IWebhook webhook, BaseDiscordClient client, Action<WebhookProperties> func, RequestOptions options)
	{
		WebhookProperties webhookProperties = new WebhookProperties();
		func(webhookProperties);
		ModifyWebhookParams modifyWebhookParams = new ModifyWebhookParams
		{
			Avatar = (webhookProperties.Image.IsSpecified ? ((Optional<Discord.API.Image?>)(webhookProperties.Image.Value?.ToModel())) : Optional.Create<Discord.API.Image?>()),
			Name = webhookProperties.Name
		};
		if (!modifyWebhookParams.Avatar.IsSpecified && webhook.AvatarId != null)
		{
			modifyWebhookParams.Avatar = new Discord.API.Image(webhook.AvatarId);
		}
		if (webhookProperties.Channel.IsSpecified)
		{
			modifyWebhookParams.ChannelId = webhookProperties.Channel.Value.Id;
		}
		else if (webhookProperties.ChannelId.IsSpecified)
		{
			modifyWebhookParams.ChannelId = webhookProperties.ChannelId.Value;
		}
		return await client.ApiClient.ModifyWebhookAsync(webhook.Id, modifyWebhookParams, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task DeleteAsync(IWebhook webhook, BaseDiscordClient client, RequestOptions options)
	{
		await client.ApiClient.DeleteWebhookAsync(webhook.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}
}
