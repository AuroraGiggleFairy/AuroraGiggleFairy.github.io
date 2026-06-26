using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class SpeedTreeWindHistoryBufferManager
{
	public class SharedMaterialGroup
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public HashSet<Renderer> activeRenderers = new HashSet<Renderer>();

		[PublicizedFrom(EAccessModifier.Private)]
		public HashSet<Material> sharedMaterials = new HashSet<Material>();

		[PublicizedFrom(EAccessModifier.Private)]
		public MaterialPropertyBlock previousProperties;

		public SharedMaterialGroup()
		{
			previousProperties = new MaterialPropertyBlock();
		}

		public void MergeMaterials(HashSet<Material> newMaterialsSet)
		{
			sharedMaterials.UnionWith(newMaterialsSet);
		}

		public void RegisterActiveRenderer(Renderer renderer)
		{
			activeRenderers.Add(renderer);
		}

		public void DeregisterActiveRenderer(Renderer renderer)
		{
			activeRenderers.Remove(renderer);
		}

		public void Update()
		{
			using (s_CheckVis.Auto())
			{
				if (activeRenderers.Count == 0)
				{
					return;
				}
			}
			Renderer renderer = null;
			using (s_ManagerGetFirstRenderer.Auto())
			{
				using HashSet<Renderer>.Enumerator enumerator = activeRenderers.GetEnumerator();
				if (!enumerator.MoveNext())
				{
					return;
				}
				renderer = enumerator.Current;
			}
			using (s_GetMatProps.Auto())
			{
				renderer.GetPropertyBlock(previousProperties);
			}
			using (s_SetMatProps.Auto())
			{
				foreach (Material sharedMaterial in sharedMaterials)
				{
					sharedMaterial.SetVector(_ST_PF_WindVector, previousProperties.GetVector(_ST_WindVector));
					sharedMaterial.SetVector(_ST_PF_WindGlobal, previousProperties.GetVector(_ST_WindGlobal));
					sharedMaterial.SetVector(_ST_PF_WindBranch, previousProperties.GetVector(_ST_WindBranch));
					sharedMaterial.SetVector(_ST_PF_WindBranchTwitch, previousProperties.GetVector(_ST_WindBranchTwitch));
					sharedMaterial.SetVector(_ST_PF_WindBranchWhip, previousProperties.GetVector(_ST_WindBranchWhip));
					sharedMaterial.SetVector(_ST_PF_WindBranchAnchor, previousProperties.GetVector(_ST_WindBranchAnchor));
					sharedMaterial.SetVector(_ST_PF_WindBranchAdherences, previousProperties.GetVector(_ST_WindBranchAdherences));
					sharedMaterial.SetVector(_ST_PF_WindTurbulences, previousProperties.GetVector(_ST_WindTurbulences));
					sharedMaterial.SetVector(_ST_PF_WindLeaf1Ripple, previousProperties.GetVector(_ST_WindLeaf1Ripple));
					sharedMaterial.SetVector(_ST_PF_WindLeaf1Tumble, previousProperties.GetVector(_ST_WindLeaf1Tumble));
					sharedMaterial.SetVector(_ST_PF_WindLeaf1Twitch, previousProperties.GetVector(_ST_WindLeaf1Twitch));
					sharedMaterial.SetVector(_ST_PF_WindLeaf2Ripple, previousProperties.GetVector(_ST_WindLeaf2Ripple));
					sharedMaterial.SetVector(_ST_PF_WindLeaf2Tumble, previousProperties.GetVector(_ST_WindLeaf2Tumble));
					sharedMaterial.SetVector(_ST_PF_WindLeaf2Twitch, previousProperties.GetVector(_ST_WindLeaf2Twitch));
					sharedMaterial.SetVector(_ST_PF_WindFrondRipple, previousProperties.GetVector(_ST_WindFrondRipple));
					sharedMaterial.SetVector(_ST_PF_WindAnimation, previousProperties.GetVector(_ST_WindAnimation));
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_GetMatProps = new ProfilerMarker("SpeedTreeWindPropertyBuffer.GetMatProps");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_SetMatProps = new ProfilerMarker("SpeedTreeWindPropertyBuffer.SetMatProps");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_CheckVis = new ProfilerMarker("SpeedTreeWindPropertyBuffer.CheckVis");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ManagerUpdate = new ProfilerMarker("SpeedTreeWindPropertyBuffer.ManagerUpdate");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ManagerRegistrations = new ProfilerMarker("SpeedTreeWindPropertyBuffer.ManagerRegistrations");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ManagerDeregistrations = new ProfilerMarker("SpeedTreeWindPropertyBuffer.ManagerDeregistrations");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ManagerGetFirstRenderer = new ProfilerMarker("SpeedTreeWindPropertyBuffer.ManagerGetFirstRenderer");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ManagerTotal = new ProfilerMarker("SpeedTreeWindPropertyBuffer.ManagerTotal");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindVector = Shader.PropertyToID("_ST_WindVector");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindGlobal = Shader.PropertyToID("_ST_WindGlobal");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindBranch = Shader.PropertyToID("_ST_WindBranch");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindBranchTwitch = Shader.PropertyToID("_ST_WindBranchTwitch");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindBranchWhip = Shader.PropertyToID("_ST_WindBranchWhip");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindBranchAnchor = Shader.PropertyToID("_ST_WindBranchAnchor");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindBranchAdherences = Shader.PropertyToID("_ST_WindBranchAdherences");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindTurbulences = Shader.PropertyToID("_ST_WindTurbulences");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindLeaf1Ripple = Shader.PropertyToID("_ST_WindLeaf1Ripple");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindLeaf1Tumble = Shader.PropertyToID("_ST_WindLeaf1Tumble");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindLeaf1Twitch = Shader.PropertyToID("_ST_WindLeaf1Twitch");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindLeaf2Ripple = Shader.PropertyToID("_ST_WindLeaf2Ripple");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindLeaf2Tumble = Shader.PropertyToID("_ST_WindLeaf2Tumble");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindLeaf2Twitch = Shader.PropertyToID("_ST_WindLeaf2Twitch");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindFrondRipple = Shader.PropertyToID("_ST_WindFrondRipple");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindAnimation = Shader.PropertyToID("_ST_WindAnimation");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindVector = Shader.PropertyToID("_ST_PF_WindVector");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindGlobal = Shader.PropertyToID("_ST_PF_WindGlobal");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindBranch = Shader.PropertyToID("_ST_PF_WindBranch");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindBranchTwitch = Shader.PropertyToID("_ST_PF_WindBranchTwitch");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindBranchWhip = Shader.PropertyToID("_ST_PF_WindBranchWhip");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindBranchAnchor = Shader.PropertyToID("_ST_PF_WindBranchAnchor");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindBranchAdherences = Shader.PropertyToID("_ST_PF_WindBranchAdherences");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindTurbulences = Shader.PropertyToID("_ST_PF_WindTurbulences");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindLeaf1Ripple = Shader.PropertyToID("_ST_PF_WindLeaf1Ripple");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindLeaf1Tumble = Shader.PropertyToID("_ST_PF_WindLeaf1Tumble");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindLeaf1Twitch = Shader.PropertyToID("_ST_PF_WindLeaf1Twitch");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindLeaf2Ripple = Shader.PropertyToID("_ST_PF_WindLeaf2Ripple");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindLeaf2Tumble = Shader.PropertyToID("_ST_PF_WindLeaf2Tumble");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindLeaf2Twitch = Shader.PropertyToID("_ST_PF_WindLeaf2Twitch");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindFrondRipple = Shader.PropertyToID("_ST_PF_WindFrondRipple");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindAnimation = Shader.PropertyToID("_ST_PF_WindAnimation");

	[PublicizedFrom(EAccessModifier.Private)]
	public static SpeedTreeWindHistoryBufferManager m_Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Renderer, SharedMaterialGroup> rendererToGroupMap = new Dictionary<Renderer, SharedMaterialGroup>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Material, SharedMaterialGroup> materialToGroupMap = new Dictionary<Material, SharedMaterialGroup>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Material> tempMaterialsList = new List<Material>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<Material> newMaterialsSet = new HashSet<Material>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<SharedMaterialGroup> sharedMaterialGroups = new HashSet<SharedMaterialGroup>();

	public static SpeedTreeWindHistoryBufferManager Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = new SpeedTreeWindHistoryBufferManager();
			}
			return m_Instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SpeedTreeWindHistoryBufferManager()
	{
	}

	public bool TryRegisterActiveRenderer(Renderer renderer)
	{
		using (s_ManagerTotal.Auto())
		{
			using (s_ManagerRegistrations.Auto())
			{
				if (renderer == null)
				{
					Debug.LogError("Cannot register a null renderer.");
					return false;
				}
				if (!rendererToGroupMap.TryGetValue(renderer, out var value))
				{
					newMaterialsSet.Clear();
					tempMaterialsList.Clear();
					if (renderer is BillboardRenderer billboardRenderer)
					{
						tempMaterialsList.Add(billboardRenderer.billboard.material);
					}
					else
					{
						renderer.GetSharedMaterials(tempMaterialsList);
					}
					foreach (Material tempMaterials in tempMaterialsList)
					{
						if (!(tempMaterials == null) && !materialToGroupMap.TryGetValue(tempMaterials, out value))
						{
							newMaterialsSet.Add(tempMaterials);
						}
					}
					if (value == null)
					{
						if (newMaterialsSet.Count == 0)
						{
							return false;
						}
						value = new SharedMaterialGroup();
						sharedMaterialGroups.Add(value);
					}
					if (newMaterialsSet.Count > 0)
					{
						value.MergeMaterials(newMaterialsSet);
						foreach (Material item in newMaterialsSet)
						{
							materialToGroupMap[item] = value;
						}
					}
					rendererToGroupMap[renderer] = value;
					newMaterialsSet.Clear();
					tempMaterialsList.Clear();
				}
				value.RegisterActiveRenderer(renderer);
				return true;
			}
		}
	}

	public void DeregisterActiveRenderer(Renderer renderer)
	{
		using (s_ManagerTotal.Auto())
		{
			using (s_ManagerDeregistrations.Auto())
			{
				if (rendererToGroupMap.TryGetValue(renderer, out var value))
				{
					value.DeregisterActiveRenderer(renderer);
				}
			}
		}
	}

	public void Update()
	{
		using (s_ManagerTotal.Auto())
		{
			using (s_ManagerUpdate.Auto())
			{
				foreach (SharedMaterialGroup sharedMaterialGroup in sharedMaterialGroups)
				{
					sharedMaterialGroup.Update();
				}
			}
		}
	}
}
