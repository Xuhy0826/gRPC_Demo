using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;

namespace gRPC.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    //ʹ��֤��
                    webBuilder.ConfigureKestrel(opt =>
                    {
                        //����֤��
                        var cert = new X509Certificate2("gRPCDemoSslCert.pfx", "P@ssw0rd!");
                        //����
                        opt.ConfigureHttpsDefaults(h =>
                            {
                                h.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
                                h.CheckCertificateRevocation = false;
                                h.ServerCertificate = cert;
                            });
                    });
                });
    }
}
