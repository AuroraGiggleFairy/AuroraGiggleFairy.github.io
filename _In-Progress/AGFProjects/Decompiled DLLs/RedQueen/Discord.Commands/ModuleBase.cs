using System;
using System.Threading.Tasks;
using Discord.Commands.Builders;

namespace Discord.Commands;

internal abstract class ModuleBase : ModuleBase<ICommandContext>
{
}
internal abstract class ModuleBase<T> : IModuleBase where T : class, ICommandContext
{
	public T Context { get; private set; }

	protected virtual async Task<IUserMessage> ReplyAsync(string message = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null)
	{
		return await Context.Channel.SendMessageAsync(message, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds).ConfigureAwait(continueOnCapturedContext: false);
	}

	protected virtual Task BeforeExecuteAsync(CommandInfo command)
	{
		return Task.CompletedTask;
	}

	protected virtual void BeforeExecute(CommandInfo command)
	{
	}

	protected virtual Task AfterExecuteAsync(CommandInfo command)
	{
		return Task.CompletedTask;
	}

	protected virtual void AfterExecute(CommandInfo command)
	{
	}

	protected virtual void OnModuleBuilding(CommandService commandService, ModuleBuilder builder)
	{
	}

	void IModuleBase.SetContext(ICommandContext context)
	{
		T val = context as T;
		Context = val ?? throw new InvalidOperationException("Invalid context type. Expected " + typeof(T).Name + ", got " + context.GetType().Name + ".");
	}

	Task IModuleBase.BeforeExecuteAsync(CommandInfo command)
	{
		return BeforeExecuteAsync(command);
	}

	void IModuleBase.BeforeExecute(CommandInfo command)
	{
		BeforeExecute(command);
	}

	Task IModuleBase.AfterExecuteAsync(CommandInfo command)
	{
		return AfterExecuteAsync(command);
	}

	void IModuleBase.AfterExecute(CommandInfo command)
	{
		AfterExecute(command);
	}

	void IModuleBase.OnModuleBuilding(CommandService commandService, ModuleBuilder builder)
	{
		OnModuleBuilding(commandService, builder);
	}
}
