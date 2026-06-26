using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace mumblelib;

[PublicizedFrom(EAccessModifier.Internal)]
public class UnixLinkFile : ILinkFile, IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct LinuxLinkMemory
	{
		public uint uiVersion;

		public uint uiTick;

		public unsafe fixed float fAvatarPosition[3];

		public unsafe fixed float fAvatarFront[3];

		public unsafe fixed float fAvatarTop[3];

		public unsafe fixed uint name[256];

		public unsafe fixed float fCameraPosition[3];

		public unsafe fixed float fCameraFront[3];

		public unsafe fixed float fCameraTop[3];

		public unsafe fixed uint identity[256];

		public uint context_len;

		public unsafe fixed byte context[256];

		public unsafe fixed uint description[2048];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int O_RDONLY = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int O_WRONLY = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int O_RDWR = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int O_CREAT = 64;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int O_EXCL = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int O_TRUNC = 512;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int PROT_READ = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int PROT_WRITE = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int PROT_EXEC = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int PROT_NONE = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MAP_SHARED = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MAP_PRIVATE = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MAP_SHARED_VALIDATE = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool disposed;

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe readonly LinuxLinkMemory* ptr;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int fd;

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
			Util.SetString(ptr->name, value, 256, Encoding.UTF32);
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
			Util.SetString(ptr->identity, value, 256, Encoding.UTF32);
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
			Util.SetString(ptr->description, value, 2048, Encoding.UTF32);
		}
	}

	[DllImport("libc")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static extern int shm_open([MarshalAs(UnmanagedType.LPStr)] string name, int oflag, uint mode);

	[DllImport("libc")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static extern uint getuid();

	[DllImport("libc")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static extern int ftruncate(int fd, long length);

	[DllImport("libc")]
	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe static extern void* mmap(void* addr, long length, int prot, int flags, int fd, long off);

	[DllImport("libc")]
	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe static extern void* munmap(void* addr, long length);

	[DllImport("libc")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static extern int close(int fd);

	public unsafe UnixLinkFile()
	{
		fd = shm_open("/MumbleLink." + getuid(), 66, 384u);
		if (fd < 0)
		{
			throw new Exception("[MumbleLF] Failed to open shm");
		}
		Log.Out("[MumbleLF] FD opened");
		int num = Marshal.SizeOf<LinuxLinkMemory>();
		if (ftruncate(fd, num) != 0)
		{
			Log.Error("[MumbleLF] Failed resizing shm");
			return;
		}
		Log.Out("[MumbleLF] Resized");
		ptr = (LinuxLinkMemory*)mmap(null, num, 3, 1, fd, 0L);
		Log.Out("[MumbleLF] MemMapped");
	}

	public unsafe void Tick()
	{
		ptr->uiTick++;
	}

	public unsafe void Dispose()
	{
		if (!disposed)
		{
			munmap(ptr, Marshal.SizeOf<LinuxLinkMemory>());
			close(fd);
			disposed = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~UnixLinkFile()
	{
		Dispose();
	}
}
