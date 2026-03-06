using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Commands;

internal class RoleTypeReader<T> : TypeReader where T : class, IRole
{
	public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
	{
		if (context.Guild != null)
		{
			Dictionary<ulong, TypeReaderValue> dictionary = new Dictionary<ulong, TypeReaderValue>();
			IReadOnlyCollection<IRole> roles = context.Guild.Roles;
			if (MentionUtils.TryParseRole(input, out var roleId))
			{
				AddResult(dictionary, context.Guild.GetRole(roleId) as T, 1f);
			}
			if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out roleId))
			{
				AddResult(dictionary, context.Guild.GetRole(roleId) as T, 0.9f);
			}
			foreach (IRole item in roles.Where((IRole x) => string.Equals(input, x.Name, StringComparison.OrdinalIgnoreCase)))
			{
				AddResult(dictionary, item as T, (item.Name == input) ? 0.8f : 0.7f);
			}
			if (dictionary.Count > 0)
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(dictionary.Values.ToReadOnlyCollection()));
			}
		}
		return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Role not found."));
	}

	private void AddResult(Dictionary<ulong, TypeReaderValue> results, T role, float score)
	{
		if (role != null && !results.ContainsKey(role.Id))
		{
			results.Add(role.Id, new TypeReaderValue(role, score));
		}
	}
}
