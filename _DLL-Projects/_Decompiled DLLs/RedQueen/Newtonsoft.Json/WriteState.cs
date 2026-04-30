namespace Newtonsoft.Json;

internal enum WriteState
{
	Error,
	Closed,
	Object,
	Array,
	Constructor,
	Property,
	Start
}
