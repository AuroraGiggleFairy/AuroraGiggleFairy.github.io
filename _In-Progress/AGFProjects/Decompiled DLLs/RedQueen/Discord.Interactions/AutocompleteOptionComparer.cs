using System.Collections.Generic;

namespace Discord.Interactions;

internal class AutocompleteOptionComparer : IComparer<ApplicationCommandOptionType>
{
	public int Compare(ApplicationCommandOptionType x, ApplicationCommandOptionType y)
	{
		switch (x)
		{
		case ApplicationCommandOptionType.SubCommandGroup:
			if (y == ApplicationCommandOptionType.SubCommandGroup)
			{
				return 0;
			}
			return 1;
		case ApplicationCommandOptionType.SubCommand:
			return y switch
			{
				ApplicationCommandOptionType.SubCommandGroup => -1, 
				ApplicationCommandOptionType.SubCommand => 0, 
				_ => 1, 
			};
		default:
			if (y == ApplicationCommandOptionType.SubCommand || y == ApplicationCommandOptionType.SubCommandGroup)
			{
				return -1;
			}
			return 0;
		}
	}
}
