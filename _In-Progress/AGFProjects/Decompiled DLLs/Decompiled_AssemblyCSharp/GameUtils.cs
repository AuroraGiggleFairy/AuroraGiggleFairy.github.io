using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Epic.OnlineServices.AntiCheatCommon;
using GamePath;
using Platform;
using UnityEngine;
using UnityEngine.Rendering;

public class GameUtils
{
	public class WorldInfo
	{
		public readonly bool Valid;

		public readonly string Name;

		public readonly string Description;

		public readonly string[] Modes;

		public readonly Vector2i HeightmapSize;

		public readonly int Scale;

		public readonly bool FixedWaterLevel;

		public readonly bool RandomGeneratedWorld;

		public readonly VersionInformation GameVersionCreated;

		public readonly DynamicProperties DynamicProperties;

		public Vector2i WorldSize => HeightmapSize * Scale;

		public WorldInfo(string _name, string _description, string[] _modes, Vector2i _heightmapSize, int _scale, bool _fixedWaterLevel, bool _randomGeneratedWorld, VersionInformation _gameVersionCreated, DynamicProperties _dynamicProperties = null)
		{
			Valid = true;
			Name = _name;
			Description = _description;
			Modes = _modes;
			HeightmapSize = _heightmapSize;
			Scale = _scale;
			FixedWaterLevel = _fixedWaterLevel;
			RandomGeneratedWorld = _randomGeneratedWorld;
			GameVersionCreated = _gameVersionCreated;
			DynamicProperties = _dynamicProperties;
		}

		public void Save(PathAbstractions.AbstractedLocation _worldLocation)
		{
			if (_worldLocation.Type == PathAbstractions.EAbstractedLocationType.None)
			{
				Log.Warning("No world location given");
				return;
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.CreateXmlDeclaration();
			XmlElement node = xmlDocument.AddXmlElement("MapInfo");
			node.AddXmlElement("property").SetAttrib("name", "Name").SetAttrib("value", Name);
			node.AddXmlElement("property").SetAttrib("name", "Modes").SetAttrib("value", string.Join(",", Modes));
			node.AddXmlElement("property").SetAttrib("name", "Description").SetAttrib("value", Description);
			XmlElement element = node.AddXmlElement("property").SetAttrib("name", "Scale");
			int scale = Scale;
			element.SetAttrib("value", scale.ToString());
			node.AddXmlElement("property").SetAttrib("name", "HeightMapSize").SetAttrib("value", HeightmapSize.ToString());
			XmlElement element2 = node.AddXmlElement("property").SetAttrib("name", "FixedWaterLevel");
			bool fixedWaterLevel = FixedWaterLevel;
			element2.SetAttrib("value", fixedWaterLevel.ToString());
			XmlElement element3 = node.AddXmlElement("property").SetAttrib("name", "RandomGeneratedWorld");
			fixedWaterLevel = RandomGeneratedWorld;
			element3.SetAttrib("value", fixedWaterLevel.ToString());
			node.AddXmlElement("property").SetAttrib("name", "GameVersion").SetAttrib("value", GameVersionCreated.SerializableString);
			xmlDocument.SdSave(_worldLocation.FullPath + "/map_info.xml");
		}

		public static WorldInfo LoadWorldInfo(PathAbstractions.AbstractedLocation _worldLocation)
		{
			try
			{
				if (_worldLocation.Type == PathAbstractions.EAbstractedLocationType.None)
				{
					return null;
				}
				string text = _worldLocation.FullPath + "/map_info.xml";
				if (!SdFile.Exists(text))
				{
					return null;
				}
				IEnumerable<XElement> enumerable = from s in SdXDocument.Load(text).Elements("MapInfo")
					from p in s.Elements("property")
					select p;
				DynamicProperties dynamicProperties = new DynamicProperties();
				foreach (XElement item in enumerable)
				{
					dynamicProperties.Add(item);
				}
				string name = null;
				string description = null;
				string[] modes = null;
				int x = 4096;
				int y = 4096;
				int scale = 1;
				bool fixedWaterLevel = false;
				bool randomGeneratedWorld = false;
				VersionInformation _result = new VersionInformation(VersionInformation.EGameReleaseType.Alpha, -1, -1, -1);
				if (dynamicProperties.Values.ContainsKey("Name"))
				{
					name = dynamicProperties.Values["Name"];
				}
				if (dynamicProperties.Values.ContainsKey("Modes"))
				{
					modes = dynamicProperties.Values["Modes"].Replace(" ", "").Split(',');
				}
				if (dynamicProperties.Values.ContainsKey("Description"))
				{
					description = Localization.Get(dynamicProperties.Values["Description"]);
				}
				if (dynamicProperties.Values.ContainsKey("Scale"))
				{
					scale = int.Parse(dynamicProperties.Values["Scale"]);
				}
				if (dynamicProperties.Values.ContainsKey("HeightMapSize"))
				{
					Vector2i vector2i = StringParsers.ParseVector2i(dynamicProperties.Values["HeightMapSize"]);
					x = vector2i.x;
					y = vector2i.y;
				}
				if (dynamicProperties.Values.ContainsKey("FixedWaterLevel"))
				{
					fixedWaterLevel = StringParsers.ParseBool(dynamicProperties.Values["FixedWaterLevel"]);
				}
				if (dynamicProperties.Values.ContainsKey("RandomGeneratedWorld"))
				{
					randomGeneratedWorld = StringParsers.ParseBool(dynamicProperties.Values["RandomGeneratedWorld"]);
				}
				if (dynamicProperties.Values.ContainsKey("GameVersion") && !VersionInformation.TryParseSerializedString(dynamicProperties.Values["GameVersion"], out _result))
				{
					_result = new VersionInformation(VersionInformation.EGameReleaseType.Alpha, -1, -1, -1);
					Log.Warning("World '" + _worldLocation.Name + "' has an invalid GameVersion value: " + dynamicProperties.Values["GameVersion"]);
				}
				return new WorldInfo(name, description, modes, new Vector2i(x, y), scale, fixedWaterLevel, randomGeneratedWorld, _result, dynamicProperties);
			}
			catch (Exception e)
			{
				Log.Error("Error reading WorldInfo for " + (_worldLocation.FullPath ?? "<null>"));
				Log.Exception(e);
			}
			return null;
		}
	}

	public delegate void OutputDelegate(string _text);

	public enum EKickReason
	{
		EmptyNameOrPlayerID,
		InvalidUserId,
		DuplicatePlayerID,
		InvalidAuthTicket,
		VersionMismatch,
		PlayerLimitExceeded,
		Banned,
		NotOnWhitelist,
		PlatformAuthenticationBeginFailed,
		PlatformAuthenticationFailed,
		ManualKick,
		EacViolation,
		EacBan,
		PlayerLimitExceededNonVIP,
		GameStillLoading,
		GamePaused,
		ModDecision,
		FriendsOnly,
		UnknownNetPackage,
		EncryptionFailure,
		UnsupportedPlatform,
		CrossPlatformAuthenticationBeginFailed,
		CrossPlatformAuthenticationFailed,
		WrongCrossPlatform,
		EosEacViolation,
		MultiplayerBlockedForHostAccount,
		BadMTUPackets,
		CrossplayDisabled,
		InternalNetConnectionError,
		InviteOnly,
		SessionClosed,
		PersistentPlayerDataExceeded,
		PlatformPlayerLimitExceeded,
		EncryptionAgreementInvalidSignature,
		EncryptionAgreementError
	}

	public struct KickPlayerData(EKickReason _kickReason, int _apiResponseEnum = 0, DateTime _banUntil = default(DateTime), string _customReason = "")
	{
		public EKickReason reason = _kickReason;

		public int apiResponseEnum = _apiResponseEnum;

		public DateTime banUntil = _banUntil;

		public string customReason = _customReason ?? string.Empty;

		public string LocalizedMessage()
		{
			switch (reason)
			{
			case EKickReason.EmptyNameOrPlayerID:
			case EKickReason.InvalidUserId:
			case EKickReason.DuplicatePlayerID:
			case EKickReason.InvalidAuthTicket:
			case EKickReason.NotOnWhitelist:
			case EKickReason.PersistentPlayerDataExceeded:
				return Localization.Get("auth_" + reason.ToStringCached());
			case EKickReason.VersionMismatch:
				return string.Format(Localization.Get("auth_VersionMismatch"), Constants.cVersionInformation.LongStringNoBuild, customReason);
			case EKickReason.PlayerLimitExceeded:
				return string.Format(Localization.Get("auth_PlayerLimitExceeded"), customReason);
			case EKickReason.PlatformPlayerLimitExceeded:
				return string.Format(Localization.Get("auth_PlatformPlayerLimitExceeded"), customReason);
			case EKickReason.Banned:
				return string.Format(Localization.Get("auth_Banned"), banUntil.ToCultureInvariantString()) + (string.IsNullOrEmpty(customReason) ? string.Empty : ("\n" + string.Format(Localization.Get("auth_reason"), customReason)));
			case EKickReason.ManualKick:
				return string.Format(Localization.Get("auth_ManualKick")) + (string.IsNullOrEmpty(customReason) ? string.Empty : ("\n" + string.Format(Localization.Get("auth_reason"), customReason)));
			case EKickReason.PlatformAuthenticationBeginFailed:
			{
				EBeginUserAuthenticationResult eBeginUserAuthenticationResult = (EBeginUserAuthenticationResult)apiResponseEnum;
				if ((uint)(eBeginUserAuthenticationResult - 1) <= 4u)
				{
					return string.Format(Localization.Get("platformauth_" + eBeginUserAuthenticationResult.ToStringCached()), PlatformManager.NativePlatform.PlatformDisplayName);
				}
				return string.Format(Localization.Get("platformauth_unknown"), PlatformManager.NativePlatform.PlatformDisplayName);
			}
			case EKickReason.PlatformAuthenticationFailed:
			{
				EUserAuthenticationResult eUserAuthenticationResult = (EUserAuthenticationResult)apiResponseEnum;
				switch (eUserAuthenticationResult)
				{
				case EUserAuthenticationResult.UserNotConnectedToPlatform:
				case EUserAuthenticationResult.NoLicenseOrExpired:
				case EUserAuthenticationResult.PlatformBanned:
				case EUserAuthenticationResult.LoggedInElseWhere:
				case EUserAuthenticationResult.PlatformBanCheckTimedOut:
				case EUserAuthenticationResult.AuthTicketCanceled:
				case EUserAuthenticationResult.AuthTicketInvalidAlreadyUsed:
				case EUserAuthenticationResult.AuthTicketInvalid:
					return string.Format(Localization.Get("platformauth_" + eUserAuthenticationResult.ToStringCached()), PlatformManager.NativePlatform.PlatformDisplayName);
				case EUserAuthenticationResult.PublisherIssuedBan:
					if (banUntil == default(DateTime))
					{
						return string.Format(Localization.Get("platformauth_" + eUserAuthenticationResult.ToStringCached()), PlatformManager.NativePlatform.PlatformDisplayName) + (string.IsNullOrEmpty(customReason) ? string.Empty : ("\n" + string.Format(Localization.Get("auth_reason"))));
					}
					return string.Format("\n" + Localization.Get("auth_Banned"), banUntil.ToCultureInvariantString()) + (string.IsNullOrEmpty(customReason) ? string.Empty : ("\n" + string.Format(Localization.Get("auth_reason"), customReason)));
				default:
					return string.Format(Localization.Get("platformauth_unknown"), PlatformManager.NativePlatform.PlatformDisplayName);
				}
			}
			case EKickReason.PlayerLimitExceededNonVIP:
				return string.Format(Localization.Get("auth_PlayerLimitExceededNonVIP"), customReason);
			case EKickReason.GameStillLoading:
				return Localization.Get("auth_stillloading");
			case EKickReason.GamePaused:
				return Localization.Get("auth_gamepaused");
			case EKickReason.ModDecision:
			{
				string text = Localization.Get("auth_mod");
				if (!string.IsNullOrEmpty(customReason))
				{
					text = text + "\n" + customReason;
				}
				return text;
			}
			case EKickReason.FriendsOnly:
				return Localization.Get("auth_friendsonly");
			case EKickReason.InviteOnly:
				return Localization.Get("auth_inviteOnly");
			case EKickReason.SessionClosed:
				return Localization.Get("auth_sessionClosed");
			case EKickReason.UnknownNetPackage:
				return Localization.Get("auth_unknownnetpackage");
			case EKickReason.EncryptionFailure:
				return Localization.Get("auth_encryptionfailure");
			case EKickReason.EncryptionAgreementError:
				return Localization.Get("auth_encryptionagreementerror");
			case EKickReason.EncryptionAgreementInvalidSignature:
				return Localization.Get("auth_encryptionagreementinvalidsignature");
			case EKickReason.UnsupportedPlatform:
				return string.Format(Localization.Get("auth_unsupportedplatform"), Localization.Get("platformName" + customReason));
			case EKickReason.CrossPlatformAuthenticationBeginFailed:
			{
				EBeginUserAuthenticationResult eBeginUserAuthenticationResult2 = (EBeginUserAuthenticationResult)apiResponseEnum;
				if ((uint)(eBeginUserAuthenticationResult2 - 1) <= 4u)
				{
					return string.Format(Localization.Get("platformauth_" + eBeginUserAuthenticationResult2.ToStringCached()), PlatformManager.CrossplatformPlatform.PlatformDisplayName);
				}
				return string.Format(Localization.Get("platformauth_unknown"), PlatformManager.CrossplatformPlatform.PlatformDisplayName);
			}
			case EKickReason.CrossPlatformAuthenticationFailed:
			{
				EUserAuthenticationResult eUserAuthenticationResult2 = (EUserAuthenticationResult)apiResponseEnum;
				switch (eUserAuthenticationResult2)
				{
				case EUserAuthenticationResult.UserNotConnectedToPlatform:
				case EUserAuthenticationResult.NoLicenseOrExpired:
				case EUserAuthenticationResult.PlatformBanned:
				case EUserAuthenticationResult.LoggedInElseWhere:
				case EUserAuthenticationResult.PlatformBanCheckTimedOut:
				case EUserAuthenticationResult.AuthTicketCanceled:
				case EUserAuthenticationResult.AuthTicketInvalidAlreadyUsed:
				case EUserAuthenticationResult.AuthTicketInvalid:
				case EUserAuthenticationResult.PublisherIssuedBan:
					return string.Format(Localization.Get("platformauth_" + eUserAuthenticationResult2.ToStringCached()), PlatformManager.CrossplatformPlatform.PlatformDisplayName);
				case EUserAuthenticationResult.EosTicketFailed:
					return string.Format(Localization.Get("platformauth_" + eUserAuthenticationResult2.ToStringCached()), PlatformManager.CrossplatformPlatform.PlatformDisplayName, customReason);
				default:
					return string.Format(Localization.Get("platformauth_unknown"), PlatformManager.CrossplatformPlatform.PlatformDisplayName);
				}
			}
			case EKickReason.WrongCrossPlatform:
				return string.Format(Localization.Get("auth_wrongcrossplatform"), Localization.Get("platformName" + customReason));
			case EKickReason.EosEacViolation:
			{
				AntiCheatCommonClientActionReason antiCheatCommonClientActionReason = (AntiCheatCommonClientActionReason)apiResponseEnum;
				if ((uint)antiCheatCommonClientActionReason <= 10u)
				{
					string arg = Localization.Get("eacauth_known_" + ((AntiCheatCommonClientActionReason)apiResponseEnum).ToStringCached());
					if (string.IsNullOrEmpty(customReason))
					{
						return string.Format(Localization.Get("eacauth_known"), arg);
					}
					return string.Format(Localization.Get("eacauth_known_with_text"), arg, customReason);
				}
				return Localization.Get("eacauth_unknown");
			}
			case EKickReason.MultiplayerBlockedForHostAccount:
				return Localization.Get("auth_multiplayerblocked");
			case EKickReason.BadMTUPackets:
				return Localization.Get("auth_badPackets");
			case EKickReason.CrossplayDisabled:
				return Localization.Get("auth_crossplaydisabled");
			case EKickReason.InternalNetConnectionError:
				return Localization.Get("auth_internalnetconnectionerror");
			default:
				return Localization.Get("auth_unknown");
			}
		}

		public override string ToString()
		{
			return reason switch
			{
				EKickReason.EmptyNameOrPlayerID => "Empty name or player ID", 
				EKickReason.InvalidUserId => "Invalid SteamID", 
				EKickReason.DuplicatePlayerID => "Duplicate player ID", 
				EKickReason.InvalidAuthTicket => "Invalid authentication ticket", 
				EKickReason.VersionMismatch => "Version mismatch", 
				EKickReason.PlayerLimitExceeded => "Player limit exceeded", 
				EKickReason.Banned => "Banned until: " + banUntil.ToCultureInvariantString() + (string.IsNullOrEmpty(customReason) ? "" : (", reason: " + customReason)), 
				EKickReason.NotOnWhitelist => "Not on whitelist", 
				EKickReason.PlatformAuthenticationBeginFailed => "Platform auth failed: " + ((EBeginUserAuthenticationResult)apiResponseEnum).ToStringCached(), 
				EKickReason.PlatformAuthenticationFailed => "Platform auth failed: " + ((EUserAuthenticationResult)apiResponseEnum).ToStringCached(), 
				EKickReason.ManualKick => "Kick: " + ((customReason != null) ? customReason : "no reason given"), 
				EKickReason.PlayerLimitExceededNonVIP => "Player limit for non VIPs / unreserved slots exceeded", 
				EKickReason.GameStillLoading => "Server is still initializing", 
				EKickReason.GamePaused => "Server is paused", 
				EKickReason.ModDecision => "Denied by mod", 
				EKickReason.FriendsOnly => "Friends Only host", 
				EKickReason.InviteOnly => "Invite Only host", 
				EKickReason.SessionClosed => "Session is Closed", 
				EKickReason.UnknownNetPackage => "Unknown NetPackage", 
				EKickReason.EncryptionFailure => "Encryption failure", 
				EKickReason.EncryptionAgreementError => "Error while performing encryption key agreement", 
				EKickReason.EncryptionAgreementInvalidSignature => "Encryption key agreement authentication invalid", 
				EKickReason.UnsupportedPlatform => "Unsupported client platform: " + customReason, 
				EKickReason.CrossPlatformAuthenticationBeginFailed => "Cross platform auth failed: " + ((EBeginUserAuthenticationResult)apiResponseEnum).ToStringCached() + (string.IsNullOrEmpty(customReason) ? "" : (" - " + customReason)), 
				EKickReason.CrossPlatformAuthenticationFailed => "Cross platform auth failed: " + ((EUserAuthenticationResult)apiResponseEnum).ToStringCached() + (string.IsNullOrEmpty(customReason) ? "" : (" - " + customReason)), 
				EKickReason.WrongCrossPlatform => "Unsupported client cross platform: " + customReason, 
				EKickReason.EosEacViolation => "EOS-ACS violation: " + ((AntiCheatCommonClientActionReason)apiResponseEnum).ToStringCached() + (string.IsNullOrEmpty(customReason) ? "" : (" - " + customReason)), 
				EKickReason.MultiplayerBlockedForHostAccount => "Multiplayer blocked for host's account", 
				EKickReason.CrossplayDisabled => "Crossplay disabled for host's account", 
				_ => "Unknown reason", 
			};
		}
	}

	public enum EScreenshotMode
	{
		File,
		Clipboard,
		Both
	}

	public enum EPlayerHomeType
	{
		None,
		Landclaim,
		Bedroll
	}

	public enum DirEightWay : sbyte
	{
		None = -1,
		N,
		NE,
		E,
		SE,
		S,
		SW,
		W,
		NW,
		COUNT
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Collider[] overlapBoxHits = new Collider[50];

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameRandom random;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Vector3> tempVertices = new List<Vector3>(16384);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<int> tempTriangles = new List<int>(16384);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, string> arguments;

	public static string lastSavedScreenshotFilename;

	public static List<Vector2i> NeighborsEightWay = new List<Vector2i>
	{
		new Vector2i(0, 1),
		new Vector2i(1, 1),
		new Vector2i(1, 0),
		new Vector2i(1, -1),
		new Vector2i(0, -1),
		new Vector2i(-1, -1),
		new Vector2i(-1, 0),
		new Vector2i(-1, 1)
	};

	public static bool FindMasterBlockForEntityModelBlock(World _world, Vector3 _dirNormalized, string _phsxTag, Vector3 _hitPointPos, Transform _hitTransform, WorldRayHitInfo _hitInfo)
	{
		int num = 0;
		if (_phsxTag.Length > 2)
		{
			char c = _phsxTag[_phsxTag.Length - 2];
			char c2 = _phsxTag[_phsxTag.Length - 1];
			if (c >= '0' && c <= '9' && c2 >= '0' && c2 <= '9')
			{
				num += (c - 48) * 10;
				num += c2 - 48;
			}
		}
		ChunkCluster chunkCluster = _world.ChunkClusters[num];
		if (chunkCluster == null)
		{
			return false;
		}
		Vector3 vector = chunkCluster.ToLocalPosition(_hitPointPos);
		Vector3 vector2 = chunkCluster.ToLocalVector(_dirNormalized);
		Vector3i vector3i = World.worldToBlockPos(vector);
		Transform parentTransform = RootTransformRefParent.FindRoot(_hitTransform);
		int num2 = World.toBlockXZ(vector3i.x);
		int num3 = World.toBlockXZ(vector3i.z);
		int num4 = World.toChunkXZ(vector3i.x);
		int num5 = World.toChunkXZ(vector3i.z);
		BlockValue bv;
		if (checkChunk(chunkCluster, num4, num5, parentTransform, vector, vector2, _hitInfo, out var _ebcd))
		{
			_hitInfo.hit.pos = _hitPointPos;
			_hitInfo.lastBlockPos = World.worldToBlockPos(_hitInfo.hit.pos);
			if (!_ebcd.blockValue.Block.isMultiBlock || _ebcd.blockValue.Block.multiBlockPos.ContainsPos(_world, _ebcd.pos, _ebcd.blockValue, _hitInfo.lastBlockPos))
			{
				_hitInfo.lastBlockPos = Voxel.GoBackOnVoxels(chunkCluster, new Ray(chunkCluster.ToLocalPosition(_hitPointPos + vector2 * 0.01f), -vector2), out bv);
			}
			return true;
		}
		if (checkChunk(chunkCluster, (num2 < 8) ? (num4 - 1) : (num4 + 1), num5, parentTransform, vector, vector2, _hitInfo, out _ebcd))
		{
			_hitInfo.hit.pos = _hitPointPos;
			_hitInfo.lastBlockPos = World.worldToBlockPos(_hitInfo.hit.pos);
			if (!_ebcd.blockValue.Block.isMultiBlock || _ebcd.blockValue.Block.multiBlockPos.ContainsPos(_world, _ebcd.pos, _ebcd.blockValue, _hitInfo.lastBlockPos))
			{
				_hitInfo.lastBlockPos = Voxel.GoBackOnVoxels(chunkCluster, new Ray(chunkCluster.ToLocalPosition(_hitPointPos + vector2 * 0.01f), -vector2), out bv);
			}
			return true;
		}
		if (checkChunk(chunkCluster, num4, (num3 < 8) ? (num5 - 1) : (num5 + 1), parentTransform, vector, vector2, _hitInfo, out _ebcd))
		{
			_hitInfo.hit.pos = _hitPointPos;
			_hitInfo.lastBlockPos = World.worldToBlockPos(_hitInfo.hit.pos);
			if (!_ebcd.blockValue.Block.isMultiBlock || _ebcd.blockValue.Block.multiBlockPos.ContainsPos(_world, _ebcd.pos, _ebcd.blockValue, _hitInfo.lastBlockPos))
			{
				_hitInfo.lastBlockPos = Voxel.GoBackOnVoxels(chunkCluster, new Ray(chunkCluster.ToLocalPosition(_hitPointPos + vector2 * 0.01f), -vector2), out bv);
			}
			return true;
		}
		if (checkChunk(chunkCluster, (num2 < 8) ? (num4 - 1) : (num4 + 1), (num3 < 8) ? (num5 - 1) : (num5 + 1), parentTransform, vector, vector2, _hitInfo, out _ebcd))
		{
			_hitInfo.hit.pos = _hitPointPos;
			_hitInfo.lastBlockPos = World.worldToBlockPos(_hitInfo.hit.pos);
			if (!_ebcd.blockValue.Block.isMultiBlock || _ebcd.blockValue.Block.multiBlockPos.ContainsPos(_world, _ebcd.pos, _ebcd.blockValue, _hitInfo.lastBlockPos))
			{
				_hitInfo.lastBlockPos = Voxel.GoBackOnVoxels(chunkCluster, new Ray(chunkCluster.ToLocalPosition(_hitPointPos + vector2 * 0.01f), -vector2), out bv);
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool checkChunk(ChunkCluster cc, int cX, int cZ, Transform parentTransform, Vector3 localHitPos, Vector3 localDirNormalized, WorldRayHitInfo _hitInfo, out BlockEntityData _ebcd)
	{
		_ebcd = null;
		IChunk chunkSync = cc.GetChunkSync(cX, cZ);
		if (chunkSync == null)
		{
			return false;
		}
		if ((_ebcd = chunkSync.GetBlockEntity(parentTransform)) != null)
		{
			_hitInfo.hit.clrIdx = 0;
			_hitInfo.hit.blockPos = _ebcd.pos;
			_hitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(chunkSync, World.toBlockXZ(_ebcd.pos.x), World.toBlockY(_ebcd.pos.y), World.toBlockXZ(_ebcd.pos.z));
			Ray ray = new Ray(localHitPos, -1f * localDirNormalized);
			int num = 0;
			do
			{
				_hitInfo.lastBlockPos = Voxel.OneVoxelStep(Vector3i.FromVector3Rounded(ray.origin), ray.origin, ray.direction, out var hitPos, out var _);
				ray.origin = hitPos + localDirNormalized * 0.001f;
			}
			while (!cc.GetBlock(_hitInfo.lastBlockPos).isair && num++ < 3);
			return true;
		}
		return false;
	}

	public static void EnableRagdoll(GameObject _model, bool _bEnable, bool _bUseGravity)
	{
		Rigidbody[] componentsInChildren = _model.GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody obj in componentsInChildren)
		{
			obj.isKinematic = !_bEnable;
			obj.useGravity = _bUseGravity;
		}
		Collider[] componentsInChildren2 = _model.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].enabled = _bEnable;
		}
	}

	public static Entity GetHitRootEntity(string _tag, Transform _hitTransform)
	{
		if (_tag.StartsWith("E_BP_"))
		{
			if (_hitTransform.TryGetComponent<RootTransformRefEntity>(out var component))
			{
				if ((bool)component.RootTransform)
				{
					return component.RootTransform.GetComponent<Entity>();
				}
			}
			else
			{
				Transform transform = RootTransformRefEntity.FindEntityUpwards(_hitTransform);
				if ((bool)transform)
				{
					return transform.GetComponent<Entity>();
				}
			}
		}
		else if (_tag.StartsWith("E_Vehicle"))
		{
			return CollisionCallForward.FindEntity(_hitTransform);
		}
		return null;
	}

	public static Transform GetHitRootTransform(string _tag, Transform _hitTransform)
	{
		if (_tag.StartsWith("E_BP_"))
		{
			if (_hitTransform.TryGetComponent<RootTransformRefEntity>(out var component))
			{
				return component.RootTransform;
			}
			return RootTransformRefEntity.FindEntityUpwards(_hitTransform);
		}
		if (_tag.Equals("E_Vehicle"))
		{
			Entity entity = CollisionCallForward.FindEntity(_hitTransform);
			if ((bool)entity)
			{
				return entity.transform;
			}
		}
		return _hitTransform;
	}

	public static string GetTransformPath(Transform _t)
	{
		if (!_t)
		{
			return "null";
		}
		if (!_t.parent)
		{
			return _t.name;
		}
		return GetTransformPath(_t.parent) + "/" + _t.name;
	}

	public static string GetChildTransformPath(Transform _parent, Transform _child)
	{
		if (_child.parent == null)
		{
			throw new Exception("GetChildTransformPath: '" + _child.name + "' is a root object and not in the path underneath '" + _parent.name + "'");
		}
		if (_child.parent == _parent)
		{
			return _child.name;
		}
		return GetChildTransformPath(_parent, _child.parent) + "/" + _child.name;
	}

	public static void FindTagInChilds(Transform _parent, string _tag, List<Transform> _list)
	{
		int childCount = _parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = _parent.GetChild(i);
			if (child.CompareTag(_tag))
			{
				_list.Add(child);
			}
			FindTagInChilds(child, _tag, _list);
		}
	}

	public static Transform FindTagInChilds(Transform _parent, string _tag)
	{
		int childCount = _parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = _parent.GetChild(i);
			if (child.CompareTag(_tag))
			{
				return child;
			}
		}
		for (int j = 0; j < childCount; j++)
		{
			Transform transform = FindTagInChilds(_parent.GetChild(j), _tag);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public static Transform FindTagInDirectChilds(Transform _parent, string _tag)
	{
		int childCount = _parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = _parent.GetChild(i);
			if (child.CompareTag(_tag))
			{
				return child;
			}
		}
		return null;
	}

	public static Transform FindChildWithPartialName(Transform root, params string[] names)
	{
		foreach (string b in names)
		{
			if (root.name.ContainsCaseInsensitive(b))
			{
				return root;
			}
			for (int j = 0; j < root.childCount; j++)
			{
				Transform child = root.GetChild(j);
				if (child.name.ContainsCaseInsensitive(b))
				{
					return child;
				}
			}
		}
		return null;
	}

	public static void FindDeepChildWithPartialName(Transform root, string name, ref List<Transform> found)
	{
		if (root.name.ContainsCaseInsensitive(name))
		{
			found.Add(root);
		}
		for (int i = 0; i < root.childCount; i++)
		{
			FindDeepChildWithPartialName(root.GetChild(i), name, ref found);
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void HideObjectInEditor(GameObject _obj)
	{
		UnityEngine.Object.DontDestroyOnLoad(_obj);
	}

	public static bool IsColliderWithinBlock(Vector3i blockPosition, BlockValue blockValue)
	{
		int num = 3899392;
		Quaternion rotation = blockValue.Block.shape.GetRotation(blockValue);
		Bounds blockPlacementBounds = GetBlockPlacementBounds(blockValue.Block);
		Vector3 size = blockPlacementBounds.size;
		Vector3 center = World.blockToTransformPos(blockPosition) - Origin.position + new Vector3(0f, 0.5f, 0f);
		if (blockPlacementBounds.center != Vector3.zero)
		{
			center += rotation * blockPlacementBounds.center;
		}
		if (blockValue.Block.isOversized)
		{
			num |= 0x40800000;
			size -= new Vector3(0.1f, 0.1f, 0.1f);
		}
		else if (blockValue.Block.shape.IsTerrain())
		{
			center -= new Vector3(0f, 0.25f, 0f);
		}
		Vector3 halfExtents = size * 0.5f;
		int num2 = Physics.OverlapBoxNonAlloc(center, halfExtents, overlapBoxHits, rotation, num);
		if (num2 == overlapBoxHits.Length)
		{
			UnityEngine.Debug.LogError($"OverlapBox reached maximum hit count ({num2}); overlapBoxHits array size may be insufficient.");
		}
		for (int i = 0; i < num2; i++)
		{
			if (blockValue.Block.isOversized)
			{
				if (!overlapBoxHits[i].CompareTag("T_Mesh"))
				{
					return true;
				}
				continue;
			}
			if (!IsBlockOrTerrain(overlapBoxHits[i]) && !overlapBoxHits[i].CompareTag("Item"))
			{
				return true;
			}
			if (overlapBoxHits[i].CompareTag("T_Block"))
			{
				Transform entityParentTransform = RootTransformRefParent.FindRoot(overlapBoxHits[i].transform);
				if (TryFindEntityData(blockPosition, entityParentTransform, out var ebcd) && ebcd.blockValue.Block.isOversized)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static Vector3 GetMultiBlockBoundsOffset(Vector3 multiBlockDim)
	{
		return new Vector3((multiBlockDim.x % 2f == 0f) ? (-0.5f) : 0f, multiBlockDim.y / 2f - 0.5f, (multiBlockDim.z % 2f == 0f) ? (-0.5f) : 0f);
	}

	public static Bounds GetBlockPlacementBounds(Block block)
	{
		if (block.isOversized)
		{
			return block.oversizedBounds;
		}
		if (block.isMultiBlock)
		{
			Vector3 vector = block.multiBlockPos.dim;
			return new Bounds(GetMultiBlockBoundsOffset(vector), vector);
		}
		return new Bounds(Vector3.zero, Vector3.one);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool TryFindEntityData(Vector3i entityWorldHitPosition, Transform entityParentTransform, out BlockEntityData ebcd)
	{
		int num = World.toBlockXZ(entityWorldHitPosition.x);
		int num2 = World.toBlockXZ(entityWorldHitPosition.z);
		int num3 = World.toChunkXZ(entityWorldHitPosition.x);
		int num4 = World.toChunkXZ(entityWorldHitPosition.z);
		int num5 = ((num >= 8) ? 1 : (-1));
		int num6 = ((num2 >= 8) ? 1 : (-1));
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		if (chunkCache != null)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					ebcd = chunkCache.GetChunkSync(num3 + j * num5, num4 + i * num6)?.GetBlockEntity(entityParentTransform);
					if (ebcd != null)
					{
						return true;
					}
				}
			}
		}
		UnityEngine.Debug.LogWarning($"Failed to find entity data for Transform \"{entityParentTransform.name}\" with hit position \"{entityWorldHitPosition}\"", entityParentTransform);
		ebcd = null;
		return false;
	}

	public static void DebugDrawPathFromEntity(Entity _e, PathEntity _path, Color _color)
	{
	}

	public static void CreateEmptyFlatLevel(string _worldName, int _worldSize, int _terrainHeight = 60)
	{
		PathAbstractions.AbstractedLocation worldLocation = new PathAbstractions.AbstractedLocation(PathAbstractions.EAbstractedLocationType.UserDataPath, _worldName, GameIO.GetUserGameDataDir() + "/GeneratedWorlds/" + _worldName, null, _isFolder: true);
		SdDirectory.CreateDirectory(worldLocation.FullPath);
		World world = new World();
		WorldState worldState = new WorldState();
		worldState.SetFrom(world, EnumChunkProviderId.ChunkDataDriven);
		worldState.ResetDynamicData();
		worldState.Save(worldLocation.FullPath + "/main.ttw");
		CreateWorldFilesForFlatLevel(_worldName, _worldSize, worldLocation, _terrainHeight);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateWorldFilesForFlatLevel(string _worldName, int _worldSize, PathAbstractions.AbstractedLocation _worldLocation, int _terrainHeight)
	{
		int num = _worldSize * 2;
		byte[] array = new byte[num];
		for (int i = 0; i < _worldSize; i++)
		{
			array[2 * i] = 0;
			array[2 * i + 1] = 60;
		}
		using (BufferedStream bufferedStream = new BufferedStream(SdFile.OpenWrite(_worldLocation.FullPath + "/dtm.raw")))
		{
			for (int j = 0; j < _worldSize; j++)
			{
				bufferedStream.Write(array, 0, num);
			}
		}
		new WorldInfo(_worldName, "Empty World", new string[2] { "Survival", "Creative" }, new Vector2i(_worldSize, _worldSize), 1, _fixedWaterLevel: false, _randomGeneratedWorld: false, Constants.cVersionInformation).Save(_worldLocation);
		SpawnPointManager spawnPointManager = new SpawnPointManager();
		spawnPointManager.spawnPointList.Add(new SpawnPoint(new Vector3i(0, _terrainHeight + 1, 0)));
		spawnPointManager.Save(_worldLocation.FullPath);
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.CreateXmlDeclaration();
		xmlDocument.AddXmlElement("WaterSources");
		xmlDocument.SdSave(_worldLocation.FullPath + "/water_info.xml");
		CreateForestBiomeMap(_worldSize, _worldLocation);
		XmlDocument xmlDocument2 = new XmlDocument();
		xmlDocument2.CreateXmlDeclaration();
		xmlDocument2.AddXmlElement("prefabs");
		xmlDocument2.SdSave(_worldLocation.FullPath + "/prefabs.xml");
		CreateSimpleRadiationMap(_worldSize, _worldLocation, (_worldSize <= 1024) ? 128 : ((_worldSize <= 2048) ? 256 : 512));
		CreateEmptySplatMap(_worldSize, _worldLocation);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateSimpleRadiationMap(int _worldSize, PathAbstractions.AbstractedLocation _worldLocation, int _radiationBorderSize = 128, int _downScale = 16)
	{
		int num = _worldSize / _downScale;
		Color red = Color.red;
		Color32 color = new Color32(0, 0, 0, byte.MaxValue);
		Texture2D texture2D = new Texture2D(num, num, TextureFormat.RGBA32, mipChain: false);
		texture2D.FillTexture(red);
		int num2 = _radiationBorderSize / _downScale;
		int num3 = num - 2 * num2;
		Color32[] array = new Color32[num3];
		for (int i = 0; i < num3; i++)
		{
			array[i] = color;
		}
		for (int j = num2; j < num - num2; j++)
		{
			texture2D.SetPixels32(num2, j, num3, 1, array);
		}
		texture2D.Apply();
		SdFile.WriteAllBytes(_worldLocation.FullPath + "/radiation.png", texture2D.EncodeToPNG());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateEmptySplatMap(int _worldSize, PathAbstractions.AbstractedLocation _worldLocation)
	{
		Color32 color = new Color32(0, 0, 0, 0);
		Texture2D texture2D = new Texture2D(_worldSize, _worldSize, TextureFormat.RGBA32, mipChain: false);
		texture2D.FillTexture(color);
		texture2D.Apply();
		byte[] bytes = texture2D.EncodeToPNG();
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		SdFile.WriteAllBytes(_worldLocation.FullPath + "/splat1.png", bytes);
		SdFile.WriteAllBytes(_worldLocation.FullPath + "/splat2.png", bytes);
		SdFile.WriteAllBytes(_worldLocation.FullPath + "/splat3.png", bytes);
		Log.Out($"Write tex took {microStopwatch.ElapsedMilliseconds} ms");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateForestBiomeMap(int _worldSize, PathAbstractions.AbstractedLocation _worldLocation, BiomeDefinition.BiomeType _biome = BiomeDefinition.BiomeType.PineForest)
	{
		Color32 color = UIntToColor(BiomeDefinition.GetBiomeColor(_biome));
		Texture2D texture2D = new Texture2D(_worldSize, _worldSize, TextureFormat.RGBA32, mipChain: false);
		texture2D.FillTexture(color);
		texture2D.Apply();
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		SdFile.WriteAllBytes(_worldLocation.FullPath + "/biomes.png", texture2D.EncodeToPNG());
		Log.Out($"Write tex took {microStopwatch.ElapsedMilliseconds} ms");
	}

	public static void DeleteWorld(PathAbstractions.AbstractedLocation _worldLocation)
	{
		if (!string.IsNullOrEmpty(_worldLocation.Name))
		{
			SdDirectory.Delete(_worldLocation.FullPath, recursive: true);
			string saveGameDir = GameIO.GetSaveGameDir(_worldLocation.Name);
			if (SdDirectory.Exists(saveGameDir))
			{
				SdDirectory.Delete(saveGameDir, recursive: true);
			}
		}
	}

	public static int WorldTimeToDays(ulong _worldTime)
	{
		return (int)(_worldTime / 24000 + 1);
	}

	public static int WorldTimeToHours(ulong _worldTime)
	{
		return (int)(_worldTime / 1000) % 24;
	}

	public static int WorldTimeToMinutes(ulong _worldTime)
	{
		return (int)((double)_worldTime / 1000.0 * 60.0) % 60;
	}

	public static float WorldTimeToTotalSeconds(float _worldTime)
	{
		return _worldTime * 3.6f;
	}

	public static uint WorldTimeToTotalMinutes(ulong _worldTime)
	{
		return (uint)((double)_worldTime * 0.06);
	}

	public static int WorldTimeToTotalHours(ulong _worldTime)
	{
		return (int)(_worldTime / 1000);
	}

	public static ulong TotalMinutesToWorldTime(uint _totalMinutes)
	{
		return (ulong)((double)_totalMinutes / 0.06);
	}

	public static (int Days, int Hours, int Minutes) WorldTimeToElements(ulong _worldTime)
	{
		int item = (int)(_worldTime / 24000 + 1);
		int item2 = (int)(_worldTime / 1000) % 24;
		int item3 = (int)((double)_worldTime * 0.06) % 60;
		return (Days: item, Hours: item2, Minutes: item3);
	}

	public static string WorldTimeToString(ulong _worldTime)
	{
		var (num, num2, num3) = WorldTimeToElements(_worldTime);
		return $"{num} {num2:D2}:{num3:D2}";
	}

	public static string WorldTimeDeltaToString(ulong _worldTime)
	{
		var (num, num2, num3) = WorldTimeToElements(_worldTime);
		return $"{num - 1} {num2:D2}:{num3:D2}";
	}

	public static ulong DayTimeToWorldTime(int _day, int _hours, int _minutes)
	{
		if (_day < 1)
		{
			return 0uL;
		}
		return (ulong)((long)(_day - 1) * 24000L + _hours * 1000 + _minutes * 1000 / 60);
	}

	public static ulong DaysToWorldTime(int _day)
	{
		if (_day < 1)
		{
			return 0uL;
		}
		return (ulong)(((long)_day - 1L) * 24000);
	}

	public static ulong DaysToWorldTimeMidnight(int _day)
	{
		return DaysToWorldTime(_day) + 16000;
	}

	public static (int duskHour, int dawnHour) CalcDuskDawnHours(int _dayLightLength)
	{
		(int, int) result = default((int, int));
		result.Item1 = 22;
		if (_dayLightLength > 22)
		{
			result.Item1 = Mathf.Clamp(_dayLightLength, 0, 23);
		}
		result.Item2 = Mathf.Clamp(result.Item1 - _dayLightLength, 0, 23);
		return result;
	}

	public static bool IsBloodMoonTime(ulong _worldTime, (int duskHour, int dawnHour) _duskDawnTimes, int _bmDay)
	{
		var (day, hour, _) = WorldTimeToElements(_worldTime);
		return IsBloodMoonTime(_duskDawnTimes, hour, _bmDay, day);
	}

	public static bool IsBloodMoonTime((int duskHour, int dawnHour) _duskDawnTimes, int _hour, int _bmDay, int _day)
	{
		if (_day == _bmDay)
		{
			if (_hour >= _duskDawnTimes.duskHour)
			{
				return true;
			}
		}
		else if (_day > 1 && _day == _bmDay + 1 && _hour < _duskDawnTimes.dawnHour)
		{
			return true;
		}
		return false;
	}

	public static List<string> GetWorldFilesToTransmitToClient(string _worldFolder)
	{
		string[] files = SdDirectory.GetFiles(_worldFolder);
		for (int i = 0; i < files.Length; i++)
		{
			files[i] = GameIO.GetFilenameFromPath(files[i]);
		}
		return GetWorldFilesToTransmitToClient(files);
	}

	public static List<string> GetWorldFilesToTransmitToClient(ICollection<string> _files)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string _file in _files)
		{
			hashSet.Add(GameIO.GetFilenameFromPathWithoutExtension(_file));
		}
		List<string> list = new List<string>();
		foreach (string _file2 in _files)
		{
			string filenameFromPathWithoutExtension = GameIO.GetFilenameFromPathWithoutExtension(_file2);
			if ((1u & ((!hashSet.Contains(filenameFromPathWithoutExtension + "_processed")) ? 1u : 0u) & ((!_file2.ContainsCaseInsensitive("GenerationInfo")) ? 1u : 0u) & ((!_file2.EndsWith(".bak", StringComparison.OrdinalIgnoreCase)) ? 1u : 0u) & ((!_file2.ContainsCaseInsensitive("Version.txt")) ? 1u : 0u) & ((!_file2.ContainsCaseInsensitive("checksums.txt")) ? 1u : 0u)) != 0)
			{
				list.Add(_file2);
			}
		}
		return list;
	}

	public static void DebugOutputGamePrefs(OutputDelegate _output)
	{
		SortedList<string, string> sortedList = new SortedList<string, string>();
		for (int i = 0; i != 285; i++)
		{
			string text = ((EnumGamePrefs)i).ToStringCached();
			if (!text.Contains("Password") && text != "ServerHistoryCache")
			{
				sortedList.Add(text, text + " = " + GamePrefs.GetObject((EnumGamePrefs)i));
			}
		}
		foreach (string key in sortedList.Keys)
		{
			_output(sortedList[key]);
		}
	}

	public static void DebugOutputGameStats(OutputDelegate _output)
	{
		SortedList<string, string> sortedList = new SortedList<string, string>();
		for (int i = 0; i != 70; i++)
		{
			string text = ((EnumGameStats)i).ToStringCached();
			sortedList.Add(text, text + " = " + GameStats.GetObject((EnumGameStats)i));
		}
		foreach (string key in sortedList.Keys)
		{
			_output(sortedList[key]);
		}
	}

	public static void KickPlayerForClientInfo(ClientInfo _cInfo, KickPlayerData _kickData)
	{
		_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerDenied>().Setup(_kickData));
		string text = _cInfo.ToString();
		KickPlayerData kickPlayerData = _kickData;
		Log.Out("Kicking player (" + kickPlayerData.ToString() + "): " + text);
		ThreadManager.StartCoroutine(disconnectLater(0.5f, _cInfo));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static IEnumerator disconnectLater(float _delayInSec, ClientInfo _clientInfo)
	{
		_clientInfo.disconnecting = true;
		yield return new WaitForSecondsRealtime(_delayInSec);
		SingletonMonoBehaviour<ConnectionManager>.Instance.DisconnectClient(_clientInfo);
	}

	public static void ForceDisconnect()
	{
		ForceDisconnect(new KickPlayerData(EKickReason.InternalNetConnectionError));
	}

	public static void ForceDisconnect(KickPlayerData _kickData)
	{
		ThreadManager.StartCoroutine(ForceDisconnectRoutine(0.5f, _kickData));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static IEnumerator ForceDisconnectRoutine(float delay, KickPlayerData kickData)
	{
		yield return new WaitForSeconds(delay);
		GameManager.Instance.Disconnect();
		if (!GameManager.IsDedicatedServer)
		{
			yield return new WaitForSeconds(0.5f);
			GameManager.Instance.ShowMessagePlayerDenied(kickData);
		}
	}

	public static void WriteItemStack(BinaryWriter _bw, IList<ItemStack> _itemStack)
	{
		_bw.Write((ushort)_itemStack.Count);
		for (int i = 0; i < _itemStack.Count; i++)
		{
			_itemStack[i].Write(_bw);
		}
	}

	public static ItemStack[] ReadItemStackOld(BinaryReader _br)
	{
		int num = _br.ReadUInt16();
		ItemStack[] array = ItemStack.CreateArray(num);
		for (int i = 0; i < num; i++)
		{
			array[i].ReadOld(_br);
			if (ItemClass.GetForId(array[i].itemValue.type) == null)
			{
				array[i] = ItemStack.Empty.Clone();
			}
		}
		return array;
	}

	public static ItemStack[] ReadItemStack(BinaryReader _br)
	{
		int num = _br.ReadUInt16();
		ItemStack[] array = ItemStack.CreateArray(num);
		for (int i = 0; i < num; i++)
		{
			array[i].Read(_br);
			if (ItemClass.GetForId(array[i].itemValue.type) == null)
			{
				array[i] = ItemStack.Empty.Clone();
			}
		}
		return array;
	}

	public static void WriteItemValueArray(BinaryWriter _bw, ItemValue[] _items)
	{
		_bw.Write((ushort)_items.Length);
		foreach (ItemValue itemValue in _items)
		{
			bool flag = itemValue != null;
			_bw.Write(flag);
			if (flag)
			{
				itemValue.Write(_bw);
			}
		}
	}

	public static ItemValue[] ReadItemValueArray(BinaryReader _br)
	{
		ItemValue[] array = new ItemValue[_br.ReadUInt16()];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new ItemValue();
			if (_br.ReadBoolean())
			{
				array[i].Read(_br);
			}
		}
		return array;
	}

	public static void HarvestOnAttack(ItemActionData _actionData, Dictionary<string, ItemActionAttack.Bonuses> ToolBonuses)
	{
		if (_actionData.invData.world.IsEditor() || !(_actionData.invData.holdingEntity is EntityPlayerLocal) || _actionData.attackDetails == null || _actionData.attackDetails.itemsToDrop == null)
		{
			return;
		}
		if (random == null)
		{
			random = GameRandomManager.Instance.CreateGameRandom();
			random.SetSeed((int)Stopwatch.GetTimestamp());
		}
		Block block = _actionData.attackDetails.blockBeingDamaged.Block;
		if (block.RepairItemsMeshDamage != null)
		{
			BlockValue blockBeingDamaged = _actionData.attackDetails.blockBeingDamaged;
			blockBeingDamaged.damage += _actionData.attackDetails.damageGiven;
			_actionData.attackDetails.bKilled = blockBeingDamaged.damage < block.MaxDamage && block.shape.UseRepairDamageState(blockBeingDamaged);
		}
		if (_actionData.attackDetails.bKilled)
		{
			if (!_actionData.attackDetails.itemsToDrop.ContainsKey(EnumDropEvent.Destroy))
			{
				if (!_actionData.attackDetails.blockBeingDamaged.isair && _actionData.attackDetails.bBlockHit)
				{
					ItemValue iv = _actionData.attackDetails.blockBeingDamaged.ToItemValue();
					int count = 1;
					collectHarvestedItem(_actionData, iv, count, 1f, _bScaleCountOnDamage: false);
				}
			}
			else
			{
				List<Block.SItemDropProb> list = _actionData.attackDetails.itemsToDrop[EnumDropEvent.Destroy];
				for (int i = 0; i < list.Count; i++)
				{
					if (_actionData.attackDetails.bBlockHit && list[i].name.Equals("[recipe]"))
					{
						List<Recipe> recipes = CraftingManager.GetRecipes(_actionData.attackDetails.blockBeingDamaged.Block.GetBlockName());
						if (recipes.Count <= 0)
						{
							continue;
						}
						for (int j = 0; j < recipes[0].ingredients.Count; j++)
						{
							if (recipes[0].ingredients[j].count / 2 > 0)
							{
								collectHarvestedItem(_actionData, recipes[0].ingredients[j].itemValue, recipes[0].ingredients[j].count / 2, 1f, _bScaleCountOnDamage: false);
							}
						}
						continue;
					}
					float originalValue = 1f;
					if (list[i].toolCategory != null)
					{
						originalValue = 0f;
						if (ToolBonuses != null && ToolBonuses.ContainsKey(list[i].toolCategory))
						{
							originalValue = ToolBonuses[list[i].toolCategory].Tool;
						}
					}
					originalValue = EffectManager.GetValue(PassiveEffects.HarvestCount, _actionData.invData.itemValue, originalValue, _actionData.invData.holdingEntity, null, FastTags<TagGroup.Global>.Parse(list[i].tag));
					ItemValue itemValue = (list[i].name.Equals("*") ? _actionData.attackDetails.blockBeingDamaged.ToItemValue() : new ItemValue(ItemClass.GetItem(list[i].name).type));
					if (itemValue.type != 0 && ItemClass.list[itemValue.type] != null && (list[i].prob > 0.999f || random.RandomFloat <= list[i].prob))
					{
						int num = (int)((float)random.RandomRange(list[i].minCount, list[i].maxCount + 1) * originalValue);
						if (num > 0)
						{
							collectHarvestedItem(_actionData, itemValue, num, 1f, _bScaleCountOnDamage: false);
						}
					}
				}
			}
		}
		if (_actionData.attackDetails.bBlockHit)
		{
			_actionData.invData.holdingEntity.MinEventContext.BlockValue = _actionData.attackDetails.blockBeingDamaged;
			_actionData.invData.holdingEntity.FireEvent(MinEventTypes.onSelfHarvestBlock);
		}
		else
		{
			_actionData.invData.holdingEntity.MinEventContext.Other = _actionData.attackDetails.entityHit as EntityAlive;
			_actionData.invData.holdingEntity.FireEvent(MinEventTypes.onSelfHarvestOther);
		}
		if (_actionData.attackDetails.itemsToDrop.ContainsKey(EnumDropEvent.Harvest))
		{
			List<Block.SItemDropProb> list2 = _actionData.attackDetails.itemsToDrop[EnumDropEvent.Harvest];
			for (int k = 0; k < list2.Count; k++)
			{
				float originalValue2 = 0f;
				if (list2[k].toolCategory != null)
				{
					originalValue2 = 0f;
					if (ToolBonuses != null && ToolBonuses.ContainsKey(list2[k].toolCategory))
					{
						originalValue2 = ToolBonuses[list2[k].toolCategory].Tool;
					}
				}
				ItemValue itemValue2 = (list2[k].name.Equals("*") ? _actionData.attackDetails.blockBeingDamaged.ToItemValue() : new ItemValue(ItemClass.GetItem(list2[k].name).type));
				if (itemValue2.type == 0 || ItemClass.list[itemValue2.type] == null)
				{
					continue;
				}
				originalValue2 = EffectManager.GetValue(PassiveEffects.HarvestCount, _actionData.invData.itemValue, originalValue2, _actionData.invData.holdingEntity, null, FastTags<TagGroup.Global>.Parse(list2[k].tag));
				int num2 = (int)((float)random.RandomRange(list2[k].minCount, list2[k].maxCount + 1) * originalValue2);
				int num3 = num2 - num2 / 3;
				if (num3 > 0)
				{
					collectHarvestedItem(_actionData, itemValue2, num3, list2[k].prob);
				}
				if (!_actionData.attackDetails.bKilled)
				{
					continue;
				}
				num3 = num2 / 3;
				float num4 = list2[k].prob;
				float resourceScale = list2[k].resourceScale;
				if (resourceScale > 0f && resourceScale < 1f)
				{
					num4 /= resourceScale;
					num3 = (int)((float)num3 * resourceScale);
					if (num3 < 1)
					{
						num3++;
					}
				}
				if (num3 > 0)
				{
					collectHarvestedItem(_actionData, itemValue2, num3, num4, _bScaleCountOnDamage: false);
				}
			}
		}
		_actionData.attackDetails.blockBeingDamaged = BlockValue.Air;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void collectHarvestedItem(ItemActionData _actionData, ItemValue _iv, int _count, float _prob, bool _bScaleCountOnDamage = true)
	{
		if (random == null)
		{
			random = GameRandomManager.Instance.CreateGameRandom();
			random.SetSeed((int)Stopwatch.GetTimestamp());
		}
		if (_bScaleCountOnDamage)
		{
			float num = (float)_actionData.attackDetails.damageMax / (float)_count;
			int num2 = (int)((Utils.FastMin(_actionData.attackDetails.damageTotalOfTarget, _actionData.attackDetails.damageMax) - (float)_actionData.attackDetails.damageGiven) / num + 0.5f);
			int num3 = Mathf.Min((int)(_actionData.attackDetails.damageTotalOfTarget / num + 0.5f), _count);
			int b = _count;
			_count = num3 - num2;
			if (_actionData.attackDetails.damageTotalOfTarget > (float)_actionData.attackDetails.damageMax)
			{
				_count = Mathf.Min(_count, b);
			}
		}
		if (random.RandomFloat <= _prob && _count > 0)
		{
			ItemStack itemStack = new ItemStack(_iv, _count);
			LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_actionData.invData.holdingEntity as EntityPlayerLocal);
			XUiM_PlayerInventory playerInventory = uIForPlayer.xui.PlayerInventory;
			QuestEventManager.Current.HarvestedItem(_actionData.invData.itemValue, itemStack, _actionData.attackDetails.blockBeingDamaged);
			if (!playerInventory.AddItem(itemStack))
			{
				GameManager.Instance.ItemDropServer(new ItemStack(_iv, itemStack.count), GameManager.Instance.World.GetPrimaryPlayer().GetDropPosition(), new Vector3(0.5f, 0.5f, 0.5f), GameManager.Instance.World.GetPrimaryPlayerId());
			}
			uIForPlayer.entityPlayer.Progression.AddLevelExp((int)(itemStack.itemValue.ItemClass.MadeOfMaterial.Experience * (float)_count), "_xpFromHarvesting", Progression.XPTypes.Harvesting);
		}
	}

	public static void DrawCube(Vector3 _pos, Color _col)
	{
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.1f, 0.1f, 0.1f), _pos + new Vector3(0.9f, 0.1f, 0.1f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.1f, 0.1f, 0.1f), _pos + new Vector3(0.1f, 0.1f, 0.9f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.9f, 0.1f, 0.1f), _pos + new Vector3(0.9f, 0.1f, 0.9f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.9f, 0.1f, 0.9f), _pos + new Vector3(0.1f, 0.1f, 0.9f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.1f, 0.9f, 0.1f), _pos + new Vector3(0.9f, 0.9f, 0.1f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.1f, 0.9f, 0.1f), _pos + new Vector3(0.1f, 0.9f, 0.9f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.9f, 0.9f, 0.1f), _pos + new Vector3(0.9f, 0.9f, 0.9f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.9f, 0.9f, 0.9f), _pos + new Vector3(0.1f, 0.9f, 0.9f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.1f, 0.1f, 0.1f), _pos + new Vector3(0.1f, 0.9f, 0.1f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.1f, 0.1f, 0.9f), _pos + new Vector3(0.1f, 0.9f, 0.9f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.9f, 0.1f, 0.1f), _pos + new Vector3(0.9f, 0.9f, 0.1f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.9f, 0.1f, 0.9f), _pos + new Vector3(0.9f, 0.9f, 0.9f), _col, 10f);
	}

	public static string SafeStringFormat(string _s)
	{
		return _s.Replace("{", "{{").Replace("}", "}}");
	}

	public static Vector3 GetNormalFromHitInfo(Vector3i _blockPos, Collider _hitCollider, int _hitTriangleIdx, out Vector3 _hitFaceCenter)
	{
		_hitFaceCenter = Vector3.zero;
		if (_hitTriangleIdx < 0)
		{
			return Vector3.zero;
		}
		MeshCollider meshCollider = _hitCollider as MeshCollider;
		if (meshCollider != null && meshCollider.sharedMesh != null && meshCollider.sharedMesh.isReadable)
		{
			Mesh sharedMesh = meshCollider.sharedMesh;
			tempVertices.Clear();
			sharedMesh.GetVertices(tempVertices);
			tempTriangles.Clear();
			sharedMesh.GetTriangles(tempTriangles, 0);
			int num = _hitTriangleIdx * 3;
			if (num >= tempTriangles.Count || tempTriangles[num] >= tempVertices.Count)
			{
				return Vector3.zero;
			}
			Vector3 vector = tempVertices[tempTriangles[num]];
			Vector3 vector2 = tempVertices[tempTriangles[num + 1]];
			Vector3 vector3 = tempVertices[tempTriangles[num + 2]];
			Vector3 result = Vector3.Cross(vector - vector2, vector - vector3);
			Vector3 vector4 = (vector + vector2 + vector3) / 3f;
			_hitFaceCenter = vector4 + World.toChunkXyzWorldPos(_blockPos);
			return result;
		}
		return Vector3.zero;
	}

	public static BlockFace GetBlockFaceFromHitInfo(Vector3i _blockPos, BlockValue _blockValue, Collider _hitCollider, int _hitTriangleIdx, out Vector3 _hitFaceCenter, out Vector3 _hitFaceNormal)
	{
		_hitFaceCenter = Vector3.zero;
		_hitFaceNormal = Vector3.zero;
		if (_hitTriangleIdx < 0)
		{
			return BlockFace.None;
		}
		MeshCollider meshCollider = _hitCollider as MeshCollider;
		if (meshCollider != null && meshCollider.sharedMesh != null && meshCollider.sharedMesh.isReadable)
		{
			Mesh sharedMesh = meshCollider.sharedMesh;
			tempVertices.Clear();
			sharedMesh.GetVertices(tempVertices);
			tempTriangles.Clear();
			sharedMesh.GetTriangles(tempTriangles, 0);
			int num = _hitTriangleIdx * 3;
			if (num >= tempTriangles.Count || tempTriangles[num] >= tempVertices.Count)
			{
				return BlockFace.None;
			}
			Vector3 vector = tempVertices[tempTriangles[num]];
			Vector3 vector2 = tempVertices[tempTriangles[num + 1]];
			Vector3 vector3 = tempVertices[tempTriangles[num + 2]];
			_hitFaceNormal = Vector3.Cross(vector - vector2, vector - vector3);
			Vector3 vector4 = (vector + vector2 + vector3) / 3f;
			_hitFaceCenter = vector4 + World.toChunkXyzWorldPos(_blockPos);
			Vector3 vector5 = World.toBlock(_blockPos).ToVector3();
			vector -= vector5;
			vector2 -= vector5;
			vector3 -= vector5;
			if (!_blockValue.Block.isMultiBlock)
			{
				if ((double)vector.x < -0.001)
				{
					vector.x += 16f;
				}
				else if (vector.x > 15f)
				{
					vector.x -= 16f;
				}
				if ((double)vector.y < -0.001)
				{
					vector.y += 16f;
				}
				else if (vector.y > 15f)
				{
					vector.y -= 16f;
				}
				if ((double)vector.z < -0.001)
				{
					vector.z += 16f;
				}
				else if (vector.z > 15f)
				{
					vector.z -= 16f;
				}
				if ((double)vector2.x < -0.001)
				{
					vector2.x += 16f;
				}
				else if (vector2.x > 15f)
				{
					vector2.x -= 16f;
				}
				if ((double)vector2.y < -0.001)
				{
					vector2.y += 16f;
				}
				else if (vector2.y > 15f)
				{
					vector2.y -= 16f;
				}
				if ((double)vector2.z < -0.001)
				{
					vector2.z += 16f;
				}
				else if (vector2.z > 15f)
				{
					vector2.z -= 16f;
				}
				if ((double)vector3.x < -0.001)
				{
					vector3.x += 16f;
				}
				else if (vector3.x > 15f)
				{
					vector3.x -= 16f;
				}
				if ((double)vector3.y < -0.001)
				{
					vector3.y += 16f;
				}
				else if (vector3.y > 15f)
				{
					vector3.y -= 16f;
				}
				if ((double)vector3.z < -0.001)
				{
					vector3.z += 16f;
				}
				else if (vector3.z > 15f)
				{
					vector3.z -= 16f;
				}
			}
			if (_blockValue.Block.shape is BlockShapeNew blockShapeNew)
			{
				Vector3 vector6 = Vector3.one * 0.5f;
				Quaternion quaternion = Quaternion.Inverse(blockShapeNew.GetRotation(_blockValue));
				vector = quaternion * (vector - vector6) + vector6;
				vector2 = quaternion * (vector2 - vector6) + vector6;
				vector3 = quaternion * (vector3 - vector6) + vector6;
				return blockShapeNew.GetBlockFaceFromColliderTriangle(_blockValue, vector, vector2, vector3);
			}
		}
		return BlockFace.None;
	}

	public static string GetLaunchArgument(string _argumentName)
	{
		if (arguments == null)
		{
			arguments = new CaseInsensitiveStringDictionary<string>();
			string[] commandLineArgs = GameStartupHelper.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				if (!string.IsNullOrEmpty(commandLineArgs[i]) && commandLineArgs[i][0] == '-')
				{
					int num = commandLineArgs[i].IndexOf('=');
					string key;
					string value;
					if (num >= 0)
					{
						key = commandLineArgs[i].Substring(1, num - 1);
						value = commandLineArgs[i].Substring(num + 1);
					}
					else
					{
						key = commandLineArgs[i].Substring(1);
						value = string.Empty;
					}
					arguments[key] = value;
				}
			}
		}
		if (arguments.ContainsKey(_argumentName))
		{
			return arguments[_argumentName];
		}
		return null;
	}

	public static bool IsBlockOrTerrain(string _tag)
	{
		switch (_tag)
		{
		default:
			return _tag == "T_Deco";
		case "B_Mesh":
		case "T_Mesh":
		case "T_Mesh_B":
		case "T_Block":
			return true;
		}
	}

	public static bool IsBlockOrTerrain(Component component)
	{
		if (!component.CompareTag("B_Mesh") && !component.CompareTag("T_Mesh") && !component.CompareTag("T_Mesh_B") && !component.CompareTag("T_Block"))
		{
			return component.CompareTag("T_Deco");
		}
		return true;
	}

	public static ulong Vector3iToUInt64(Vector3i _v)
	{
		return (ulong)(((long)((_v.x + 32768) & 0xFFFF) << 32) | ((long)((_v.y + 32768) & 0xFFFF) << 16)) | ((ulong)(_v.z + 32768) & 0xFFFFuL);
	}

	public static Vector3i UInt64ToVector3i(ulong _fullValue)
	{
		return new Vector3i((int)((_fullValue >> 32) & 0xFFFF) - 32768, (int)((_fullValue >> 16) & 0xFFFF) - 32768, (int)(_fullValue & 0xFFFF) - 32768);
	}

	public static char ValidateGameNameInput(string _text, int _charIndex, char _addedChar)
	{
		if (_addedChar >= 'Ā')
		{
			return _addedChar;
		}
		if ((_addedChar >= 'a' && _addedChar <= 'z') || (_addedChar >= 'A' && _addedChar <= 'Z') || (_addedChar >= '0' && _addedChar <= '9'))
		{
			return _addedChar;
		}
		if (_addedChar == '_' || _addedChar == '-')
		{
			return _addedChar;
		}
		if (_charIndex > 0 && (_addedChar == '.' || _addedChar == ' '))
		{
			return _addedChar;
		}
		return '\0';
	}

	public static char ValidateHexInput(string _text, int _charIndex, char _addedChar)
	{
		if ((_addedChar >= 'a' && _addedChar <= 'f') || (_addedChar >= 'A' && _addedChar <= 'F') || (_addedChar >= '0' && _addedChar <= '9'))
		{
			return _addedChar;
		}
		return '\0';
	}

	public static bool ValidateGameName(string _gameName)
	{
		string text = _gameName.Trim();
		if (string.IsNullOrEmpty(text) || text.Length != _gameName.Length)
		{
			return false;
		}
		for (int i = 0; i < _gameName.Length; i++)
		{
			if (ValidateGameNameInput(_gameName, i, _gameName[i]) == '\0')
			{
				return false;
			}
		}
		if (_gameName.EndsWith("."))
		{
			return false;
		}
		return true;
	}

	public static PrefabInstance FindPrefabForBlockPos(List<PrefabInstance> prefabs, Vector3i hitPointBlockPos)
	{
		for (int i = 0; i < prefabs.Count; i++)
		{
			if (prefabs[i].boundingBoxPosition.x <= hitPointBlockPos.x && prefabs[i].boundingBoxPosition.x + prefabs[i].boundingBoxSize.x >= hitPointBlockPos.x && prefabs[i].boundingBoxPosition.z <= hitPointBlockPos.z && prefabs[i].boundingBoxPosition.z + prefabs[i].boundingBoxSize.z >= hitPointBlockPos.z)
			{
				return prefabs[i];
			}
		}
		return null;
	}

	public static int FindPaintIdForBlockFace(BlockValue _bv, BlockFace blockFace, out string _name, int _channel)
	{
		int sideTextureId = _bv.Block.GetSideTextureId(_bv, blockFace, _channel);
		for (int i = 0; i < BlockTextureData.list.Length; i++)
		{
			if (BlockTextureData.list[i] != null && BlockTextureData.list[i].TextureID == sideTextureId)
			{
				_name = BlockTextureData.list[i].Name;
				return i;
			}
		}
		_name = string.Empty;
		return 0;
	}

	public static Vector3i Mirror(EnumMirrorAlong _axis, Vector3i _pos, Vector3i _prefabSize)
	{
		return _axis switch
		{
			EnumMirrorAlong.XAxis => new Vector3i(_prefabSize.x - _pos.x - 1, _pos.y, _pos.z), 
			EnumMirrorAlong.YAxis => new Vector3i(_pos.x, _prefabSize.y - _pos.y - 1, _pos.z), 
			_ => new Vector3i(_pos.x, _pos.y, _prefabSize.z - _pos.z - 1), 
		};
	}

	public static Vector3 Mirror(EnumMirrorAlong _axis, Vector3 _pos, Vector3i _prefabSize)
	{
		return _axis switch
		{
			EnumMirrorAlong.XAxis => new Vector3((float)_prefabSize.x - _pos.x, _pos.y, _pos.z), 
			EnumMirrorAlong.YAxis => new Vector3(_pos.x, (float)_prefabSize.y - _pos.y, _pos.z), 
			_ => new Vector3(_pos.x, _pos.y, (float)_prefabSize.z - _pos.z), 
		};
	}

	public static void TakeScreenShot(EScreenshotMode _screenshotMode, string _overrideScreenshotFilePath = null, float _borderPerc = 0f, bool _b4to3 = false, int _rescaleToW = 0, int _rescaleToH = 0, bool _isSaveTGA = false)
	{
		ThreadManager.StartCoroutine(TakeScreenshotEnum(_screenshotMode, _overrideScreenshotFilePath, _borderPerc, _b4to3, _rescaleToW, _rescaleToH, _isSaveTGA));
	}

	public static IEnumerator TakeScreenshotEnum(EScreenshotMode _screenshotMode, string _overrideScreenshotFilePath = null, float _borderPerc = 0f, bool _b4to3 = false, int _rescaleToW = 0, int _rescaleToH = 0, bool _isSaveTGA = false)
	{
		yield return new WaitForEndOfFrame();
		Rect screenshotRect = GetScreenshotRect(_borderPerc, _b4to3);
		Texture2D texture2D = new Texture2D((int)screenshotRect.width, (int)screenshotRect.height, TextureFormat.RGB24, mipChain: false);
		texture2D.ReadPixels(screenshotRect, 0, 0);
		if (_rescaleToW != 0 && _rescaleToH != 0)
		{
			TextureScale.Bilinear(texture2D, _rescaleToW, _rescaleToH);
		}
		texture2D.Apply();
		if (_screenshotMode != EScreenshotMode.File)
		{
			TextureUtils.CopyToClipboard(texture2D);
		}
		if (_screenshotMode != EScreenshotMode.Clipboard)
		{
			string text3;
			if (_overrideScreenshotFilePath == null)
			{
				string text = GameIO.GetUserGameDataDir() + "/Screenshots";
				if (!SdDirectory.Exists(text))
				{
					SdDirectory.CreateDirectory(text);
				}
				string text2 = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
				text3 = text + "/" + Constants.cVersionInformation.ShortString + "_" + text2;
			}
			else
			{
				text3 = _overrideScreenshotFilePath;
			}
			text3 = (lastSavedScreenshotFilename = text3 + (_isSaveTGA ? ".tga" : ".jpg"));
			if (_isSaveTGA)
			{
				SdFile.WriteAllBytes(text3, texture2D.EncodeToTGA());
			}
			else
			{
				SdFile.WriteAllBytes(text3, texture2D.EncodeToJPG());
			}
		}
		UnityEngine.Object.Destroy(texture2D);
	}

	public static Rect GetScreenshotRect(float _borderPerc = 0f, bool _b4to3 = false)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = Screen.width;
		float num4 = Screen.height;
		if (_borderPerc > 0.001f)
		{
			float num5 = (float)Screen.width * _borderPerc;
			float num6 = (float)Screen.height * _borderPerc;
			num += num5;
			num3 -= num5 * 2f;
			num2 += num6;
			num4 -= num6 * 2f;
		}
		if (_b4to3)
		{
			num3 = 1.3333334f * num4;
			num = ((float)Screen.width - num3) / 2f;
		}
		return new Rect(num, num2, num3, num4);
	}

	public static void StartPlaytesting()
	{
		if (!string.IsNullOrEmpty(GamePrefs.GetString(EnumGamePrefs.LastLoadedPrefab)))
		{
			GameManager.bHideMainMenuNextTime = true;
			GameManager.Instance.Disconnect();
			ThreadManager.StartCoroutine(startPlaytestLater());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator startPlaytestLater()
	{
		yield return new WaitForSeconds(2f);
		string value = GamePrefs.GetString(EnumGamePrefs.LastLoadedPrefab);
		GamePrefs.Set(EnumGamePrefs.GameWorld, "Playtesting");
		GamePrefs.Set(EnumGamePrefs.GameMode, EnumGameMode.Survival.ToStringCached());
		GamePrefs.Set(EnumGamePrefs.GameName, value);
		string saveGameDir = GameIO.GetSaveGameDir();
		if (SdDirectory.Exists(saveGameDir))
		{
			SdDirectory.Delete(saveGameDir, recursive: true);
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), _offline: false);
	}

	public static bool IsPlaytesting()
	{
		return GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Playtesting";
	}

	public static bool IsWorldEditor()
	{
		if (GameModeEditWorld.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)))
		{
			return GamePrefs.GetString(EnumGamePrefs.GameName) == "WorldEditor";
		}
		return false;
	}

	public static void StartSinglePrefabEditing()
	{
		GameManager.bHideMainMenuNextTime = true;
		GameManager.Instance.Disconnect();
		ThreadManager.StartCoroutine(startSinglePrefabEditingLater());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator startSinglePrefabEditingLater()
	{
		yield return new WaitForSeconds(2f);
		GamePrefs.Set(EnumGamePrefs.GameWorld, "Empty");
		GamePrefs.Set(EnumGamePrefs.GameMode, GameModeEditWorld.TypeName);
		GamePrefs.Set(EnumGamePrefs.GameName, "PrefabEditor");
		SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), _offline: false);
	}

	public static float GetOreNoiseAt(PerlinNoise _noise, int _x, int _y, int _z)
	{
		return ((float)_noise.Noise((float)_x * 0.05f, (float)_y * 0.05f, (float)_z * 0.05f) - 0.333f) * 3f;
	}

	public static bool CheckOreNoiseAt(PerlinNoise _noise, int _x, int _y, int _z)
	{
		return GetOreNoiseAt(_noise, _x, _y, _z) > 0f;
	}

	public static Color32 UIntToColor(uint color, bool _includeAlpha = false)
	{
		if (_includeAlpha)
		{
			return new Color32((byte)(color >> 16), (byte)(color >> 8), (byte)color, (byte)(color >> 24));
		}
		return new Color32((byte)(color >> 16), (byte)(color >> 8), (byte)color, byte.MaxValue);
	}

	public static uint ColorToUInt(Color32 color, bool _includeAlpha = false)
	{
		if (_includeAlpha)
		{
			return (uint)((color.r << 24) | (color.r << 16) | (color.g << 8) | color.b);
		}
		return (uint)((color.r << 16) | (color.g << 8) | color.b);
	}

	public static void WaterFloodFill(GridCompressedData<byte> _cols, byte[] _waterChunks16x16Height, int _width, HeightMap _heightMap, int _posX, int _maxY, int _posZ, byte _colWater, byte _colBorder, List<Vector2i> _listPos, int _minX = int.MinValue, int _maxX = int.MaxValue, int _minZ = int.MinValue, int _maxZ = int.MaxValue, int _worldScale = 1)
	{
		int num = _heightMap.GetHeight() * _worldScale;
		Vector2i item = default(Vector2i);
		do
		{
			int num2 = _posX + _width / 2;
			int num3 = _posZ + num / 2;
			if (_heightMap.GetAt(num2, num3) < (float)(_maxY + 1))
			{
				_cols.SetValue(num2, num3, _colWater);
				_waterChunks16x16Height[num2 / 16 + num3 / 16 * _width / 16] = (byte)_maxY;
				if (num2 < _width - 1 && _posX < _maxX && _cols.GetValue(num2 + 1, num3) == 0 && _listPos.Count < 100000)
				{
					item.x = _posX + 1;
					item.y = _posZ;
					_listPos.Add(item);
				}
				if (num2 > 0 && _posX > _minX && _cols.GetValue(num2 - 1, num3) == 0 && _listPos.Count < 100000)
				{
					item.x = _posX - 1;
					item.y = _posZ;
					_listPos.Add(item);
				}
				if (num3 > 0 && _posZ > _minZ && _cols.GetValue(num2, num3 - 1) == 0 && _listPos.Count < 100000)
				{
					item.x = _posX;
					item.y = _posZ - 1;
					_listPos.Add(item);
				}
				if (num3 < num - 1 && _posZ < _maxZ && _cols.GetValue(num2, num3 + 1) == 0 && _listPos.Count < 100000)
				{
					item.x = _posX;
					item.y = _posZ + 1;
					_listPos.Add(item);
				}
			}
			else
			{
				_cols.SetValue(num2, num3, _colBorder);
			}
			int count = _listPos.Count;
			if (count > 0)
			{
				item = _listPos[count - 1];
				_posX = item.x;
				_posZ = item.y;
				_listPos.RemoveAt(count - 1);
			}
		}
		while (_listPos.Count > 0);
	}

	public static EPlayerHomeType CheckForAnyPlayerHome(World world, Vector3i BoxMin, Vector3i BoxMax)
	{
		double num = (double)GameStats.GetInt(EnumGameStats.LandClaimExpiryTime) * 24.0;
		double num2 = (double)GameStats.GetInt(EnumGameStats.BedrollExpiryTime) * 24.0;
		int num3 = GamePrefs.GetInt(EnumGamePrefs.BedrollDeadZoneSize);
		Vector3i vector3i = new Vector3i(num3, num3, num3);
		Vector3i vector3i2 = BoxMin - vector3i;
		Vector3i vector3i3 = BoxMax + vector3i;
		int num4 = GameStats.GetInt(EnumGameStats.LandClaimSize);
		int num5 = num4 / 2;
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> player in GameManager.Instance.GetPersistentPlayerList().Players)
		{
			if (player.Value.OfflineHours < num2 && player.Value.HasBedrollPos)
			{
				Vector3i bedrollPos = player.Value.BedrollPos;
				if (bedrollPos.x >= vector3i2.x && bedrollPos.x < vector3i3.x && bedrollPos.z >= vector3i2.z && bedrollPos.z < vector3i3.z)
				{
					return EPlayerHomeType.Bedroll;
				}
			}
			List<Vector3i> lPBlocks = player.Value.LPBlocks;
			if (!(player.Value.OfflineHours < num) || lPBlocks == null || lPBlocks.Count <= 0)
			{
				continue;
			}
			for (int i = 0; i < lPBlocks.Count; i++)
			{
				Vector3i vector3i4 = lPBlocks[i];
				vector3i4.x -= num5;
				vector3i4.z -= num5;
				if (vector3i4.x <= BoxMax.x && vector3i4.x + num4 >= BoxMin.x && vector3i4.z <= BoxMax.z && vector3i4.z + num4 >= BoxMin.z)
				{
					return EPlayerHomeType.Landclaim;
				}
			}
		}
		return EPlayerHomeType.None;
	}

	public static Transform FindDeepChild(Transform _parent, string _transformName)
	{
		Transform transform = _parent.Find(_transformName);
		if (transform != null)
		{
			return transform;
		}
		int childCount = _parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			transform = FindDeepChild(_parent.GetChild(i), _transformName);
			if (transform != null)
			{
				return transform;
			}
		}
		return transform;
	}

	public static Transform FindDeepChildActive(Transform _parent, string _transformName)
	{
		Transform transform = _parent.Find(_transformName);
		if (transform != null && transform.gameObject.activeSelf)
		{
			return transform;
		}
		int childCount = _parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = _parent.GetChild(i);
			if (child.gameObject.activeSelf)
			{
				transform = FindDeepChildActive(child, _transformName);
				if (transform != null)
				{
					return transform;
				}
			}
		}
		return transform;
	}

	public static int GetViewDistance()
	{
		if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty")
		{
			return 12;
		}
		return GameStats.GetInt(EnumGameStats.AllowedViewDistance);
	}

	public static Vector3 GetUpdatedNormalAtPosition(Vector3i _worldPos, int _clrIdx, bool _saveNrmToChunk = false)
	{
		int terrainHeight = GameManager.Instance.World.GetTerrainHeight(_worldPos.x, _worldPos.z);
		int terrainHeight2 = GameManager.Instance.World.GetTerrainHeight(_worldPos.x + 1, _worldPos.z);
		byte terrainHeight3 = GameManager.Instance.World.GetTerrainHeight(_worldPos.x, _worldPos.z + 1);
		float num = (float)GameManager.Instance.World.GetDensity(_clrIdx, _worldPos.x, terrainHeight, _worldPos.z) / -128f;
		float num2 = (float)GameManager.Instance.World.GetDensity(_clrIdx, _worldPos.x + 1, terrainHeight, _worldPos.z) / -128f;
		float num3 = (float)GameManager.Instance.World.GetDensity(_clrIdx, _worldPos.x, terrainHeight, _worldPos.z + 1) / -128f;
		float num4 = (float)GameManager.Instance.World.GetDensity(_clrIdx, _worldPos.x, terrainHeight + 1, _worldPos.z) / -128f;
		float num5 = (float)GameManager.Instance.World.GetDensity(_clrIdx, _worldPos.x + 1, terrainHeight + 1, _worldPos.z) / -128f;
		float num6 = (float)GameManager.Instance.World.GetDensity(_clrIdx, _worldPos.x, terrainHeight + 1, _worldPos.z + 1) / -128f;
		if (num > 0.999f && num4 > 0.999f)
		{
			num = 0.5f;
		}
		if (num2 > 0.999f && num5 > 0.999f)
		{
			num2 = 0.5f;
		}
		if (num3 > 0.999f && num6 > 0.999f)
		{
			num3 = 0.5f;
		}
		float y = (float)terrainHeight + num;
		float y2 = (float)terrainHeight2 + num2;
		float y3 = (float)(int)terrainHeight3 + num3;
		Vector3 lhs = new Vector3(0f, y3, 1f) - new Vector3(0f, y, 0f);
		Vector3 rhs = new Vector3(1f, y2, 0f) - new Vector3(0f, y, 0f);
		return Vector3.Cross(lhs, rhs).normalized;
	}

	public static DirEightWay GetDirByNormal(Vector2 _normal)
	{
		_normal.Normalize();
		return GetDirByNormal(new Vector2i(Mathf.RoundToInt(_normal.x), Mathf.RoundToInt(_normal.y)));
	}

	public static DirEightWay GetDirByNormal(Vector2i _normal)
	{
		for (int i = 0; i < NeighborsEightWay.Count; i++)
		{
			if (NeighborsEightWay[i] == _normal)
			{
				return (DirEightWay)i;
			}
		}
		return DirEightWay.None;
	}

	public static DirEightWay GetClosestDirection(float _rotation, bool _limitTo90Degress = false)
	{
		_rotation = MathUtils.Mod(_rotation, 360f);
		if (_limitTo90Degress)
		{
			if (_rotation > 315f || _rotation <= 45f)
			{
				return DirEightWay.N;
			}
			if (_rotation <= 135f)
			{
				return DirEightWay.E;
			}
			if (_rotation <= 225f)
			{
				return DirEightWay.S;
			}
			return DirEightWay.W;
		}
		if ((double)_rotation > 337.5 || (double)_rotation <= 22.5)
		{
			return DirEightWay.N;
		}
		if ((double)_rotation <= 67.5)
		{
			return DirEightWay.NE;
		}
		if ((double)_rotation <= 112.5)
		{
			return DirEightWay.E;
		}
		if ((double)_rotation <= 157.5)
		{
			return DirEightWay.SE;
		}
		if ((double)_rotation <= 202.5)
		{
			return DirEightWay.S;
		}
		if ((double)_rotation <= 247.5)
		{
			return DirEightWay.SW;
		}
		if ((double)_rotation <= 292.5)
		{
			return DirEightWay.W;
		}
		return DirEightWay.NW;
	}

	public static void DestroyAllChildrenBut(Transform t, string _excluded)
	{
		bool isPlaying = Application.isPlaying;
		int num = 0;
		List<string> list = new List<string>(_excluded.Split(','));
		while (t.childCount != num)
		{
			Transform child = t.GetChild(num);
			if (list.Contains(child.name))
			{
				num++;
			}
			else if (isPlaying)
			{
				child.parent = null;
				UnityEngine.Object.Destroy(child.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(child.gameObject);
			}
		}
	}

	public static void DestroyAllChildrenBut(Transform t, List<string> _excluded)
	{
		bool isPlaying = Application.isPlaying;
		for (int num = t.childCount - 1; num >= 0; num--)
		{
			Transform child = t.GetChild(num);
			if (!_excluded.Contains(child.name))
			{
				if (isPlaying)
				{
					child.SetParent(null, worldPositionStays: false);
					UnityEngine.Object.Destroy(child.gameObject);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(child.gameObject);
				}
			}
		}
	}

	public static void DestroyAllChildrenImmediatelyBut(Transform t, List<string> _excluded)
	{
		for (int num = t.childCount - 1; num >= 0; num--)
		{
			Transform child = t.GetChild(num);
			if (!_excluded.Contains(child.name))
			{
				UnityEngine.Object.DestroyImmediate(child.gameObject);
			}
		}
	}

	public static void SetMeshVertexAttributes(Mesh mesh, bool compressPosition = true)
	{
		VertexAttributeDescriptor[] vertexAttributes = mesh.GetVertexAttributes();
		ApplyCompressedVertexAttributes(vertexAttributes, compressPosition);
		mesh.SetVertexBufferParams(mesh.vertexCount, vertexAttributes);
	}

	public static void ApplyCompressedVertexAttributes(Span<VertexAttributeDescriptor> attributes, bool compressPosition = true)
	{
		for (int i = 0; i < attributes.Length; i++)
		{
			VertexAttributeDescriptor vertexAttributeDescriptor = attributes[i];
			VertexAttribute attribute = vertexAttributeDescriptor.attribute;
			if (attribute == VertexAttribute.Position && !compressPosition)
			{
				vertexAttributeDescriptor.format = VertexAttributeFormat.Float32;
				vertexAttributeDescriptor.dimension = 3;
			}
			else
			{
				switch (attribute)
				{
				case VertexAttribute.Position:
				case VertexAttribute.Normal:
					vertexAttributeDescriptor.format = VertexAttributeFormat.Float16;
					vertexAttributeDescriptor.dimension = 4;
					break;
				case VertexAttribute.Tangent:
				case VertexAttribute.Color:
					vertexAttributeDescriptor.format = VertexAttributeFormat.Float16;
					vertexAttributeDescriptor.dimension = 4;
					break;
				case VertexAttribute.TexCoord0:
				case VertexAttribute.TexCoord1:
				case VertexAttribute.TexCoord2:
				case VertexAttribute.TexCoord3:
					vertexAttributeDescriptor.format = VertexAttributeFormat.Float16;
					vertexAttributeDescriptor.dimension = 2;
					break;
				}
			}
			attributes[i] = vertexAttributeDescriptor;
		}
	}
}
