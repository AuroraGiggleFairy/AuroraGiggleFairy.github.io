using System.Threading.Tasks;

namespace Discord;

internal interface IUpdateable
{
	Task UpdateAsync(RequestOptions options = null);
}
