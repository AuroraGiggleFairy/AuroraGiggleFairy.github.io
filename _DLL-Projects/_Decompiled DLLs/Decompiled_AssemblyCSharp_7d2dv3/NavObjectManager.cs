using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class NavObjectManager
{
	public delegate void NavObjectChangedDelegate(NavObject newNavObject);

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate bool includeInList(NavObject navObject);

	[PublicizedFrom(EAccessModifier.Private)]
	public static NavObjectManager instance;

	public List<NavObject> NavObjectList = new List<NavObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<NavObject> removedNavObjectPool = new List<NavObject>();

	public Dictionary<string, List<NavObject>> tagList = new Dictionary<string, List<NavObject>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<NavObject> removeList = new List<NavObject>();

	public static NavObjectManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new NavObjectManager();
			}
			return instance;
		}
	}

	public static bool HasInstance => instance != null;

	public event NavObjectChangedDelegate OnNavObjectAdded;

	public event NavObjectChangedDelegate OnNavObjectRemoved;

	public event NavObjectChangedDelegate OnNavObjectRefreshed;

	[PublicizedFrom(EAccessModifier.Private)]
	public NavObjectManager()
	{
		instance = this;
	}

	public void Cleanup()
	{
		instance = null;
		NavObjectList.Clear();
		removedNavObjectPool.Clear();
		tagList.Clear();
	}

	public NavObject RegisterNavObject(string className, Transform trackedTransform, string overrideSprite = "", bool hiddenOnCompass = false)
	{
		for (int i = 0; i < NavObjectList.Count; i++)
		{
			if (NavObjectList[i].IsTrackedTransform(trackedTransform))
			{
				return NavObjectList[i];
			}
		}
		NavObject navObject = null;
		if (removedNavObjectPool.Count > 0)
		{
			navObject = removedNavObjectPool[0];
			removedNavObjectPool.RemoveAt(0);
			navObject.IsActive = true;
			navObject.Reset(className);
		}
		else
		{
			navObject = new NavObject(className);
		}
		navObject.hiddenOnCompass = hiddenOnCompass;
		navObject.TrackedTransform = trackedTransform;
		navObject.OverrideSpriteName = overrideSprite;
		AddNavObjectTag(navObject);
		if (this.OnNavObjectAdded != null)
		{
			this.OnNavObjectAdded(navObject);
		}
		NavObjectList.Add(navObject);
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			navObject.HandleActiveNavClass(primaryPlayer);
		}
		return navObject;
	}

	public NavObject RegisterNavObject(string className, Vector3 trackedPosition, string overrideSprite = "", bool hiddenOnCompass = false, int entityId = -1, Entity ownerEntity = null)
	{
		for (int i = 0; i < NavObjectList.Count; i++)
		{
			if (NavObjectList[i].NavObjectClass != null && !(className != NavObjectList[i].NavObjectClass.NavObjectClassName) && (!(ownerEntity != null) || !(ownerEntity != NavObjectList[i].OwnerEntity)))
			{
				if (entityId != -1 && NavObjectList[i].EntityID == entityId)
				{
					return NavObjectList[i];
				}
				if (NavObjectList[i].IsTrackedPosition(trackedPosition))
				{
					return NavObjectList[i];
				}
			}
		}
		NavObject navObject = null;
		if (removedNavObjectPool.Count > 0)
		{
			navObject = removedNavObjectPool[0];
			removedNavObjectPool.RemoveAt(0);
			navObject.IsActive = true;
			navObject.Reset(className);
		}
		else
		{
			navObject = new NavObject(className);
		}
		navObject.EntityID = entityId;
		navObject.OwnerEntity = ownerEntity;
		navObject.hiddenOnCompass = hiddenOnCompass;
		navObject.TrackedPosition = trackedPosition;
		navObject.OverrideSpriteName = overrideSprite;
		AddNavObjectTag(navObject);
		if (this.OnNavObjectAdded != null)
		{
			this.OnNavObjectAdded(navObject);
		}
		NavObjectList.Add(navObject);
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			navObject.HandleActiveNavClass(primaryPlayer);
		}
		return navObject;
	}

	public NavObject RegisterNavObject(string className, Entity trackedEntity, string overrideSprite = "", bool hiddenOnCompass = false)
	{
		for (int i = 0; i < NavObjectList.Count; i++)
		{
			if (NavObjectList[i].IsTrackedEntity(trackedEntity) && (NavObjectList[i].NavObjectClass == null || NavObjectList[i].NavObjectClass.NavObjectClassName == className))
			{
				NavObjectList[i].TrackedEntity = trackedEntity;
				return NavObjectList[i];
			}
		}
		NavObject navObject = null;
		if (removedNavObjectPool.Count > 0)
		{
			navObject = removedNavObjectPool[0];
			removedNavObjectPool.RemoveAt(0);
			navObject.IsActive = true;
			navObject.Reset(className);
		}
		else
		{
			navObject = new NavObject(className);
		}
		navObject.OverrideSpriteName = overrideSprite;
		navObject.TrackedEntity = trackedEntity;
		navObject.hiddenOnCompass = hiddenOnCompass;
		AddNavObjectTag(navObject);
		if (this.OnNavObjectAdded != null)
		{
			this.OnNavObjectAdded(navObject);
		}
		NavObjectList.Add(navObject);
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			navObject.HandleActiveNavClass(primaryPlayer);
		}
		return navObject;
	}

	public void UnRegisterNavObject(NavObject navObject)
	{
		if (navObject == null)
		{
			return;
		}
		bool flag = false;
		if (NavObjectList.Contains(navObject))
		{
			flag = true;
			NavObjectList.Remove(navObject);
		}
		if (!removedNavObjectPool.Contains(navObject))
		{
			removedNavObjectPool.Add(navObject);
		}
		if (flag)
		{
			RemoveNavObjectTag(navObject);
			if (this.OnNavObjectRemoved != null)
			{
				this.OnNavObjectRemoved(navObject);
			}
		}
	}

	public void UnRegisterNavObjectByPosition(Vector3 position, string navObjectClass)
	{
		unRegisterNavObjects([PublicizedFrom(EAccessModifier.Internal)] (NavObject navObject) => navObject.IsTrackedPosition(position) && navObject.NavObjectClass != null && navObject.NavObjectClass.NavObjectClassName == navObjectClass, $"UnRegisterNavObjectByOwnerEntity {position}");
	}

	public void UnRegisterNavObjectByOwnerEntity(Entity ownerEntity, string navObjectClass)
	{
		unRegisterNavObjects([PublicizedFrom(EAccessModifier.Internal)] (NavObject navObject) => navObject.OwnerEntity == ownerEntity && navObject.NavObjectClass != null && navObject.NavObjectClass.NavObjectClassName == navObjectClass, $"\"UnRegisterNavObjectByOwnerEntity {ownerEntity}");
	}

	public void UnRegisterNavObjectByEntityID(int entityId)
	{
		unRegisterNavObjects([PublicizedFrom(EAccessModifier.Internal)] (NavObject navObject) => navObject.EntityID == entityId, $"UnRegisterNavObjectByEntityID {entityId}");
	}

	public void UnRegisterNavObjectByClass(string className)
	{
		unRegisterNavObjects([PublicizedFrom(EAccessModifier.Internal)] (NavObject navObject) => navObject.NavObjectClass != null && navObject.NavObjectClass.NavObjectClassName == className, $"UnRegisterNavObjectByClass {className}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void unRegisterNavObjects(includeInList includeTest, string logStringPrefix)
	{
		foreach (NavObject navObject in NavObjectList)
		{
			if (includeTest(navObject))
			{
				removeList.Add(navObject);
			}
		}
		foreach (NavObject remove in removeList)
		{
			NavObjectList.Remove(remove);
			RemoveNavObjectTag(remove);
			if (!removedNavObjectPool.Contains(remove))
			{
				removedNavObjectPool.Add(remove);
			}
			this.OnNavObjectRemoved?.Invoke(remove);
		}
		removeList.Clear();
	}

	public void AddNavObjectTag(NavObject navObject)
	{
		for (int i = 0; i < navObject.NavObjectClassList.Count; i++)
		{
			if (!string.IsNullOrEmpty(navObject.NavObjectClassList[i].Tag))
			{
				if (!tagList.ContainsKey(navObject.NavObjectClassList[i].Tag))
				{
					tagList.Add(navObject.NavObjectClassList[i].Tag, new List<NavObject>());
				}
				tagList[navObject.NavObjectClassList[i].Tag].Add(navObject);
			}
		}
	}

	public void RemoveNavObjectTag(NavObject navObject)
	{
		for (int i = 0; i < navObject.NavObjectClassList.Count; i++)
		{
			if (!string.IsNullOrEmpty(navObject.NavObjectClassList[i].Tag))
			{
				if (!tagList.ContainsKey(navObject.NavObjectClassList[i].Tag))
				{
					tagList.Add(navObject.NavObjectClassList[i].Tag, new List<NavObject>());
				}
				tagList[navObject.NavObjectClassList[i].Tag].Remove(navObject);
			}
		}
	}

	public bool HasNavObjectTag(string tag)
	{
		if (tagList.ContainsKey(tag))
		{
			for (int i = 0; i < tagList[tag].Count; i++)
			{
				if (tagList[tag][i].NavObjectClass != null && tagList[tag][i].NavObjectClass.Tag == tag)
				{
					return true;
				}
			}
		}
		return false;
	}

	public NavObject GetNavObjectByEntityID(int entityId)
	{
		for (int num = NavObjectList.Count - 1; num >= 0; num--)
		{
			NavObject navObject = NavObjectList[num];
			if (navObject != null && navObject.EntityID == entityId)
			{
				return NavObjectList[num];
			}
		}
		return null;
	}

	public void RefreshNavObjects()
	{
		bool flag = false;
		for (int i = 0; i < NavObjectList.Count; i++)
		{
			flag = false;
			if (NavObjectList[i].NavObjectClass != null)
			{
				NavObjectList[i].NavObjectClass = NavObjectClass.GetNavObjectClass(NavObjectList[i].NavObjectClass.NavObjectClassName);
				flag = true;
			}
			if (NavObjectList[i].NavObjectClassList != null)
			{
				for (int j = 0; j < NavObjectList[i].NavObjectClassList.Count; j++)
				{
					NavObjectList[i].NavObjectClassList[j] = NavObjectClass.GetNavObjectClass(NavObjectList[i].NavObjectClassList[j].NavObjectClassName);
					flag = true;
				}
			}
			if (flag && this.OnNavObjectRefreshed != null)
			{
				this.OnNavObjectRefreshed(NavObjectList[i]);
			}
		}
	}

	public void Update()
	{
		if (GameManager.Instance.World == null)
		{
			return;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		for (int num = NavObjectList.Count - 1; num >= 0; num--)
		{
			NavObject navObject = NavObjectList[num];
			if (!navObject.IsValid())
			{
				UnRegisterNavObject(navObject);
			}
			else if (primaryPlayer != null)
			{
				navObject.HandleActiveNavClass(primaryPlayer);
			}
		}
	}

	[Conditional("DEBUG_NAV")]
	public static void LogNav(string _format = "", params object[] _args)
	{
		_format = $"{GameManager.frameCount} Nav {_format}";
		Log.Out(_format, _args);
	}
}
