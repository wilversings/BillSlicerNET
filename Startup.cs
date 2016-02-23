using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BillSlicer.Startup))]
namespace BillSlicer
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
