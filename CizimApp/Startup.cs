
using CizimApp.Hubs;
using CizimAppData;
using CizimAppData.Helpers;
using CizimAppData.Repository;
using CizimAppEntity.Models;
using Microsoft.AspNetCore.Builder;

using Microsoft.AspNetCore.Hosting;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


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
            services.AddHostedService<RoomWorker.Worker>();
            services.AddHostedService<ConnectedUserWorker.Worker>();

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
                opt.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("CizimApp"));
            });
       
            services.AddScoped(typeof(IGenericRepository<>), typeof(EfRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IWordRepository, WordRepository>();
            services.AddScoped<IConnectedUserRepository, ConnectedUserRepository>();
            services.AddScoped<IRedisHandler, RedisHandler>();




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
