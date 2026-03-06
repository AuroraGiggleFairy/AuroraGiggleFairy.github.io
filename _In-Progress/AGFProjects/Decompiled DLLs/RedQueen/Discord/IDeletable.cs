using System.Threading.Tasks;

namespace Discord;

internal interface IDeletable
{
	Task DeleteAsync(RequestOptions options = null);
}
