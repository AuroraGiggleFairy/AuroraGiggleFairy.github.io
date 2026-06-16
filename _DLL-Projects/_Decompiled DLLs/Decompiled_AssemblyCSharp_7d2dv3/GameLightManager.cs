using System.Collections.Generic;
using UnityEngine;

public class GameLightManager
{
	public static GameLightManager Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<LightLOD> lights = new List<LightLOD>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<LightLOD> priorityLights = new List<LightLOD>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<LightLOD> removeLights = new List<LightLOD>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int lightUpdateIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isUpdating;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool isWaterLevelChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<UpdateLight> newULs = new List<UpdateLight>(512);

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cFastULGroups = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cFastULGroupMask = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<UpdateLight>[] fastULs = new List<UpdateLight>[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public int fastULUpdateIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cULGroups = 64;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSlowULGroupMask = 63;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<UpdateLight>[] slowULs = new List<UpdateLight>[64];

	[PublicizedFrom(EAccessModifier.Private)]
	public int slowULUpdateIndex;

	public static GameLightManager Create(EntityPlayerLocal player)
	{
		GameLightManager gameLightManager = new GameLightManager();
		gameLightManager.player = player;
		gameLightManager.Init();
		return gameLightManager;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		Instance = this;
		UpdateLightInit();
	}

	public void Destroy()
	{
		lights.Clear();
		priorityLights.Clear();
		removeLights.Clear();
		UpdateLightCleanup();
		Instance = null;
	}

	public void FrameUpdate()
	{
		if (GameManager.IsDedicatedServer || GameManager.Instance.World == null || MeshDescription.bDebugStability || LightViewer.IsAllOff)
		{
			return;
		}
		isUpdating = true;
		Vector3 position = player.cameraTransform.position;
		int count = lights.Count;
		int num = (count + 19) / 20;
		if (lightUpdateIndex >= count)
		{
			lightUpdateIndex = 0;
		}
		for (int i = 0; i < num; i++)
		{
			LightLOD lightLOD = lights[lightUpdateIndex];
			if (lightLOD.priority <= 0f)
			{
				lightLOD.FrameUpdate(position);
				if (lightLOD.priority > 0f)
				{
					priorityLights.Add(lightLOD);
				}
			}
			if (++lightUpdateIndex >= count)
			{
				lightUpdateIndex = 0;
			}
		}
		int num2 = 0;
		while (num2 < priorityLights.Count)
		{
			LightLOD lightLOD2 = priorityLights[num2];
			lightLOD2.FrameUpdate(position);
			if (lightLOD2.priority <= 0f)
			{
				priorityLights.RemoveAt(num2);
			}
			else
			{
				num2++;
			}
		}
		int count2 = removeLights.Count;
		if (count2 > 0)
		{
			for (int num3 = count2 - 1; num3 >= 0; num3--)
			{
				LightLOD lightLOD3 = removeLights[num3];
				RemoveLightFromLists(lightLOD3);
			}
			removeLights.Clear();
		}
		if (isWaterLevelChanged)
		{
			isWaterLevelChanged = false;
			foreach (LightLOD light in lights)
			{
				if (!light.bWorksUnderwater)
				{
					light.WaterLevelDirty = true;
				}
			}
		}
		isUpdating = false;
		UpdateLightFrameUpdate();
	}

	public void AddLight(LightLOD lightLOD)
	{
		lights.Add(lightLOD);
		lightLOD.priority = 1f;
		priorityLights.Add(lightLOD);
		if (OcclusionManager.Instance.cullLights)
		{
			OcclusionManager.AddLight(lightLOD);
		}
	}

	public void RemoveLight(LightLOD lightLOD)
	{
		if (isUpdating)
		{
			removeLights.Add(lightLOD);
		}
		else
		{
			RemoveLightFromLists(lightLOD);
		}
		if (OcclusionManager.Instance.cullLights)
		{
			OcclusionManager.RemoveLight(lightLOD);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveLightFromLists(LightLOD lightLOD)
	{
		int num = lights.IndexOf(lightLOD);
		if (num < 0)
		{
			Log.Warning("RemoveLightFromLists none");
			return;
		}
		lights.RemoveAt(num);
		if (num < lightUpdateIndex)
		{
			lightUpdateIndex--;
		}
		priorityLights.Remove(lightLOD);
	}

	public void MakeLightAPriority(LightLOD lightLOD)
	{
		if (lightLOD.priority <= 0f)
		{
			lightLOD.priority = 1f;
			priorityLights.Add(lightLOD);
		}
	}

	public Vector3 CameraPos()
	{
		return player.cameraTransform.position;
	}

	public void HandleWaterLevelChanged()
	{
		isWaterLevelChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLightInit()
	{
		for (int i = 0; i < fastULs.Length; i++)
		{
			fastULs[i] = new List<UpdateLight>(64);
		}
		for (int j = 0; j < slowULs.Length; j++)
		{
			slowULs[j] = new List<UpdateLight>(256);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLightCleanup()
	{
		newULs.Clear();
		for (int i = 0; i < fastULs.Length; i++)
		{
			fastULs[i] = null;
		}
		for (int j = 0; j < slowULs.Length; j++)
		{
			slowULs[j] = null;
		}
	}

	public void AddUpdateLight(UpdateLight _ul)
	{
		newULs.Add(_ul);
	}

	public void RemoveUpdateLight(UpdateLight _ul)
	{
		bool flag;
		if (_ul.IsDynamicObject)
		{
			int num = (_ul.GetHashCode() >> 2) & 3;
			flag = fastULs[num].Remove(_ul);
		}
		else
		{
			int num2 = (_ul.GetHashCode() >> 2) & 0x3F;
			flag = slowULs[num2].Remove(_ul);
		}
		if (!flag && !newULs.Remove(_ul))
		{
			Log.Warning("RemoveUpdateLight {0} dy{1} missing!", _ul.transform.name, _ul.IsDynamicObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLightFrameUpdate()
	{
		if (GameManager.Instance == null || GameManager.Instance.World == null || !GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return;
		}
		float step = Time.deltaTime * 4f;
		fastULUpdateIndex = (fastULUpdateIndex + 1) & 3;
		List<UpdateLight> list = fastULs[fastULUpdateIndex];
		for (int num = list.Count - 1; num >= 0; num--)
		{
			UpdateLight updateLight = list[num];
			if ((bool)updateLight)
			{
				updateLight.UpdateLighting(step);
			}
			else
			{
				list.RemoveAt(num);
			}
		}
		slowULUpdateIndex = (slowULUpdateIndex + 1) & 0x3F;
		List<UpdateLight> list2 = slowULs[slowULUpdateIndex];
		for (int num2 = list2.Count - 1; num2 >= 0; num2--)
		{
			UpdateLight updateLight2 = list2[num2];
			if ((bool)updateLight2)
			{
				if (updateLight2.appliedLit < 0f)
				{
					updateLight2.UpdateLighting(1f);
				}
			}
			else
			{
				list2.RemoveAt(num2);
			}
		}
		int num3 = Utils.FastMin(160, newULs.Count);
		for (int i = 0; i < num3; i++)
		{
			UpdateLight updateLight3 = newULs[i];
			if ((bool)updateLight3)
			{
				updateLight3.ManagerFirstUpdate();
				int num4 = updateLight3.GetHashCode() >> 2;
				if (updateLight3.IsDynamicObject)
				{
					fastULs[num4 & 3].Add(updateLight3);
				}
				else
				{
					slowULs[num4 & 0x3F].Add(updateLight3);
				}
			}
		}
		newULs.RemoveRange(0, num3);
	}
}
