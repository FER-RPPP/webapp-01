using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;
using FluentValidation.AspNetCore;
using FluentValidation;
using RPPP_WebApp.ModelsValidation;

namespace RPPP_WebApp {
  /// <summary>
  /// Extension methods for configuring the startup of the web application.
  /// </summary>
  public static class StartupExtensions {

    /// <summary>
    /// Configures services for the web application.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> used to configure the application.</param>
    /// <returns>The configured <see cref="WebApplication"/>.</returns>
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder) {
      builder.Services.AddControllersWithViews();


      builder.Services.AddFluentValidationAutoValidation()
                      .AddFluentValidationClientsideAdapters()
                      .AddValidatorsFromAssemblyContaining<OwnerValidator>()
                      .AddValidatorsFromAssemblyContaining<ProjectCardValidator>()
                      .AddValidatorsFromAssemblyContaining<TransactionValidator>();

      IConfiguration configuration = builder.Configuration;
      builder.Services.AddDbContext<Rppp01Context>(options => {
        options.UseSqlServer(configuration.GetConnectionString("RPPP01"));
      }, contextLifetime: ServiceLifetime.Transient);

      return builder.Build();
    }

    /// <summary>
    /// Configures the middleware pipeline for the web application.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance.</param>
    /// <returns>The configured <see cref="WebApplication"/>.</returns>
    public static WebApplication ConfigurePipeline(this WebApplication app) {
      #region Needed for nginx and Kestrel (do not remove or change this region)
      app.UseForwardedHeaders(new ForwardedHeadersOptions {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                           ForwardedHeaders.XForwardedProto
      });
      string pathBase = app.Configuration["PathBase"];
      if (!string.IsNullOrWhiteSpace(pathBase)) {
        app.UsePathBase(pathBase);
      }
      #endregion

      if (app.Environment.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
      }

      app.UseStaticFiles()
         .UseRouting()
         .UseEndpoints(endpoints => {
           endpoints.MapDefaultControllerRoute();
         });

      return app;
    }

  }
}