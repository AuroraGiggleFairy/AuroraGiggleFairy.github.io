using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Discord.Commands;

internal struct CommandMatch
{
	public CommandInfo Command
	{
		[_003Cc51ba3f1_002D43fc_002D4bc9_002D811d_002D97306e7eb83f_003EIsReadOnly]
		get;
	}

	public string Alias
	{
		[_003Cc51ba3f1_002D43fc_002D4bc9_002D811d_002D97306e7eb83f_003EIsReadOnly]
		get;
	}

	public CommandMatch(CommandInfo command, string alias)
	{
		Command = command;
		Alias = alias;
	}

	public Task<PreconditionResult> CheckPreconditionsAsync(ICommandContext context, IServiceProvider services = null)
	{
		return Command.CheckPreconditionsAsync(context, services);
	}

	public Task<ParseResult> ParseAsync(ICommandContext context, SearchResult searchResult, PreconditionResult preconditionResult = null, IServiceProvider services = null)
	{
		return Command.ParseAsync(context, Alias.Length, searchResult, preconditionResult, services);
	}

	public Task<IResult> ExecuteAsync(ICommandContext context, IEnumerable<object> argList, IEnumerable<object> paramList, IServiceProvider services)
	{
		return Command.ExecuteAsync(context, argList, paramList, services);
	}

	public Task<IResult> ExecuteAsync(ICommandContext context, ParseResult parseResult, IServiceProvider services)
	{
		return Command.ExecuteAsync(context, parseResult, services);
	}
}
