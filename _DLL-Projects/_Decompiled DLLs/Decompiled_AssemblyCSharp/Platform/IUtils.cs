using InControl;

namespace Platform;

public interface IUtils
{
	void Init(IPlatform _owner);

	bool OpenBrowser(string _url);

	void ControllerDisconnected(InputDevice inputDevice);

	string GetPlatformLanguage();

	string GetAppLanguage();

	string GetCountry();

	void ClearTempFiles();

	string GetTempFileName(string prefix = "", string suffix = "");

	string GetCrossplayPlayerIcon(EPlayGroup _playGroup, bool _fetchGenericIcons, EPlatformIdentifier _nativePlatform = EPlatformIdentifier.None);
}
