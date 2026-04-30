using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Discord.Interactions.Builders;

internal static class ModuleClassBuilder
{
	private static readonly TypeInfo ModuleTypeInfo = typeof(IInteractionModuleBase).GetTypeInfo();

	public const int MaxCommandDepth = 3;

	public static async Task<IEnumerable<TypeInfo>> SearchAsync(Assembly assembly, InteractionService commandService)
	{
		List<TypeInfo> result = new List<TypeInfo>();
		foreach (TypeInfo definedType in assembly.DefinedTypes)
		{
			if ((definedType.IsPublic || definedType.IsNestedPublic) && IsValidModuleDefinition(definedType))
			{
				result.Add(definedType);
			}
			else if (IsLoadableModule(definedType))
			{
				await commandService._cmdLogger.WarningAsync("Class " + definedType.FullName + " is not public and cannot be loaded.").ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		return result;
		static bool IsLoadableModule(TypeInfo info)
		{
			return info.DeclaredMethods.Any((MethodInfo x) => x.GetCustomAttribute<SlashCommandAttribute>() != null);
		}
	}

	public static async Task<Dictionary<Type, ModuleInfo>> BuildAsync(IEnumerable<TypeInfo> validTypes, InteractionService commandService, IServiceProvider services)
	{
		IEnumerable<TypeInfo> enumerable = validTypes.Where((TypeInfo x) => x.DeclaringType == null || !IsValidModuleDefinition(x.DeclaringType.GetTypeInfo()));
		List<TypeInfo> list = new List<TypeInfo>();
		Dictionary<Type, ModuleInfo> result = new Dictionary<Type, ModuleInfo>();
		foreach (TypeInfo item in enumerable)
		{
			ModuleBuilder moduleBuilder = new ModuleBuilder(commandService);
			BuildModule(moduleBuilder, item, commandService, services);
			BuildSubModules(moduleBuilder, item.DeclaredNestedTypes, list, commandService, services);
			list.Add(item);
			ModuleInfo value = moduleBuilder.Build(commandService, services);
			result.Add(item.AsType(), value);
		}
		await commandService._cmdLogger.DebugAsync($"Successfully built {list.Count} Slash Command modules.").ConfigureAwait(continueOnCapturedContext: false);
		return result;
	}

	private static void BuildModule(ModuleBuilder builder, TypeInfo typeInfo, InteractionService commandService, IServiceProvider services)
	{
		IEnumerable<Attribute> customAttributes = typeInfo.GetCustomAttributes();
		builder.Name = typeInfo.Name;
		builder.TypeInfo = typeInfo;
		foreach (Attribute item in customAttributes)
		{
			if (!(item is GroupAttribute groupAttribute))
			{
				if (!(item is DefaultPermissionAttribute defaultPermissionAttribute))
				{
					if (!(item is EnabledInDmAttribute enabledInDmAttribute))
					{
						if (!(item is DefaultMemberPermissionsAttribute defaultMemberPermissionsAttribute))
						{
							if (!(item is PreconditionAttribute preconditionAttribute))
							{
								if (item is DontAutoRegisterAttribute)
								{
									builder.DontAutoRegister = true;
									continue;
								}
								builder.AddAttributes(item);
							}
							else
							{
								builder.AddPreconditions(preconditionAttribute);
							}
						}
						else
						{
							builder.DefaultMemberPermissions = defaultMemberPermissionsAttribute.Permissions;
						}
					}
					else
					{
						builder.IsEnabledInDm = enabledInDmAttribute.IsEnabled;
					}
				}
				else
				{
					builder.DefaultPermission = defaultPermissionAttribute.IsDefaultPermission;
				}
			}
			else
			{
				builder.SlashGroupName = groupAttribute.Name;
				builder.Description = groupAttribute.Description;
			}
		}
		MethodInfo[] methods = typeInfo.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		IEnumerable<MethodInfo> enumerable = methods.Where(IsValidSlashCommandDefinition);
		IEnumerable<MethodInfo> enumerable2 = methods.Where(IsValidContextCommandDefinition);
		IEnumerable<MethodInfo> enumerable3 = methods.Where(IsValidComponentCommandDefinition);
		IEnumerable<MethodInfo> enumerable4 = methods.Where(IsValidAutocompleteCommandDefinition);
		IEnumerable<MethodInfo> enumerable5 = methods.Where(IsValidModalCommanDefinition);
		Func<IServiceProvider, IInteractionModuleBase> createInstance = (commandService._useCompiledLambda ? ReflectionUtils<IInteractionModuleBase>.CreateLambdaBuilder(typeInfo, commandService) : ReflectionUtils<IInteractionModuleBase>.CreateBuilder(typeInfo, commandService));
		foreach (MethodInfo method in enumerable)
		{
			builder.AddSlashCommand(delegate(SlashCommandBuilder x)
			{
				BuildSlashCommand(x, createInstance, method, commandService, services);
			});
		}
		foreach (MethodInfo method2 in enumerable2)
		{
			builder.AddContextCommand(delegate(ContextCommandBuilder x)
			{
				BuildContextCommand(x, createInstance, method2, commandService, services);
			});
		}
		foreach (MethodInfo method3 in enumerable3)
		{
			builder.AddComponentCommand(delegate(ComponentCommandBuilder x)
			{
				BuildComponentCommand(x, createInstance, method3, commandService, services);
			});
		}
		foreach (MethodInfo method4 in enumerable4)
		{
			builder.AddAutocompleteCommand(delegate(AutocompleteCommandBuilder x)
			{
				BuildAutocompleteCommand(x, createInstance, method4, commandService, services);
			});
		}
		foreach (MethodInfo method5 in enumerable5)
		{
			builder.AddModalCommand(delegate(ModalCommandBuilder x)
			{
				BuildModalCommand(x, createInstance, method5, commandService, services);
			});
		}
	}

	private static void BuildSubModules(ModuleBuilder parent, IEnumerable<TypeInfo> subModules, IList<TypeInfo> builtTypes, InteractionService commandService, IServiceProvider services, int slashGroupDepth = 0)
	{
		foreach (TypeInfo submodule in subModules.Where(IsValidModuleDefinition))
		{
			if (builtTypes.Contains(submodule))
			{
				continue;
			}
			parent.AddModule(delegate(ModuleBuilder builder)
			{
				BuildModule(builder, submodule, commandService, services);
				if (slashGroupDepth >= 2)
				{
					throw new InvalidOperationException($"Slash Commands only support {2} command prefixes for sub-commands");
				}
				BuildSubModules(builder, submodule.DeclaredNestedTypes, builtTypes, commandService, services, builder.IsSlashGroup ? (slashGroupDepth + 1) : slashGroupDepth);
			});
			builtTypes.Add(submodule);
		}
	}

	private static void BuildSlashCommand(SlashCommandBuilder builder, Func<IServiceProvider, IInteractionModuleBase> createInstance, MethodInfo methodInfo, InteractionService commandService, IServiceProvider services)
	{
		IEnumerable<Attribute> customAttributes = methodInfo.GetCustomAttributes();
		builder.MethodName = methodInfo.Name;
		foreach (Attribute item in customAttributes)
		{
			if (!(item is SlashCommandAttribute slashCommandAttribute))
			{
				if (!(item is DefaultPermissionAttribute defaultPermissionAttribute))
				{
					if (!(item is EnabledInDmAttribute enabledInDmAttribute))
					{
						if (!(item is DefaultMemberPermissionsAttribute defaultMemberPermissionsAttribute))
						{
							if (item is PreconditionAttribute preconditionAttribute)
							{
								builder.WithPreconditions(preconditionAttribute);
							}
							else
							{
								builder.WithAttributes(item);
							}
						}
						else
						{
							builder.DefaultMemberPermissions = defaultMemberPermissionsAttribute.Permissions;
						}
					}
					else
					{
						builder.IsEnabledInDm = enabledInDmAttribute.IsEnabled;
					}
				}
				else
				{
					builder.DefaultPermission = defaultPermissionAttribute.IsDefaultPermission;
				}
			}
			else
			{
				builder.Name = slashCommandAttribute.Name;
				builder.Description = slashCommandAttribute.Description;
				builder.IgnoreGroupNames = slashCommandAttribute.IgnoreGroupNames;
				builder.RunMode = slashCommandAttribute.RunMode;
			}
		}
		ParameterInfo[] parameters = methodInfo.GetParameters();
		foreach (ParameterInfo parameter in parameters)
		{
			builder.AddParameter(delegate(SlashCommandParameterBuilder x)
			{
				BuildSlashParameter(x, parameter, services);
			});
		}
		builder.Callback = CreateCallback(createInstance, methodInfo, commandService);
	}

	private static void BuildContextCommand(ContextCommandBuilder builder, Func<IServiceProvider, IInteractionModuleBase> createInstance, MethodInfo methodInfo, InteractionService commandService, IServiceProvider services)
	{
		IEnumerable<Attribute> customAttributes = methodInfo.GetCustomAttributes();
		builder.MethodName = methodInfo.Name;
		foreach (Attribute item in customAttributes)
		{
			if (!(item is ContextCommandAttribute contextCommandAttribute))
			{
				if (!(item is DefaultPermissionAttribute defaultPermissionAttribute))
				{
					if (!(item is EnabledInDmAttribute enabledInDmAttribute))
					{
						if (!(item is DefaultMemberPermissionsAttribute defaultMemberPermissionsAttribute))
						{
							if (item is PreconditionAttribute preconditionAttribute)
							{
								builder.WithPreconditions(preconditionAttribute);
							}
							else
							{
								builder.WithAttributes(item);
							}
						}
						else
						{
							builder.DefaultMemberPermissions = defaultMemberPermissionsAttribute.Permissions;
						}
					}
					else
					{
						builder.IsEnabledInDm = enabledInDmAttribute.IsEnabled;
					}
				}
				else
				{
					builder.DefaultPermission = defaultPermissionAttribute.IsDefaultPermission;
				}
			}
			else
			{
				builder.Name = contextCommandAttribute.Name;
				builder.CommandType = contextCommandAttribute.CommandType;
				builder.RunMode = contextCommandAttribute.RunMode;
				contextCommandAttribute.CheckMethodDefinition(methodInfo);
			}
		}
		ParameterInfo[] parameters = methodInfo.GetParameters();
		foreach (ParameterInfo parameter in parameters)
		{
			builder.AddParameter(delegate(CommandParameterBuilder x)
			{
				BuildParameter(x, parameter);
			});
		}
		builder.Callback = CreateCallback(createInstance, methodInfo, commandService);
	}

	private static void BuildComponentCommand(ComponentCommandBuilder builder, Func<IServiceProvider, IInteractionModuleBase> createInstance, MethodInfo methodInfo, InteractionService commandService, IServiceProvider services)
	{
		IEnumerable<Attribute> customAttributes = methodInfo.GetCustomAttributes();
		builder.MethodName = methodInfo.Name;
		foreach (Attribute item in customAttributes)
		{
			if (!(item is ComponentInteractionAttribute componentInteractionAttribute))
			{
				if (item is PreconditionAttribute preconditionAttribute)
				{
					builder.WithPreconditions(preconditionAttribute);
				}
				else
				{
					builder.WithAttributes(item);
				}
			}
			else
			{
				builder.Name = componentInteractionAttribute.CustomId;
				builder.RunMode = componentInteractionAttribute.RunMode;
				builder.IgnoreGroupNames = componentInteractionAttribute.IgnoreGroupNames;
			}
		}
		ParameterInfo[] parameters = methodInfo.GetParameters();
		int wildCardCount = Regex.Matches(Regex.Escape(builder.Name), Regex.Escape(commandService._wildCardExp)).Count;
		ParameterInfo[] array = parameters;
		foreach (ParameterInfo parameter in array)
		{
			builder.AddParameter(delegate(ComponentCommandParameterBuilder x)
			{
				BuildComponentParameter(x, parameter, parameter.Position >= wildCardCount);
			});
		}
		builder.Callback = CreateCallback(createInstance, methodInfo, commandService);
	}

	private static void BuildAutocompleteCommand(AutocompleteCommandBuilder builder, Func<IServiceProvider, IInteractionModuleBase> createInstance, MethodInfo methodInfo, InteractionService commandService, IServiceProvider services)
	{
		IEnumerable<Attribute> customAttributes = methodInfo.GetCustomAttributes();
		builder.MethodName = methodInfo.Name;
		foreach (Attribute item in customAttributes)
		{
			if (!(item is AutocompleteCommandAttribute autocompleteCommandAttribute))
			{
				if (item is PreconditionAttribute preconditionAttribute)
				{
					builder.WithPreconditions(preconditionAttribute);
				}
				else
				{
					builder.WithAttributes(item);
				}
			}
			else
			{
				builder.ParameterName = autocompleteCommandAttribute.ParameterName;
				builder.CommandName = autocompleteCommandAttribute.CommandName;
				builder.Name = autocompleteCommandAttribute.CommandName + " " + autocompleteCommandAttribute.ParameterName;
				builder.RunMode = autocompleteCommandAttribute.RunMode;
			}
		}
		ParameterInfo[] parameters = methodInfo.GetParameters();
		foreach (ParameterInfo parameter in parameters)
		{
			builder.AddParameter(delegate(CommandParameterBuilder x)
			{
				BuildParameter(x, parameter);
			});
		}
		builder.Callback = CreateCallback(createInstance, methodInfo, commandService);
	}

	private static void BuildModalCommand(ModalCommandBuilder builder, Func<IServiceProvider, IInteractionModuleBase> createInstance, MethodInfo methodInfo, InteractionService commandService, IServiceProvider services)
	{
		ParameterInfo[] parameters = methodInfo.GetParameters();
		if (parameters.Count((ParameterInfo x) => typeof(IModal).IsAssignableFrom(x.ParameterType)) > 1)
		{
			throw new InvalidOperationException("A modal command can only have one IModal parameter.");
		}
		if (!typeof(IModal).IsAssignableFrom(parameters.Last().ParameterType))
		{
			throw new InvalidOperationException("Last parameter of a modal command must be an implementation of IModal");
		}
		IEnumerable<Attribute> customAttributes = methodInfo.GetCustomAttributes();
		builder.MethodName = methodInfo.Name;
		foreach (Attribute item in customAttributes)
		{
			if (!(item is ModalInteractionAttribute modalInteractionAttribute))
			{
				if (item is PreconditionAttribute preconditionAttribute)
				{
					builder.WithPreconditions(preconditionAttribute);
				}
				else
				{
					builder.WithAttributes(item);
				}
			}
			else
			{
				builder.Name = modalInteractionAttribute.CustomId;
				builder.RunMode = modalInteractionAttribute.RunMode;
				builder.IgnoreGroupNames = modalInteractionAttribute.IgnoreGroupNames;
			}
		}
		ParameterInfo[] array = parameters;
		foreach (ParameterInfo parameter in array)
		{
			builder.AddParameter(delegate(ModalCommandParameterBuilder x)
			{
				BuildParameter(x, parameter);
			});
		}
		builder.Callback = CreateCallback(createInstance, methodInfo, commandService);
	}

	private static ExecuteCallback CreateCallback(Func<IServiceProvider, IInteractionModuleBase> createInstance, MethodInfo methodInfo, InteractionService commandService)
	{
		Func<IInteractionModuleBase, object[], Task> commandInvoker = (commandService._useCompiledLambda ? ReflectionUtils<IInteractionModuleBase>.CreateMethodInvoker(methodInfo) : ((Func<IInteractionModuleBase, object[], Task>)((IInteractionModuleBase module, object[] args) => methodInfo.Invoke(module, args) as Task)));
		return ExecuteCallback;
		async Task<IResult> ExecuteCallback(IInteractionContext context, object[] args, IServiceProvider serviceProvider, ICommandInfo commandInfo)
		{
			IInteractionModuleBase instance = createInstance(serviceProvider);
			instance.SetContext(context);
			IResult result;
			try
			{
				await instance.BeforeExecuteAsync(commandInfo).ConfigureAwait(continueOnCapturedContext: false);
				instance.BeforeExecute(commandInfo);
				Task task = commandInvoker(instance, args) ?? Task.Delay(0);
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
			catch (Exception exception)
			{
				await commandService._cmdLogger.ErrorAsync(exception).ConfigureAwait(continueOnCapturedContext: false);
				result = ExecuteResult.FromError(exception);
			}
			finally
			{
				await instance.AfterExecuteAsync(commandInfo).ConfigureAwait(continueOnCapturedContext: false);
				instance.AfterExecute(commandInfo);
				(instance as IDisposable)?.Dispose();
			}
			return result;
		}
	}

	private static void BuildSlashParameter(SlashCommandParameterBuilder builder, ParameterInfo paramInfo, IServiceProvider services)
	{
		IEnumerable<Attribute> customAttributes = paramInfo.GetCustomAttributes();
		Type parameterType = paramInfo.ParameterType;
		builder.Name = paramInfo.Name;
		builder.Description = paramInfo.Name;
		builder.IsRequired = !paramInfo.IsOptional;
		builder.DefaultValue = paramInfo.DefaultValue;
		foreach (Attribute item in customAttributes)
		{
			if (!(item is SummaryAttribute summaryAttribute))
			{
				if (!(item is ChoiceAttribute choiceAttribute))
				{
					if (!(item is ParamArrayAttribute))
					{
						if (!(item is ParameterPreconditionAttribute parameterPreconditionAttribute))
						{
							if (!(item is ChannelTypesAttribute channelTypesAttribute))
							{
								if (!(item is AutocompleteAttribute autocompleteAttribute))
								{
									if (!(item is MaxValueAttribute maxValueAttribute))
									{
										if (!(item is MinValueAttribute minValueAttribute))
										{
											if (!(item is MinLengthAttribute minLengthAttribute))
											{
												if (!(item is MaxLengthAttribute maxLengthAttribute))
												{
													if (item is ComplexParameterAttribute complexParameter)
													{
														builder.IsComplexParameter = true;
														ConstructorInfo complexParameterConstructor = GetComplexParameterConstructor(paramInfo.ParameterType.GetTypeInfo(), complexParameter);
														ParameterInfo[] parameters = complexParameterConstructor.GetParameters();
														foreach (ParameterInfo parameter in parameters)
														{
															if (parameter.IsDefined(typeof(ComplexParameterAttribute)))
															{
																throw new InvalidOperationException("You cannot create nested complex parameters.");
															}
															builder.AddComplexParameterField(delegate(SlashCommandParameterBuilder fieldBuilder)
															{
																BuildSlashParameter(fieldBuilder, parameter, services);
															});
														}
														Func<object[], object> initializer = (builder.Command.Module.InteractionService._useCompiledLambda ? ReflectionUtils<object>.CreateLambdaConstructorInvoker(paramInfo.ParameterType.GetTypeInfo()) : new Func<object[], object>(complexParameterConstructor.Invoke));
														builder.ComplexParameterInitializer = (object[] args) => initializer(args);
													}
													else
													{
														builder.AddAttributes(item);
													}
												}
												else
												{
													builder.MaxLength = maxLengthAttribute.Length;
												}
											}
											else
											{
												builder.MinLength = minLengthAttribute.Length;
											}
										}
										else
										{
											builder.MinValue = minValueAttribute.Value;
										}
									}
									else
									{
										builder.MaxValue = maxValueAttribute.Value;
									}
								}
								else
								{
									builder.Autocomplete = true;
									if ((object)autocompleteAttribute.AutocompleteHandlerType != null)
									{
										builder.WithAutocompleteHandler(autocompleteAttribute.AutocompleteHandlerType, services);
									}
								}
							}
							else
							{
								builder.WithChannelTypes(channelTypesAttribute.ChannelTypes);
							}
						}
						else
						{
							builder.AddPreconditions(parameterPreconditionAttribute);
						}
					}
					else
					{
						builder.IsParameterArray = true;
					}
				}
				else
				{
					builder.WithChoices(new ParameterChoice(choiceAttribute.Name, choiceAttribute.Value));
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(summaryAttribute.Name))
				{
					builder.Name = summaryAttribute.Name;
				}
				if (!string.IsNullOrEmpty(summaryAttribute.Description))
				{
					builder.Description = summaryAttribute.Description;
				}
			}
		}
		builder.SetParameterType(parameterType, services);
		builder.Name = Regex.Replace(builder.Name, "(?<=[a-z])(?=[A-Z])", "-").ToLower();
	}

	private static void BuildComponentParameter(ComponentCommandParameterBuilder builder, ParameterInfo paramInfo, bool isComponentParam)
	{
		builder.SetIsRouteSegment(!isComponentParam);
		BuildParameter(builder, paramInfo);
	}

	private static void BuildParameter<TInfo, TBuilder>(ParameterBuilder<TInfo, TBuilder> builder, ParameterInfo paramInfo) where TInfo : class, IParameterInfo where TBuilder : ParameterBuilder<TInfo, TBuilder>
	{
		IEnumerable<Attribute> customAttributes = paramInfo.GetCustomAttributes();
		Type parameterType = paramInfo.ParameterType;
		builder.Name = paramInfo.Name;
		builder.IsRequired = !paramInfo.IsOptional;
		builder.DefaultValue = paramInfo.DefaultValue;
		builder.SetParameterType(parameterType);
		foreach (Attribute item in customAttributes)
		{
			if (!(item is ParameterPreconditionAttribute parameterPreconditionAttribute))
			{
				if (item is ParamArrayAttribute)
				{
					builder.IsParameterArray = true;
					continue;
				}
				builder.AddAttributes(item);
			}
			else
			{
				builder.AddPreconditions(parameterPreconditionAttribute);
			}
		}
	}

	public static ModalInfo BuildModalInfo(Type modalType, InteractionService interactionService)
	{
		if (!typeof(IModal).IsAssignableFrom(modalType))
		{
			throw new InvalidOperationException(modalType.FullName + " isn't an implementation of " + typeof(IModal).FullName);
		}
		IModal instance = Activator.CreateInstance(modalType, nonPublic: false) as IModal;
		try
		{
			ModalBuilder modalBuilder = new ModalBuilder(modalType, interactionService)
			{
				Title = instance.Title
			};
			foreach (PropertyInfo prop in modalType.GetProperties().Where(IsValidModalInputDefinition))
			{
				ComponentType? componentType = prop.GetCustomAttribute<ModalInputAttribute>()?.ComponentType;
				if (componentType.HasValue)
				{
					if (componentType == ComponentType.TextInput)
					{
						modalBuilder.AddTextComponent(delegate(TextInputComponentBuilder x)
						{
							BuildTextInput(x, prop, prop.GetValue(instance));
						});
						continue;
					}
					throw new InvalidOperationException($"Component type {componentType} cannot be used in modals.");
				}
				throw new InvalidOperationException(prop.Name + " of " + prop.DeclaringType.Name + " isn't a valid modal input field.");
			}
			Func<object[], object[], IModal> memberInit = ReflectionUtils<IModal>.CreateLambdaMemberInit(modalType.GetTypeInfo(), modalType.GetConstructor(Type.EmptyTypes), (PropertyInfo x) => x.IsDefined(typeof(ModalInputAttribute)));
			modalBuilder.ModalInitializer = (object[] args) => memberInit(Array.Empty<object>(), args);
			return modalBuilder.Build();
		}
		finally
		{
			(instance as IDisposable)?.Dispose();
		}
	}

	private static void BuildTextInput(TextInputComponentBuilder builder, PropertyInfo propertyInfo, object defaultValue)
	{
		IEnumerable<Attribute> customAttributes = propertyInfo.GetCustomAttributes();
		builder.Label = propertyInfo.Name;
		builder.DefaultValue = defaultValue;
		builder.WithType(propertyInfo.PropertyType);
		foreach (Attribute item in customAttributes)
		{
			if (!(item is ModalTextInputAttribute modalTextInputAttribute))
			{
				if (!(item is RequiredInputAttribute requiredInputAttribute))
				{
					if (item is InputLabelAttribute inputLabelAttribute)
					{
						builder.Label = inputLabelAttribute.Label;
						continue;
					}
					builder.WithAttributes(item);
				}
				else
				{
					builder.IsRequired = requiredInputAttribute.IsRequired;
				}
			}
			else
			{
				builder.CustomId = modalTextInputAttribute.CustomId;
				builder.ComponentType = modalTextInputAttribute.ComponentType;
				builder.Style = modalTextInputAttribute.Style;
				builder.Placeholder = modalTextInputAttribute.Placeholder;
				builder.MaxLength = modalTextInputAttribute.MaxLength;
				builder.MinLength = modalTextInputAttribute.MinLength;
				builder.InitialValue = modalTextInputAttribute.InitialValue;
			}
		}
	}

	internal static bool IsValidModuleDefinition(TypeInfo typeInfo)
	{
		if (ModuleTypeInfo.IsAssignableFrom(typeInfo) && !typeInfo.IsAbstract)
		{
			return !typeInfo.ContainsGenericParameters;
		}
		return false;
	}

	private static bool IsValidSlashCommandDefinition(MethodInfo methodInfo)
	{
		if (methodInfo.IsDefined(typeof(SlashCommandAttribute)) && (methodInfo.ReturnType == typeof(Task) || methodInfo.ReturnType == typeof(Task<RuntimeResult>)) && !methodInfo.IsStatic)
		{
			return !methodInfo.IsGenericMethod;
		}
		return false;
	}

	private static bool IsValidContextCommandDefinition(MethodInfo methodInfo)
	{
		if (methodInfo.IsDefined(typeof(ContextCommandAttribute)) && (methodInfo.ReturnType == typeof(Task) || methodInfo.ReturnType == typeof(Task<RuntimeResult>)) && !methodInfo.IsStatic)
		{
			return !methodInfo.IsGenericMethod;
		}
		return false;
	}

	private static bool IsValidComponentCommandDefinition(MethodInfo methodInfo)
	{
		if (methodInfo.IsDefined(typeof(ComponentInteractionAttribute)) && (methodInfo.ReturnType == typeof(Task) || methodInfo.ReturnType == typeof(Task<RuntimeResult>)) && !methodInfo.IsStatic)
		{
			return !methodInfo.IsGenericMethod;
		}
		return false;
	}

	private static bool IsValidAutocompleteCommandDefinition(MethodInfo methodInfo)
	{
		if (methodInfo.IsDefined(typeof(AutocompleteCommandAttribute)) && (methodInfo.ReturnType == typeof(Task) || methodInfo.ReturnType == typeof(Task<RuntimeResult>)) && !methodInfo.IsStatic && !methodInfo.IsGenericMethod)
		{
			return methodInfo.GetParameters().Length == 0;
		}
		return false;
	}

	private static bool IsValidModalCommanDefinition(MethodInfo methodInfo)
	{
		if (methodInfo.IsDefined(typeof(ModalInteractionAttribute)) && (methodInfo.ReturnType == typeof(Task) || methodInfo.ReturnType == typeof(Task<RuntimeResult>)) && !methodInfo.IsStatic && !methodInfo.IsGenericMethod)
		{
			return typeof(IModal).IsAssignableFrom(methodInfo.GetParameters().Last().ParameterType);
		}
		return false;
	}

	private static bool IsValidModalInputDefinition(PropertyInfo propertyInfo)
	{
		MethodInfo setMethod = propertyInfo.SetMethod;
		if ((object)setMethod != null && setMethod.IsPublic)
		{
			MethodInfo setMethod2 = propertyInfo.SetMethod;
			if ((object)setMethod2 != null && !setMethod2.IsStatic)
			{
				return propertyInfo.IsDefined(typeof(ModalInputAttribute));
			}
		}
		return false;
	}

	private static ConstructorInfo GetComplexParameterConstructor(TypeInfo typeInfo, ComplexParameterAttribute complexParameter)
	{
		ConstructorInfo[] constructors = typeInfo.GetConstructors();
		if (constructors.Length == 0)
		{
			throw new InvalidOperationException("No constructor found for \"" + typeInfo.FullName + "\".");
		}
		if (complexParameter.PrioritizedCtorSignature != null)
		{
			return typeInfo.GetConstructor(complexParameter.PrioritizedCtorSignature) ?? throw new InvalidOperationException("No constructor was found with the signature: " + string.Join(",", complexParameter.PrioritizedCtorSignature.Select((Type x) => x.Name)));
		}
		IEnumerable<ConstructorInfo> source = constructors.Where((ConstructorInfo x) => x.IsDefined(typeof(ComplexParameterCtorAttribute), inherit: true));
		int num = source.Count();
		if (num <= 1)
		{
			if (num == 1)
			{
				return source.First();
			}
			if (constructors.Length > 1)
			{
				throw new InvalidOperationException("Multiple constructors found for \"" + typeInfo.FullName + "\".");
			}
			return constructors.First();
		}
		throw new InvalidOperationException("ComplexParameterCtorAttribute can only be used once in a type.");
	}
}
