using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Terrain))]
[DisallowMultipleComponent]
public class MicroSplatTerrain : MicroSplatObject
{
	public delegate void MaterialSyncAll();

	public delegate void MaterialSync(Material m);

	[HideInInspector]
	public Shader addPass;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<MicroSplatTerrain> sInstances = new List<MicroSplatTerrain>();

	public Terrain terrain;

	[HideInInspector]
	public Texture2D customControl0;

	[HideInInspector]
	public Texture2D customControl1;

	[HideInInspector]
	public Texture2D customControl2;

	[HideInInspector]
	public Texture2D customControl3;

	[HideInInspector]
	public Texture2D customControl4;

	[HideInInspector]
	public Texture2D customControl5;

	[HideInInspector]
	public Texture2D customControl6;

	[HideInInspector]
	public Texture2D customControl7;

	[HideInInspector]
	public bool reenabled;

	public static event MaterialSyncAll OnMaterialSyncAll;

	public event MaterialSync OnMaterialSync;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		terrain = GetComponent<Terrain>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		terrain = GetComponent<Terrain>();
		sInstances.Add(this);
		if (reenabled)
		{
			Sync();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		Sync();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		sInstances.Remove(this);
		Cleanup();
		reenabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Cleanup()
	{
		if (matInstance != null && matInstance != templateMaterial)
		{
			UnityEngine.Object.DestroyImmediate(matInstance);
			terrain.materialTemplate = null;
		}
	}

	public void Sync()
	{
		if (templateMaterial == null)
		{
			return;
		}
		Material material = null;
		if (terrain.materialTemplate == matInstance && matInstance != null)
		{
			terrain.materialTemplate.CopyPropertiesFromMaterial(templateMaterial);
			material = terrain.materialTemplate;
		}
		else
		{
			material = new Material(templateMaterial);
		}
		if (terrain.drawInstanced && keywordSO.IsKeywordEnabled("_TESSDISTANCE") && keywordSO.IsKeywordEnabled("_MSRENDERLOOP_SURFACESHADER"))
		{
			Debug.LogWarning("Disabling terrain instancing when tessellation is enabled, as Unity has not made surface shader tessellation compatible with terrain instancing");
			terrain.drawInstanced = false;
		}
		material.hideFlags = HideFlags.HideAndDontSave;
		terrain.materialTemplate = material;
		matInstance = material;
		ApplyMaps(material);
		if (keywordSO.IsKeywordEnabled("_CUSTOMSPLATTEXTURES"))
		{
			material.SetTexture("_CustomControl0", (customControl0 != null) ? customControl0 : Texture2D.blackTexture);
			material.SetTexture("_CustomControl1", (customControl1 != null) ? customControl1 : Texture2D.blackTexture);
			material.SetTexture("_CustomControl2", (customControl2 != null) ? customControl2 : Texture2D.blackTexture);
			material.SetTexture("_CustomControl3", (customControl3 != null) ? customControl3 : Texture2D.blackTexture);
			material.SetTexture("_CustomControl4", (customControl4 != null) ? customControl4 : Texture2D.blackTexture);
			material.SetTexture("_CustomControl5", (customControl5 != null) ? customControl5 : Texture2D.blackTexture);
			material.SetTexture("_CustomControl6", (customControl6 != null) ? customControl6 : Texture2D.blackTexture);
			material.SetTexture("_CustomControl7", (customControl7 != null) ? customControl7 : Texture2D.blackTexture);
		}
		else
		{
			if (terrain == null || terrain.terrainData == null)
			{
				Debug.LogError("Terrain or terrain data is null, cannot sync");
				return;
			}
			Texture2D[] alphamapTextures = terrain.terrainData.alphamapTextures;
			ApplyControlTextures(alphamapTextures, material);
		}
		ApplyBlendMap();
		if (this.OnMaterialSync != null)
		{
			this.OnMaterialSync(material);
		}
	}

	public override Bounds GetBounds()
	{
		return terrain.terrainData.bounds;
	}

	public new static void SyncAll()
	{
		for (int i = 0; i < sInstances.Count; i++)
		{
			sInstances[i].Sync();
		}
		if (MicroSplatTerrain.OnMaterialSyncAll != null)
		{
			MicroSplatTerrain.OnMaterialSyncAll();
		}
	}
}
