using System;
using System.Threading.Tasks;

namespace Discord.Commands;

internal abstract class TypeReader
{
	public abstract Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services);
}
