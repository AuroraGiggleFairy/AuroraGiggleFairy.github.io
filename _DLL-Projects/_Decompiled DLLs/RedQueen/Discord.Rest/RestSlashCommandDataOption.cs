using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class RestSlashCommandDataOption : IApplicationCommandInteractionDataOption
{
	public string Name { get; private set; }

	public object Value { get; private set; }

	public ApplicationCommandOptionType Type { get; private set; }

	public IReadOnlyCollection<RestSlashCommandDataOption> Options { get; private set; }

	IReadOnlyCollection<IApplicationCommandInteractionDataOption> IApplicationCommandInteractionDataOption.Options => Options;

	internal RestSlashCommandDataOption()
	{
	}

	internal RestSlashCommandDataOption(RestSlashCommandData data, ApplicationCommandInteractionDataOption model)
	{
		Name = model.Name;
		Type = model.Type;
		if (model.Value.IsSpecified)
		{
			switch (Type)
			{
			case ApplicationCommandOptionType.User:
			case ApplicationCommandOptionType.Channel:
			case ApplicationCommandOptionType.Role:
			case ApplicationCommandOptionType.Mentionable:
			case ApplicationCommandOptionType.Attachment:
			{
				if (!ulong.TryParse($"{model.Value.Value}", out var valueId))
				{
					break;
				}
				switch (Type)
				{
				case ApplicationCommandOptionType.User:
				{
					RestGuildUser value2 = data.ResolvableData.GuildMembers.FirstOrDefault((KeyValuePair<ulong, RestGuildUser> x) => x.Key == valueId).Value;
					if (value2 != null)
					{
						Value = value2;
						break;
					}
					Value = data.ResolvableData.Users.FirstOrDefault((KeyValuePair<ulong, RestUser> x) => x.Key == valueId).Value;
					break;
				}
				case ApplicationCommandOptionType.Channel:
					Value = data.ResolvableData.Channels.FirstOrDefault((KeyValuePair<ulong, RestChannel> x) => x.Key == valueId).Value;
					break;
				case ApplicationCommandOptionType.Role:
					Value = data.ResolvableData.Roles.FirstOrDefault((KeyValuePair<ulong, RestRole> x) => x.Key == valueId).Value;
					break;
				case ApplicationCommandOptionType.Mentionable:
					if (data.ResolvableData.GuildMembers.Any((KeyValuePair<ulong, RestGuildUser> x) => x.Key == valueId) || data.ResolvableData.Users.Any((KeyValuePair<ulong, RestUser> x) => x.Key == valueId))
					{
						RestGuildUser value = data.ResolvableData.GuildMembers.FirstOrDefault((KeyValuePair<ulong, RestGuildUser> x) => x.Key == valueId).Value;
						if (value != null)
						{
							Value = value;
							break;
						}
						Value = data.ResolvableData.Users.FirstOrDefault((KeyValuePair<ulong, RestUser> x) => x.Key == valueId).Value;
					}
					else if (data.ResolvableData.Roles.Any((KeyValuePair<ulong, RestRole> x) => x.Key == valueId))
					{
						Value = data.ResolvableData.Roles.FirstOrDefault((KeyValuePair<ulong, RestRole> x) => x.Key == valueId).Value;
					}
					break;
				case ApplicationCommandOptionType.Attachment:
					Value = data.ResolvableData.Attachments.FirstOrDefault((KeyValuePair<ulong, Attachment> x) => x.Key == valueId).Value;
					break;
				default:
					Value = model.Value.Value;
					break;
				}
				break;
			}
			case ApplicationCommandOptionType.String:
				Value = model.Value.ToString();
				break;
			case ApplicationCommandOptionType.Integer:
			{
				long result2;
				if (model.Value.Value is long num2)
				{
					Value = num2;
				}
				else if (long.TryParse(model.Value.Value.ToString(), out result2))
				{
					Value = result2;
				}
				break;
			}
			case ApplicationCommandOptionType.Boolean:
			{
				bool result3;
				if (model.Value.Value is bool flag)
				{
					Value = flag;
				}
				else if (bool.TryParse(model.Value.Value.ToString(), out result3))
				{
					Value = result3;
				}
				break;
			}
			case ApplicationCommandOptionType.Number:
			{
				double result;
				if (model.Value.Value is int num)
				{
					Value = num;
				}
				else if (double.TryParse(model.Value.Value.ToString(), out result))
				{
					Value = result;
				}
				break;
			}
			}
		}
		Options = (model.Options.IsSpecified ? model.Options.Value.Select((ApplicationCommandInteractionDataOption x) => new RestSlashCommandDataOption(data, x)).ToImmutableArray() : System.Collections.Immutable.ImmutableArray.Create<RestSlashCommandDataOption>());
	}

	public static explicit operator bool(RestSlashCommandDataOption option)
	{
		return (bool)option.Value;
	}

	public static explicit operator int(RestSlashCommandDataOption option)
	{
		return (int)option.Value;
	}

	public static explicit operator string(RestSlashCommandDataOption option)
	{
		return option.Value.ToString();
	}
}
