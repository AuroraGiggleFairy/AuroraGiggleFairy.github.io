namespace Discord;

internal class RoleProperties
{
	public Optional<string> Name { get; set; }

	public Optional<GuildPermissions> Permissions { get; set; }

	public Optional<int> Position { get; set; }

	public Optional<Color> Color { get; set; }

	public Optional<bool> Hoist { get; set; }

	public Optional<Image?> Icon { get; set; }

	public Optional<Emoji> Emoji { get; set; }

	public Optional<bool> Mentionable { get; set; }
}
