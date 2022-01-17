#pragma warning disable CS1591 // Missing XML comment
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AuthServicePluginBase;
using AutoMapper;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using Laboremus.Messaging.Email.Extensions;
using Laboremus_AuthorizationService.Core;
using Laboremus_AuthorizationService.Core.Extensions;
using Laboremus_AuthorizationService.Core.Helpers;
using Laboremus_AuthorizationService.Core.Helpers.HangFire;
using Laboremus_AuthorizationService.Data;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Models;
using Laboremus_AuthorizationService.Services;
using Laboremus_AuthorizationService.Services.EmailSender;
using Laboremus_AuthorizationService.Services.ZipService;
using Laboremus_AuthorizationService.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PluginBase;
using PluginBase.AsemblyLoaderManager;
using Swashbuckle.AspNetCore.ReDoc;
using Swashbuckle.AspNetCore.Swagger;

namespace Laboremus_AuthorizationService
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;
        
        public Startup(IConfiguration configuration, IHostingEnvironment env, ILogger<Startup> logger)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            Environment = env;

            _logger = logger;

            _logger.LogInformation("##########Authorization Service started");
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;

            services.AddAutoMapper(AutoMapperMiddleware.Configure, GetType().Assembly);

            services.Configure<CookiePolicyOptions>(options =>
            {
                _logger.LogInformation(
                    "determine whether user consent for non-essential cookies is needed for a given request.");
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
                options.Secure = CookieSecurePolicy.Always;
                options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
            });

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddFluentValidation(fv =>
                {
                    fv.RegisterValidatorsFromAssemblyContaining<UserLockOutViewModelValidator>();
                });

            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            services.AddCors(options =>
            {
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins("*")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            #region Dependency Injection

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>(); 
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<DbContext, ConfigurationDbContext>();

            services.AddDependencyInjection();

            #endregion

            #region LoadPlugins

            //get plugin section from application.json
            var pluginsFromConfiguration = Configuration.GetSection("Plugins").Get<List<PlugIn>>();

            if (pluginsFromConfiguration != null)
            {
                //load plugins from plugin folder collection of .dlls
                var plugins = GenericPluginLoader<IPlugin>.LoadPlugins(@"Plugins");

                if (plugins != null)
                {
                    //Load plug in after trying to find it in list and check if it's enable
                    if (pluginsFromConfiguration.FirstOrDefault(p => p.Name == "IRolesInfoPlugin")?.Enable == true)
                    {
                        var rolesInfoPlugin = plugins.FirstOrDefault(x => x is IRolesInfoPlugin);
                        if (rolesInfoPlugin != null)
                            services.AddScoped(typeof(IRolesInfoPlugin), rolesInfoPlugin.GetType());
                    }

                    //Load plug in after trying to find it in list and check if it's enabled
                    if (pluginsFromConfiguration.FirstOrDefault(p => p.Name == "IClaimsAdjusterPlugin")?.Enable == true)
                    {
                        var additionalClaimsGeneratorPlugin = plugins.FirstOrDefault(x => x is IClaimsAdjusterPlugin);
                        if (additionalClaimsGeneratorPlugin != null)
                            services.AddScoped(typeof(IClaimsAdjusterPlugin), additionalClaimsGeneratorPlugin.GetType());
                    }

                    //Load plug in after trying to find it in list and check if it's enable
                    if (pluginsFromConfiguration.FirstOrDefault(p => p.Name == "IIdentityTokenPlugin")?.Enable == true)
                    {
                        var identityTokenPlugin = plugins.FirstOrDefault(x => x is IIdentityTokenPlugin);
                        if (identityTokenPlugin != null)
                            services.AddScoped(typeof(IIdentityTokenPlugin), identityTokenPlugin.GetType());
                    }
                }
            }

            #endregion

            #region IdentityOptions

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 1;

                // Set Lockout settings.
                options.Lockout.AllowedForNewUsers = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(36500);
            });

            #endregion

            #region MVC

            services.AddMvc(options => { options.Filters.Add(new ValidateModelAttribute()); })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.ContractResolver = new DefaultContractResolver()
                        {
                            NamingStrategy = new CamelCaseNamingStrategy()
                        };
                        options.SerializerSettings.Formatting = Formatting.Indented;
                        options.SerializerSettings.StringEscapeHandling = StringEscapeHandling.Default;
                        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
                    }
                );

            #endregion

            #region Identity Server Configurations

            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddConfigurationStore(configDb =>
                {
                    configDb.ConfigureDbContext = db =>
                        db.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(operationalDb =>
                {
                    operationalDb.ConfigureDbContext = db =>
                        db.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddAspNetIdentity<ApplicationUser>()
                .AddProfileService<ProfileService>();

            #endregion

            services.AddExternalOidcProviders(_logger, Configuration);

            #region Add local api authentication

            services.AddAuthentication("Bearer")
                .AddJwtBearer(options =>
                {
                    options.Authority = Configuration.GetAuthServer();
                    options.RequireHttpsMetadata = false;
                    options.Audience = IdentityServerConstants.LocalApi.ScopeName;
                });

            #endregion

            #region Swagger Documentation

            var enableSwagger = Configuration.GetSwaggerEnabled();
            if (enableSwagger)
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Info
                    {
                        Title = "Authorization Service",
                        Version = "beta",
                        Description = "This is a restful web api for authentication and authorization.",
                        Contact = new Contact
                        {
                            Name = "Wilson Kiggundu",
                            Email = "wilson@laboremus.no"
                        }
                    });

                    c.DescribeAllEnumsAsStrings();

                    var basePath = Environment.WebRootPath;
                    var xmlPath = Path.Combine(basePath, "Documentation", "AuthService.xml");
                    c.IncludeXmlComments(xmlPath);
                });

            #endregion

            #region Add a default in-memory implementation of IDistributedCache

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.MaxAge = TimeSpan.FromSeconds(0);
            });

            #endregion

            // Add Hangfire services.
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

            // Add the processing server as IHostedService
            services.AddHangfireServer();

            // call email config static file 
            services.ConfigureMessagingEmail(Configuration);

            services.Configure<ExportSettings>(options => Configuration.GetSection(ExportSettings.ConfigurationName)
                .Bind(options));

            services.AddDataProtection();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            #region Logging

            loggerFactory.AddConsole();
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            #endregion

            #region Exception Handling

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCustomErrorHandling();

            #endregion

            app.UseCors("default");
            app.UseIdentityServer();

            #region Swagger Documentation

            var enableSwagger = Configuration.GetSwaggerEnabled();
            if (enableSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Authorization Service V1");
                    c.InjectStylesheet("/swagger-ui/custom.css");
                    c.DocumentTitle = "Authorization Service";
                    c.RoutePrefix = "docs";
                });

                app.UseReDoc(c =>
                {
                    c.SpecUrl = "/swagger/v1/swagger.json";
                    c.DocumentTitle = "Documentation";
                    c.RoutePrefix = "docs/new";
                    c.ConfigObject = new ConfigObject
                    {
                        ExpandResponses = "200",
                        PathInMiddlePanel = false
                    };

                });
            }

            #endregion

            app.UseStaticFiles();

            app.UseHangfireDashboard("/jobs", new DashboardOptions
            {
                Authorization = new[]
                {
                    new HangFireAuthorizationFilter(),
                }
            });

            RecurringJob.AddOrUpdate("Delete Old export files",
                () => serviceProvider.GetService<IZipService>()
                        .DeleteRequestExportAsync(),
                            Cron.Daily());

            app.UseCookiePolicy();
            app.UseMvcWithDefaultRoute();
        }
    }
}