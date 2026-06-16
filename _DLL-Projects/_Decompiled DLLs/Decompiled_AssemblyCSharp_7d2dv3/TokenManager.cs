using System;
using System.Collections.Generic;
using UnityEngine;

public class TokenManager
{
	public class TokenRequester
	{
		public EntityAlive Entity;

		public double WorldTimeExpireRequest;

		public double WorldTimeExpireClaim;

		public double WorldTimeLastToken;
	}

	public struct TokenState(int maxClaims)
	{
		public readonly int MaxClaims = maxClaims;

		public readonly HashSet<TokenRequester> Claims = new HashSet<TokenRequester>();

		public readonly Queue<TokenRequester> Requesters = new Queue<TokenRequester>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class EntityTokenState
	{
		public readonly EntityAlive Entity;

		public readonly TokenState[] TokenStates = new TokenState[Enum.GetValues(typeof(AITokenType)).Length];

		public EntityTokenState(EntityAlive entity)
		{
			Entity = entity;
			Dictionary<AITokenType, AITokenConfig> tokenManagerConfig = Entity.EntityClass.TokenManagerConfig;
			for (int i = 0; i < TokenStates.Length; i++)
			{
				if (tokenManagerConfig != null && tokenManagerConfig.TryGetValue((AITokenType)i, out var value))
				{
					TokenStates[i] = new TokenState(value.MaxClaims);
				}
				else
				{
					TokenStates[i] = new TokenState(0);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Stack<EntityAlive> deadEntities = new Stack<EntityAlive>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<TokenRequester> deadRequesters = new List<TokenRequester>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<EntityAlive, EntityTokenState> EntityTokenStates = new Dictionary<EntityAlive, EntityTokenState>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static TokenManager Instance
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public AITokenClaim TryClaimToken(EntityAlive claimer, AITokenType tokenType, EntityAlive target, float claimTimeout, AIClaimTokenFlags claimTokenFlags = AIClaimTokenFlags.None)
	{
		if (claimer == null || target == null)
		{
			return AITokenClaim.NoneAvailable;
		}
		if (!EntityTokenStates.TryGetValue(target, out var value))
		{
			value = new EntityTokenState(target);
			EntityTokenStates[target] = value;
		}
		double num = (double)GameManager.Instance.World.worldTime / 20.0;
		double num2 = 0.0;
		int num3 = 0;
		int num4 = 0;
		ref TokenState reference = ref value.TokenStates[(int)tokenType];
		bool flag = false;
		TokenRequester tokenRequester = null;
		int count = reference.Requesters.Count;
		for (int i = 0; i < count; i++)
		{
			TokenRequester tokenRequester2 = reference.Requesters.Dequeue();
			if (tokenRequester2.Entity == null)
			{
				continue;
			}
			if (tokenRequester2.WorldTimeExpireRequest > num)
			{
				num3++;
				num2 += num - tokenRequester2.WorldTimeLastToken;
			}
			bool flag2 = reference.Claims.Contains(tokenRequester2);
			if (flag2)
			{
				num4++;
			}
			if (tokenRequester == null && tokenRequester2.Entity == claimer)
			{
				flag = flag2;
				if ((claimTokenFlags & AIClaimTokenFlags.UpdateTimeout) != AIClaimTokenFlags.None)
				{
					tokenRequester2.WorldTimeExpireRequest = num + 0.5;
				}
				tokenRequester = tokenRequester2;
				reference.Requesters.Enqueue(tokenRequester2);
			}
			else
			{
				reference.Requesters.Enqueue(tokenRequester2);
			}
		}
		if (tokenRequester == null)
		{
			tokenRequester = new TokenRequester
			{
				Entity = claimer,
				WorldTimeExpireRequest = num + 0.5
			};
			reference.Requesters.Enqueue(tokenRequester);
		}
		if (flag)
		{
			return AITokenClaim.AlreadyClaimed;
		}
		if (num4 < reference.MaxClaims)
		{
			double num5 = (num - tokenRequester.WorldTimeLastToken) / num2;
			if ((double)UnityEngine.Random.value <= num5)
			{
				tokenRequester.Entity = claimer;
				tokenRequester.WorldTimeExpireClaim = num + (double)claimTimeout;
				reference.Claims.Add(tokenRequester);
				return AITokenClaim.Claimed;
			}
		}
		return AITokenClaim.NoneAvailable;
	}

	public void ReleaseClaimToken(AITokenType tokenType, EntityAlive claimer, EntityAlive target)
	{
		if (!EntityTokenStates.TryGetValue(target, out var value))
		{
			return;
		}
		ref TokenState reference = ref value.TokenStates[(int)tokenType];
		foreach (TokenRequester requester in reference.Requesters)
		{
			if (requester.Entity == claimer)
			{
				requester.WorldTimeExpireClaim = 0.0;
				requester.WorldTimeLastToken = (double)GameManager.Instance.World.worldTime / 20.0;
				reference.Claims.Remove(requester);
			}
		}
	}

	public bool HasClaimToken(AITokenType tokenType, EntityAlive claimer, EntityAlive target)
	{
		if (EntityTokenStates.TryGetValue(target, out var value))
		{
			ref TokenState reference = ref value.TokenStates[(int)tokenType];
			foreach (TokenRequester requester in reference.Requesters)
			{
				if (requester.Entity == claimer)
				{
					return reference.Claims.Contains(requester);
				}
			}
		}
		return false;
	}

	public static void Init()
	{
		Instance = new TokenManager();
		Instance.Load();
	}

	public static void Cleanup()
	{
		Instance?.EntityTokenStates.Clear();
		Instance = null;
	}

	public void Clear()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Load()
	{
	}

	public void Update()
	{
		using (new ProfilerScope("TokenManager"))
		{
			double num = (double)GameManager.Instance.World.worldTime / 20.0;
			deadEntities.Clear();
			foreach (KeyValuePair<EntityAlive, EntityTokenState> entityTokenState in EntityTokenStates)
			{
				if (entityTokenState.Key == null)
				{
					deadEntities.Push(entityTokenState.Key);
					continue;
				}
				for (int i = 0; i < entityTokenState.Value.TokenStates.Length; i++)
				{
					ref TokenState reference = ref entityTokenState.Value.TokenStates[i];
					deadRequesters.Clear();
					foreach (TokenRequester claim in reference.Claims)
					{
						if (claim.WorldTimeExpireClaim < num)
						{
							deadRequesters.Add(claim);
						}
						else
						{
							claim.WorldTimeLastToken = num;
						}
					}
					if (deadRequesters.Count > 0)
					{
						reference.Claims.RemoveWhere([PublicizedFrom(EAccessModifier.Internal)] (TokenRequester x) => deadRequesters.Contains(x));
					}
				}
			}
			EntityAlive result;
			while (deadEntities.TryPop(out result))
			{
				EntityTokenStates.Remove(result);
			}
		}
	}
}
