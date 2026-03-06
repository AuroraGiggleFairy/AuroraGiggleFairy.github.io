using System;
using UnityEngine;

public class CloneToTransform : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform m_transform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject m_storage;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform m_storageTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject m_clone;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform m_cloneTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform m_lastCloneTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_lastLocalPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion m_lastLocalRotation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_hasParentEntity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_hasParentEntityLocal;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Entity m_parentEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		m_transform = base.transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		m_parentEntity = GetComponentInParent<Entity>();
		if (m_parentEntity != null)
		{
			m_hasParentEntity = true;
		}
		if (m_parentEntity is EntityPlayerLocal)
		{
			m_hasParentEntityLocal = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		DestroyClone();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DestroyClone()
	{
		if ((bool)m_clone)
		{
			m_cloneTransform = null;
			UnityEngine.Object.Destroy(m_clone);
			m_clone = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		if ((bool)m_clone)
		{
			m_clone.SetActive(value: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		if ((bool)m_clone)
		{
			m_clone.SetActive(value: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (!m_storage)
		{
			m_storage = UnityEngine.Object.Instantiate(base.gameObject, m_transform, worldPositionStays: true);
			m_storageTransform = m_storage.transform;
			m_storageTransform.name = m_transform.name + "(CloneToTransform)";
			m_storage.SetActive(value: false);
			if (m_storage.TryGetComponent<CloneToTransform>(out var component))
			{
				UnityEngine.Object.Destroy(component);
			}
			foreach (Transform item in m_transform)
			{
				if (!(item == m_storageTransform))
				{
					UnityEngine.Object.Destroy(item.gameObject);
				}
			}
			Component[] components = GetComponents<Component>();
			foreach (Component component2 in components)
			{
				if (!(component2 == this) && !(component2 is Transform))
				{
					UnityEngine.Object.Destroy(component2);
				}
			}
		}
		Transform transform2 = null;
		if ((m_hasParentEntityLocal || !m_hasParentEntity) && (bool)Camera.main)
		{
			transform2 = Camera.main.transform;
		}
		else if (m_parentEntity != null && m_parentEntity.emodel != null)
		{
			transform2 = m_parentEntity.emodel.GetModelTransformParent();
		}
		if (!transform2)
		{
			DestroyClone();
			m_lastCloneTarget = null;
			return;
		}
		if (!m_clone)
		{
			m_lastCloneTarget = transform2;
			m_clone = UnityEngine.Object.Instantiate(m_storage, transform2, worldPositionStays: true);
			m_cloneTransform = m_clone.transform;
			m_cloneTransform.name = m_transform.name + "(clone)";
			m_clone.SetActive(value: true);
			if (m_clone.TryGetComponent<CloneToTransform>(out var component3))
			{
				UnityEngine.Object.Destroy(component3);
			}
		}
		if (m_lastCloneTarget != transform2)
		{
			m_lastCloneTarget = transform2;
			m_cloneTransform.parent = transform2;
			m_lastLocalPosition = default(Vector3);
			m_lastLocalRotation = default(Quaternion);
		}
		CheckTransform();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckTransform()
	{
		if (!(m_lastLocalPosition == m_transform.localPosition) || !(m_lastLocalRotation == m_transform.localRotation))
		{
			m_lastLocalPosition = m_transform.localPosition;
			m_lastLocalRotation = m_transform.localRotation;
			m_cloneTransform.SetPositionAndRotation(m_transform.position, m_transform.rotation);
		}
	}
}
