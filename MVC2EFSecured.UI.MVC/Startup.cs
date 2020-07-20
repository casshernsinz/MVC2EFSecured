using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MVC2EFSecured.UI.MVC.Startup))]
namespace MVC2EFSecured.UI.MVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
