using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Grynwald.Extensions.Statiq.Git
{
    internal static class StreamExtensions
    {
        public static string ReadAsString(this Stream stream)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
