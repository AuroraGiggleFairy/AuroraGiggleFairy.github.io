namespace Discord;

internal interface IIntegrationApplication
{
	ulong Id { get; }

	string Name { get; }

	string Icon { get; }

	string Description { get; }

	string Summary { get; }

	IUser Bot { get; }
}
