using System;
using UnityEngine;

namespace mumblelib;

public interface ILinkFile : IDisposable
{
	uint UIVersion { get; set; }

	Vector3 AvatarPosition { set; }

	Vector3 AvatarForward { set; }

	Vector3 AvatarTop { set; }

	string Name { set; }

	Vector3 CameraPosition { set; }

	Vector3 CameraForward { set; }

	Vector3 CameraTop { set; }

	string Identity { set; }

	string Context { set; }

	string Description { set; }

	void Tick();
}
