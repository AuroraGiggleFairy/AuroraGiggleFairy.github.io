using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Services.Analytics.Events;

public class HardwareInfoEventData : BaseEventData
{
	public override string EventType => "hardware_info";

	[JsonProperty(PropertyName = "os")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string OperatingSystem
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = SystemInfo.operatingSystem;

	[JsonProperty(PropertyName = "cpu_data")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, string> CpuData
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = new Dictionary<string, string>
	{
		{
			"type",
			SystemInfo.processorType
		},
		{
			"count",
			SystemInfo.processorCount.ToString()
		}
	};

	[JsonProperty(PropertyName = "gpu_data")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, string> GpuData
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = new Dictionary<string, string>
	{
		{
			"name",
			SystemInfo.graphicsDeviceName
		},
		{
			"type",
			SystemInfo.graphicsDeviceType.ToString()
		},
		{
			"version",
			SystemInfo.graphicsDeviceVersion
		},
		{
			"memory",
			SystemInfo.graphicsMemorySize.ToString()
		},
		{
			"vendor",
			SystemInfo.graphicsDeviceVendor
		}
	};

	[JsonProperty(PropertyName = "memory_ram")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int MemoryRam
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = SystemInfo.systemMemorySize;
}
