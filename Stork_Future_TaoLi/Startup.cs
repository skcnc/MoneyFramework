using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Stork_Future_TaoLi.Startup))]
namespace Stork_Future_TaoLi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
        }
    }
}