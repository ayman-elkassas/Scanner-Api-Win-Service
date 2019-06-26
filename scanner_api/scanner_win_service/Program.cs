using scanner_win_service.Service;
using System.ServiceProcess;


namespace scanner_win_service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new HttpApiService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
