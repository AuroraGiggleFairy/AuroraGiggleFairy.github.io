using System;
using Epic.OnlineServices;

namespace Platform.EOS;

[PublicizedFrom(EAccessModifier.Internal)]
public struct EOSSanction
{
	public DateTime expiry;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string ReferenceId { get; }

	public EOSSanction(DateTime? expiryDate, Utf8String referenceId)
	{
		ReferenceId = referenceId;
		expiry = expiryDate.GetValueOrDefault();
	}
}
