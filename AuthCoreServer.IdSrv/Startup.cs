using AuthCoreServer.IdSrv.Configuration;
using AuthCoreServer.IdSrv.Contexto;
using AuthCoreServer.IdSrv.Entidades;
using IdentityServer4;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuthCoreServer.IdSrv
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowAll", builder =>
            //    {
            //        builder.WithOrigins("http://localhost:4200") // URL do seu Angular
            //               .AllowAnyHeader()
            //               .AllowAnyMethod()
            //               .AllowCredentials(); // Importante para OIDC
            //    });
            //});

            services.AddControllersWithViews();
            services.AddDataBaseConfiguration(Configuration);

            services.AddIdentity<Usuario, Funcao>()
                .AddEntityFrameworkStores<AuthDBContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IProfileService, ProfileService>();

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                options.AccessTokenJwtType = "JWT";

                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                //options.EmitStaticAudienceClaim = true;
            })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryApiResources(Config.ApiResources)
                .AddInMemoryClients(Config.Clients)
                .AddAspNetIdentity<Usuario>();
               

            // not recommended for production - you need to store your key material somewhere secure
             builder.AddDeveloperSigningCredential();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    // register your IdentityServer with Google at https://console.developers.google.com
                    // enable the Google+ API
                    // set the redirect URI to https://localhost:5001/signin-google
                    options.ClientId = "copy client ID from Google here";
                    options.ClientSecret = "copy client secret from Google here";
                });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = ".AspNetCore.Identity.Application";
                options.Cookie.SameSite = SameSiteMode.None; // Obrigatório para Cross-Origin
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Obrigatório para SameSite=None
            });

            //services.AddCookiePolicy


        }

        public void Configure(IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                // Adiciona "data:" na lista de fontes permitidas para imagens (img-src)
                context.Response.Headers.Add("Content-Security-Policy", "img-src 'self' data:;");
                await next();
            });

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseStaticFiles();

           
            app.UseRouting();
           // app.UseCors("AllowAll");
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });

            
        }
    }
}