using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSystemInfo : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "SystemInfo" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "List SystemInfo";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Device Model                   :" + SystemInfo.deviceModel);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("deviceModel                    :" + SystemInfo.deviceModel);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("deviceName                     :" + SystemInfo.deviceName);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("deviceType                     :" + SystemInfo.deviceType.ToStringCached());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("deviceUniqueIdentifier         :" + SystemInfo.deviceUniqueIdentifier);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("graphicsDeviceID               :" + SystemInfo.graphicsDeviceID);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("graphicsDeviceName             :" + SystemInfo.graphicsDeviceName);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("graphicsDeviceType             :" + SystemInfo.graphicsDeviceType.ToStringCached());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("graphicsDeviceVendor           :" + SystemInfo.graphicsDeviceVendor);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("graphicsDeviceVendorID         :" + SystemInfo.graphicsDeviceVendorID);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("graphicsDeviceVersion          :" + SystemInfo.graphicsDeviceVersion);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("graphicsMemorySize             :" + SystemInfo.graphicsMemorySize);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("graphicsMultiThreaded          :" + SystemInfo.graphicsMultiThreaded);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("graphicsShaderLevel            :" + SystemInfo.graphicsShaderLevel);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("maxTextureSize                 :" + SystemInfo.maxTextureSize);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("npotSupport                    :" + SystemInfo.npotSupport.ToStringCached());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("operatingSystem                :" + SystemInfo.operatingSystem);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("processorCount                 :" + SystemInfo.processorCount);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("processorType                  :" + SystemInfo.processorType);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportedRenderTargetCount     :" + SystemInfo.supportedRenderTargetCount);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supports3DTextures             :" + SystemInfo.supports3DTextures);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsAccelerometer          :" + SystemInfo.supportsAccelerometer);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsComputeShaders         :" + SystemInfo.supportsComputeShaders);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsGyroscope              :" + SystemInfo.supportsGyroscope);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsImageEffects           : true (always)");
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsInstancing             :" + SystemInfo.supportsInstancing);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsLocationService        :" + SystemInfo.supportsLocationService);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsRenderToCubemap        : true (always)");
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsShadows                :" + SystemInfo.supportsShadows);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsSparseTextures         :" + SystemInfo.supportsSparseTextures);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsVibration              :" + SystemInfo.supportsVibration);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("systemMemorySize               :" + SystemInfo.systemMemorySize);
	}
}
