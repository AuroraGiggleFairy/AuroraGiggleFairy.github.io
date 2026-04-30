using System;
using System.Text;
using Unity.Profiling;
using UnityEngine;

public class UnityMemoryProfilerLabel : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum SiSizeUnits
	{
		Byte,
		KiB,
		MiB,
		GiB,
		TiB,
		PiB,
		EiB,
		ZiB,
		YiB
	}

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public UILabel label;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerRecorder totalRecorder;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerRecorder totalReservedRecorder;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerRecorder systemRecorder;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerRecorder gcReservedMemoryRecorder;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerRecorder gcUsedMemoryRecorder;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerRecorder gcAllocInFrameMemoryRecorder;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerRecorder gfxUsedMemoryRecorder;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerRecorder mainThreadRecorder;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerRecorder meshBytesRecorder;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerRecorder meshCountRecorder;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerRecorder textureBytesRecorder;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerRecorder textureCountRecorder;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerRecorder renderTextureBytesRecorder;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerRecorder renderTextureCountRecorder;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerRecorder setPassCallsRecorder;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerRecorder drawCallsRecorder;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerRecorder verticesRecorder;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public StringBuilder sb = new StringBuilder(500);

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		if (label == null)
		{
			base.enabled = !base.gameObject.TryGetComponent<UILabel>(out label);
			if (!base.enabled)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		totalRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
		totalReservedRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");
		systemRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
		gcReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
		gcUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory");
		gcAllocInFrameMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
		gfxUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx Used Memory");
		mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
		meshBytesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Mesh Memory");
		meshCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Mesh Count");
		textureBytesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Used Textures Bytes");
		textureCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Used Textures Count");
		renderTextureBytesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Render Textures Bytes");
		renderTextureCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Render Textures Count");
		setPassCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
		drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
		verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		sb.Clear();
		sb.AppendLine("MEMORY");
		if (systemRecorder.Valid)
		{
			sb.AppendLine("System Used Memory " + ToSize(systemRecorder.LastValue, SiSizeUnits.GiB) + "GiB");
		}
		if (totalReservedRecorder.Valid)
		{
			sb.AppendLine("Total Reserved Memory " + ToSize(totalReservedRecorder.LastValue, SiSizeUnits.GiB) + "GiB");
		}
		if (totalRecorder.Valid)
		{
			sb.AppendLine("Total Used Memory " + ToSize(totalRecorder.LastValue, SiSizeUnits.GiB) + "GiB");
		}
		if (gcReservedMemoryRecorder.Valid)
		{
			sb.AppendLine("GC Reserved Memory " + ToSize(gcReservedMemoryRecorder.LastValue, SiSizeUnits.GiB) + "GiB");
		}
		if (gcUsedMemoryRecorder.Valid)
		{
			sb.AppendLine("GC Used Memory " + ToSize(gcUsedMemoryRecorder.LastValue, SiSizeUnits.GiB) + "GiB");
		}
		if (gcAllocInFrameMemoryRecorder.Valid)
		{
			sb.AppendLine("GC Allocated This Frame " + ToSize(gcAllocInFrameMemoryRecorder.LastValue, SiSizeUnits.MiB) + "MiB");
		}
		if (mainThreadRecorder.Valid)
		{
			sb.AppendLine("Main Thread Memory " + ToSize(mainThreadRecorder.LastValue, SiSizeUnits.MiB) + "MiB");
		}
		if (gfxUsedMemoryRecorder.Valid)
		{
			sb.AppendLine("GFX Used Memory " + ToSize(gfxUsedMemoryRecorder.LastValue, SiSizeUnits.GiB) + "GiB");
		}
		sb.AppendLine();
		sb.AppendLine("Rendering");
		if (meshCountRecorder.Valid)
		{
			sb.AppendLine($"Mesh Count {meshCountRecorder.LastValue}");
		}
		if (meshBytesRecorder.Valid)
		{
			if (meshBytesRecorder.LastValue > 1073741824)
			{
				sb.AppendLine("Mesh Memory " + ToSize(meshBytesRecorder.LastValue, SiSizeUnits.GiB) + "GiB");
			}
			else
			{
				sb.AppendLine("Mesh Memory " + ToSize(meshBytesRecorder.LastValue, SiSizeUnits.MiB) + "MiB");
			}
		}
		if (textureCountRecorder.Valid && textureCountRecorder.LastValue > 0)
		{
			sb.AppendLine($"Used Textures Count {textureCountRecorder.LastValue}");
		}
		if (textureBytesRecorder.Valid && textureBytesRecorder.LastValue > 0)
		{
			if (meshBytesRecorder.LastValue > 1073741824)
			{
				sb.AppendLine("Used Textures " + ToSize(textureBytesRecorder.LastValue, SiSizeUnits.GiB) + "GiB");
			}
			else
			{
				sb.AppendLine("Used Textures " + ToSize(textureBytesRecorder.LastValue, SiSizeUnits.MiB) + "MiB");
			}
		}
		if (renderTextureCountRecorder.Valid)
		{
			sb.AppendLine($"Render Textures {renderTextureCountRecorder.LastValue}");
		}
		if (renderTextureBytesRecorder.Valid)
		{
			sb.AppendLine("Render Textures " + ToSize(renderTextureBytesRecorder.LastValue, SiSizeUnits.MiB) + "MiB");
		}
		if (setPassCallsRecorder.Valid)
		{
			sb.AppendLine($"SetPass Calls: {setPassCallsRecorder.LastValue}");
		}
		if (drawCallsRecorder.Valid)
		{
			sb.AppendLine($"Draw Calls: {drawCallsRecorder.LastValue}");
		}
		if (verticesRecorder.Valid)
		{
			sb.AppendLine($"Vertices: {verticesRecorder.LastValue}");
		}
		label.text = sb.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		totalRecorder.Dispose();
		totalReservedRecorder.Dispose();
		systemRecorder.Dispose();
		gcReservedMemoryRecorder.Dispose();
		gcUsedMemoryRecorder.Dispose();
		gcAllocInFrameMemoryRecorder.Dispose();
		gfxUsedMemoryRecorder.Dispose();
		mainThreadRecorder.Dispose();
		textureBytesRecorder.Dispose();
		textureCountRecorder.Dispose();
		renderTextureBytesRecorder.Dispose();
		renderTextureCountRecorder.Dispose();
		setPassCallsRecorder.Dispose();
		drawCallsRecorder.Dispose();
		verticesRecorder.Dispose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string ToSize(long _value, SiSizeUnits _unit)
	{
		return ((double)_value / Math.Pow(1024.0, (double)_unit)).ToString("0.0000");
	}
}
