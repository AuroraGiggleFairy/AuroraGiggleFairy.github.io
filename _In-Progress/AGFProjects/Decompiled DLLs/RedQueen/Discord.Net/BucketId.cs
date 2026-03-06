using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord.Net;

internal class BucketId : IEquatable<BucketId>
{
	public string HttpMethod { get; }

	public string Endpoint { get; }

	public IOrderedEnumerable<KeyValuePair<string, string>> MajorParameters { get; }

	public string BucketHash { get; }

	public bool IsHashBucket => BucketHash != null;

	private BucketId(string httpMethod, string endpoint, IEnumerable<KeyValuePair<string, string>> majorParameters, string bucketHash)
	{
		HttpMethod = httpMethod;
		Endpoint = endpoint;
		MajorParameters = majorParameters.OrderBy((KeyValuePair<string, string> x) => x.Key);
		BucketHash = bucketHash;
	}

	public static BucketId Create(string httpMethod, string endpoint, Dictionary<string, string> majorParams)
	{
		Preconditions.NotNullOrWhitespace(endpoint, "endpoint");
		if (majorParams == null)
		{
			majorParams = new Dictionary<string, string>();
		}
		return new BucketId(httpMethod, endpoint, majorParams, null);
	}

	public static BucketId Create(string hash, BucketId oldBucket)
	{
		Preconditions.NotNullOrWhitespace(hash, "hash");
		Preconditions.NotNull(oldBucket, "oldBucket");
		return new BucketId(null, null, oldBucket.MajorParameters, hash);
	}

	public string GetBucketHash()
	{
		if (!IsHashBucket)
		{
			return null;
		}
		return BucketHash + ":" + string.Join("/", MajorParameters.Select((KeyValuePair<string, string> x) => x.Value));
	}

	public string GetUniqueEndpoint()
	{
		if (HttpMethod == null)
		{
			return Endpoint;
		}
		return HttpMethod + " " + Endpoint;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as BucketId);
	}

	public override int GetHashCode()
	{
		if (!IsHashBucket)
		{
			return (HttpMethod, Endpoint).GetHashCode();
		}
		return (BucketHash, string.Join("/", MajorParameters.Select((KeyValuePair<string, string> x) => x.Value))).GetHashCode();
	}

	public override string ToString()
	{
		return GetBucketHash() ?? GetUniqueEndpoint();
	}

	public bool Equals(BucketId other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (GetType() != other.GetType())
		{
			return false;
		}
		return ToString() == other.ToString();
	}
}
