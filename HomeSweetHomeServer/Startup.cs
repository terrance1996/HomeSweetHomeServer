using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HomeSweetHomeServer.Contexts;
using Microsoft.EntityFrameworkCore;
using HomeSweetHomeServer.Repositories;
using HomeSweetHomeServer.Models;
using HomeSweetHomeServer.Services;
using Newtonsoft.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using System;
using HomeSweetHomeServer.Exceptions;
using Newtonsoft.Json;
using Microsoft.AspNetCore.HttpOverrides;

namespace HomeSweetHomeServer
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
            //Jwt token authentication options
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration.GetValue<string>("JWT:Issuer"),
                    ValidAudience = Configuration.GetValue<string>("JWT:Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("JWT:Key"))),
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };
            });

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ExceptionFilter));
                options.Filters.Add(typeof(ModelValidationFilter));
            }).AddJsonOptions(a => a.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver())
              .AddJsonOptions(a => a.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
            
            services.AddDbContext<DatabaseContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IInformationRepository, InformationRepository>();
            services.AddScoped<IUserInformationRepository, UserInformationRepository>();
            services.AddScoped<IFriendshipRepository, FriendshipRepository>();
            services.AddScoped<IHomeRepository, HomeRepository>();
            services.AddScoped<INotepadRepository, NotepadRepository>();
            services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
            services.AddScoped<IExpenseRepository, ExpenseRepository>();
            services.AddScoped<IUserExpenseRepository, UserExpenseRepository>();
            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<IMealRepository, MealRepository>();
            services.AddScoped<IMenuMealRepository, MenuMealRepository>();
            services.AddScoped<IHouseworkRepository, HouseworkRepository>();

            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IHomeService, HomeService>();
            services.AddScoped<IFCMService, FCMService>();
            services.AddScoped<INotepadService, NotepadService>();
            services.AddScoped<IShoppingListService, ShoppingListService>();
            services.AddScoped<IUserExpenseService, UserExpenseService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IHouseworkService, HouseworkService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
            //    app.UseDeveloperExceptionPage();
            }
            
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
                context.Database.Migrate();
              //  context.Database.EnsureCreated();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
