using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class BakeScheduler
{
	public enum BakeJobState
	{
		None,
		Pending,
		InProgress,
		GeneratingMips,
		Complete
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct BakeJob
	{
		public SignBakeRequest Request;

		public RenderTexture RT;

		public int Step;

		public BakeJobState State;

		public void Begin(SignBakeRequest request, RenderTexture rt)
		{
			Request = request;
			RT = rt;
			Step = 0;
			State = BakeJobState.Pending;
		}

		public void End()
		{
			RT = null;
			Step = 0;
			State = BakeJobState.None;
			Request = default(SignBakeRequest);
		}
	}

	public static readonly ProfilerMarker s_SignTextureManagerClear = new ProfilerMarker("SignTextureManager.BakeScheduler.Clear");

	public static readonly ProfilerMarker s_SignTextureManagerResetRequests = new ProfilerMarker("SignTextureManager.BakeScheduler.ResetRequests");

	public static readonly ProfilerMarker s_SignTextureManagerTickOnce = new ProfilerMarker("SignTextureManager.BakeScheduler.TickOnce");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<double> s_BakeJobProgress = new ProfilerCounterValue<double>(ProfilerCategory.Scripts, "STM Job Progress", ProfilerMarkerDataUnit.Percent, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_PendingBakeJobs = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "STM Pending Jobs", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<SignBakeRequest> _requests = new List<SignBakeRequest>(256);

	[PublicizedFrom(EAccessModifier.Private)]
	public int _index;

	[PublicizedFrom(EAccessModifier.Private)]
	public BakeJob _current;

	public BakeJobState CurrentState => _current.State;

	public void Clear()
	{
		using (s_SignTextureManagerClear.Auto())
		{
			_requests.Clear();
			_index = 0;
			_current.End();
			s_PendingBakeJobs.Value = 0;
			s_BakeJobProgress.Value = 0.0;
		}
	}

	public void ResetRequests(List<SignBakeRequest> newRequests)
	{
		using (s_SignTextureManagerResetRequests.Auto())
		{
			_requests.Clear();
			for (int i = 0; i < newRequests.Count; i++)
			{
				_requests.Add(newRequests[i]);
			}
			_index = 0;
			_current.End();
		}
	}

	public bool TickOnce(int tileSize, Material material, CommandBuffer cmd, SignTextureStore store, SignPrioritizer prioritizer, bool showProgress = false)
	{
		using (s_SignTextureManagerTickOnce.Auto())
		{
			s_PendingBakeJobs.Value = _requests.Count - _index;
			if (_current.State == BakeJobState.None)
			{
				while (_index < _requests.Count)
				{
					SignBakeRequest request = _requests[_index++];
					RenderTexture renderTexture = store.AcquireForBake(request.Tier);
					if (renderTexture == null)
					{
						Log.Warning($"[STM] Pool exhausted at tier {request.Tier}; skipping requested bake.");
						continue;
					}
					_current.Begin(request, renderTexture);
					break;
				}
				if (_current.State == BakeJobState.None)
				{
					s_BakeJobProgress.Value = 0.0;
					return false;
				}
			}
			if (_current.RT == null)
			{
				_current.End();
				return true;
			}
			RenderTargetIdentifier renderTargetIdentifier = new RenderTargetIdentifier(_current.RT);
			cmd.Clear();
			if (_current.State == BakeJobState.Pending)
			{
				GlobalSignId groupSignId = prioritizer.GetGroupSignId(_current.Request.GroupIndex);
				SignDataManager.Instance.TryApplyRenderingData(groupSignId, 1f, material, null, SignUIStyle.Baked);
				material.SetVector(SignShaderIDs._CanvasAspect, Vector2.one);
				cmd.SetRenderTarget(renderTargetIdentifier);
				cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
				if (showProgress)
				{
					prioritizer.ApplyToGroupCanvases(_current.Request.GroupIndex, _current.RT);
				}
				_current.State = BakeJobState.InProgress;
			}
			if (_current.State == BakeJobState.InProgress)
			{
				int num = Mathf.Max(_current.RT.width / tileSize, 1);
				int num2 = num * num;
				int num3 = _current.Step % num;
				int num4 = _current.Step / num;
				Vector2 vector = Vector2.one / num * 2f;
				Vector2 vector2 = new Vector2(-1f + (float)num3 * vector.x, -1f + (float)num4 * vector.y);
				material.SetVector(SignShaderIDs._MinUV, vector2);
				material.SetVector(SignShaderIDs._MaxUV, vector2 + vector);
				cmd.Blit(BuiltinRenderTextureType.None, renderTargetIdentifier, material, 0);
				Graphics.ExecuteCommandBuffer(cmd);
				_current.Step++;
				s_BakeJobProgress.Value = 100f * (float)_current.Step / (float)num2;
				if (_current.Step >= num2)
				{
					_current.State = BakeJobState.Complete;
				}
				return true;
			}
			if (_current.State == BakeJobState.Complete)
			{
				GlobalSignId groupSignId2 = prioritizer.GetGroupSignId(_current.Request.GroupIndex);
				store.SetBaked(groupSignId2, _current.Request.Tier, _current.RT);
				prioritizer.ApplyToGroupCanvases(_current.Request.GroupIndex, _current.RT);
				_current.End();
				return true;
			}
			Log.Error($"Unexpected case in SignTextureManager: TickOnce reached failsafe return case. Current job state is: {_current.State}.");
			_current.State = BakeJobState.None;
			return false;
		}
	}
}
