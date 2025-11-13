using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
// using DotNetEnv;

namespace Obituary.AppHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = DistributedApplication.CreateBuilder(args);
            // Env.Load();
            var sqlServer = builder.AddSqlServer("theserver")
                .AddDatabase("sqldata");

            var api = builder.AddProject<Projects.ObituaryApp>("obituaryapi")
                .WithReference(sqlServer)
                .WaitFor(sqlServer);
                // .WithEnvironment("GOOGLE_API_KEY", "AIzaSyA6_zAz4iWS565Z5QwATQOftExevOxO8gY")
                

            var frontend = builder.AddProject<Projects.Frontend_Blazor>("frontend")
                .WithReference(api);

            builder.Build().Run();
        }
    }
}