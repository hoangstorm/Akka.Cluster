namespace Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            return (int) HostFactory.Run(x =>
            {
                x.SetServiceName("Lighthouse");
                x.SetDisplayName("Lighthouse Service Discovery");
                x.SetDescription("Lighthouse Service Discovery for Akka.NET Clusters");
                
                x.UseAssemblyInfoForServiceInfo();
                x.RunAsLocalSystem();
                //x.StartAutomatically();
                x.DependsOnEventLog();
                x.UseLog4Net();
                x.Service<LighthouseService>();
                x.EnableServiceRecovery(r => r.RestartService(1));
            });
        }
    }
}