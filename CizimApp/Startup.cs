using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CizimApp.Data;
using CizimApp.Hubs;
using CizimApp.Models;
using CizimApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CizimApp
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
            services.AddSignalR(o => {
                o.EnableDetailedErrors = true;

            });;
            services.AddCors(options =>
            {
                options.AddPolicy("Cors",
                builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader()
                                .AllowAnyMethod();
                });
            });
            services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

















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
            app.UseCors("Cors");


            app.UseEndpoints(endpoints =>
                     {
                    endpoints.MapControllers();
                    endpoints.MapHub<Chathub>("/chatHub");
                 

                     });
        }
    }
}
