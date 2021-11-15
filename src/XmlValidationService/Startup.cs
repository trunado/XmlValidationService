using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using XmlValidationService.Configurations;
using XmlValidationService.Controllers;
using XmlValidationService.GlobalExceptionHandler;

namespace XmlValidationService
{
  /// <summary>
  /// Startup the service
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class Startup
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    /// <summary>
    /// 
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// 
    /// </summary>
    public ILifetimeScope AutofacContainer { get; private set; }

    /// <summary>
    /// Register objects directly  with Autofac. This runs after ConfigureServices so the things
    /// here will override registrations made in ConfigureServices.
    /// </summary>
    /// <param name="builder"></param>
    public void ConfigureContainer(ContainerBuilder builder)
    {
      // Register your own things directly with Autofac here.
      builder.RegisterType<ServerResourceControl>().As<IServerResourceControl>();
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllers();

      // set the Anti forgery token cookie to use a secure cookie.
      services.AddAntiforgery(
          options =>
          {
            options.Cookie.Name = "_af";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.HeaderName = "X-XSRF-TOKEN";
          }
      );

      if (ShouldSwaggerBeEnabled())
      {
        ConfigureSwagger(services);
      }
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <remarks>
    /// This version of ConfigureServices is called when the service is started in Development. Similar to how env.IsDevelopment() works.
    /// </remarks>
    /// <param name="services"></param>
    public void ConfigureDevelopmentServices(IServiceCollection services)
    {
      ConfigureServices(services);
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    /// <param name="logger"></param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<XmlValidationServiceController> logger)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.ConfigureExceptionHandler();
      }

      app.UseRouting();

      app.UseAuthorization();


      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });

      if (ShouldSwaggerBeEnabled())
      {
        logger.LogInformation($"Swagger is enabled");

        app.UseSwagger();

        app.UseSwaggerUI(c =>
        {
          c.SwaggerEndpoint("/swagger/v1/swagger.json", "XML Validation Service");
          c.RoutePrefix = string.Empty;
        });
      }

      AutofacContainer = app.ApplicationServices.GetAutofacRoot();

      logger.LogInformation($"The local timezone is {TimeZoneInfo.Local.StandardName}");
    }

    private bool ShouldSwaggerBeEnabled()
    {
            return true;
      // Don't really need to do this right now, just always enable swagger
      //DiagnosticSettings diagnosticsSettings = Configuration.GetSection("Diagnostics").Get<DiagnosticSettings>();

      //if (string.IsNullOrWhiteSpace(diagnosticsSettings?.EnvironmentVariableToEnableSwagger))
      //{
      //  return false;
      //}

      //return Environment.GetEnvironmentVariable(diagnosticsSettings.EnvironmentVariableToEnableSwagger) != null;
    }

    private void ConfigureSwagger(IServiceCollection services)
    {
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Xml Validation Service API", Version = "v1", Description = "An XML Validator" });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
        c.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
        c.TagActionsBy(api =>
        {
          if (api.GroupName != null)
          {
            return new[] { api.GroupName };
          }

          var controllerActionDescriptor = api.ActionDescriptor as ControllerActionDescriptor;
          if (controllerActionDescriptor != null)
          {
            return new[] { controllerActionDescriptor.ControllerName };
          }

          throw new InvalidOperationException("Unable to determine tag for endpoint.");
        });
        c.DocInclusionPredicate((name, api) => true);
      });
    }
  }
}
