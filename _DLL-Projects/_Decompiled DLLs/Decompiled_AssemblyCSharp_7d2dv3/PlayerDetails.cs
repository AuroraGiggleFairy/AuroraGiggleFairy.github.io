using System;
using UnityEngine;

[Serializable]
public class PlayerDetails
{
	public string displayName;

	[HideInInspector]
	public string userEmail;

	public string userId;
}
