namespace Discord;

internal interface IVoiceRegion
{
	string Id { get; }

	string Name { get; }

	bool IsVip { get; }

	bool IsOptimal { get; }

	bool IsDeprecated { get; }

	bool IsCustom { get; }
}
