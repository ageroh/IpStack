using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Novibet.IpStack.Abstractions;
using Novibet.IpStack.Business.Data;
using Novibet.IpStack.Business.Services;
using Novibet.IpStack.Client;

namespace Novibet.IpStack.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            
            services.AddMemoryCache();
            
            services.AddDbContext<IpStackContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddHttpClient<IIPInfoProvider, IpStackInfoClient>();
            services.AddTransient<IIpStackService, IpStackService>();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
