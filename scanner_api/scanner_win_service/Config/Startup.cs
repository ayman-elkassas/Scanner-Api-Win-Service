using Owin;
using System.Web.Http;

namespace scanner_win_service.Config
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "ScannerAPI",
                routeTemplate: "api/{controller}/"
            );
            app.UseWebApi(config);
        }
    }
}
