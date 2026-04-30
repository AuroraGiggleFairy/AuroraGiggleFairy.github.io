using System.Collections.Generic;
using UnityEngine;

public class WireManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool USE_FAST_WIRE_NODES = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static WireManager instance = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public int wireIndex;

	public bool ShowPulse;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color standardPulseColor = new Color32(0, 97, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color tripWirePulseColor = Color.magenta;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<IWireNode> activeWires;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<GameObject> activePulseObjects;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform wireManagerRoot;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform wirePool;

	public bool WiresShowing;

	public static WireManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new WireManager();
			}
			return instance;
		}
	}

	public static bool HasInstance => instance != null;

	public Transform WireManagerRoot => wireManagerRoot;

	public void Init()
	{
		activeWires = new HashSet<IWireNode>();
		activePulseObjects = new HashSet<GameObject>();
		GameObject gameObject = GameObject.Find("WireManager");
		if (gameObject == null)
		{
			wireManagerRoot = new GameObject("WireManager").transform;
		}
		else
		{
			wireManagerRoot = gameObject.transform;
		}
		wirePool = wireManagerRoot.Find("Pool");
		if (wirePool == null)
		{
			wirePool = new GameObject("Pool").transform;
			wirePool.parent = wireManagerRoot;
		}
		Origin.Add(wireManagerRoot.transform, 0);
		if (wirePool.transform.childCount == 0)
		{
			for (int i = 0; i < 200; i++)
			{
				addNewNode();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addNewNode()
	{
		GameObject gameObject = ((!USE_FAST_WIRE_NODES) ? ((GameObject)Object.Instantiate(Resources.Load("Prefabs/WireNode"))) : ((GameObject)Object.Instantiate(Resources.Load("Prefabs/WireNode2"))));
		gameObject.name = $"WireNode_{wireIndex++.ToString()}";
		gameObject.SetActive(value: false);
		gameObject.transform.parent = wirePool;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
	}

	public void ReturnToPool(IWireNode wireNode)
	{
		activeWires.Remove(wireNode);
		GameObject gameObject = wireNode.GetGameObject();
		gameObject.transform.parent = wirePool;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.SetActive(value: false);
		wireNode.Reset();
	}

	public IWireNode GetWireNodeFromPool()
	{
		IWireNode wireNode = null;
		if (wirePool.childCount < 1)
		{
			addNewNode();
		}
		Transform child = wirePool.GetChild(wirePool.childCount - 1);
		child.gameObject.SetActive(value: true);
		child.parent = wireManagerRoot;
		wireNode = ((!USE_FAST_WIRE_NODES) ? ((IWireNode)child.gameObject.GetComponent<WireNode>()) : ((IWireNode)child.gameObject.GetComponent<FastWireNode>()));
		activeWires.Add(wireNode);
		return wireNode;
	}

	public bool AddActiveWire(IWireNode wire)
	{
		return activeWires.Add(wire);
	}

	public bool RemoveActiveWire(IWireNode wire)
	{
		return activeWires.Remove(wire);
	}

	public bool AddPulseObject(GameObject pulseObject)
	{
		return activePulseObjects.Add(pulseObject);
	}

	public bool RemovePulseObject(GameObject pulseObject)
	{
		return activePulseObjects.Remove(pulseObject);
	}

	public void ToggleAllWirePulse(bool isPulseOn)
	{
		World world = GameManager.Instance.World;
		ShowPulse = isPulseOn;
		WiresShowing = isPulseOn;
		if (activeWires == null)
		{
			return;
		}
		if (ShowPulse)
		{
			Dictionary<Vector3, bool> dictionary = new Dictionary<Vector3, bool>(Vector3EqualityComparer.Instance);
			foreach (IWireNode activeWire in activeWires)
			{
				bool flag = true;
				Vector3 startPosition = activeWire.GetStartPosition();
				if (dictionary.ContainsKey(startPosition) ? dictionary[startPosition] : (dictionary[startPosition] = world.CanPlaceBlockAt(new Vector3i(startPosition), world.gameManager.GetPersistentLocalPlayer())))
				{
					activeWire.TogglePulse(isPulseOn);
					activeWire.SetVisible(WiresShowing);
				}
				else
				{
					activeWire.SetVisible(_visible: false);
				}
			}
			dictionary.Clear();
			{
				foreach (GameObject activePulseObject in activePulseObjects)
				{
					Vector3i blockPos = new Vector3i(activePulseObject.transform.position);
					if (world.CanPlaceBlockAt(blockPos, world.gameManager.GetPersistentLocalPlayer()))
					{
						activePulseObject.SetActive(isPulseOn);
					}
					activePulseObject.layer = 0;
				}
				return;
			}
		}
		foreach (IWireNode activeWire2 in activeWires)
		{
			activeWire2.TogglePulse(isOn: false);
			activeWire2.SetVisible(_visible: false);
		}
		foreach (GameObject activePulseObject2 in activePulseObjects)
		{
			activePulseObject2.SetActive(isPulseOn);
			activePulseObject2.layer = 11;
		}
	}

	public void SetWirePulse(IWireNode node)
	{
		node.TogglePulse(ShowPulse);
	}

	public void RefreshPulseObjects()
	{
		foreach (GameObject activePulseObject in activePulseObjects)
		{
			activePulseObject.SetActive(ShowPulse);
			activePulseObject.layer = ((!ShowPulse) ? 11 : 0);
		}
	}

	public void Cleanup()
	{
		foreach (IWireNode activeWire in activeWires)
		{
			Object.Destroy(activeWire.GetGameObject());
		}
		activeWires.Clear();
		for (int num = wirePool.childCount - 1; num >= 0; num--)
		{
			Object.Destroy(wirePool.GetChild(num).gameObject);
		}
		Object.Destroy(wirePool.gameObject);
		Origin.Remove(wireManagerRoot);
		Object.Destroy(wireManagerRoot.gameObject);
		instance = null;
	}
}
