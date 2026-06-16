using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace mumblelib;

[PublicizedFrom(EAccessModifier.Internal)]
public class WindowsLinkFile : ILinkFile, IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct WindowsLinkMemory
	{
		public uint uiVersion;

		public uint uiTick;

		public unsafe fixed float fAvatarPosition[3];

		public unsafe fixed float fAvatarFront[3];

		public unsafe fixed float fAvatarTop[3];

		public unsafe fixed ushort name[256];

		public unsafe fixed float fCameraPosition[3];

		public unsafe fixed float fCameraFront[3];

		public unsafe fixed float fCameraTop[3];

		public unsafe fixed ushort identity[256];

		public uint context_len;

		public unsafe fixed byte context[256];

		public unsafe fixed ushort description[2048];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly MemoryMappedFile memoryMappedFile;

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe readonly WindowsLinkMemory* ptr;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool disposed;

	public unsafe uint UIVersion
	{
		get
		{
			return ptr->uiVersion;
		}
		set
		{
			ptr->uiVersion = value;
		}
	}

	public unsafe Vector3 AvatarPosition
	{
		set
		{
			Util.SetVector3(ptr->fAvatarPosition, value);
		}
	}

	public unsafe Vector3 AvatarForward
	{
		set
		{
			Util.SetVector3(ptr->fAvatarFront, value);
		}
	}

	public unsafe Vector3 AvatarTop
	{
		set
		{
			Util.SetVector3(ptr->fAvatarTop, value);
		}
	}

	public unsafe string Name
	{
		set
		{
			Util.SetString(ptr->name, value, 256, Encoding.Unicode);
		}
	}

	public unsafe Vector3 CameraPosition
	{
		set
		{
			Util.SetVector3(ptr->fCameraPosition, value);
		}
	}

	public unsafe Vector3 CameraForward
	{
		set
		{
			Util.SetVector3(ptr->fCameraFront, value);
		}
	}

	public unsafe Vector3 CameraTop
	{
		set
		{
			Util.SetVector3(ptr->fCameraTop, value);
		}
	}

	public unsafe string Identity
	{
		set
		{
			Util.SetString(ptr->identity, value, 256, Encoding.Unicode);
		}
	}

	public unsafe string Context
	{
		set
		{
			Util.SetContext(ptr->context, &ptr->context_len, value);
		}
	}

	public unsafe string Description
	{
		set
		{
			Util.SetString(ptr->description, value, 2048, Encoding.Unicode);
		}
	}

	public unsafe WindowsLinkFile()
	{
		memoryMappedFile = MemoryMappedFile.CreateOrOpen("MumbleLink", Marshal.SizeOf<WindowsLinkMemory>());
		byte* pointer = null;
		memoryMappedFile.CreateViewAccessor().SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
		ptr = (WindowsLinkMemory*)pointer;
	}

	public unsafe void Tick()
	{
		ptr->uiTick++;
	}

	public void Dispose()
	{
		Dispose(_disposing: true);
		GC.SuppressFinalize(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~WindowsLinkFile()
	{
		Dispose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Dispose(bool _disposing)
	{
		Log.Out("[MumbleLF] Disposing shm");
		if (!disposed)
		{
			if (_disposing)
			{
				memoryMappedFile.Dispose();
			}
			disposed = true;
		}
	}
}
