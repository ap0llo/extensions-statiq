using System.Threading.Tasks;
using Statiq.App;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Demo
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await Bootstrapper
                .Factory
                .CreateDefault(args)
                .AddDocsTemplate()
                .AddSetting(DocsTemplateKeys.DocsTemplateThemeName, DocsTemplateThemeNames.Default)
                .AddSetting(DocsTemplateKeys.SiteName, "ChangeLog Docs")
                .AddSetting("GitRemoteUrl", "https://github.com/ap0llo/changelog.git")
                .RunAsync();
        }
    }
}
