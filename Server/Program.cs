using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Command;
using Server.Game;
using Server.Package;
using SuperSocket;
using SuperSocket.Command;

namespace Server
{
    class Program
    {
        private static GameWorld _sGameWorld; 
        
        static void Main(string[] args)
        {
            InitLog();
            
            var contex = new SynchronizationQueue();
            var superSocket = RunSuperSocket(args);
            Console.WriteLine("Main Thread ID " + Thread.CurrentThread.ManagedThreadId);
            _sGameWorld = new GameWorld();
            try
            {
                while (true)
                {
                    Thread.Sleep(3);
                    contex.Update();
                    _sGameWorld.Update();
                    if (superSocket.IsCanceled || superSocket.Exception != null)
                    {
                        break;
                    }
                }
                _sGameWorld?.OnDestroy();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        static void InitLog()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            MDC.Set("tab", "\t");
        }

        static Task RunSuperSocket(string[] args)
        {
            var host = SuperSocketHostBuilder.Create<DefaultPackage, DefaultPackageFilter>(args)
                .UseHostedService<GameService<DefaultPackage>>()
                .UseCommand((commandOptions) =>
                {
                    commandOptions.AddCommand<LoginCommand>();
                    // commandOptions.AddCommandAssembly(typeof(LoginCommand).GetTypeInfo().Assembly);
                })
                .ConfigureSuperSocket(options =>
                {
                    options.Name = "CustomProtocol Server";
                    options.Listeners = new List<ListenOptions>
                    {
                        new ListenOptions
                        {
                            Ip = "Any",
                            Port = 4040
                        }
                    };
                })
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                }).Build();
            
            return host.RunAsync();
        }
    }
}
