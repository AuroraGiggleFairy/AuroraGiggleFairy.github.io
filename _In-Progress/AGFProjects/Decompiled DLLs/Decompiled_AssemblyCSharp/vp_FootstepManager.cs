using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_FootstepManager : MonoBehaviour
{
	[Serializable]
	public class vp_SurfaceTypes
	{
		public Vector2 RandomPitch = new Vector2(1f, 1.5f);

		public bool Foldout = true;

		public bool SoundsFoldout = true;

		public bool TexturesFoldout = true;

		public string SurfaceName = "";

		public List<AudioClip> Sounds = new List<AudioClip>();

		public List<Texture> Textures = new List<Texture>();
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static vp_FootstepManager[] m_FootstepManagers;

	public static bool mIsDirty = true;

	public List<vp_SurfaceTypes> SurfaceTypes = new List<vp_SurfaceTypes>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPPlayerEventHandler m_Player;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPCamera m_Camera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPController m_Controller;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioSource m_Audio;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioClip m_SoundToPlay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioClip m_LastPlayedSound;

	public static vp_FootstepManager[] FootstepManagers
	{
		get
		{
			if (mIsDirty)
			{
				mIsDirty = false;
				m_FootstepManagers = UnityEngine.Object.FindObjectsOfType(typeof(vp_FootstepManager)) as vp_FootstepManager[];
				if (m_FootstepManagers == null)
				{
					m_FootstepManagers = Resources.FindObjectsOfTypeAll(typeof(vp_FootstepManager)) as vp_FootstepManager[];
				}
			}
			return m_FootstepManagers;
		}
	}

	public bool IsDirty => mIsDirty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		m_Player = base.transform.root.GetComponentInChildren<vp_FPPlayerEventHandler>();
		m_Camera = base.transform.root.GetComponentInChildren<vp_FPCamera>();
		m_Controller = base.transform.root.GetComponentInChildren<vp_FPController>();
		m_Audio = base.gameObject.AddComponent<AudioSource>();
	}

	public virtual void SetDirty(bool dirty)
	{
		mIsDirty = dirty;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (m_Camera.BobStepCallback == null)
		{
			vp_FPCamera camera = m_Camera;
			camera.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Combine(camera.BobStepCallback, new vp_FPCamera.BobStepDelegate(Footstep));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		vp_FPCamera camera = m_Camera;
		camera.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Combine(camera.BobStepCallback, new vp_FPCamera.BobStepDelegate(Footstep));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		vp_FPCamera camera = m_Camera;
		camera.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Remove(camera.BobStepCallback, new vp_FPCamera.BobStepDelegate(Footstep));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Footstep()
	{
		if (!m_Controller.Grounded || (m_Player.GroundTexture.Get() == null && m_Player.SurfaceType.Get() == null))
		{
			return;
		}
		if (m_Player.SurfaceType.Get() != null)
		{
			PlaySound(SurfaceTypes[m_Player.SurfaceType.Get().SurfaceID]);
			return;
		}
		foreach (vp_SurfaceTypes surfaceType in SurfaceTypes)
		{
			foreach (Texture texture in surfaceType.Textures)
			{
				if (texture == m_Player.GroundTexture.Get())
				{
					PlaySound(surfaceType);
					break;
				}
			}
		}
	}

	public virtual void PlaySound(vp_SurfaceTypes st)
	{
		if (st.Sounds == null || st.Sounds.Count == 0)
		{
			return;
		}
		do
		{
			m_SoundToPlay = st.Sounds[UnityEngine.Random.Range(0, st.Sounds.Count)];
			if (m_SoundToPlay == null)
			{
				return;
			}
		}
		while (m_SoundToPlay == m_LastPlayedSound && st.Sounds.Count > 1);
		m_Audio.pitch = UnityEngine.Random.Range(st.RandomPitch.x, st.RandomPitch.y) * Time.timeScale;
		m_Audio.clip = m_SoundToPlay;
		m_Audio.Play();
		m_LastPlayedSound = m_SoundToPlay;
	}

	public static int GetMainTerrainTexture(Vector3 worldPos, Terrain terrain)
	{
		TerrainData terrainData = terrain.terrainData;
		Vector3 position = terrain.transform.position;
		int x = (int)((worldPos.x - position.x) / terrainData.size.x * (float)terrainData.alphamapWidth);
		int y = (int)((worldPos.z - position.z) / terrainData.size.z * (float)terrainData.alphamapHeight);
		float[,,] alphamaps = terrainData.GetAlphamaps(x, y, 1, 1);
		float[] array = new float[alphamaps.GetUpperBound(2) + 1];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = alphamaps[0, 0, i];
		}
		float num = 0f;
		int result = 0;
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] > num)
			{
				result = j;
				num = array[j];
			}
		}
		return result;
	}
}
