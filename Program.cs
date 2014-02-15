using System.ServiceProcess;

namespace StartIISAppPool
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            var servicesToRun = new ServiceBase[]
                {
                    new StartStoppedService()
                };
            ServiceBase.Run(servicesToRun);
        }
    }
}