using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LCA_WEB.Startup))]
namespace LCA_WEB
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
