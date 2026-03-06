using System.Collections.Generic;

namespace Twitch;

public class TwitchActionPreset
{
	public string Name;

	public bool IsEnabled = true;

	public bool IsDefault;

	public bool IsEmpty;

	public string Title;

	public string Description;

	public bool AllowPointGeneration = true;

	public bool UseHelperReward = true;

	public bool ShowNewCommands = true;

	public List<TwitchActionCooldownModifier> ActionCooldownModifiers;

	public List<string> AddedActions = new List<string>();

	public List<string> RemovedActions = new List<string>();

	public void AddCooldownModifier(TwitchActionCooldownModifier modifier)
	{
		if (ActionCooldownModifiers == null)
		{
			ActionCooldownModifiers = new List<TwitchActionCooldownModifier>();
		}
		ActionCooldownModifiers.Add(modifier);
	}

	public void HandleCooldowns()
	{
		foreach (TwitchAction value in TwitchActionManager.TwitchActions.Values)
		{
			value.Cooldown = value.OriginalCooldown;
			if (!value.IsInPreset(this) || ActionCooldownModifiers == null)
			{
				continue;
			}
			for (int i = 0; i < ActionCooldownModifiers.Count; i++)
			{
				TwitchActionCooldownModifier twitchActionCooldownModifier = ActionCooldownModifiers[i];
				if (twitchActionCooldownModifier.ActionName == value.Name || twitchActionCooldownModifier.CategoryName == value.MainCategory.Name)
				{
					switch (twitchActionCooldownModifier.Modifier)
					{
					case PassiveEffect.ValueModifierTypes.base_set:
						value.Cooldown = twitchActionCooldownModifier.Value;
						break;
					case PassiveEffect.ValueModifierTypes.perc_set:
						value.Cooldown *= twitchActionCooldownModifier.Value;
						break;
					case PassiveEffect.ValueModifierTypes.base_add:
						value.Cooldown += twitchActionCooldownModifier.Value;
						break;
					case PassiveEffect.ValueModifierTypes.perc_add:
						value.Cooldown += value.Cooldown * twitchActionCooldownModifier.Value;
						break;
					case PassiveEffect.ValueModifierTypes.base_subtract:
						value.Cooldown -= twitchActionCooldownModifier.Value;
						break;
					case PassiveEffect.ValueModifierTypes.perc_subtract:
						value.Cooldown -= value.Cooldown * twitchActionCooldownModifier.Value;
						break;
					}
				}
			}
		}
	}
}
