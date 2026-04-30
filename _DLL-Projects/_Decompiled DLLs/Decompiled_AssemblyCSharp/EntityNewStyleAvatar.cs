using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityNewStyleAvatar : Entity
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public class StringTags
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public HashSet<string> tags;

		public void AddTag(string tag)
		{
			tags.Add(tag);
		}

		public bool HasTag(string tag)
		{
			return tags.Contains(tag);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public class BodySlot
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public string submeshName;

		[PublicizedFrom(EAccessModifier.Private)]
		public StringTags tags;
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, BodySlot> m_entitySlots = new Dictionary<string, BodySlot>();

	public void EnableSubmesh(string submeshName, bool enable)
	{
		Transform transform = base.transform;
		Transform transform2 = transform.Find("Graphics/Model");
		if (transform2 == null)
		{
			transform2 = transform;
		}
		Transform transform3 = transform2.Find("base");
		if (!(transform3 != null))
		{
			return;
		}
		int childCount = transform3.childCount;
		for (int i = 0; i < childCount; i++)
		{
			GameObject gameObject = transform3.GetChild(i).gameObject;
			if (gameObject.name == submeshName)
			{
				gameObject.SetActive(enable);
			}
		}
	}

	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
		Transform transform = base.transform;
		Transform transform2 = transform.Find("Graphics/Model");
		if (transform2 == null)
		{
			transform2 = transform;
		}
		Transform transform3 = null;
		if (transform2 != null)
		{
			transform3 = DataLoader.LoadAsset<Transform>("@:Entities/Player/Male/maleTestPrefab.prefab");
			if (transform3 != null)
			{
				transform3 = UnityEngine.Object.Instantiate(transform3, transform2);
				transform3.name = "base";
			}
		}
		if ((bool)transform3)
		{
			int childCount = transform3.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = transform3.GetChild(i);
				Renderer component = child.GetComponent<Renderer>();
				if (!(component == null) && component.sharedMaterials != null)
				{
					child.gameObject.SetActive(value: false);
				}
			}
		}
		base.gameObject.AddComponent<NewAvatarRootMotion>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
	}
}
