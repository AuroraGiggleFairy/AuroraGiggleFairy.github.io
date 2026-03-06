using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord.Interactions.Builders;
using Discord.Logging;
using Discord.Rest;
using Discord.WebSocket;

namespace Discord.Interactions;

internal class InteractionService : IDisposable
{
	internal readonly AsyncEvent<Func<LogMessage, Task>> _logEvent = new AsyncEvent<Func<LogMessage, Task>>();

	internal readonly AsyncEvent<Func<SlashCommandInfo, IInteractionContext, IResult, Task>> _slashCommandExecutedEvent = new AsyncEvent<Func<SlashCommandInfo, IInteractionContext, IResult, Task>>();

	internal readonly AsyncEvent<Func<ContextCommandInfo, IInteractionContext, IResult, Task>> _contextCommandExecutedEvent = new AsyncEvent<Func<ContextCommandInfo, IInteractionContext, IResult, Task>>();

	internal readonly AsyncEvent<Func<ComponentCommandInfo, IInteractionContext, IResult, Task>> _componentCommandExecutedEvent = new AsyncEvent<Func<ComponentCommandInfo, IInteractionContext, IResult, Task>>();

	internal readonly AsyncEvent<Func<AutocompleteCommandInfo, IInteractionContext, IResult, Task>> _autocompleteCommandExecutedEvent = new AsyncEvent<Func<AutocompleteCommandInfo, IInteractionContext, IResult, Task>>();

	internal readonly AsyncEvent<Func<IAutocompleteHandler, IInteractionContext, IResult, Task>> _autocompleteHandlerExecutedEvent = new AsyncEvent<Func<IAutocompleteHandler, IInteractionContext, IResult, Task>>();

	internal readonly AsyncEvent<Func<ModalCommandInfo, IInteractionContext, IResult, Task>> _modalCommandExecutedEvent = new AsyncEvent<Func<ModalCommandInfo, IInteractionContext, IResult, Task>>();

	private readonly ConcurrentDictionary<Type, ModuleInfo> _typedModuleDefs;

	private readonly CommandMap<SlashCommandInfo> _slashCommandMap;

	private readonly ConcurrentDictionary<ApplicationCommandType, CommandMap<ContextCommandInfo>> _contextCommandMaps;

	private readonly CommandMap<ComponentCommandInfo> _componentCommandMap;

	private readonly CommandMap<AutocompleteCommandInfo> _autocompleteCommandMap;

	private readonly CommandMap<ModalCommandInfo> _modalCommandMap;

	private readonly HashSet<ModuleInfo> _moduleDefs;

	private readonly TypeMap<TypeConverter, IApplicationCommandInteractionDataOption> _typeConverterMap;

	private readonly TypeMap<ComponentTypeConverter, IComponentInteractionData> _compTypeConverterMap;

	private readonly TypeMap<TypeReader, string> _typeReaderMap;

	private readonly ConcurrentDictionary<Type, IAutocompleteHandler> _autocompleteHandlers = new ConcurrentDictionary<Type, IAutocompleteHandler>();

	private readonly ConcurrentDictionary<Type, ModalInfo> _modalInfos = new ConcurrentDictionary<Type, ModalInfo>();

	private readonly SemaphoreSlim _lock;

	internal readonly Logger _cmdLogger;

	internal readonly LogManager _logManager;

	internal readonly Func<DiscordRestClient> _getRestClient;

	internal readonly bool _throwOnError;

	internal readonly bool _useCompiledLambda;

	internal readonly bool _enableAutocompleteHandlers;

	internal readonly bool _autoServiceScopes;

	internal readonly bool _exitOnMissingModalField;

	internal readonly string _wildCardExp;

	internal readonly RunMode _runMode;

	internal readonly RestResponseCallback _restResponseCallback;

	public ILocalizationManager LocalizationManager { get; set; }

	public DiscordRestClient RestClient => _getRestClient();

	public IReadOnlyList<ModuleInfo> Modules => _moduleDefs.ToList();

	public IReadOnlyList<SlashCommandInfo> SlashCommands => _moduleDefs.SelectMany((ModuleInfo x) => x.SlashCommands).ToList();

	public IReadOnlyList<ContextCommandInfo> ContextCommands => _moduleDefs.SelectMany((ModuleInfo x) => x.ContextCommands).ToList();

	public IReadOnlyCollection<ComponentCommandInfo> ComponentCommands => _moduleDefs.SelectMany((ModuleInfo x) => x.ComponentCommands).ToList();

	public IReadOnlyCollection<ModalCommandInfo> ModalCommands => _moduleDefs.SelectMany((ModuleInfo x) => x.ModalCommands).ToList();

	public IReadOnlyCollection<ModalInfo> Modals => ModalUtils.Modals;

	public event Func<LogMessage, Task> Log
	{
		add
		{
			_logEvent.Add(value);
		}
		remove
		{
			_logEvent.Remove(value);
		}
	}

	public event Func<ICommandInfo, IInteractionContext, IResult, Task> InteractionExecuted
	{
		add
		{
			SlashCommandExecuted += value;
			ContextCommandExecuted += value;
			ComponentCommandExecuted += value;
			AutocompleteCommandExecuted += value;
			ModalCommandExecuted += value;
		}
		remove
		{
			SlashCommandExecuted -= value;
			ContextCommandExecuted -= value;
			ComponentCommandExecuted -= value;
			AutocompleteCommandExecuted -= value;
			ModalCommandExecuted -= value;
		}
	}

	public event Func<SlashCommandInfo, IInteractionContext, IResult, Task> SlashCommandExecuted
	{
		add
		{
			_slashCommandExecutedEvent.Add(value);
		}
		remove
		{
			_slashCommandExecutedEvent.Remove(value);
		}
	}

	public event Func<ContextCommandInfo, IInteractionContext, IResult, Task> ContextCommandExecuted
	{
		add
		{
			_contextCommandExecutedEvent.Add(value);
		}
		remove
		{
			_contextCommandExecutedEvent.Remove(value);
		}
	}

	public event Func<ComponentCommandInfo, IInteractionContext, IResult, Task> ComponentCommandExecuted
	{
		add
		{
			_componentCommandExecutedEvent.Add(value);
		}
		remove
		{
			_componentCommandExecutedEvent.Remove(value);
		}
	}

	public event Func<AutocompleteCommandInfo, IInteractionContext, IResult, Task> AutocompleteCommandExecuted
	{
		add
		{
			_autocompleteCommandExecutedEvent.Add(value);
		}
		remove
		{
			_autocompleteCommandExecutedEvent.Remove(value);
		}
	}

	public event Func<IAutocompleteHandler, IInteractionContext, IResult, Task> AutocompleteHandlerExecuted
	{
		add
		{
			_autocompleteHandlerExecutedEvent.Add(value);
		}
		remove
		{
			_autocompleteHandlerExecutedEvent.Remove(value);
		}
	}

	public event Func<ModalCommandInfo, IInteractionContext, IResult, Task> ModalCommandExecuted
	{
		add
		{
			_modalCommandExecutedEvent.Add(value);
		}
		remove
		{
			_modalCommandExecutedEvent.Remove(value);
		}
	}

	public InteractionService(DiscordSocketClient discord, InteractionServiceConfig config = null)
		: this(() => discord.Rest, config ?? new InteractionServiceConfig())
	{
	}

	public InteractionService(DiscordShardedClient discord, InteractionServiceConfig config = null)
		: this(() => discord.Rest, config ?? new InteractionServiceConfig())
	{
	}

	public InteractionService(BaseSocketClient discord, InteractionServiceConfig config = null)
		: this(() => discord.Rest, config ?? new InteractionServiceConfig())
	{
	}

	public InteractionService(DiscordRestClient discord, InteractionServiceConfig config = null)
		: this(() => discord, config ?? new InteractionServiceConfig())
	{
	}

	private InteractionService(Func<DiscordRestClient> getRestClient, InteractionServiceConfig config = null)
	{
		if (config == null)
		{
			config = new InteractionServiceConfig();
		}
		_lock = new SemaphoreSlim(1, 1);
		_typedModuleDefs = new ConcurrentDictionary<Type, ModuleInfo>();
		_moduleDefs = new HashSet<ModuleInfo>();
		_logManager = new LogManager(config.LogLevel);
		_logManager.Message += async delegate(LogMessage msg)
		{
			await _logEvent.InvokeAsync(msg).ConfigureAwait(continueOnCapturedContext: false);
		};
		_cmdLogger = _logManager.CreateLogger("App Commands");
		_slashCommandMap = new CommandMap<SlashCommandInfo>(this);
		_contextCommandMaps = new ConcurrentDictionary<ApplicationCommandType, CommandMap<ContextCommandInfo>>();
		_componentCommandMap = new CommandMap<ComponentCommandInfo>(this, config.InteractionCustomIdDelimiters);
		_autocompleteCommandMap = new CommandMap<AutocompleteCommandInfo>(this);
		_modalCommandMap = new CommandMap<ModalCommandInfo>(this, config.InteractionCustomIdDelimiters);
		_getRestClient = getRestClient;
		_runMode = config.DefaultRunMode;
		if (_runMode == RunMode.Default)
		{
			throw new InvalidOperationException($"RunMode cannot be set to {RunMode.Default}");
		}
		_throwOnError = config.ThrowOnError;
		_wildCardExp = config.WildCardExpression;
		_useCompiledLambda = config.UseCompiledLambda;
		_exitOnMissingModalField = config.ExitOnMissingModalField;
		_enableAutocompleteHandlers = config.EnableAutocompleteHandlers;
		_autoServiceScopes = config.AutoServiceScopes;
		_restResponseCallback = config.RestResponseCallback;
		LocalizationManager = config.LocalizationManager;
		_typeConverterMap = new TypeMap<TypeConverter, IApplicationCommandInteractionDataOption>(this, new ConcurrentDictionary<Type, TypeConverter> { [typeof(TimeSpan)] = new TimeSpanConverter() }, new ConcurrentDictionary<Type, Type>
		{
			[typeof(IChannel)] = typeof(DefaultChannelConverter<>),
			[typeof(IRole)] = typeof(DefaultRoleConverter<>),
			[typeof(IAttachment)] = typeof(DefaultAttachmentConverter<>),
			[typeof(IUser)] = typeof(DefaultUserConverter<>),
			[typeof(IMentionable)] = typeof(DefaultMentionableConverter<>),
			[typeof(IConvertible)] = typeof(DefaultValueConverter<>),
			[typeof(Enum)] = typeof(EnumConverter<>),
			[typeof(Nullable<>)] = typeof(NullableConverter<>)
		});
		_compTypeConverterMap = new TypeMap<ComponentTypeConverter, IComponentInteractionData>(this, new ConcurrentDictionary<Type, ComponentTypeConverter>(), new ConcurrentDictionary<Type, Type>
		{
			[typeof(Array)] = typeof(DefaultArrayComponentConverter<>),
			[typeof(IConvertible)] = typeof(DefaultValueComponentConverter<>),
			[typeof(Nullable<>)] = typeof(NullableComponentConverter<>)
		});
		_typeReaderMap = new TypeMap<TypeReader, string>(this, new ConcurrentDictionary<Type, TypeReader>(), new ConcurrentDictionary<Type, Type>
		{
			[typeof(IChannel)] = typeof(DefaultChannelReader<>),
			[typeof(IRole)] = typeof(DefaultRoleReader<>),
			[typeof(IUser)] = typeof(DefaultUserReader<>),
			[typeof(IMessage)] = typeof(DefaultMessageReader<>),
			[typeof(IConvertible)] = typeof(DefaultValueReader<>),
			[typeof(Enum)] = typeof(EnumReader<>),
			[typeof(Nullable<>)] = typeof(NullableReader<>)
		});
	}

	public async Task<ModuleInfo> CreateModuleAsync(string name, IServiceProvider services, Action<ModuleBuilder> buildFunc)
	{
		if (services == null)
		{
			services = EmptyServiceProvider.Instance;
		}
		await _lock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			ModuleBuilder moduleBuilder = new ModuleBuilder(this, name);
			buildFunc(moduleBuilder);
			ModuleInfo moduleInfo = moduleBuilder.Build(this, services);
			LoadModuleInternal(moduleInfo);
			return moduleInfo;
		}
		finally
		{
			_lock.Release();
		}
	}

	public async Task<IEnumerable<ModuleInfo>> AddModulesAsync(Assembly assembly, IServiceProvider services)
	{
		if (services == null)
		{
			services = EmptyServiceProvider.Instance;
		}
		await _lock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			Dictionary<Type, ModuleInfo> dictionary = await ModuleClassBuilder.BuildAsync(await ModuleClassBuilder.SearchAsync(assembly, this), this, services);
			foreach (KeyValuePair<Type, ModuleInfo> item in dictionary)
			{
				_typedModuleDefs[item.Key] = item.Value;
				LoadModuleInternal(item.Value);
			}
			return dictionary.Values;
		}
		finally
		{
			_lock.Release();
		}
	}

	public Task<ModuleInfo> AddModuleAsync<T>(IServiceProvider services) where T : class
	{
		return AddModuleAsync(typeof(T), services);
	}

	public async Task<ModuleInfo> AddModuleAsync(Type type, IServiceProvider services)
	{
		if (!typeof(IInteractionModuleBase).IsAssignableFrom(type))
		{
			throw new ArgumentException("Type parameter must be a type of Slash Module", "type");
		}
		if (services == null)
		{
			services = EmptyServiceProvider.Instance;
		}
		await _lock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			TypeInfo typeInfo = type.GetTypeInfo();
			if (_typedModuleDefs.ContainsKey(typeInfo))
			{
				throw new ArgumentException("Module definition for this type already exists.");
			}
			KeyValuePair<Type, ModuleInfo> keyValuePair = (await ModuleClassBuilder.BuildAsync(new List<TypeInfo> { typeInfo }, this, services).ConfigureAwait(continueOnCapturedContext: false)).FirstOrDefault();
			if (keyValuePair.Value == null)
			{
				throw new InvalidOperationException("Could not build the module " + typeInfo.FullName + ", did you pass an invalid type?");
			}
			if (!_typedModuleDefs.TryAdd(type, keyValuePair.Value))
			{
				throw new ArgumentException("Module definition for this type already exists.");
			}
			_typedModuleDefs[keyValuePair.Key] = keyValuePair.Value;
			LoadModuleInternal(keyValuePair.Value);
			return keyValuePair.Value;
		}
		finally
		{
			_lock.Release();
		}
	}

	public async Task<IReadOnlyCollection<RestGuildCommand>> RegisterCommandsToGuildAsync(ulong guildId, bool deleteMissing = true)
	{
		EnsureClientReady();
		IEnumerable<ModuleInfo> source = _moduleDefs.Where((ModuleInfo x) => !x.IsSubModule);
		List<ApplicationCommandProperties> props = source.SelectMany((ModuleInfo x) => x.ToApplicationCommandProps()).ToList();
		if (!deleteMissing)
		{
			IEnumerable<RestGuildCommand> source2 = (await RestClient.GetGuildApplicationCommands(guildId).ConfigureAwait(continueOnCapturedContext: false)).Where((RestGuildCommand x) => !props.Any((ApplicationCommandProperties y) => y.Name.IsSpecified && y.Name.Value == x.Name));
			props.AddRange(source2.Select((RestGuildCommand x) => x.ToApplicationCommandProps()));
		}
		return await RestClient.BulkOverwriteGuildCommands(props.ToArray(), guildId).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<RestGlobalCommand>> RegisterCommandsGloballyAsync(bool deleteMissing = true)
	{
		EnsureClientReady();
		IEnumerable<ModuleInfo> source = _moduleDefs.Where((ModuleInfo x) => !x.IsSubModule);
		List<ApplicationCommandProperties> props = source.SelectMany((ModuleInfo x) => x.ToApplicationCommandProps()).ToList();
		if (!deleteMissing)
		{
			IEnumerable<RestGlobalCommand> source2 = (await RestClient.GetGlobalApplicationCommands().ConfigureAwait(continueOnCapturedContext: false)).Where((RestGlobalCommand x) => !props.Any((ApplicationCommandProperties y) => y.Name.IsSpecified && y.Name.Value == x.Name));
			props.AddRange(source2.Select((RestGlobalCommand x) => x.ToApplicationCommandProps()));
		}
		return await RestClient.BulkOverwriteGlobalCommands(props.ToArray()).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<RestGuildCommand>> AddCommandsToGuildAsync(IGuild guild, bool deleteMissing = false, params ICommandInfo[] commands)
	{
		if (guild == null)
		{
			throw new ArgumentNullException("guild");
		}
		return await AddCommandsToGuildAsync(guild.Id, deleteMissing, commands).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<RestGuildCommand>> AddCommandsToGuildAsync(ulong guildId, bool deleteMissing = false, params ICommandInfo[] commands)
	{
		EnsureClientReady();
		List<ApplicationCommandProperties> props = new List<ApplicationCommandProperties>();
		foreach (ICommandInfo commandInfo in commands)
		{
			if (!(commandInfo is SlashCommandInfo commandInfo2))
			{
				if (!(commandInfo is ContextCommandInfo commandInfo3))
				{
					throw new InvalidOperationException("Command type " + commandInfo.GetType().FullName + " isn't supported yet");
				}
				props.Add(commandInfo3.ToApplicationCommandProps());
			}
			else
			{
				props.Add(commandInfo2.ToApplicationCommandProps());
			}
		}
		if (!deleteMissing)
		{
			IEnumerable<RestGuildCommand> source = (await RestClient.GetGuildApplicationCommands(guildId).ConfigureAwait(continueOnCapturedContext: false)).Where((RestGuildCommand x) => !props.Any((ApplicationCommandProperties y) => y.Name.IsSpecified && y.Name.Value == x.Name));
			props.AddRange(source.Select((RestGuildCommand x) => x.ToApplicationCommandProps()));
		}
		return await RestClient.BulkOverwriteGuildCommands(props.ToArray(), guildId).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<RestGuildCommand>> AddModulesToGuildAsync(IGuild guild, bool deleteMissing = false, params ModuleInfo[] modules)
	{
		if (guild == null)
		{
			throw new ArgumentNullException("guild");
		}
		return await AddModulesToGuildAsync(guild.Id, deleteMissing, modules).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<RestGuildCommand>> AddModulesToGuildAsync(ulong guildId, bool deleteMissing = false, params ModuleInfo[] modules)
	{
		EnsureClientReady();
		List<ApplicationCommandProperties> props = modules.SelectMany((ModuleInfo x) => x.ToApplicationCommandProps(ignoreDontRegister: true)).ToList();
		if (!deleteMissing)
		{
			IEnumerable<RestGuildCommand> source = (await RestClient.GetGuildApplicationCommands(guildId).ConfigureAwait(continueOnCapturedContext: false)).Where((RestGuildCommand x) => !props.Any((ApplicationCommandProperties y) => y.Name.IsSpecified && y.Name.Value == x.Name));
			props.AddRange(source.Select((RestGuildCommand x) => x.ToApplicationCommandProps()));
		}
		return await RestClient.BulkOverwriteGuildCommands(props.ToArray(), guildId).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<RestGlobalCommand>> AddModulesGloballyAsync(bool deleteMissing = false, params ModuleInfo[] modules)
	{
		EnsureClientReady();
		List<ApplicationCommandProperties> props = modules.SelectMany((ModuleInfo x) => x.ToApplicationCommandProps(ignoreDontRegister: true)).ToList();
		if (!deleteMissing)
		{
			IEnumerable<RestGlobalCommand> source = (await RestClient.GetGlobalApplicationCommands().ConfigureAwait(continueOnCapturedContext: false)).Where((RestGlobalCommand x) => !props.Any((ApplicationCommandProperties y) => y.Name.IsSpecified && y.Name.Value == x.Name));
			props.AddRange(source.Select((RestGlobalCommand x) => x.ToApplicationCommandProps()));
		}
		return await RestClient.BulkOverwriteGlobalCommands(props.ToArray()).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<RestGlobalCommand>> AddCommandsGloballyAsync(bool deleteMissing = false, params IApplicationCommandInfo[] commands)
	{
		EnsureClientReady();
		List<ApplicationCommandProperties> props = new List<ApplicationCommandProperties>();
		foreach (IApplicationCommandInfo applicationCommandInfo in commands)
		{
			if (!(applicationCommandInfo is SlashCommandInfo commandInfo))
			{
				if (!(applicationCommandInfo is ContextCommandInfo commandInfo2))
				{
					throw new InvalidOperationException("Command type " + applicationCommandInfo.GetType().FullName + " isn't supported yet");
				}
				props.Add(commandInfo2.ToApplicationCommandProps());
			}
			else
			{
				props.Add(commandInfo.ToApplicationCommandProps());
			}
		}
		if (!deleteMissing)
		{
			IEnumerable<RestGlobalCommand> source = (await RestClient.GetGlobalApplicationCommands().ConfigureAwait(continueOnCapturedContext: false)).Where((RestGlobalCommand x) => !props.Any((ApplicationCommandProperties y) => y.Name.IsSpecified && y.Name.Value == x.Name));
			props.AddRange(source.Select((RestGlobalCommand x) => x.ToApplicationCommandProps()));
		}
		return await RestClient.BulkOverwriteGlobalCommands(props.ToArray()).ConfigureAwait(continueOnCapturedContext: false);
	}

	private void LoadModuleInternal(ModuleInfo module)
	{
		_moduleDefs.Add(module);
		foreach (SlashCommandInfo slashCommand in module.SlashCommands)
		{
			_slashCommandMap.AddCommand(slashCommand, slashCommand.IgnoreGroupNames);
		}
		foreach (ContextCommandInfo contextCommand in module.ContextCommands)
		{
			_contextCommandMaps.GetOrAdd(contextCommand.CommandType, new CommandMap<ContextCommandInfo>(this)).AddCommand(contextCommand, contextCommand.IgnoreGroupNames);
		}
		foreach (ComponentCommandInfo componentCommand in module.ComponentCommands)
		{
			_componentCommandMap.AddCommand(componentCommand, componentCommand.IgnoreGroupNames);
		}
		foreach (AutocompleteCommandInfo autocompleteCommand in module.AutocompleteCommands)
		{
			_autocompleteCommandMap.AddCommand(autocompleteCommand.GetCommandKeywords(), autocompleteCommand);
		}
		foreach (ModalCommandInfo modalCommand in module.ModalCommands)
		{
			_modalCommandMap.AddCommand(modalCommand, modalCommand.IgnoreGroupNames);
		}
		foreach (ModuleInfo subModule in module.SubModules)
		{
			LoadModuleInternal(subModule);
		}
	}

	public Task<bool> RemoveModuleAsync<T>()
	{
		return RemoveModuleAsync(typeof(T));
	}

	public async Task<bool> RemoveModuleAsync(Type type)
	{
		await _lock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			if (!_typedModuleDefs.TryRemove(type, out var value))
			{
				return false;
			}
			return RemoveModuleInternal(value);
		}
		finally
		{
			_lock.Release();
		}
	}

	public async Task<bool> RemoveModuleAsync(ModuleInfo module)
	{
		await _lock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			KeyValuePair<Type, ModuleInfo> keyValuePair = _typedModuleDefs.FirstOrDefault((KeyValuePair<Type, ModuleInfo> x) => x.Value.Equals(module));
			if (!keyValuePair.Equals(default(KeyValuePair<Type, ModuleInfo>)))
			{
				_typedModuleDefs.TryRemove(keyValuePair.Key, out var _);
			}
			return RemoveModuleInternal(module);
		}
		finally
		{
			_lock.Release();
		}
	}

	private bool RemoveModuleInternal(ModuleInfo moduleInfo)
	{
		if (!_moduleDefs.Remove(moduleInfo))
		{
			return false;
		}
		foreach (SlashCommandInfo slashCommand in moduleInfo.SlashCommands)
		{
			_slashCommandMap.RemoveCommand(slashCommand);
		}
		return true;
	}

	public SearchResult<SlashCommandInfo> SearchSlashCommand(ISlashCommandInteraction slashCommandInteraction)
	{
		return _slashCommandMap.GetCommand(slashCommandInteraction.Data.GetCommandKeywords());
	}

	public SearchResult<ComponentCommandInfo> SearchComponentCommand(IComponentInteraction componentInteraction)
	{
		return _componentCommandMap.GetCommand(componentInteraction.Data.CustomId);
	}

	public SearchResult<ContextCommandInfo> SearchUserCommand(IUserCommandInteraction userCommandInteraction)
	{
		return _contextCommandMaps[ApplicationCommandType.User].GetCommand(userCommandInteraction.Data.Name);
	}

	public SearchResult<ContextCommandInfo> SearchMessageCommand(IMessageCommandInteraction messageCommandInteraction)
	{
		return _contextCommandMaps[ApplicationCommandType.Message].GetCommand(messageCommandInteraction.Data.Name);
	}

	public SearchResult<AutocompleteCommandInfo> SearchAutocompleteCommand(IAutocompleteInteraction autocompleteInteraction)
	{
		IList<string> commandKeywords = autocompleteInteraction.Data.GetCommandKeywords();
		commandKeywords.Add(autocompleteInteraction.Data.Current.Name);
		return _autocompleteCommandMap.GetCommand(commandKeywords);
	}

	public async Task<IResult> ExecuteCommandAsync(IInteractionContext context, IServiceProvider services)
	{
		IDiscordInteraction interaction = context.Interaction;
		IResult result;
		if (!(interaction is ISlashCommandInteraction interaction2))
		{
			if (!(interaction is IComponentInteraction componentInteraction))
			{
				if (!(interaction is IUserCommandInteraction userCommandInteraction))
				{
					if (!(interaction is IMessageCommandInteraction messageCommandInteraction))
					{
						if (!(interaction is IAutocompleteInteraction interaction3))
						{
							if (!(interaction is IModalInteraction modalInteraction))
							{
								throw new InvalidOperationException($"{interaction.Type} interaction type cannot be executed by the Interaction service");
							}
							result = await ExecuteModalCommandAsync(context, modalInteraction.Data.CustomId, services).ConfigureAwait(continueOnCapturedContext: false);
						}
						else
						{
							result = await ExecuteAutocompleteAsync(context, interaction3, services).ConfigureAwait(continueOnCapturedContext: false);
						}
					}
					else
					{
						result = await ExecuteContextCommandAsync(context, messageCommandInteraction.Data.Name, ApplicationCommandType.Message, services).ConfigureAwait(continueOnCapturedContext: false);
					}
				}
				else
				{
					result = await ExecuteContextCommandAsync(context, userCommandInteraction.Data.Name, ApplicationCommandType.User, services).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
			else
			{
				result = await ExecuteComponentCommandAsync(context, componentInteraction.Data.CustomId, services).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		else
		{
			result = await ExecuteSlashCommandAsync(context, interaction2, services).ConfigureAwait(continueOnCapturedContext: false);
		}
		return result;
	}

	private async Task<IResult> ExecuteSlashCommandAsync(IInteractionContext context, ISlashCommandInteraction interaction, IServiceProvider services)
	{
		IList<string> commandKeywords = interaction.Data.GetCommandKeywords();
		SearchResult<SlashCommandInfo> result = _slashCommandMap.GetCommand(commandKeywords);
		if (!result.IsSuccess)
		{
			await _cmdLogger.DebugAsync("Unknown slash command, skipping execution (" + string.Join(" ", commandKeywords).ToUpper() + ")");
			await _slashCommandExecutedEvent.InvokeAsync(null, context, result).ConfigureAwait(continueOnCapturedContext: false);
			return result;
		}
		return await result.Command.ExecuteAsync(context, services).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<IResult> ExecuteContextCommandAsync(IInteractionContext context, string input, ApplicationCommandType commandType, IServiceProvider services)
	{
		if (!_contextCommandMaps.TryGetValue(commandType, out var value))
		{
			return SearchResult<ContextCommandInfo>.FromError(input, InteractionCommandError.UnknownCommand, $"No {commandType} command found.");
		}
		SearchResult<ContextCommandInfo> result = value.GetCommand(input);
		if (!result.IsSuccess)
		{
			await _cmdLogger.DebugAsync("Unknown context command, skipping execution (" + result.Text.ToUpper() + ")");
			await _contextCommandExecutedEvent.InvokeAsync(null, context, result).ConfigureAwait(continueOnCapturedContext: false);
			return result;
		}
		return await result.Command.ExecuteAsync(context, services).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<IResult> ExecuteComponentCommandAsync(IInteractionContext context, string input, IServiceProvider services)
	{
		SearchResult<ComponentCommandInfo> result = _componentCommandMap.GetCommand(input);
		if (!result.IsSuccess)
		{
			await _cmdLogger.DebugAsync("Unknown custom interaction id, skipping execution (" + input.ToUpper() + ")");
			await _componentCommandExecutedEvent.InvokeAsync(null, context, result).ConfigureAwait(continueOnCapturedContext: false);
			return result;
		}
		SetMatchesIfApplicable(context, result);
		return await result.Command.ExecuteAsync(context, services, result.RegexCaptureGroups).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<IResult> ExecuteAutocompleteAsync(IInteractionContext context, IAutocompleteInteraction interaction, IServiceProvider services)
	{
		IList<string> commandKeywords = interaction.Data.GetCommandKeywords();
		if (_enableAutocompleteHandlers)
		{
			SearchResult<SlashCommandInfo> command = _slashCommandMap.GetCommand(commandKeywords);
			if (command.IsSuccess && command.Command._flattenedParameterDictionary.TryGetValue(interaction.Data.Current.Name, out var value) && value?.AutocompleteHandler != null)
			{
				return await value.AutocompleteHandler.ExecuteAsync(context, interaction, value, services).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		commandKeywords.Add(interaction.Data.Current.Name);
		SearchResult<AutocompleteCommandInfo> commandResult = _autocompleteCommandMap.GetCommand(commandKeywords);
		if (!commandResult.IsSuccess)
		{
			await _cmdLogger.DebugAsync("Unknown command name, skipping autocomplete process (" + interaction.Data.CommandName.ToUpper() + ")");
			await _autocompleteCommandExecutedEvent.InvokeAsync(null, context, commandResult).ConfigureAwait(continueOnCapturedContext: false);
			return commandResult;
		}
		return await commandResult.Command.ExecuteAsync(context, services).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<IResult> ExecuteModalCommandAsync(IInteractionContext context, string input, IServiceProvider services)
	{
		SearchResult<ModalCommandInfo> result = _modalCommandMap.GetCommand(input);
		if (!result.IsSuccess)
		{
			await _cmdLogger.DebugAsync("Unknown custom interaction id, skipping execution (" + input.ToUpper() + ")");
			await _componentCommandExecutedEvent.InvokeAsync(null, context, result).ConfigureAwait(continueOnCapturedContext: false);
			return result;
		}
		SetMatchesIfApplicable(context, result);
		return await result.Command.ExecuteAsync(context, services, result.RegexCaptureGroups).ConfigureAwait(continueOnCapturedContext: false);
	}

	private static void SetMatchesIfApplicable<T>(IInteractionContext context, SearchResult<T> searchResult) where T : class, ICommandInfo
	{
		if (!searchResult.Command.SupportsWildCards || !(context is IRouteMatchContainer routeMatchContainer))
		{
			return;
		}
		string[] regexCaptureGroups = searchResult.RegexCaptureGroups;
		if (regexCaptureGroups != null && regexCaptureGroups.Length != 0)
		{
			RouteSegmentMatch[] array = new RouteSegmentMatch[searchResult.RegexCaptureGroups.Length];
			for (int i = 0; i < searchResult.RegexCaptureGroups.Length; i++)
			{
				array[i] = new RouteSegmentMatch(searchResult.RegexCaptureGroups[i]);
			}
			routeMatchContainer.SetSegmentMatches(array);
		}
		else
		{
			routeMatchContainer.SetSegmentMatches(Array.Empty<RouteSegmentMatch>());
		}
	}

	internal TypeConverter GetTypeConverter(Type type, IServiceProvider services = null)
	{
		return _typeConverterMap.Get(type, services);
	}

	public void AddTypeConverter<T>(TypeConverter converter)
	{
		_typeConverterMap.AddConcrete<T>(converter);
	}

	public void AddTypeConverter(Type type, TypeConverter converter)
	{
		_typeConverterMap.AddConcrete(type, converter);
	}

	public void AddGenericTypeConverter<T>(Type converterType)
	{
		_typeConverterMap.AddGeneric<T>(converterType);
	}

	public void AddGenericTypeConverter(Type targetType, Type converterType)
	{
		_typeConverterMap.AddGeneric(targetType, converterType);
	}

	internal ComponentTypeConverter GetComponentTypeConverter(Type type, IServiceProvider services = null)
	{
		return _compTypeConverterMap.Get(type, services);
	}

	public void AddComponentTypeConverter<T>(ComponentTypeConverter converter)
	{
		AddComponentTypeConverter(typeof(T), converter);
	}

	public void AddComponentTypeConverter(Type type, ComponentTypeConverter converter)
	{
		_compTypeConverterMap.AddConcrete(type, converter);
	}

	public void AddGenericComponentTypeConverter<T>(Type converterType)
	{
		AddGenericComponentTypeConverter(typeof(T), converterType);
	}

	public void AddGenericComponentTypeConverter(Type targetType, Type converterType)
	{
		_compTypeConverterMap.AddGeneric(targetType, converterType);
	}

	internal TypeReader GetTypeReader(Type type, IServiceProvider services = null)
	{
		return _typeReaderMap.Get(type, services);
	}

	public void AddTypeReader<T>(TypeReader reader)
	{
		AddTypeReader(typeof(T), reader);
	}

	public void AddTypeReader(Type type, TypeReader reader)
	{
		_typeReaderMap.AddConcrete(type, reader);
	}

	public void AddGenericTypeReader<T>(Type readerType)
	{
		AddGenericTypeReader(typeof(T), readerType);
	}

	public void AddGenericTypeReader(Type targetType, Type readerType)
	{
		_typeReaderMap.AddGeneric(targetType, readerType);
	}

	public bool TryRemoveTypeReader<T>(out TypeReader reader)
	{
		return TryRemoveTypeReader(typeof(T), out reader);
	}

	public bool TryRemoveTypeReader(Type type, out TypeReader reader)
	{
		return _typeReaderMap.TryRemoveConcrete(type, out reader);
	}

	public bool TryRemoveGenericTypeReader<T>(out Type readerType)
	{
		return TryRemoveGenericTypeReader(typeof(T), out readerType);
	}

	public bool TryRemoveGenericTypeReader(Type type, out Type readerType)
	{
		return _typeReaderMap.TryRemoveGeneric(type, out readerType);
	}

	public Task<string> SerializeValueAsync<T>(T obj, IServiceProvider services)
	{
		return _typeReaderMap.Get(typeof(T), services).SerializeAsync(obj, services);
	}

	public async Task<string> GenerateCustomIdStringAsync(string format, IServiceProvider services, params object[] args)
	{
		string[] serializedValues = new string[args.Length];
		for (int i = 0; i < args.Length; i++)
		{
			object obj = args[i];
			serializedValues[i] = await _typeReaderMap.Get(obj.GetType()).SerializeAsync(obj, services).ConfigureAwait(continueOnCapturedContext: false);
		}
		return string.Format(format, serializedValues);
	}

	public ModalInfo AddModalInfo<T>() where T : class, IModal
	{
		Type typeFromHandle = typeof(T);
		if (_modalInfos.ContainsKey(typeFromHandle))
		{
			throw new InvalidOperationException("Modal type " + typeFromHandle.FullName + " already exists.");
		}
		return ModalUtils.GetOrAdd(typeFromHandle, this);
	}

	internal IAutocompleteHandler GetAutocompleteHandler(Type autocompleteHandlerType, IServiceProvider services = null)
	{
		if (services == null)
		{
			services = EmptyServiceProvider.Instance;
		}
		if (!_enableAutocompleteHandlers)
		{
			throw new InvalidOperationException("IAutocompleteHandlers are not enabled. To use this feature set EnableAutocompleteHandlers to TRUE");
		}
		if (_autocompleteHandlers.TryGetValue(autocompleteHandlerType, out var value))
		{
			return value;
		}
		value = ReflectionUtils<IAutocompleteHandler>.CreateObject(autocompleteHandlerType.GetTypeInfo(), this, services);
		_autocompleteHandlers[autocompleteHandlerType] = value;
		return value;
	}

	public async Task<GuildApplicationCommandPermission> ModifySlashCommandPermissionsAsync(ModuleInfo module, IGuild guild, params ApplicationCommandPermission[] permissions)
	{
		if (module == null)
		{
			throw new ArgumentNullException("module");
		}
		if (guild == null)
		{
			throw new ArgumentNullException("guild");
		}
		return await ModifySlashCommandPermissionsAsync(module, guild.Id, permissions).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GuildApplicationCommandPermission> ModifySlashCommandPermissionsAsync(ModuleInfo module, ulong guildId, params ApplicationCommandPermission[] permissions)
	{
		if (module == null)
		{
			throw new ArgumentNullException("module");
		}
		if (!module.IsSlashGroup)
		{
			throw new InvalidOperationException("This module does not have a GroupAttribute and does not represent an Application Command");
		}
		if (!module.IsTopLevelGroup)
		{
			throw new InvalidOperationException("This module is not a top level application command. You cannot change its permissions");
		}
		return await (await RestClient.GetGuildApplicationCommands(guildId).ConfigureAwait(continueOnCapturedContext: false)).First((RestGuildCommand x) => x.Name == module.SlashGroupName).ModifyCommandPermissions(permissions).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GuildApplicationCommandPermission> ModifySlashCommandPermissionsAsync(SlashCommandInfo command, IGuild guild, params ApplicationCommandPermission[] permissions)
	{
		if (command == null)
		{
			throw new ArgumentNullException("command");
		}
		if (guild == null)
		{
			throw new ArgumentNullException("guild");
		}
		return await ModifyApplicationCommandPermissionsAsync(command, guild.Id, permissions).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GuildApplicationCommandPermission> ModifySlashCommandPermissionsAsync(SlashCommandInfo command, ulong guildId, params ApplicationCommandPermission[] permissions)
	{
		return await ModifyApplicationCommandPermissionsAsync(command, guildId, permissions).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GuildApplicationCommandPermission> ModifyContextCommandPermissionsAsync(ContextCommandInfo command, IGuild guild, params ApplicationCommandPermission[] permissions)
	{
		if (command == null)
		{
			throw new ArgumentNullException("command");
		}
		if (guild == null)
		{
			throw new ArgumentNullException("guild");
		}
		return await ModifyApplicationCommandPermissionsAsync(command, guild.Id, permissions).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GuildApplicationCommandPermission> ModifyContextCommandPermissionsAsync(ContextCommandInfo command, ulong guildId, params ApplicationCommandPermission[] permissions)
	{
		return await ModifyApplicationCommandPermissionsAsync(command, guildId, permissions).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<GuildApplicationCommandPermission> ModifyApplicationCommandPermissionsAsync<T>(T command, ulong guildId, params ApplicationCommandPermission[] permissions) where T : class, IApplicationCommandInfo, ICommandInfo
	{
		if (command == null)
		{
			throw new ArgumentNullException("command");
		}
		if (!command.IsTopLevelCommand)
		{
			throw new InvalidOperationException("This command is not a top level application command. You cannot change its permissions");
		}
		return await (await RestClient.GetGuildApplicationCommands(guildId).ConfigureAwait(continueOnCapturedContext: false)).First((RestGuildCommand x) => x.Name == ((IApplicationCommandInfo)command).Name).ModifyCommandPermissions(permissions).ConfigureAwait(continueOnCapturedContext: false);
	}

	public SlashCommandInfo GetSlashCommandInfo<TModule>(string methodName) where TModule : class
	{
		return GetModuleInfo<TModule>().SlashCommands.First((SlashCommandInfo x) => x.MethodName == methodName);
	}

	public ContextCommandInfo GetContextCommandInfo<TModule>(string methodName) where TModule : class
	{
		return GetModuleInfo<TModule>().ContextCommands.First((ContextCommandInfo x) => x.MethodName == methodName);
	}

	public ComponentCommandInfo GetComponentCommandInfo<TModule>(string methodName) where TModule : class
	{
		return GetModuleInfo<TModule>().ComponentCommands.First((ComponentCommandInfo x) => x.MethodName == methodName);
	}

	public ModuleInfo GetModuleInfo<TModule>() where TModule : class
	{
		if (!typeof(IInteractionModuleBase).IsAssignableFrom(typeof(TModule)))
		{
			throw new ArgumentException("Type parameter must be a type of Slash Module", "TModule");
		}
		return _typedModuleDefs[typeof(TModule)] ?? throw new InvalidOperationException(typeof(TModule).FullName + " is not loaded to the Slash Command Service");
	}

	public void Dispose()
	{
		_lock.Dispose();
	}

	private void EnsureClientReady()
	{
		if (RestClient?.CurrentUser != null)
		{
			DiscordRestClient restClient = RestClient;
			if (restClient == null || restClient.CurrentUser?.Id != 0)
			{
				return;
			}
		}
		throw new InvalidOperationException("Provided client is not ready to execute this operation, invoke this operation after a `Client Ready` event");
	}
}
