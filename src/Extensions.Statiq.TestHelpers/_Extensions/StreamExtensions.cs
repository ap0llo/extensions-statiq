using System.IO;

namespace Grynwald.Extensions.Statiq.TestHelpers
{
    public static class StreamExtensions
    {
        public static string ReadAsString(this Stream stream)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
