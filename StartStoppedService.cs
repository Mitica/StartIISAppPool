using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using Microsoft.Web.Administration;

namespace StartIISAppPool
{
    public partial class StartStoppedService : ServiceBase
    {
        public StartStoppedService()
        {
            InitializeComponent();
        }

        private Thread _job;
        private static ApplicationPoolCollection _appPollCollection;

        /// <summary>
        /// Runs on start service
        /// </summary>
        /// <param name="args">empty OR: 1st MUST be running test interval in sec.(default 10 sec) and next, optional, app pool names to test</param>
        protected override void OnStart(string[] args)
        {
            var service = Assembly.GetAssembly(typeof (StartStoppedService));
            var config = System.Configuration.ConfigurationManager.OpenExeConfiguration(service.Location);
            var interval = config.AppSettings.Settings["interval"].Value;

            var list = new List<string> {interval};

            if (config.AppSettings.Settings.AllKeys.Contains("apps"))
            {
                var apps = config.AppSettings.Settings["apps"].Value.Split(new[] {" "},
                                                                           StringSplitOptions.RemoveEmptyEntries);
                list.AddRange(apps);
            }

            var serverManager = new ServerManager();
            _appPollCollection = serverManager.ApplicationPools;

            if (_job == null)
                _job = new Thread(DoJob);
            _job.Start(list.ToArray());
        }

        protected override void OnStop()
        {
            try
            {
                _job.Abort();
            }
            catch
            {
            }
        }


        private static void DoJob(object data)
        {
            var names = (string[]) data;

            var seconds = int.Parse(names[0]);
            names = names.Skip(1).ToArray();

            while (true)
            {
                TestAppPolls(names);
                Thread.Sleep(seconds*1000);
            }
        }

        private static void TestAppPolls(ICollection<string> names)
        {
            foreach (var applicationPool in _appPollCollection)
            {
                if (applicationPool.State == ObjectState.Stopped)
                {
                    if (names == null || names.Count == 0 ||
                        names.Contains(applicationPool.Name, StringComparer.InvariantCultureIgnoreCase))
                    {
                        applicationPool.Start();
                    }
                }
            }
        }
    }
}