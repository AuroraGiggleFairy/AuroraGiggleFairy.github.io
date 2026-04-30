using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Platform;
using Twitch;

public class EntityBuffs
{
	public enum BuffStatus
	{
		Added,
		FailedInvalidName,
		FailedImmune,
		FailedFriendlyFire,
		FailedEditor,
		FailedGameStat
	}

	public static byte Version = 3;

	public EntityAlive parent;

	public List<BuffValue> ActiveBuffs;

	[PublicizedFrom(EAccessModifier.Private)]
	public CaseInsensitiveStringDictionary<float> CVars;

	[PublicizedFrom(EAccessModifier.Private)]
	public CaseInsensitiveStringDictionary<float> CVarsLastNetSync;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<string> TrackedCVars;

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> physicalDamageTypes = FastTags<TagGroup.Global>.Parse("piercing,bashing,slashing,crushing,none,corrosive,barbedwire");

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> CVarsToSend = new List<string>();

	public EntityBuffs(EntityAlive _parent)
	{
		parent = _parent;
		ActiveBuffs = new List<BuffValue>();
		CVars = new CaseInsensitiveStringDictionary<float>();
		CVarsLastNetSync = new CaseInsensitiveStringDictionary<float>();
		TrackedCVars = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		AddCustomVar("_difficulty", GameStats.GetInt(EnumGameStats.GameDifficulty));
	}

	public void Tick()
	{
		int num = ActiveBuffs.Count;
		for (int i = 0; i < num; i++)
		{
			BuffValue buffValue = ActiveBuffs[i];
			if (buffValue.Invalid)
			{
				ActiveBuffs.RemoveAt(i);
				i--;
				num--;
				continue;
			}
			parent.MinEventContext.Buff = buffValue;
			if (parent.MinEventContext.Other == null)
			{
				parent.MinEventContext.Other = parent.GetAttackTarget();
			}
			if (buffValue.Finished)
			{
				FireEvent(MinEventTypes.onSelfBuffFinish, buffValue.BuffClass, parent.MinEventContext);
				buffValue.Remove = true;
			}
			if (buffValue.Remove)
			{
				if (buffValue.BuffClass != null)
				{
					FireEvent(MinEventTypes.onSelfBuffRemove, buffValue.BuffClass, parent.MinEventContext);
					if (!buffValue.BuffClass.Hidden)
					{
						parent.Stats.EntityBuffRemoved(buffValue);
					}
				}
				ActiveBuffs.RemoveAt(i);
				i--;
				num--;
			}
			else
			{
				if (buffValue.Paused || parent.bDead)
				{
					continue;
				}
				if (!buffValue.Started)
				{
					parent.MinEventContext.Instigator = null;
					if (buffValue.InstigatorId != -1)
					{
						parent.MinEventContext.Instigator = GameManager.Instance.World.GetEntity(buffValue.InstigatorId) as EntityAlive;
					}
					FireEvent(MinEventTypes.onSelfBuffStart, buffValue.BuffClass, parent.MinEventContext);
					buffValue.Started = true;
					if (!buffValue.BuffClass.Hidden)
					{
						parent.Stats.EntityBuffAdded(buffValue);
					}
					parent.BuffAdded(buffValue);
				}
				buffValue.Tick();
				if (buffValue.Update)
				{
					FireEvent(MinEventTypes.onSelfBuffUpdate, buffValue.BuffClass, parent.MinEventContext);
					buffValue.Update = false;
				}
			}
		}
		parent.MinEventContext.Buff = null;
	}

	public void ModifyValue(PassiveEffects _effect, ref float _value, ref float _perc_val, FastTags<TagGroup.Global> _tags)
	{
		for (int i = 0; i < ActiveBuffs.Count; i++)
		{
			BuffValue buffValue = ActiveBuffs[i];
			BuffClass buffClass = buffValue.BuffClass;
			if (buffClass != null && !buffValue.Paused)
			{
				buffClass.ModifyValue(parent, _effect, buffValue, ref _value, ref _perc_val, _tags);
			}
		}
	}

	public void GetModifiedValueData(List<EffectManager.ModifierValuesAndSources> _modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType _sourceType, PassiveEffects _effect, ref float _value, ref float _perc_val, FastTags<TagGroup.Global> _tags)
	{
		for (int i = 0; i < ActiveBuffs.Count; i++)
		{
			BuffValue buffValue = ActiveBuffs[i];
			BuffClass buffClass = buffValue.BuffClass;
			if (buffClass != null && !buffValue.Paused)
			{
				buffClass.GetModifiedValueData(_modValueSources, _sourceType, parent, _effect, buffValue, ref _value, ref _perc_val, _tags);
			}
		}
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _params)
	{
		for (int i = 0; i < ActiveBuffs.Count; i++)
		{
			BuffValue buffValue = ActiveBuffs[i];
			BuffClass buffClass = buffValue.BuffClass;
			if (buffClass != null && !buffValue.Paused)
			{
				buffClass.FireEvent(_eventType, _params);
			}
		}
	}

	public void FireEvent(MinEventTypes _eventType, BuffClass _buffClass, MinEventParams _params)
	{
		_buffClass?.FireEvent(_eventType, _params);
	}

	public BuffStatus AddBuff(string _name, int _instigatorId = -1, bool _netSync = true, bool _fromElectrical = false, float _buffDuration = -1f)
	{
		return AddBuff(_name, Vector3i.zero, _instigatorId, _netSync, _fromElectrical, _buffDuration);
	}

	public BuffStatus AddBuff(string _name, Vector3i _instigatorPos, int _instigatorId = -1, bool _netSync = true, bool _fromElectrical = false, float _buffDuration = -1f)
	{
		int num = -1;
		if (_fromElectrical)
		{
			num = _instigatorId;
			_instigatorId = -1;
		}
		BuffClass buff = BuffManager.GetBuff(_name);
		if (buff == null)
		{
			return BuffStatus.FailedInvalidName;
		}
		if (!buff.AllowInEditor && parent.world.IsEditor())
		{
			return BuffStatus.FailedEditor;
		}
		if (buff.RequiredGameStat != EnumGameStats.Last && !GameStats.GetBool(buff.RequiredGameStat))
		{
			return BuffStatus.FailedGameStat;
		}
		if (_netSync && HasImmunity(buff))
		{
			return BuffStatus.FailedImmune;
		}
		if (buff.DamageType != EnumDamageTypes.None && _instigatorId != parent.entityId && !parent.FriendlyFireCheck(GameManager.Instance.World.GetEntity(_instigatorId) as EntityAlive))
		{
			return BuffStatus.FailedFriendlyFire;
		}
		for (int i = 0; i < ActiveBuffs.Count; i++)
		{
			BuffValue buffValue = ActiveBuffs[i];
			if (!(buffValue.BuffClass.Name == buff.Name))
			{
				continue;
			}
			if (_buffDuration >= 0f)
			{
				buffValue.BuffClass.DurationMax = _buffDuration;
			}
			switch (buff.StackType)
			{
			case BuffEffectStackTypes.Ignore:
				if (buffValue.Remove)
				{
					buffValue.Remove = false;
				}
				break;
			case BuffEffectStackTypes.Replace:
				buffValue.DurationInTicks = 0u;
				FireEvent(MinEventTypes.onSelfBuffStack, buff, parent.MinEventContext);
				break;
			case BuffEffectStackTypes.Duration:
			{
				float num2 = _buffDuration - buffValue.DurationInSeconds;
				float num3 = buffValue.BuffClass.InitialDurationMax;
				if (_buffDuration >= 0f)
				{
					num3 = _buffDuration;
				}
				if (num2 > num3)
				{
					num3 = num2;
				}
				buffValue.DurationInTicks = 0u;
				buffValue.BuffClass.DurationMax = num3;
				FireEvent(MinEventTypes.onSelfBuffStack, buff, parent.MinEventContext);
				break;
			}
			case BuffEffectStackTypes.Effect:
				buffValue.StackEffectMultiplier++;
				FireEvent(MinEventTypes.onSelfBuffStack, buff, parent.MinEventContext);
				break;
			}
			if (_netSync)
			{
				AddBuffNetwork(_name, _buffDuration, _instigatorPos, _instigatorId);
			}
			return BuffStatus.Added;
		}
		if (!parent.isEntityRemote && parent.entityType == EntityType.Player && buff.Name.EqualsCaseInsensitive("buffLegBroken"))
		{
			PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.LegBroken, 1);
		}
		if (_fromElectrical)
		{
			_instigatorId = num;
		}
		BuffValue buffValue2 = new BuffValue(buff.Name, _instigatorPos, _instigatorId, buff);
		if (_buffDuration >= 0f)
		{
			buffValue2.BuffClass.DurationMax = _buffDuration;
		}
		else
		{
			buffValue2.BuffClass.DurationMax = buffValue2.BuffClass.InitialDurationMax;
		}
		ActiveBuffs.Add(buffValue2);
		if (_netSync)
		{
			AddBuffNetwork(_name, _buffDuration, _instigatorPos, _instigatorId);
		}
		return BuffStatus.Added;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddBuffNetwork(string _name, float _duration, Vector3i _instigatorPos, int _instigatorId = -1)
	{
		NetPackageAddRemoveBuff package = NetPackageManager.GetPackage<NetPackageAddRemoveBuff>().Setup(parent.entityId, _name, _duration, _adding: true, _instigatorId, _instigatorPos);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, -1, -1, parent.entityId);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
	}

	public void RemoveBuff(string _name, bool _netSync = true)
	{
		BuffClass buff = BuffManager.GetBuff(_name);
		if (buff == null)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < ActiveBuffs.Count; i++)
		{
			if (ActiveBuffs[i].BuffClass.Name == buff.Name)
			{
				ActiveBuffs[i].Remove = true;
				flag = true;
			}
		}
		if (flag && _netSync)
		{
			RemoveBuffNetwork(_name);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveBuffNetwork(string _name)
	{
		NetPackageAddRemoveBuff package = NetPackageManager.GetPackage<NetPackageAddRemoveBuff>().Setup(parent.entityId, _name, -1f, _adding: false, parent.entityId, Vector3i.zero);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, -1, -1, parent.entityId);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool HasBuff(string _name)
	{
		return GetBuff(_name) != null;
	}

	public bool HasBuffByTag(FastTags<TagGroup.Global> _tags)
	{
		for (int i = 0; i < ActiveBuffs.Count; i++)
		{
			BuffValue buffValue = ActiveBuffs[i];
			if (buffValue != null && _tags.Test_AnySet(buffValue.BuffClass.Tags))
			{
				return true;
			}
		}
		return false;
	}

	public BuffValue GetBuff(string _buffName)
	{
		BuffClass buff = BuffManager.GetBuff(_buffName);
		if (buff == null)
		{
			return null;
		}
		int count = ActiveBuffs.Count;
		for (int i = 0; i < count; i++)
		{
			BuffValue buffValue = ActiveBuffs[i];
			if (buffValue != null)
			{
				BuffClass buffClass = buffValue.BuffClass;
				if (buffClass != null && buffClass.Name == buff.Name)
				{
					return buffValue;
				}
			}
		}
		return null;
	}

	public void OnDeath(EntityAlive _entityThatKilledMe, bool _blockKilledMe, FastTags<TagGroup.Global> _damageTypeTags)
	{
		if (_entityThatKilledMe != null)
		{
			if (_entityThatKilledMe.entityId == parent.entityId)
			{
				parent.FireEvent(MinEventTypes.onSelfKilledSelf);
			}
			else
			{
				parent.MinEventContext.Other = _entityThatKilledMe;
				parent.FireEvent(MinEventTypes.onOtherKilledSelf);
			}
		}
		else if (_blockKilledMe)
		{
			parent.FireEvent(MinEventTypes.onBlockKilledSelf);
		}
		parent.FireEvent(MinEventTypes.onSelfDied);
		List<int> list = new List<int>();
		bool flag = parent is EntityPlayer;
		for (int i = 0; i < ActiveBuffs.Count; i++)
		{
			BuffValue buffValue = ActiveBuffs[i];
			if (buffValue == null || buffValue.BuffClass == null)
			{
				continue;
			}
			if (!flag && buffValue.BuffClass.RemoveOnDeath && !buffValue.Paused)
			{
				buffValue.Remove = true;
			}
			if (buffValue.BuffClass.DamageType == EnumDamageTypes.None || buffValue.Invalid || !buffValue.Started || (buffValue.InstigatorId == -1 && buffValue.InstigatorPos == Vector3i.zero) || buffValue.InstigatorId == parent.entityId || (_entityThatKilledMe != null && buffValue.InstigatorId == _entityThatKilledMe.entityId))
			{
				continue;
			}
			if (_entityThatKilledMe != null && buffValue.InstigatorPos != Vector3i.zero)
			{
				_entityThatKilledMe = null;
				parent.ClearEntityThatKilledMe();
			}
			if (list.Contains(buffValue.InstigatorId))
			{
				continue;
			}
			if (flag)
			{
				EntityAlive entityAlive = null;
				entityAlive = ((!(_entityThatKilledMe != null)) ? (GameManager.Instance.World.GetEntity(buffValue.InstigatorId) as EntityAlive) : _entityThatKilledMe);
				if (buffValue.BuffClass.DamageType == EnumDamageTypes.BloodLoss || buffValue.BuffClass.DamageType == EnumDamageTypes.Electrical || buffValue.BuffClass.DamageType == EnumDamageTypes.Radiation || buffValue.BuffClass.DamageType == EnumDamageTypes.Heat || buffValue.BuffClass.DamageType == EnumDamageTypes.Cold)
				{
					TwitchManager.Current.CheckKiller(parent as EntityPlayer, entityAlive, buffValue.InstigatorPos);
				}
			}
			EntityPlayerLocal entityPlayerLocal = GameManager.Instance.World.GetEntity(buffValue.InstigatorId) as EntityPlayerLocal;
			if (entityPlayerLocal == null)
			{
				continue;
			}
			if (!_damageTypeTags.Test_AnySet(physicalDamageTypes))
			{
				if (parent.Buffs.GetCustomVar("ETrapHit") == 1f)
				{
					float value = EffectManager.GetValue(PassiveEffects.ElectricalTrapXP, entityPlayerLocal.inventory.holdingItemItemValue, 0f, entityPlayerLocal);
					if (value > 0f)
					{
						entityPlayerLocal.AddKillXP(parent, value);
						parent.AwardKill(entityPlayerLocal);
					}
				}
				else
				{
					entityPlayerLocal.AddKillXP(parent);
					parent.AwardKill(entityPlayerLocal);
				}
			}
			list.Add(entityPlayerLocal.entityId);
		}
		Tick();
	}

	public void RemoveBuffsByTag(FastTags<TagGroup.Global> tags)
	{
		for (int i = 0; i < ActiveBuffs.Count; i++)
		{
			BuffValue buffValue = ActiveBuffs[i];
			if (buffValue.BuffClass.Tags.Test_AnySet(tags))
			{
				buffValue.Remove = true;
				RemoveBuffNetwork(buffValue.BuffName);
			}
		}
	}

	public void RemoveDeathBuffs(FastTags<TagGroup.Global> excludeTags)
	{
		for (int i = 0; i < ActiveBuffs.Count; i++)
		{
			BuffValue buffValue = ActiveBuffs[i];
			if (buffValue.BuffClass.RemoveOnDeath && !buffValue.BuffClass.Tags.Test_AnySet(excludeTags))
			{
				buffValue.Remove = true;
				RemoveBuffNetwork(buffValue.BuffName);
			}
		}
	}

	public void AddCustomVar(string _name, float _initialValue)
	{
		SetCustomVar(_name, _initialValue);
	}

	public void RemoveCustomVar(string _name)
	{
		if (CVars.ContainsKey(_name))
		{
			CVars.Remove(_name);
		}
		if (TrackedCVars.Contains(_name))
		{
			Log.Out("CVar " + _name + " was removed.");
		}
	}

	public void SetCustomVar(string _name, float _value, bool _netSync = true, CVarOperation _operation = CVarOperation.set)
	{
		bool flag = true;
		float value;
		bool flag2 = CVars.TryGetValue(_name, out value);
		switch (_operation)
		{
		case CVarOperation.set:
		case CVarOperation.setvalue:
			if (!flag2 || value != _value)
			{
				CVars[_name] = _value;
			}
			else
			{
				flag = false;
			}
			break;
		case CVarOperation.add:
			CVars[_name] = value + _value;
			break;
		case CVarOperation.subtract:
			CVars[_name] = value - _value;
			break;
		case CVarOperation.multiply:
			CVars[_name] = value * _value;
			break;
		case CVarOperation.divide:
			CVars[_name] = value / ((_value == 0f) ? 0.0001f : _value);
			break;
		case CVarOperation.percentadd:
			CVars[_name] = value + value * _value;
			break;
		case CVarOperation.percentsubtract:
			CVars[_name] = value - value * _value;
			break;
		}
		if (flag)
		{
			if (TrackedCVars.Contains(_name))
			{
				Log.Out($"CVar {_name} was set to {CVars[_name]}.");
			}
			if (_netSync && (parent.isEntityRemote || _name[0] == '%') && _name[0] != '.' && _name[0] != '_')
			{
				SetCustomVarNetwork(_name, _value, _operation);
			}
		}
	}

	public void SetCustomVarNetwork(string _name, float _value, CVarOperation _operation = CVarOperation.set)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageModifyCVar>().Setup(parent, _name, _value, _operation));
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageModifyCVar>().Setup(parent, _name, _value, _operation));
		}
	}

	public bool HasCustomVar(string _name)
	{
		return CVars.ContainsKey(_name);
	}

	public float GetCustomVar(string _name)
	{
		if (CVars.TryGetValue(_name, out var value))
		{
			return value;
		}
		return 0f;
	}

	public static int GetCustomVarId(string _name)
	{
		return _name.GetHashCode();
	}

	public void IncrementCustomVar(string _name, float _amount)
	{
		SetCustomVar(_name, _amount, _netSync: true, CVarOperation.add);
	}

	public int CountCustomVars()
	{
		return CVars.Count;
	}

	public IEnumerable<KeyValuePair<string, float>> EnumerateCustomVars(string searchString = null, bool startsWith = false)
	{
		foreach (KeyValuePair<string, float> cVar in CVars)
		{
			if (string.IsNullOrEmpty(searchString))
			{
				yield return cVar;
			}
			else if (startsWith)
			{
				if (cVar.Key.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
				{
					yield return cVar;
				}
			}
			else if (cVar.Key.Contains(searchString, StringComparison.OrdinalIgnoreCase))
			{
				yield return cVar;
			}
		}
	}

	public void TrackCustomVar(string _name, bool _isTracked)
	{
		if (_isTracked)
		{
			TrackedCVars.Add(_name);
			if (CVars.ContainsKey(_name))
			{
				Log.Out($"Tracking CVar {_name} with value {CVars[_name]}.");
			}
			else
			{
				Log.Out("Tracking CVar " + _name + ", it does not exist yet.");
			}
		}
		else
		{
			TrackedCVars.Remove(_name);
			Log.Out("Removed tracking from CVar " + _name + ".");
		}
	}

	public bool HasImmunity(BuffClass _buffClass)
	{
		if (parent.IsDead() && _buffClass.RemoveOnDeath)
		{
			return true;
		}
		if (parent.HasImmunity(_buffClass))
		{
			return true;
		}
		return parent.rand.RandomFloat <= Utils.FastClamp01(EffectManager.GetValue(PassiveEffects.BuffResistance, null, 0f, parent, null, _buffClass.NameTag));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeBuff(BuffValue _buffValue)
	{
		_buffValue.Remove = true;
	}

	public void Write(BinaryWriter _bw, bool _netSync = false)
	{
		_bw.Write(Version);
		_bw.Write((ushort)ActiveBuffs.Count);
		for (int i = 0; i < ActiveBuffs.Count; i++)
		{
			ActiveBuffs[i].Write(_bw);
		}
		CVarsToSend.Clear();
		foreach (string key in CVars.Keys)
		{
			if ((_netSync || CVars[key] != 0f) && key[0] != '.' && (!_netSync || !CVarsLastNetSync.ContainsKey(key) || CVars[key] != CVarsLastNetSync[key]))
			{
				CVarsToSend.Add(key);
			}
		}
		if (_netSync)
		{
			CVarsLastNetSync.Clear();
		}
		_bw.Write((ushort)CVarsToSend.Count);
		for (int j = 0; j < CVarsToSend.Count; j++)
		{
			_bw.Write(CVarsToSend[j]);
			_bw.Write(CVars[CVarsToSend[j]]);
			if (_netSync)
			{
				CVarsLastNetSync.Add(CVarsToSend[j], CVars[CVarsToSend[j]]);
			}
		}
	}

	public void Read(BinaryReader _br)
	{
		int num = _br.ReadByte();
		int num2 = _br.ReadUInt16();
		ActiveBuffs.Clear();
		if (num2 > 0)
		{
			for (int i = 0; i < num2; i++)
			{
				BuffValue buffValue = new BuffValue();
				buffValue.Read(_br, num);
				if (buffValue.BuffClass != null && (!(buffValue.BuffClass.Name == "god") || parent.world.IsEditor() || GameModeCreative.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)) || parent.IsGodMode.Value))
				{
					ActiveBuffs.Add(buffValue);
					if (!buffValue.BuffClass.Hidden)
					{
						parent.Stats.EntityBuffAdded(buffValue);
					}
				}
			}
		}
		if (num < 2)
		{
			int num3 = _br.ReadUInt16();
			Dictionary<int, float> dictionary = new Dictionary<int, float>();
			if (num3 > 0)
			{
				for (int j = 0; j < num3; j++)
				{
					dictionary[_br.ReadInt32()] = _br.ReadSingle();
				}
			}
		}
		else
		{
			int num4 = _br.ReadUInt16();
			if (num4 > 0)
			{
				for (int k = 0; k < num4; k++)
				{
					SetCustomVar(_br.ReadString(), _br.ReadSingle(), _netSync: false);
				}
			}
		}
		AddCustomVar("_difficulty", GameStats.GetInt(EnumGameStats.GameDifficulty));
	}

	public void UnPauseAll()
	{
		for (int i = 0; i < ActiveBuffs.Count; i++)
		{
			ActiveBuffs[i].Paused = false;
		}
	}

	public void ClearBuffClassLinks()
	{
		foreach (BuffValue activeBuff in ActiveBuffs)
		{
			activeBuff?.ClearBuffClassLink();
		}
	}
}
