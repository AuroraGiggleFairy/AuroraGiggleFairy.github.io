using Discord.API;

namespace Discord.Rest;

internal class RestIntegrationAccount : IIntegrationAccount
{
	public string Id { get; private set; }

	public string Name { get; private set; }

	internal RestIntegrationAccount()
	{
	}

	internal static RestIntegrationAccount Create(IntegrationAccount model)
	{
		RestIntegrationAccount restIntegrationAccount = new RestIntegrationAccount();
		restIntegrationAccount.Update(model);
		return restIntegrationAccount;
	}

	internal void Update(IntegrationAccount model)
	{
		model.Name = Name;
		model.Id = Id;
	}
}
