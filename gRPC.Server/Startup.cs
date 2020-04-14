using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using gRPC.Server.Services;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gRPC.Server
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddAuthorization();
            services.AddAuthentication()
                .AddCertificate(opt =>
            {
                opt.RevocationMode = X509RevocationMode.NoCheck;
                opt.AllowedCertificateTypes = CertificateTypes.SelfSigned;
                opt.Events = new CertificateAuthenticationEvents()
                {
                    OnCertificateValidated = context =>
                    {
                        context.Success();
                        return Task.CompletedTask;
                    }
                };
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GrpcEmployeeService>(); //将进入的请求映射到特定的服务类中
            });
        }
    }
}
