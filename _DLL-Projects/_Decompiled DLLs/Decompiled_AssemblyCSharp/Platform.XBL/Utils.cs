using Platform.Shared;

namespace Platform.XBL;

public class Utils : Platform.Shared.Utils
{
	public override string GetCrossplayPlayerIcon(EPlayGroup _playGroup, bool _fetchGenericIcons, EPlatformIdentifier _nativePlatform = EPlatformIdentifier.None)
	{
		switch (_playGroup)
		{
		case EPlayGroup.XBS:
			return "ui_platform_xbl";
		case EPlayGroup.Standalone:
			if (_nativePlatform == EPlatformIdentifier.XBL)
			{
				return "ui_platform_xbl";
			}
			if (_fetchGenericIcons)
			{
				return "ui_platform_pc";
			}
			break;
		case EPlayGroup.PS5:
			if (_fetchGenericIcons)
			{
				return "ui_platform_console";
			}
			break;
		}
		return string.Empty;
	}
}
