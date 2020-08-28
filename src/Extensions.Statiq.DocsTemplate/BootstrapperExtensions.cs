using System.IO;
using System.Reflection;
using Grynwald.Extensions.Statiq.DocsTemplate.Pipelines;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.DocsTemplate
{
    public static class BootstrapperExtensions
    {
        internal static NormalizedPath BuiltInInputPath =>
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "input");

        public static TBootstrapper AddDocsTemplate<TBootstrapper>(this TBootstrapper bootstrapper) where TBootstrapper : IBootstrapper
        {
            return bootstrapper
                .ConfigureEngine(engine => engine.FileSystem.InputPaths.Insert(0, BuiltInInputPath))
                .AddPipeline(typeof(Assets));
        }
    }
}
