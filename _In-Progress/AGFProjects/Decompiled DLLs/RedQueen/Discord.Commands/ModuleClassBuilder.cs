using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands.Builders;

namespace Discord.Commands;

internal static class ModuleClassBuilder
{
	private static readonly TypeInfo ModuleTypeInfo = typeof(IModuleBase).GetTypeInfo();

	public static async Task<IReadOnlyList<TypeInfo>> SearchAsync(Assembly assembly, CommandService service)
	{
		List<TypeInfo> result = new List<TypeInfo>();
		foreach (TypeInfo definedType in assembly.DefinedTypes)
		{
			if (definedType.IsPublic || definedType.IsNestedPublic)
			{
				if (IsValidModuleDefinition(definedType) && !definedType.IsDefined(typeof(DontAutoLoadAttribute)))
				{
					result.Add(definedType);
				}
			}
			else if (IsLoadableModule(definedType))
			{
				await service._cmdLogger.WarningAsync("Class " + definedType.FullName + " is not public and cannot be loaded. To suppress this message, mark the class with DontAutoLoadAttribute.").ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		return result;
		static bool IsLoadableModule(TypeInfo info)
		{
			if (info.DeclaredMethods.Any((MethodInfo x) => x.GetCustomAttribute<CommandAttribute>() != null))
			{
				return info.GetCustomAttribute<DontAutoLoadAttribute>() == null;
			}
			return false;
		}
	}

	public static Task<Dictionary<Type, ModuleInfo>> BuildAsync(CommandService service, IServiceProvider services, params TypeInfo[] validTypes)
	{
		return BuildAsync(validTypes, service, services);
	}

	public static async Task<Dictionary<Type, ModuleInfo>> BuildAsync(IEnumerable<TypeInfo> validTypes, CommandService service, IServiceProvider services)
	{
		IEnumerable<TypeInfo> enumerable = validTypes.Where((TypeInfo x) => x.DeclaringType == null || !IsValidModuleDefinition(x.DeclaringType.GetTypeInfo()));
		List<TypeInfo> list = new List<TypeInfo>();
		Dictionary<Type, ModuleInfo> result = new Dictionary<Type, ModuleInfo>();
		foreach (TypeInfo item in enumerable)
		{
			if (!result.ContainsKey(item.AsType()))
			{
				ModuleBuilder moduleBuilder = new ModuleBuilder(service, null);
				BuildModule(moduleBuilder, item, service, services);
				BuildSubTypes(moduleBuilder, item.DeclaredNestedTypes, list, service, services);
				list.Add(item);
				result[item.AsType()] = moduleBuilder.Build(service, services);
			}
		}
		await service._cmdLogger.DebugAsync($"Successfully built {list.Count} modules.").ConfigureAwait(continueOnCapturedContext: false);
		return result;
	}

	private static void BuildSubTypes(ModuleBuilder builder, IEnumerable<TypeInfo> subTypes, List<TypeInfo> builtTypes, CommandService service, IServiceProvider services)
	{
		foreach (TypeInfo typeInfo in subTypes)
		{
			if (IsValidModuleDefinition(typeInfo) && !builtTypes.Contains(typeInfo))
			{
				builder.AddModule(delegate(ModuleBuilder module)
				{
					BuildModule(module, typeInfo, service, services);
					BuildSubTypes(module, typeInfo.DeclaredNestedTypes, builtTypes, service, services);
				});
				builtTypes.Add(typeInfo);
			}
		}
	}

	private static void BuildModule(ModuleBuilder builder, TypeInfo typeInfo, CommandService service, IServiceProvider services)
	{
		IEnumerable<Attribute> customAttributes = typeInfo.GetCustomAttributes();
		builder.TypeInfo = typeInfo;
		foreach (Attribute item in customAttributes)
		{
			if (!(item is NameAttribute nameAttribute))
			{
				if (!(item is SummaryAttribute summaryAttribute))
				{
					if (!(item is RemarksAttribute remarksAttribute))
					{
						if (!(item is AliasAttribute aliasAttribute))
						{
							if (!(item is GroupAttribute groupAttribute))
							{
								if (item is PreconditionAttribute precondition)
								{
									builder.AddPrecondition(precondition);
									continue;
								}
								builder.AddAttributes(item);
							}
							else
							{
								if (builder.Name == null)
								{
									string text = (builder.Name = groupAttribute.Prefix);
								}
								builder.Group = groupAttribute.Prefix;
							}
						}
						else
						{
							builder.AddAliases(aliasAttribute.Aliases);
						}
					}
					else
					{
						builder.Remarks = remarksAttribute.Text;
					}
				}
				else
				{
					builder.Summary = summaryAttribute.Text;
				}
			}
			else
			{
				builder.Name = nameAttribute.Text;
			}
		}
		if (builder.Aliases.Count == 0)
		{
			builder.AddAliases("");
		}
		if (builder.Name == null)
		{
			builder.Name = typeInfo.Name;
		}
		foreach (MethodInfo method in typeInfo.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(IsValidCommandDefinition))
		{
			builder.AddCommand(delegate(CommandBuilder command)
			{
				BuildCommand(command, typeInfo, method, service, services);
			});
		}
	}

	private static void BuildCommand(CommandBuilder builder, TypeInfo typeInfo, MethodInfo method, CommandService service, IServiceProvider serviceprovider)
	{
		foreach (Attribute customAttribute in method.GetCustomAttributes())
		{
			if (!(customAttribute is CommandAttribute commandAttribute))
			{
				if (!(customAttribute is NameAttribute nameAttribute))
				{
					if (!(customAttribute is PriorityAttribute priorityAttribute))
					{
						if (!(customAttribute is SummaryAttribute summaryAttribute))
						{
							if (!(customAttribute is RemarksAttribute remarksAttribute))
							{
								if (!(customAttribute is AliasAttribute aliasAttribute))
								{
									if (customAttribute is PreconditionAttribute precondition)
									{
										builder.AddPrecondition(precondition);
										continue;
									}
									builder.AddAttributes(customAttribute);
								}
								else
								{
									builder.AddAliases(aliasAttribute.Aliases);
								}
							}
							else
							{
								builder.Remarks = remarksAttribute.Text;
							}
						}
						else
						{
							builder.Summary = summaryAttribute.Text;
						}
					}
					else
					{
						builder.Priority = priorityAttribute.Priority;
					}
				}
				else
				{
					builder.Name = nameAttribute.Text;
				}
			}
			else
			{
				builder.AddAliases(commandAttribute.Text);
				builder.RunMode = commandAttribute.RunMode;
				if (builder.Name == null)
				{
					string text = (builder.Name = commandAttribute.Text);
				}
				builder.IgnoreExtraArgs = commandAttribute.IgnoreExtraArgs ?? service._ignoreExtraArgs;
			}
		}
		if (builder.Name == null)
		{
			builder.Name = method.Name;
		}
		System.Reflection.ParameterInfo[] parameters = method.GetParameters();
		int pos = 0;
		int count = parameters.Length;
		System.Reflection.ParameterInfo[] array = parameters;
		foreach (System.Reflection.ParameterInfo paramInfo in array)
		{
			builder.AddParameter(delegate(ParameterBuilder parameter)
			{
				System.Reflection.ParameterInfo paramInfo2 = paramInfo;
				int num = pos;
				pos = num + 1;
				BuildParameter(parameter, paramInfo2, num, count, service, serviceprovider);
			});
		}
		Func<IServiceProvider, IModuleBase> createInstance = ReflectionUtils.CreateBuilder<IModuleBase>(typeInfo, service);
		builder.Callback = ExecuteCallback;
		async Task<IResult> ExecuteCallback(ICommandContext context, object[] args, IServiceProvider services, CommandInfo cmd)
		{
			IModuleBase instance = createInstance(services);
			instance.SetContext(context);
			IResult result;
			try
			{
				await instance.BeforeExecuteAsync(cmd).ConfigureAwait(continueOnCapturedContext: false);
				instance.BeforeExecute(cmd);
				Task task = (method.Invoke(instance, args) as Task) ?? Task.Delay(0);
				if (task is Task<RuntimeResult> task2)
				{
					result = await task2.ConfigureAwait(continueOnCapturedContext: false);
				}
				else
				{
					await task.ConfigureAwait(continueOnCapturedContext: false);
					result = ExecuteResult.FromSuccess();
				}
			}
			finally
			{
				await instance.AfterExecuteAsync(cmd).ConfigureAwait(continueOnCapturedContext: false);
				instance.AfterExecute(cmd);
				(instance as IDisposable)?.Dispose();
			}
			return result;
		}
	}

	private static void BuildParameter(ParameterBuilder builder, System.Reflection.ParameterInfo paramInfo, int position, int count, CommandService service, IServiceProvider services)
	{
		IEnumerable<Attribute> customAttributes = paramInfo.GetCustomAttributes();
		Type type = paramInfo.ParameterType;
		builder.Name = paramInfo.Name;
		builder.IsOptional = paramInfo.IsOptional;
		builder.DefaultValue = (paramInfo.HasDefaultValue ? paramInfo.DefaultValue : null);
		foreach (Attribute item in customAttributes)
		{
			if (!(item is SummaryAttribute summaryAttribute))
			{
				if (!(item is OverrideTypeReaderAttribute overrideTypeReaderAttribute))
				{
					if (!(item is ParamArrayAttribute))
					{
						if (!(item is ParameterPreconditionAttribute precondition))
						{
							if (!(item is NameAttribute nameAttribute))
							{
								if (item is RemainderAttribute)
								{
									if (position != count - 1)
									{
										throw new InvalidOperationException("Remainder parameters must be the last parameter in a command. Parameter: " + paramInfo.Name + " in " + paramInfo.Member.DeclaringType.Name + "." + paramInfo.Member.Name);
									}
									builder.IsRemainder = true;
								}
								else
								{
									builder.AddAttributes(item);
								}
							}
							else
							{
								builder.Name = nameAttribute.Text;
							}
						}
						else
						{
							builder.AddPrecondition(precondition);
						}
					}
					else
					{
						builder.IsMultiple = true;
						type = type.GetElementType();
					}
				}
				else
				{
					builder.TypeReader = GetTypeReader(service, type, overrideTypeReaderAttribute.TypeReader, services);
				}
			}
			else
			{
				builder.Summary = summaryAttribute.Text;
			}
		}
		builder.ParameterType = type;
		if (builder.TypeReader == null)
		{
			builder.TypeReader = service.GetDefaultTypeReader(type) ?? service.GetTypeReaders(type)?.FirstOrDefault().Value;
		}
	}

	internal static TypeReader GetTypeReader(CommandService service, Type paramType, Type typeReaderType, IServiceProvider services)
	{
		IDictionary<Type, TypeReader> typeReaders = service.GetTypeReaders(paramType);
		TypeReader value = null;
		if (typeReaders != null && typeReaders.TryGetValue(typeReaderType, out value))
		{
			return value;
		}
		value = ReflectionUtils.CreateObject<TypeReader>(typeReaderType.GetTypeInfo(), service, services);
		service.AddTypeReader(paramType, value, replaceDefault: false);
		return value;
	}

	private static bool IsValidModuleDefinition(TypeInfo typeInfo)
	{
		if (ModuleTypeInfo.IsAssignableFrom(typeInfo) && !typeInfo.IsAbstract)
		{
			return !typeInfo.ContainsGenericParameters;
		}
		return false;
	}

	private static bool IsValidCommandDefinition(MethodInfo methodInfo)
	{
		if (methodInfo.IsDefined(typeof(CommandAttribute)) && (methodInfo.ReturnType == typeof(Task) || methodInfo.ReturnType == typeof(Task<RuntimeResult>)) && !methodInfo.IsStatic)
		{
			return !methodInfo.IsGenericMethod;
		}
		return false;
	}
}
