using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(SphereCollider))]
public class vp_ItemPickup : MonoBehaviour
{
	[Serializable]
	public class ItemSection
	{
		public vp_ItemType Type;
	}

	[Serializable]
	public class RecipientTagsSection
	{
		public List<string> Tags = new List<string>();
	}

	[Serializable]
	public class SoundSection
	{
		public AudioClip PickupSound;

		public bool PickupSoundSlomo = true;

		public AudioClip PickupFailSound;

		public bool FailSoundSlomo = true;
	}

	[Serializable]
	public class MessageSection
	{
		public string SuccessSingle = "Picked up {2}.";

		public string SuccessMultiple = "Picked up {4} {1}s.";

		public string FailSingle = "Can't pick up {2} right now.";

		public string FailMultiple = "Can't pick up {4} {1}s right now.";
	}

	public int ID;

	public int Amount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Type m_ItemType;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_ItemType m_ItemTypeObject;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioSource m_Audio;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemSection m_Item;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public RecipientTagsSection m_Recipient;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public SoundSection m_Sound;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public MessageSection m_Messages;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_Depleted;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_PickedUpAmount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Rigidbody m_Rigidbody;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Transform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public string MissingItemTypeError = "Warning: {0} has no ItemType object!";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_AlreadyFailed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<Collider, vp_Inventory> m_ColliderInventories = new Dictionary<Collider, vp_Inventory>();

	public Type ItemType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_ItemType == null)
			{
				m_ItemType = m_Item.Type.GetType();
			}
			return m_ItemType;
		}
	}

	public vp_ItemType ItemTypeObject
	{
		get
		{
			if (m_ItemTypeObject == null)
			{
				m_ItemTypeObject = m_Item.Type;
			}
			return m_ItemTypeObject;
		}
	}

	public AudioSource Audio
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_Audio == null)
			{
				if (GetComponent<AudioSource>() == null)
				{
					base.gameObject.AddComponent<AudioSource>();
				}
				m_Audio = GetComponent<AudioSource>();
			}
			return m_Audio;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		if (ItemType == typeof(vp_UnitType))
		{
			Amount = Mathf.Max(1, Amount);
		}
		GetComponent<Collider>().isTrigger = true;
		m_Rigidbody = GetComponent<Rigidbody>();
		m_Transform = base.transform;
		if (m_Sound.PickupSound != null || m_Sound.PickupFailSound != null)
		{
			Audio.clip = m_Sound.PickupSound;
			Audio.playOnAwake = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		if (m_Depleted && !Audio.isPlaying)
		{
			SendMessage("Die", SendMessageOptions.DontRequireReceiver);
		}
		if (m_Depleted || !(m_Rigidbody != null) || !m_Rigidbody.IsSleeping() || m_Rigidbody.isKinematic)
		{
			return;
		}
		vp_Timer.In(0.5f, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			m_Rigidbody.isKinematic = true;
			Collider[] components = GetComponents<Collider>();
			foreach (Collider collider in components)
			{
				if (!collider.isTrigger)
				{
					collider.enabled = false;
				}
			}
		});
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (m_Rigidbody != null)
		{
			m_Rigidbody.isKinematic = false;
			Collider[] components = GetComponents<Collider>();
			foreach (Collider collider in components)
			{
				if (!collider.isTrigger)
				{
					collider.enabled = true;
				}
			}
		}
		GetComponent<Renderer>().enabled = true;
		m_Depleted = false;
		m_AlreadyFailed = false;
		vp_GlobalEvent<vp_ItemPickup>.Send("NetworkRespawnPickup", this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnTriggerEnter(Collider col)
	{
		if (!(ItemType == null) && vp_Gameplay.isMaster && GetComponent<Collider>().enabled)
		{
			TryGiveTo(col);
		}
	}

	public void TryGiveTo(Collider col)
	{
		if (m_Depleted)
		{
			return;
		}
		if (!m_ColliderInventories.TryGetValue(col, out var value))
		{
			value = vp_TargetEventReturn<vp_Inventory>.SendUpwards(col, "GetInventory");
			m_ColliderInventories.Add(col, value);
		}
		if (!(value == null) && (m_Recipient.Tags.Count <= 0 || m_Recipient.Tags.Contains(col.gameObject.tag)))
		{
			bool flag = false;
			int num = vp_TargetEventReturn<vp_ItemType, int>.SendUpwards(col, "GetItemCount", m_Item.Type);
			if (ItemType == typeof(vp_ItemType))
			{
				flag = vp_TargetEventReturn<vp_ItemType, int, bool>.SendUpwards(col, "TryGiveItem", m_Item.Type, ID);
			}
			else if (ItemType == typeof(vp_UnitBankType))
			{
				flag = vp_TargetEventReturn<vp_UnitBankType, int, int, bool>.SendUpwards(col, "TryGiveUnitBank", m_Item.Type as vp_UnitBankType, Amount, ID);
			}
			else if (ItemType == typeof(vp_UnitType))
			{
				flag = vp_TargetEventReturn<vp_UnitType, int, bool>.SendUpwards(col, "TryGiveUnits", m_Item.Type as vp_UnitType, Amount);
			}
			else if (ItemType.BaseType == typeof(vp_ItemType))
			{
				flag = vp_TargetEventReturn<vp_ItemType, int, bool>.SendUpwards(col, "TryGiveItem", m_Item.Type, ID);
			}
			else if (ItemType.BaseType == typeof(vp_UnitBankType))
			{
				flag = vp_TargetEventReturn<vp_UnitBankType, int, int, bool>.SendUpwards(col, "TryGiveUnitBank", m_Item.Type as vp_UnitBankType, Amount, ID);
			}
			else if (ItemType.BaseType == typeof(vp_UnitType))
			{
				flag = vp_TargetEventReturn<vp_UnitType, int, bool>.SendUpwards(col, "TryGiveUnits", m_Item.Type as vp_UnitType, Amount);
			}
			if (flag)
			{
				m_PickedUpAmount = vp_TargetEventReturn<vp_ItemType, int>.SendUpwards(col, "GetItemCount", m_Item.Type) - num;
				OnSuccess(col.transform);
			}
			else
			{
				OnFail(col.transform);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnTriggerExit()
	{
		m_AlreadyFailed = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnSuccess(Transform recipient)
	{
		m_Depleted = true;
		if (m_Sound.PickupSound != null)
		{
			Audio.pitch = (m_Sound.PickupSoundSlomo ? Time.timeScale : 1f);
			Audio.Play();
		}
		GetComponent<Renderer>().enabled = false;
		string text = "";
		text = ((m_PickedUpAmount >= 2 && !(ItemType == typeof(vp_UnitBankType)) && !(ItemType.BaseType == typeof(vp_UnitBankType))) ? string.Format(m_Messages.SuccessMultiple, m_Item.Type.IndefiniteArticle, m_Item.Type.DisplayName, m_Item.Type.DisplayNameFull, m_Item.Type.Description, m_PickedUpAmount.ToString()) : string.Format(m_Messages.SuccessSingle, m_Item.Type.IndefiniteArticle, m_Item.Type.DisplayName, m_Item.Type.DisplayNameFull, m_Item.Type.Description, m_PickedUpAmount.ToString()));
		vp_GlobalEvent<Transform, string>.Send("HUDText", recipient, text);
		if (vp_Gameplay.isMultiplayer && vp_Gameplay.isMaster)
		{
			vp_GlobalEvent<vp_ItemPickup, Transform>.Send("NetworkGivePickup", this, recipient);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Die()
	{
		vp_Utility.Activate(base.gameObject, activate: false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnFail(Transform recipient)
	{
		if (!m_AlreadyFailed && m_Sound.PickupFailSound != null)
		{
			Audio.pitch = (m_Sound.FailSoundSlomo ? Time.timeScale : 1f);
			Audio.PlayOneShot(m_Sound.PickupFailSound);
		}
		m_AlreadyFailed = true;
		string text = "";
		text = ((m_PickedUpAmount >= 2 && !(ItemType == typeof(vp_UnitBankType)) && !(ItemType.BaseType == typeof(vp_UnitBankType))) ? string.Format(m_Messages.FailMultiple, m_Item.Type.IndefiniteArticle, m_Item.Type.DisplayName, m_Item.Type.DisplayNameFull, m_Item.Type.Description, Amount.ToString()) : string.Format(m_Messages.FailSingle, m_Item.Type.IndefiniteArticle, m_Item.Type.DisplayName, m_Item.Type.DisplayNameFull, m_Item.Type.Description, Amount.ToString()));
		vp_GlobalEvent<Transform, string>.Send("HUDText", recipient, text);
	}
}
