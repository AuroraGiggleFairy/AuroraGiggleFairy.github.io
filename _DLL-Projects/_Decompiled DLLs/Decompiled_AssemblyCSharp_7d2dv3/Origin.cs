using System;
using System.Collections.Generic;
using Audio;
using Unity.Collections;
using UnityEngine;

public class Origin : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct TransformLevel(Transform _transform, int _level)
	{
		public readonly Transform transform = _transform;

		public readonly int level = _level;

		public readonly string name = transform.name;
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cAutoRepositionDistanceSq = 67600f;

	public static Origin Instance;

	public static Action<Vector3> OriginChanged;

	public static Vector3 position;

	public bool isAuto;

	public Vector3 OriginPos;

	[Tooltip("Force a move every x seconds")]
	public float timedMove;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timedMoveTime;

	public Vector3 timedMoveDistance = new Vector3(16f, 0f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int timedMoveCount;

	public Vector3 MoveOriginTo;

	public bool isMoveOriginNow;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<TransformLevel> RepositionObjects = new List<TransformLevel>();

	public static List<Transform> particleSystemTs = new List<Transform>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<ParticleSystem> particleSystems = new List<ParticleSystem>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public NativeArray<ParticleSystem.Particle> particles;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform physicsCheckT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 physicsCheckPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int checkRepositionDelay = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		Instance = this;
		isAuto = true;
		Shader.SetGlobalVector("_OriginPos", position);
		particles = new NativeArray<ParticleSystem.Particle>(512, Allocator.Persistent);
		physicsCheckT = base.transform.GetChild(0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		particles.Dispose();
	}

	public static void Cleanup()
	{
		position = Vector3.zero;
		Shader.SetGlobalVector("_OriginPos", position);
	}

	public static void Add(Transform _t, int _level)
	{
		RepositionObjects.Add(new TransformLevel(_t, _level));
	}

	public static void Remove(Transform _t)
	{
		for (int num = RepositionObjects.Count - 1; num >= 0; num--)
		{
			if (RepositionObjects[num].transform == _t)
			{
				RepositionObjects.RemoveAt(num);
			}
		}
	}

	public void Reposition(Vector3 _newOrigin)
	{
		DoReposition(_newOrigin);
		Physics.simulationMode = SimulationMode.Script;
		Physics.Simulate(0.01f);
		Physics.simulationMode = SimulationMode.FixedUpdate;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DoReposition(Vector3 _newOrigin)
	{
		_newOrigin.x = (int)_newOrigin.x & -16;
		_newOrigin.y = (int)_newOrigin.y & -16;
		_newOrigin.z = (int)_newOrigin.z & -16;
		Log.Out("{0}+{1} Origin Reposition {2} to {3}", GameManager.frameCount, GameManager.fixedUpdateCount, position.ToCultureInvariantString(), _newOrigin.ToCultureInvariantString());
		Vector3 vector = position - _newOrigin;
		position = _newOrigin;
		OriginPos = _newOrigin;
		physicsCheckPos = -position;
		physicsCheckPos.y -= 256f;
		physicsCheckT.position = physicsCheckPos;
		checkRepositionDelay = 0;
		Shader.SetGlobalVector("_OriginPos", position);
		for (int i = 0; i < RepositionObjects.Count; i++)
		{
			RepositionTransform(vector, RepositionObjects[i].transform, RepositionObjects[i].level);
		}
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
		if ((bool)primaryPlayer)
		{
			vp_FPController component = primaryPlayer.GetComponent<vp_FPController>();
			if ((bool)component)
			{
				component.Reposition(vector);
			}
			primaryPlayer.emodel.ClearClothMotion();
		}
		List<Entity> list = world.Entities.list;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			list[num].OriginChanged(vector);
		}
		RepositionParticles(vector);
		if (AstarManager.Instance != null)
		{
			AstarManager.Instance.OriginChanged();
		}
		if (world.m_ChunkManager != null)
		{
			world.m_ChunkManager.OriginChanged(vector);
		}
		if (DecoManager.Instance != null)
		{
			DecoManager.Instance.OriginChanged(vector);
		}
		if ((bool)OcclusionManager.Instance)
		{
			OcclusionManager.Instance.OriginChanged(vector);
		}
		Manager.OriginChanged(vector);
		DynamicMeshManager.OriginUpdate();
		OriginChanged?.Invoke(_newOrigin);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RepositionParticles(Vector3 _deltaV)
	{
		for (int num = particleSystemTs.Count - 1; num >= 0; num--)
		{
			Transform transform = particleSystemTs[num];
			if (!transform)
			{
				particleSystemTs.RemoveAt(num);
			}
			else
			{
				transform.GetComponentsInChildren(particleSystems);
				for (int num2 = particleSystems.Count - 1; num2 >= 0; num2--)
				{
					ParticleSystem particleSystem = particleSystems[num2];
					if (particleSystem.isPlaying && particleSystem.main.simulationSpace == ParticleSystemSimulationSpace.World)
					{
						int num3 = particleSystem.GetParticles(particles);
						for (int i = 0; i < num3; i++)
						{
							ParticleSystem.Particle value = particles[i];
							value.position += _deltaV;
							particles[i] = value;
						}
						particleSystem.SetParticles(particles, num3);
						particleSystem.Simulate(0f, withChildren: false, restart: false);
						particleSystem.Play(withChildren: false);
					}
				}
				particleSystems.Clear();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RepositionTransform(Vector3 _deltaV, Transform _t, int _level)
	{
		if (!_t)
		{
			return;
		}
		if (_level < 0)
		{
			_t.position += _deltaV;
			return;
		}
		int childCount = _t.childCount;
		if (_level == 0)
		{
			for (int i = 0; i < childCount; i++)
			{
				_t.GetChild(i).position += _deltaV;
			}
			return;
		}
		for (int j = 0; j < childCount; j++)
		{
			Transform child = _t.GetChild(j);
			RepositionTransform(_deltaV, child, _level - 1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdate()
	{
		if (GameManager.IsDedicatedServer || !GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return;
		}
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		if (isAuto)
		{
			List<EntityPlayerLocal> localPlayers = world.GetLocalPlayers();
			if (localPlayers.Count > 0)
			{
				EntityPlayerLocal player = localPlayers[0];
				UpdateLocalPlayer(player);
			}
		}
		else
		{
			if (timedMove > 0f)
			{
				timedMoveTime += Time.deltaTime;
				if (timedMoveTime >= timedMove)
				{
					timedMoveTime = 0f;
					timedMoveCount++;
					Vector3 newOrigin = default(Vector3);
					newOrigin.x = (float)(timedMoveCount & 3) * timedMoveDistance.x;
					newOrigin.y = (float)((timedMoveCount >> 2) & 1) * timedMoveDistance.y;
					newOrigin.z = (float)((timedMoveCount >> 1) & 1) * timedMoveDistance.z;
					Reposition(newOrigin);
				}
			}
			if (isMoveOriginNow)
			{
				Reposition(MoveOriginTo);
				MoveOriginTo = Vector3.zero;
				isMoveOriginNow = false;
			}
		}
		if (checkRepositionDelay < 0 || --checkRepositionDelay >= 0)
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			bool flag = true;
			Vector3 vector = physicsCheckPos;
			vector.y += 10f;
			if (!Physics.Raycast(vector, Vector3.down, float.MaxValue, 65536))
			{
				flag = false;
				Log.Warning("{0}+{1} Origin ray fail {2}", GameManager.frameCount, GameManager.fixedUpdateCount, vector.ToCultureInvariantString());
			}
			if (world != null)
			{
				List<EntityPlayerLocal> localPlayers2 = world.GetLocalPlayers();
				if (localPlayers2.Count > 0)
				{
					EntityPlayerLocal entityPlayerLocal = localPlayers2[0];
					if (entityPlayerLocal.IsSpawned() && !entityPlayerLocal.IsFlyMode.Value)
					{
						Vector3 vector2 = entityPlayerLocal.transform.position;
						vector2.y += 1.5f;
						if (!Physics.Raycast(vector2, Vector3.down, float.MaxValue, 65536))
						{
							flag = false;
							Log.Warning("{0}+{1} Origin player ray fail {2}", GameManager.frameCount, GameManager.fixedUpdateCount, vector2.ToCultureInvariantString());
						}
					}
				}
			}
			if (flag)
			{
				checkRepositionDelay = -1;
				break;
			}
			Vector3 newOrigin2 = position;
			newOrigin2.x += 16f;
			Reposition(newOrigin2);
		}
		if (checkRepositionDelay >= 0)
		{
			checkRepositionDelay = 3;
		}
	}

	public void UpdateLocalPlayer(EntityPlayerLocal player)
	{
		if (isAuto && player.IsSpawned() && (player.position - position).sqrMagnitude > 67600f)
		{
			Reposition(player.position);
		}
	}
}
