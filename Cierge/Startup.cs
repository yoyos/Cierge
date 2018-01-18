using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Cierge.Data;
using Cierge.Services;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Cierge.Filters;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;

namespace Cierge
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Env { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Disabling HTTPS requirement is default since it is assumed that
            // this is running behind a reverse proxy that requires HTTPS
            var requireHttps = !String.IsNullOrWhiteSpace(Configuration["RequireHttps"]) && Boolean.Parse(Configuration["RequireHttps"]) == true;

            if (requireHttps)
                services.Configure<MvcOptions>(options =>
                {
                    options.Filters.Add(new RequireHttpsAttribute());
                });

            services.AddMvc();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (Env.IsDevelopment())
                    options.UseSqlServer(Configuration["ConnectionStrings:DefaultConnection"]);
                else
                    options.UseNpgsql(Configuration["ConnectionStrings:DefaultConnection"]);

                options.UseOpenIddict();
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Lockout.AllowedForNewUsers = true;
                options.User.RequireUniqueEmail = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            if (!String.IsNullOrWhiteSpace(Configuration["ExternalAuth:Google:ClientId"]))
            {
                services.AddAuthentication().AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = Configuration["ExternalAuth:Google:ClientId"];
                    googleOptions.ClientSecret = Configuration["ExternalAuth:Google:ClientSecret"];
                });
            }

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            services.AddOpenIddict(options =>
            {
                options.AddEntityFrameworkCoreStores<ApplicationDbContext>();
                options.AddMvcBinders();
                options.EnableAuthorizationEndpoint("/connect/authorize")
                       .EnableLogoutEndpoint("/connect/logout")
                       .EnableIntrospectionEndpoint("/connect/introspect")
                       .EnableUserinfoEndpoint("/api/userinfo");
                options.AllowImplicitFlow();

                // Might need to manually set issuer if running behind reverse proxy
                var issuer = Configuration["Cierge:Issuer"];
                if (!String.IsNullOrWhiteSpace(issuer))
                    options.SetIssuer(new Uri(issuer));

                if (!requireHttps)
                    options.DisableHttpsRequirement();

                if (!Env.IsDevelopment())
                {
                    options.AddEphemeralSigningKey();
                }
                else
                {
                    options.AddSigningKey(new RsaSecurityKey(GetRsaSigningKey()));
                }
                options.UseJsonWebTokens();
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder.WithOrigins("http://localhost:8000")
                                .AllowAnyMethod()
                                .AllowAnyHeader());
            });

            if (Env.IsDevelopment())
            {
                services.AddTransient<IEmailSender, DevMessageSender>();
                services.AddTransient<ISmsSender, DevMessageSender>();
            }
            else
            {
                services.AddScoped<IEmailSender, SmtpMessageSender>();
            }

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<ValidateRecaptchaAttribute>();

            services.AddScoped<NoticeService>();

            services.AddScoped<EventsService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            // Does not work?
            var options = new RewriteOptions()
                .AddRedirectToHttps();

            app.UseCors("AllowSpecificOrigin");

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }
        public RSAParameters GetRsaSigningKey()
        {
            var path = Configuration["Cierge:RsaSigningKeyJsonPath"];
            RSAParameters parameters;

            if (!String.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                // Get RSA JSON from file
                string jsonString;
                FileStream fileStream = new FileStream(path, FileMode.Open);
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    jsonString = reader.ReadToEnd();
                }

                dynamic paramsJson = JsonConvert.DeserializeObject(jsonString);

                parameters = new RSAParameters
                {
                    Modulus = paramsJson.Modulus != null ? Convert.FromBase64String(paramsJson.Modulus.ToString()) : null,
                    Exponent = paramsJson.Exponent != null ? Convert.FromBase64String(paramsJson.Exponent.ToString()) : null,
                    P = paramsJson.P != null ? Convert.FromBase64String(paramsJson.P.ToString()) : null,
                    Q = paramsJson.Q != null ? Convert.FromBase64String(paramsJson.Q.ToString()) : null,
                    DP = paramsJson.DP != null ? Convert.FromBase64String(paramsJson.DP.ToString()) : null,
                    DQ = paramsJson.DQ != null ? Convert.FromBase64String(paramsJson.DQ.ToString()) : null,
                    InverseQ = paramsJson.InverseQ != null ? Convert.FromBase64String(paramsJson.InverseQ.ToString()) : null,
                    D = paramsJson.D != null ? Convert.FromBase64String(paramsJson.D.ToString()) : null
                };

            }
            else
            {
                // Generate RSA key
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(2048);

                parameters = RSA.ExportParameters(true);
            }

            return parameters;
        }
    }
}
