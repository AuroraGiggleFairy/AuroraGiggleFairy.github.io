using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

internal class ModalInfo
{
	internal readonly InteractionService _interactionService;

	internal readonly ModalInitializer _initializer;

	public string Title { get; }

	public Type Type { get; }

	public IReadOnlyCollection<InputComponentInfo> Components { get; }

	public IReadOnlyCollection<TextInputComponentInfo> TextComponents { get; }

	internal ModalInfo(Discord.Interactions.Builders.ModalBuilder builder)
	{
		Title = builder.Title;
		Type = builder.Type;
		Components = builder.Components.Select(delegate(IInputComponentBuilder x)
		{
			if (!(x is TextInputComponentBuilder textInputComponentBuilder))
			{
				throw new InvalidOperationException(x.GetType().FullName + " isn't a supported modal input component builder type.");
			}
			return textInputComponentBuilder.Build(this);
		}).ToImmutableArray();
		TextComponents = Components.OfType<TextInputComponentInfo>().ToImmutableArray();
		_interactionService = builder._interactionService;
		_initializer = builder.ModalInitializer;
	}

	[Obsolete("This method is no longer supported with the introduction of Component TypeConverters, please use the CreateModalAsync method.")]
	public IModal CreateModal(IModalInteraction modalInteraction, bool throwOnMissingField = false)
	{
		object[] array = new object[Components.Count];
		List<IComponentInteractionData> list = modalInteraction.Data.Components.ToList();
		for (int i = 0; i < Components.Count; i++)
		{
			InputComponentInfo input = Components.ElementAt(i);
			IComponentInteractionData componentInteractionData = list.Find((IComponentInteractionData x) => x.CustomId == input.CustomId);
			if (componentInteractionData == null)
			{
				if (throwOnMissingField)
				{
					throw new InvalidOperationException("Modal interaction is missing the required field: " + input.CustomId);
				}
				array[i] = input.DefaultValue;
			}
			else
			{
				array[i] = componentInteractionData.Value;
			}
		}
		return _initializer(array);
	}

	public async Task<IResult> CreateModalAsync(IInteractionContext context, IServiceProvider services = null, bool throwOnMissingField = false)
	{
		if (!(context.Interaction is IModalInteraction modalInteraction))
		{
			return ParseResult.FromError(InteractionCommandError.Unsuccessful, "Provided context doesn't belong to a Modal Interaction.");
		}
		if (services == null)
		{
			services = EmptyServiceProvider.Instance;
		}
		object[] args = new object[Components.Count];
		List<IComponentInteractionData> components = modalInteraction.Data.Components.ToList();
		for (int i = 0; i < Components.Count; i++)
		{
			InputComponentInfo input = Components.ElementAt(i);
			IComponentInteractionData componentInteractionData = components.Find((IComponentInteractionData x) => x.CustomId == input.CustomId);
			if (componentInteractionData == null)
			{
				if (!throwOnMissingField)
				{
					args[i] = input.DefaultValue;
					continue;
				}
				return ParseResult.FromError(InteractionCommandError.BadArgs, "Modal interaction is missing the required field: " + input.CustomId);
			}
			TypeConverterResult typeConverterResult = await input.TypeConverter.ReadAsync(context, componentInteractionData, services).ConfigureAwait(continueOnCapturedContext: false);
			if (!typeConverterResult.IsSuccess)
			{
				return typeConverterResult;
			}
			args[i] = typeConverterResult.Value;
		}
		return ParseResult.FromSuccess(_initializer(args));
	}
}
