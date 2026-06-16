using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityAnimalRabbit : EntityAnimal
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly FastTags<TagGroup.Global> ChickenTag = FastTags<TagGroup.Global>.Parse("chicken");

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
		if ((bool)component)
		{
			component.center = new Vector3(0f, 0.15f, 0f);
			component.size = new Vector3(0.4f, 0.4f, 0.4f);
		}
		base.Awake();
		Transform transform = base.transform.Find("Graphics/BlobShadowProjector");
		if ((bool)transform)
		{
			transform.gameObject.SetActive(value: false);
		}
	}

	public override bool IsAttackValid()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitLocalActivationCommands(Action<EntityActivationCommand> _addCallback)
	{
		_addCallback(new EntityActivationCommand("grab", "hand"));
	}

	public override bool AllowActivationCommand(ReadOnlySpan<char> _commandName, EntityPlayerLocal _playerFocusing)
	{
		if (CommandIs(_commandName, "grab"))
		{
			return false;
		}
		return base.AllowActivationCommand(_commandName, _playerFocusing);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEntityActivated(EntityActivationCommand _command, EntityPlayerLocal _playerFocusing)
	{
		if (CommandIs(_command.commandId, "grab"))
		{
			ItemStack itemStack = new ItemStack(ItemClass.GetItem("wildChicken"), 1);
			_playerFocusing.inventory.SetItem(_playerFocusing.inventory.holdingItemIdx, itemStack);
			Collect(_playerFocusing.entityId);
		}
	}

	public override string GetActivationText()
	{
		return string.Format(Localization.Get("overlayChickenGrab"));
	}
}
