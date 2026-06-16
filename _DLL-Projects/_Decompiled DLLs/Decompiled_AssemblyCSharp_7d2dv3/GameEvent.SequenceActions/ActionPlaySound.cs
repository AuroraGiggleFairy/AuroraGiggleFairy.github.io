using System.Collections;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionPlaySound : ActionBaseTargetAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool insideHead;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool behindPlayer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float duration;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float delay;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool canDisable = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] soundNames;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSound = "sound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropInsideHead = "inside_head";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBehindPlayer = "behind_player";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropLoopDuration = "loop_duration";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDelay = "delay";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCanDisable = "can_disable";

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		if (target != null)
		{
			if (canDisable && target is EntityAlive entity && EffectManager.GetValue(PassiveEffects.DisableGameEventNotify, null, 0f, entity) > 0f)
			{
				return ActionCompleteStates.Complete;
			}
			if (insideHead)
			{
				EntityPlayer entityPlayer = target as EntityPlayer;
				if (entityPlayer != null)
				{
					if (entityPlayer is EntityPlayerLocal)
					{
						OnClientPerform(entityPlayer);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(base.Owner.Name, entityPlayer.entityId, base.Owner.ExtraData, base.Owner.Tag, NetPackageGameEventResponse.ResponseTypes.ClientSequenceAction, -1, -1, _isDespawn: false, GetActionKey()), _onlyClientsAttachedToAnEntity: false, entityPlayer.entityId);
					}
				}
			}
			else if (soundNames != null)
			{
				if (duration == 0f)
				{
					if (behindPlayer)
					{
						Manager.BroadcastPlayByLocalPlayer(target.position + (target.transform.forward * -1f + new Vector3(Random.Range(-5f, 5f), 0f, 0f)), soundNames[Random.Range(0, soundNames.Length)]);
					}
					else
					{
						Manager.BroadcastPlayByLocalPlayer(target.position, soundNames[Random.Range(0, soundNames.Length)]);
					}
				}
				else
				{
					Vector3 position = (behindPlayer ? (target.position + (target.transform.forward * -1f + new Vector3(Random.Range(-5f, 5f), 0f, 0f))) : target.position);
					string text = soundNames[Random.Range(0, soundNames.Length)];
					Manager.BroadcastPlayByLocalPlayer(position, text);
					GameManager.Instance.StartCoroutine(StopSound(position, text));
				}
			}
		}
		else if (base.Owner.TargetPosition != Vector3.zero && soundNames != null)
		{
			if (duration == 0f)
			{
				Manager.BroadcastPlay(base.Owner.TargetPosition, soundNames[Random.Range(0, soundNames.Length)]);
			}
			else
			{
				Vector3 targetPosition = base.Owner.TargetPosition;
				string text2 = soundNames[Random.Range(0, soundNames.Length)];
				Manager.BroadcastPlay(targetPosition, text2);
				GameManager.Instance.StartCoroutine(StopSound(targetPosition, text2));
			}
		}
		return ActionCompleteStates.Complete;
	}

	public override void OnClientPerform(Entity target)
	{
		if (duration == 0f)
		{
			Manager.PlayInsidePlayerHead(soundNames[Random.Range(0, soundNames.Length)], target.entityId, delay);
			return;
		}
		string text = soundNames[Random.Range(0, soundNames.Length)];
		Manager.PlayInsidePlayerHead(text, target.entityId, delay);
		GameManager.Instance.StartCoroutine(StopInHeadSound(text));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator StopSound(Vector3 position, string soundName)
	{
		yield return new WaitForSeconds(duration + 0.001f);
		Manager.BroadcastStop(position, soundName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator StopInHeadSound(string soundName)
	{
		yield return new WaitForSeconds(duration + 0.001f);
		Manager.StopLoopInsidePlayerHead(soundName);
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropSound))
		{
			soundNames = properties.Values[PropSound].Split(',');
		}
		if (properties.Values.ContainsKey(PropInsideHead))
		{
			insideHead = StringParsers.ParseBool(properties.Values[PropInsideHead]);
		}
		if (properties.Values.ContainsKey(PropBehindPlayer))
		{
			behindPlayer = StringParsers.ParseBool(properties.Values[PropBehindPlayer]);
		}
		if (properties.Values.ContainsKey(PropLoopDuration))
		{
			duration = StringParsers.ParseFloat(properties.Values[PropLoopDuration]);
		}
		properties.ParseBool(PropCanDisable, ref canDisable);
		properties.ParseFloat(PropDelay, ref delay);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionPlaySound
		{
			targetGroup = targetGroup,
			soundNames = soundNames,
			insideHead = insideHead,
			behindPlayer = behindPlayer,
			duration = duration,
			delay = delay,
			canDisable = canDisable
		};
	}
}
