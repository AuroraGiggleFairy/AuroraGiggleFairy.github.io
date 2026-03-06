using Discord.API;

namespace Discord.Rest;

internal class RestIntegrationApplication : RestEntity<ulong>, IIntegrationApplication
{
	public string Name { get; private set; }

	public string Icon { get; private set; }

	public string Description { get; private set; }

	public string Summary { get; private set; }

	public IUser Bot { get; private set; }

	internal RestIntegrationApplication(BaseDiscordClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal static RestIntegrationApplication Create(BaseDiscordClient discord, IntegrationApplication model)
	{
		RestIntegrationApplication restIntegrationApplication = new RestIntegrationApplication(discord, model.Id);
		restIntegrationApplication.Update(model);
		return restIntegrationApplication;
	}

	internal void Update(IntegrationApplication model)
	{
		Name = model.Name;
		Icon = (model.Icon.IsSpecified ? model.Icon.Value : null);
		Description = model.Description;
		Summary = model.Summary;
		Bot = RestUser.Create(base.Discord, model.Bot.Value);
	}
}
