using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using dFakto.States.Workers.Config;
using dFakto.States.Workers.FileStores;
using dFakto.States.Workers.FileStores.Ftp;
using dFakto.States.Workers.Sql;
using dFakto.States.Workers.Sql.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace dFakto.States.Workers.TestsHost
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);

            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", true);
                    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true);
                    config.AddEnvironmentVariables();

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                    services.AddStepFunctions(
                        hostContext.Configuration.GetSection("stepFunctions").Get<StepFunctionsConfig>(),
                    hostContext.Configuration.GetSection("fileStores").Get<FileStoreFactoryConfig>(), x =>
                        {
                            x.Config.EnvironmentName = hostContext.HostingEnvironment.EnvironmentName;

                            x.AddFtpFileStore();
                            x.AddDirectoryFileStore();
                            
                            x.AddSqlWorkers(hostContext.Configuration.GetSection("databases")
                                .Get<IEnumerable<DatabaseConfig>>());

                            //x.AddWorkers(Assembly.GetExecutingAssembly());
                            x.AddWorker<HttpWorker>();
                            x.AddWorker<GZipWorker>();
                            x.AddWorker("Dummy", Task.FromResult);
                            x.AddWorker("Hello", s => Task.FromResult($"Hello {s}!"));
                        });
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.SetMinimumLevel(LogLevel.Debug);
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                }).Build();
            
            await builder.RunAsync();
        }
    }
}