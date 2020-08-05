using System.Threading.Tasks;

namespace Grynwald.Extensions.Statiq.Git
{
    internal static class Extensions
    {
        public static Task<T> AsTask<T>(this T result) => Task.FromResult(result);
    }
}
