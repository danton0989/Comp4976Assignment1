using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Obituary.AppHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = DistributedApplication.CreateBuilder(args);

            var sqlServer = builder.AddSqlServer("theserver")
                .AddDatabase("sqldata");

            var api = builder.AddProject<Projects.ObituaryApp>("obituaryapi")
                .WithReference(sqlServer);

            var frontend = builder.AddProject<Projects.Frontend_Blazor>("frontend")
                .WithReference(api);

            builder.Build().Run();
        }
    }
}