using Microsoft.Owin.Hosting;
using scanner_win_service.Config;
using System;
using System.ServiceProcess;

namespace scanner_win_service.Service
{
    class HttpApiService : ServiceBase
    {
        /// <summary>
        /// Localhost including the API port number
        /// TODO: add the required firewall settings to allow connecting via this port
        /// </summary>
        const string _url = "http://*:3001";

        /// <summary>
        /// A reference for the created web app instance. Should be disposed on service close
        /// </summary>
        IDisposable _web_app;


        protected override void OnStart(string[] args)
        {
            System.Diagnostics.Debugger.Launch();
            _web_app = WebApp.Start<Startup>(_url);
        }

        protected override void OnStop()
        {
            _web_app.Dispose();
        }
    }
}
