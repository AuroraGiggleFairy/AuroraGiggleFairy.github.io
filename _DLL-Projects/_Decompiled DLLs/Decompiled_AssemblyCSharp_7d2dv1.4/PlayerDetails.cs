using System;
using GameSparks.Core;
using UnityEngine;

[Serializable]
public class PlayerDetails
{
	public string displayName;

	[HideInInspector]
	public string userEmail;

	public string userId;

	public PlayerDetails(string _displayName, string _userId, GSData _responseData)
	{
		displayName = _displayName;
		userId = _userId;
	}
}
