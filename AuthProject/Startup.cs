using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AuthProject.AuthService;
using AuthProject.Context;
using AuthProject.EmailSender;
using AuthProject.Identities;
using AuthProject.ServiceCollectionExtensions;
using AuthProject.Services;
using AuthProject.WorkflowTest;
using Force;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

#nullable enable
namespace AuthProject
{
#nullable enable

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static SymmetricSecurityKey signingKey;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AuthDbContext>(x =>
                x.UseSqlServer(Configuration.GetConnectionString("ef")));

            services.AddIdentity<CustomIdentityUser, CustomIdentityRole>(
                    x =>
                    {
                        x.SignIn.RequireConfirmedEmail = true;
                        x.Password.RequireDigit = false;
                        x.Password.RequireLowercase = false;
                        x.Password.RequireUppercase = false;
                        x.Password.RequiredLength = 5;
                    })
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders();

            // @formatter:off 
            services.AutoRegistration();
            services.AddScoped<JwtAuthorizeService>();
            services.AddScoped<SmtpClient>();
            services.AddScoped<EmailSenderService>();
            services.AddScoped(typeof(WorkflowManager<,>));
            // @formatter:on

            services.Configure<EmailSenderConfiguration>(Configuration.GetSection("EmailSenderConfiguration"));

            var jwtAppSettingOptions = Configuration.GetSection("JwtTokenOptions");
            services.Configure<JwtTokenOptions>(Configuration.GetSection("JwtTokenOptions"));
            signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtAppSettingOptions["SecretKey"]));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "issuer",
                ValidateAudience = true,
                ValidAudience = "audience",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };


            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.ClaimsIssuer = tokenValidationParameters.ValidIssuer;
                    options.TokenValidationParameters = tokenValidationParameters;
                    options.SaveToken = true;
                });

//            services.AddMvc(config =>
//            {
//                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
//                config.Filters.Add(new AuthorizeFilter(policy));
//            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            });

            services.AddMvc();
//            services.AddMvc(options => { options.ModelBinderProviders.Insert(0, new WorkflowBinderProvider()); });
            services.AddControllers()
                .AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,EmailSenderService emailSender)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

//            app.UseHttpsRedirection();


            app.UseRouting();

            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var props = errorApp.Properties.ToList();
                    context.Response.StatusCode = 320;
                    await context.Response.WriteAsync("");
                });
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}