using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands.Builders;
using Discord.Logging;

namespace Discord.Commands;

internal class CommandService : IDisposable
{
	internal readonly AsyncEvent<Func<LogMessage, Task>> _logEvent = new AsyncEvent<Func<LogMessage, Task>>();

	internal readonly AsyncEvent<Func<Optional<CommandInfo>, ICommandContext, IResult, Task>> _commandExecutedEvent = new AsyncEvent<Func<Optional<CommandInfo>, ICommandContext, IResult, Task>>();

	private readonly SemaphoreSlim _moduleLock;

	private readonly ConcurrentDictionary<Type, ModuleInfo> _typedModuleDefs;

	private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, TypeReader>> _typeReaders;

	private readonly ConcurrentDictionary<Type, TypeReader> _defaultTypeReaders;

	private readonly ImmutableList<(Type EntityType, Type TypeReaderType)> _entityTypeReaders;

	private readonly HashSet<ModuleInfo> _moduleDefs;

	private readonly CommandMap _map;

	internal readonly bool _caseSensitive;

	internal readonly bool _throwOnError;

	internal readonly bool _ignoreExtraArgs;

	internal readonly char _separatorChar;

	internal readonly RunMode _defaultRunMode;

	internal readonly Logger _cmdLogger;

	internal readonly LogManager _logManager;

	internal readonly IReadOnlyDictionary<char, char> _quotationMarkAliasMap;

	internal bool _isDisposed;

	public IEnumerable<ModuleInfo> Modules => _moduleDefs.Select((ModuleInfo x) => x);

	public IEnumerable<CommandInfo> Commands => _moduleDefs.SelectMany((ModuleInfo x) => x.Commands);

	public ILookup<Type, TypeReader> TypeReaders => _typeReaders.SelectMany((KeyValuePair<Type, ConcurrentDictionary<Type, TypeReader>> x) => x.Value.Select((KeyValuePair<Type, TypeReader> y) => new { y.Key, y.Value })).ToLookup(x => x.Key, x => x.Value);

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

	public event Func<Optional<CommandInfo>, ICommandContext, IResult, Task> CommandExecuted
	{
		add
		{
			_commandExecutedEvent.Add(value);
		}
		remove
		{
			_commandExecutedEvent.Remove(value);
		}
	}

	public CommandService()
		: this(new CommandServiceConfig())
	{
	}

	public CommandService(CommandServiceConfig config)
	{
		_caseSensitive = config.CaseSensitiveCommands;
		_throwOnError = config.ThrowOnError;
		_ignoreExtraArgs = config.IgnoreExtraArgs;
		_separatorChar = config.SeparatorChar;
		_defaultRunMode = config.DefaultRunMode;
		_quotationMarkAliasMap = (config.QuotationMarkAliasMap ?? new Dictionary<char, char>()).ToImmutableDictionary();
		if (_defaultRunMode == RunMode.Default)
		{
			throw new InvalidOperationException("The default run mode cannot be set to Default.");
		}
		_logManager = new LogManager(config.LogLevel);
		_logManager.Message += async delegate(LogMessage msg)
		{
			await _logEvent.InvokeAsync(msg).ConfigureAwait(continueOnCapturedContext: false);
		};
		_cmdLogger = _logManager.CreateLogger("Command");
		_moduleLock = new SemaphoreSlim(1, 1);
		_typedModuleDefs = new ConcurrentDictionary<Type, ModuleInfo>();
		_moduleDefs = new HashSet<ModuleInfo>();
		_map = new CommandMap(this);
		_typeReaders = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, TypeReader>>();
		_defaultTypeReaders = new ConcurrentDictionary<Type, TypeReader>();
		foreach (Type supportedType in PrimitiveParsers.SupportedTypes)
		{
			_defaultTypeReaders[supportedType] = PrimitiveTypeReader.Create(supportedType);
			_defaultTypeReaders[typeof(Nullable<>).MakeGenericType(supportedType)] = NullableTypeReader.Create(supportedType, _defaultTypeReaders[supportedType]);
		}
		TimeSpanTypeReader timeSpanTypeReader = new TimeSpanTypeReader();
		_defaultTypeReaders[typeof(TimeSpan)] = timeSpanTypeReader;
		_defaultTypeReaders[typeof(TimeSpan?)] = NullableTypeReader.Create(typeof(TimeSpan), timeSpanTypeReader);
		_defaultTypeReaders[typeof(string)] = new PrimitiveTypeReader<string>(delegate(string x, out string y)
		{
			y = x;
			return true;
		}, 0f);
		ImmutableList<(Type, Type)>.Builder builder = ImmutableList.CreateBuilder<(Type, Type)>();
		builder.Add((typeof(IMessage), typeof(MessageTypeReader<>)));
		builder.Add((typeof(IChannel), typeof(ChannelTypeReader<>)));
		builder.Add((typeof(IRole), typeof(RoleTypeReader<>)));
		builder.Add((typeof(IUser), typeof(UserTypeReader<>)));
		_entityTypeReaders = builder.ToImmutable();
	}

	public async Task<ModuleInfo> CreateModuleAsync(string primaryAlias, Action<ModuleBuilder> buildFunc)
	{
		await _moduleLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			ModuleBuilder moduleBuilder = new ModuleBuilder(this, null, primaryAlias);
			buildFunc(moduleBuilder);
			ModuleInfo module = moduleBuilder.Build(this, null);
			return LoadModuleInternal(module);
		}
		finally
		{
			_moduleLock.Release();
		}
	}

	public Task<ModuleInfo> AddModuleAsync<T>(IServiceProvider services)
	{
		return AddModuleAsync(typeof(T), services);
	}

	public async Task<ModuleInfo> AddModuleAsync(Type type, IServiceProvider services)
	{
		if (services == null)
		{
			services = EmptyServiceProvider.Instance;
		}
		await _moduleLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			TypeInfo typeInfo = type.GetTypeInfo();
			if (_typedModuleDefs.ContainsKey(type))
			{
				throw new ArgumentException("This module has already been added.");
			}
			KeyValuePair<Type, ModuleInfo> keyValuePair = (await ModuleClassBuilder.BuildAsync(this, services, typeInfo).ConfigureAwait(continueOnCapturedContext: false)).FirstOrDefault();
			if (keyValuePair.Value == null)
			{
				throw new InvalidOperationException("Could not build the module " + type.FullName + ", did you pass an invalid type?");
			}
			_typedModuleDefs[keyValuePair.Key] = keyValuePair.Value;
			return LoadModuleInternal(keyValuePair.Value);
		}
		finally
		{
			_moduleLock.Release();
		}
	}

	public async Task<IEnumerable<ModuleInfo>> AddModulesAsync(Assembly assembly, IServiceProvider services)
	{
		if (services == null)
		{
			services = EmptyServiceProvider.Instance;
		}
		await _moduleLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			Dictionary<Type, ModuleInfo> dictionary = await ModuleClassBuilder.BuildAsync(await ModuleClassBuilder.SearchAsync(assembly, this).ConfigureAwait(continueOnCapturedContext: false), this, services).ConfigureAwait(continueOnCapturedContext: false);
			foreach (KeyValuePair<Type, ModuleInfo> item in dictionary)
			{
				_typedModuleDefs[item.Key] = item.Value;
				LoadModuleInternal(item.Value);
			}
			return dictionary.Select((KeyValuePair<Type, ModuleInfo> x) => x.Value).ToImmutableArray();
		}
		finally
		{
			_moduleLock.Release();
		}
	}

	private ModuleInfo LoadModuleInternal(ModuleInfo module)
	{
		_moduleDefs.Add(module);
		foreach (CommandInfo command in module.Commands)
		{
			_map.AddCommand(command);
		}
		foreach (ModuleInfo submodule in module.Submodules)
		{
			LoadModuleInternal(submodule);
		}
		return module;
	}

	public async Task<bool> RemoveModuleAsync(ModuleInfo module)
	{
		await _moduleLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
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
			_moduleLock.Release();
		}
	}

	public Task<bool> RemoveModuleAsync<T>()
	{
		return RemoveModuleAsync(typeof(T));
	}

	public async Task<bool> RemoveModuleAsync(Type type)
	{
		await _moduleLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
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
			_moduleLock.Release();
		}
	}

	private bool RemoveModuleInternal(ModuleInfo module)
	{
		if (!_moduleDefs.Remove(module))
		{
			return false;
		}
		foreach (CommandInfo command in module.Commands)
		{
			_map.RemoveCommand(command);
		}
		foreach (ModuleInfo submodule in module.Submodules)
		{
			RemoveModuleInternal(submodule);
		}
		return true;
	}

	public void AddTypeReader<T>(TypeReader reader)
	{
		AddTypeReader(typeof(T), reader);
	}

	public void AddTypeReader(Type type, TypeReader reader)
	{
		if (_defaultTypeReaders.ContainsKey(type))
		{
			_cmdLogger.WarningAsync("The default TypeReader for " + type.FullName + " was replaced by " + reader.GetType().FullName + ".To suppress this message, use AddTypeReader<T>(reader, true).");
		}
		AddTypeReader(type, reader, replaceDefault: true);
	}

	public void AddTypeReader<T>(TypeReader reader, bool replaceDefault)
	{
		AddTypeReader(typeof(T), reader, replaceDefault);
	}

	public void AddTypeReader(Type type, TypeReader reader, bool replaceDefault)
	{
		if (replaceDefault && HasDefaultTypeReader(type))
		{
			_defaultTypeReaders.AddOrUpdate(type, reader, (Type k, TypeReader v) => reader);
			if (type.GetTypeInfo().IsValueType)
			{
				Type key = typeof(Nullable<>).MakeGenericType(type);
				TypeReader nullableReader = NullableTypeReader.Create(type, reader);
				_defaultTypeReaders.AddOrUpdate(key, nullableReader, (Type k, TypeReader v) => nullableReader);
			}
		}
		else
		{
			_typeReaders.GetOrAdd(type, (Type x) => new ConcurrentDictionary<Type, TypeReader>())[reader.GetType()] = reader;
			if (type.GetTypeInfo().IsValueType)
			{
				AddNullableTypeReader(type, reader);
			}
		}
	}

	public bool TryRemoveTypeReader(Type type, bool isDefaultTypeReader, out IDictionary<Type, TypeReader> readers)
	{
		readers = new Dictionary<Type, TypeReader>();
		if (isDefaultTypeReader)
		{
			TypeReader value;
			bool num = _defaultTypeReaders.TryRemove(type, out value);
			if (num)
			{
				readers.Add(value?.GetType(), value);
			}
			return num;
		}
		ConcurrentDictionary<Type, TypeReader> value2;
		bool num2 = _typeReaders.TryRemove(type, out value2);
		if (num2)
		{
			readers = value2;
		}
		return num2;
	}

	internal bool HasDefaultTypeReader(Type type)
	{
		if (_defaultTypeReaders.ContainsKey(type))
		{
			return true;
		}
		TypeInfo typeInfo = type.GetTypeInfo();
		if (typeInfo.IsEnum)
		{
			return true;
		}
		return _entityTypeReaders.Any(((Type EntityType, Type TypeReaderType) x) => type == x.EntityType || typeInfo.ImplementedInterfaces.Contains(x.EntityType));
	}

	internal void AddNullableTypeReader(Type valueType, TypeReader valueTypeReader)
	{
		ConcurrentDictionary<Type, TypeReader> orAdd = _typeReaders.GetOrAdd(typeof(Nullable<>).MakeGenericType(valueType), (Type x) => new ConcurrentDictionary<Type, TypeReader>());
		TypeReader typeReader = NullableTypeReader.Create(valueType, valueTypeReader);
		orAdd[typeReader.GetType()] = typeReader;
	}

	internal IDictionary<Type, TypeReader> GetTypeReaders(Type type)
	{
		if (_typeReaders.TryGetValue(type, out var value))
		{
			return value;
		}
		return null;
	}

	internal TypeReader GetDefaultTypeReader(Type type)
	{
		if (_defaultTypeReaders.TryGetValue(type, out var value))
		{
			return value;
		}
		TypeInfo typeInfo = type.GetTypeInfo();
		if (typeInfo.IsEnum)
		{
			value = EnumTypeReader.GetReader(type);
			_defaultTypeReaders[type] = value;
			return value;
		}
		Type underlyingType = Nullable.GetUnderlyingType(type);
		if (underlyingType != null && underlyingType.IsEnum)
		{
			value = NullableTypeReader.Create(underlyingType, EnumTypeReader.GetReader(underlyingType));
			_defaultTypeReaders[type] = value;
			return value;
		}
		for (int i = 0; i < _entityTypeReaders.Count; i++)
		{
			if (type == _entityTypeReaders[i].EntityType || typeInfo.ImplementedInterfaces.Contains(_entityTypeReaders[i].EntityType))
			{
				value = Activator.CreateInstance(_entityTypeReaders[i].TypeReaderType.MakeGenericType(type)) as TypeReader;
				_defaultTypeReaders[type] = value;
				return value;
			}
		}
		return null;
	}

	public SearchResult Search(ICommandContext context, int argPos)
	{
		return Search(context.Message.Content.Substring(argPos));
	}

	public SearchResult Search(ICommandContext context, string input)
	{
		return Search(input);
	}

	public SearchResult Search(string input)
	{
		string text = (_caseSensitive ? input : input.ToLowerInvariant());
		System.Collections.Immutable.ImmutableArray<CommandMatch> immutableArray = (from x in _map.GetCommands(text)
			orderby x.Command.Priority descending
			select x).ToImmutableArray();
		if (immutableArray.Length > 0)
		{
			return SearchResult.FromSuccess(input, immutableArray);
		}
		return SearchResult.FromError(CommandError.UnknownCommand, "Unknown command.");
	}

	public Task<IResult> ExecuteAsync(ICommandContext context, int argPos, IServiceProvider services, MultiMatchHandling multiMatchHandling = MultiMatchHandling.Exception)
	{
		return ExecuteAsync(context, context.Message.Content.Substring(argPos), services, multiMatchHandling);
	}

	public async Task<IResult> ExecuteAsync(ICommandContext context, string input, IServiceProvider services, MultiMatchHandling multiMatchHandling = MultiMatchHandling.Exception)
	{
		if (services == null)
		{
			services = EmptyServiceProvider.Instance;
		}
		SearchResult matches = Search(input);
		IResult result = await ValidateAndGetBestMatch(matches, context, services, multiMatchHandling);
		if (result is SearchResult result2)
		{
			await _commandExecutedEvent.InvokeAsync(Optional.Create<CommandInfo>(), context, result2).ConfigureAwait(continueOnCapturedContext: false);
			return result2;
		}
		if (result is MatchResult matchResult)
		{
			return await HandleCommandPipeline(matchResult, context, services);
		}
		return result;
	}

	private async Task<IResult> HandleCommandPipeline(MatchResult matchResult, ICommandContext context, IServiceProvider services)
	{
		if (!matchResult.IsSuccess)
		{
			return matchResult;
		}
		if (matchResult.Pipeline is ParseResult parseResult)
		{
			if (!parseResult.IsSuccess)
			{
				await _commandExecutedEvent.InvokeAsync(matchResult.Match.Value.Command, context, parseResult);
				return parseResult;
			}
			IResult executeResult = await matchResult.Match.Value.ExecuteAsync(context, parseResult, services);
			if (!executeResult.IsSuccess && !(executeResult is RuntimeResult) && !(executeResult is ExecuteResult))
			{
				await _commandExecutedEvent.InvokeAsync(matchResult.Match.Value.Command, context, executeResult);
			}
			return executeResult;
		}
		IResult pipeline = matchResult.Pipeline;
		if (pipeline is PreconditionResult preconditionResult)
		{
			await _commandExecutedEvent.InvokeAsync(matchResult.Match.Value.Command, context, preconditionResult).ConfigureAwait(continueOnCapturedContext: false);
			return preconditionResult;
		}
		return matchResult;
	}

	private float CalculateScore(CommandMatch match, ParseResult parseResult)
	{
		float num = 0f;
		float num2 = 0f;
		if (match.Command.Parameters.Count > 0)
		{
			float num3 = parseResult.ArgValues?.Sum((TypeReaderResult x) => x.Values.OrderByDescending((TypeReaderValue y) => y.Score).FirstOrDefault().Score) ?? 0f;
			float num4 = parseResult.ParamValues?.Sum((TypeReaderResult x) => x.Values.OrderByDescending((TypeReaderValue y) => y.Score).FirstOrDefault().Score) ?? 0f;
			num = num3 / (float)match.Command.Parameters.Count;
			num2 = num4 / (float)match.Command.Parameters.Count;
		}
		float num5 = (num + num2) / 2f;
		return (float)match.Command.Priority + num5 * 0.99f;
	}

	public async Task<IResult> ValidateAndGetBestMatch(SearchResult matches, ICommandContext context, IServiceProvider provider, MultiMatchHandling multiMatchHandling = MultiMatchHandling.Exception)
	{
		if (!matches.IsSuccess)
		{
			return matches;
		}
		IReadOnlyList<CommandMatch> commands = matches.Commands;
		Dictionary<CommandMatch, PreconditionResult> preconditionResults = new Dictionary<CommandMatch, PreconditionResult>();
		foreach (CommandMatch item in commands)
		{
			Dictionary<CommandMatch, PreconditionResult> dictionary = preconditionResults;
			CommandMatch key = item;
			dictionary[key] = await item.CheckPreconditionsAsync(context, provider);
		}
		KeyValuePair<CommandMatch, PreconditionResult>[] array = preconditionResults.Where((KeyValuePair<CommandMatch, PreconditionResult> x) => x.Value.IsSuccess).ToArray();
		if (array.Length == 0)
		{
			KeyValuePair<CommandMatch, PreconditionResult> keyValuePair = preconditionResults.OrderByDescending((KeyValuePair<CommandMatch, PreconditionResult> x) => x.Key.Command.Priority).FirstOrDefault((KeyValuePair<CommandMatch, PreconditionResult> x) => !x.Value.IsSuccess);
			return MatchResult.FromSuccess(keyValuePair.Key, keyValuePair.Value);
		}
		Dictionary<CommandMatch, ParseResult> parseResults = new Dictionary<CommandMatch, ParseResult>();
		KeyValuePair<CommandMatch, PreconditionResult>[] array2 = array;
		for (int num = 0; num < array2.Length; num++)
		{
			KeyValuePair<CommandMatch, PreconditionResult> pair = array2[num];
			ParseResult value = await pair.Key.ParseAsync(context, matches, pair.Value, provider).ConfigureAwait(continueOnCapturedContext: false);
			if (value.Error == CommandError.MultipleMatches && multiMatchHandling == MultiMatchHandling.Best)
			{
				object argValues = value.ArgValues.Select((TypeReaderResult x) => x.Values.OrderByDescending((TypeReaderValue y) => y.Score).First()).ToImmutableArray();
				IReadOnlyList<TypeReaderValue> paramValues = value.ParamValues.Select((TypeReaderResult x) => x.Values.OrderByDescending((TypeReaderValue y) => y.Score).First()).ToImmutableArray();
				value = ParseResult.FromSuccess((IReadOnlyList<TypeReaderValue>)argValues, paramValues);
			}
			parseResults[pair.Key] = value;
		}
		KeyValuePair<CommandMatch, ParseResult>[] array3 = (from x in parseResults
			orderby CalculateScore(x.Key, x.Value) descending
			where x.Value.IsSuccess
			select x).ToArray();
		if (array3.Length == 0)
		{
			KeyValuePair<CommandMatch, ParseResult> keyValuePair2 = parseResults.FirstOrDefault((KeyValuePair<CommandMatch, ParseResult> x) => !x.Value.IsSuccess);
			return MatchResult.FromSuccess(keyValuePair2.Key, keyValuePair2.Value);
		}
		KeyValuePair<CommandMatch, ParseResult> keyValuePair3 = array3[0];
		return MatchResult.FromSuccess(keyValuePair3.Key, keyValuePair3.Value);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				_moduleLock?.Dispose();
			}
			_isDisposed = true;
		}
	}

	void IDisposable.Dispose()
	{
		Dispose(disposing: true);
	}
}
