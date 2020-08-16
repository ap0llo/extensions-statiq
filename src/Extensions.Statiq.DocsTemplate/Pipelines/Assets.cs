using System;
using System.Collections.Generic;
using Statiq.Common;
using Statiq.Core;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Pipelines
{
    /// <summary>
    /// Pipeline to copy static assets required by the configured theme to the output.
    /// This pipeline is automatically added by <see cref="BootstrapperExtensions.AddDocsTemplate{TBootstrapper}(TBootstrapper)"/>
    /// </summary>
    public class Assets : Pipeline
    {
        public Assets()
        {
            OutputModules = new ModuleList()
            {
                Copy(themeName => $"theme/{themeName}/js/**").To(path => path.RemovePrefixDirectories(3).Prepend("assets/js")),

                Copy(themeName => $"theme/{themeName}/css/**").To(path => path.RemovePrefixDirectories(3).Prepend("assets/css")),

                Copy(themeName => $"theme/{themeName}/dependencies/**/*.js").To(path => path.RemovePrefixDirectories(3).Prepend("assets/js")),

                Copy(themeName => $"theme/{themeName}/dependencies/**/*.css").To(path => path.RemovePrefixDirectories(3).Prepend("assets/css"))
            };
        }


        private CopyFiles Copy(Func<string, string> fromThemeName)
        {
            var config = Config.FromContext<IEnumerable<string>>(ctx =>
            {
                var themeName = ctx.GetString(DocsTemplateKeys.DocsTemplateThemeName);
                if (String.IsNullOrEmpty(themeName))
                {
                    themeName = DocsTemplateThemeNames.Default;
                }

                return new[] { fromThemeName(themeName) };
            });

            return new CopyFiles(config);
        }
    }
}
