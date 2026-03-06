using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public interface IConsoleCommand
{
	bool IsExecuteOnClient { get; }

	int DefaultPermissionLevel { get; }

	bool AllowedInMainMenu { get; }

	DeviceFlag AllowedDeviceTypes { get; }

	DeviceFlag AllowedDeviceTypesClient { get; }

	bool CanExecuteForDevice
	{
		get
		{
			if (!Submission.Enabled)
			{
				return true;
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				return AllowedDeviceTypesClient.IsCurrent();
			}
			return AllowedDeviceTypes.IsCurrent();
		}
	}

	string PrimaryCommand { get; }

	string[] GetCommands();

	string GetDescription();

	string GetHelp();

	void Execute(List<string> _params, CommandSenderInfo _senderInfo);
}
