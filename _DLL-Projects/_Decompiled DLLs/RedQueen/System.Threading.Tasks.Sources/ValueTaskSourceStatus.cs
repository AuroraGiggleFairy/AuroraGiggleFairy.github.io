namespace System.Threading.Tasks.Sources;

internal enum ValueTaskSourceStatus
{
	Pending,
	Succeeded,
	Faulted,
	Canceled
}
