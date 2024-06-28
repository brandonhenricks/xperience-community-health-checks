# XperienceCommunity.HealthChecks

This project is a NuGet package specifically designed to integrate Kentico Health Checks into applications using the Microsoft.AspNetCore.Health framework. It provides a set of custom health checks that monitor various aspects of a Kentico application, ensuring its optimal performance and stability.

The health checks included in this package cover a wide range of Kentico functionalities, from site configuration and event logs to web farm and search tasks. By leveraging the Microsoft.AspNetCore.Health framework, these health checks can be easily added to any ASP.NET Core application, providing developers with immediate insights into the health of their Kentico applications.

This package is an essential tool for any developer working with Kentico in an ASP.NET Core environment, simplifying the process of monitoring and maintaining the health of their applications.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Installation Instructions

To integrate Kentico Health Checks into your ASP.NET Core application, follow these steps:

1. **Ensure Prerequisites:**
    - Make sure your application is running on `.NET 6` or `.NET 8`.
    - Verify that your application is a Xperience by Kentico application.

2. **Install Required Package:**
    - You need the `Microsoft.Extensions.Diagnostics.HealthChecks` package for utilizing the `IHealthChecksBuilder` interface. If not already installed, you can add it via NuGet with the following command:
      ```
      dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks
      ```

3. **Install Kentico Health Checks Package:**
    - Install the XperienceCommunity.HealthChecks package via NuGet using the following command:
      ```
      dotnet add package XperienceCommunity.HealthChecks
      ```

4. **Configure Services:**
    - In your `Startup.cs` or `Program.cs` file, where you configure your services, add the Kentico Health Checks. You can add all available health checks or specific ones based on your needs.

      **To add all Kentico Health Checks:**
      ```csharp
      public void ConfigureServices(IServiceCollection services)
      {
            services
            .AddHealthChecks()
            .AddKenticoHealthChecks();
      }
      ```

      **To add specific Kentico Health Checks:**
      ```csharp
      public void ConfigureServices(IServiceCollection services)
      {
            services.AddHealthChecks()
                      .AddCheck<WebFarmHealthCheck>("Site Configuration Health Check");
      }
      ```

5. **Middleware Registration:**
    - Register the health checks middleware in your `Startup.cs` or equivalent configuration file to expose the health checks via an HTTP endpoint.

      ```csharp
      public void Configure(IApplicationBuilder app)
      {    
            app.UseHealthChecks("/kentico-health");
      }
      ```

6. **Endpoint Registration**

    - In your `Startup.cs` file (or equivalent configuration file), use the `MapHealthChecks` extension method on your `IEndpointRouteBuilder` instance. This registers the health checks as an endpoint.


    ```csharp
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapHealthChecks("/kentico-health");
    }
    ```

7. **Endpoint Registration with Custom Writer**

    - In your `Startup.cs` file (or equivalent configuration file), use the `MapHealthChecks` extension method on your `IEndpointRouteBuilder` instance. This registers the health checks as an endpoint.
    This allows you to customize your HealthCheckOptions class, and specifiy a custom Response Writer.

    Here's an example:

    ```csharp
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapHealthChecks("/kentico-health",
            new HealthCheckOptions()
            {
                AllowCachingResponses = true,
                ResponseWriter = HealthCheckResponseWriter.WriteResponse
            });
    }
    ```

By following these steps, you will successfully integrate Kentico Health Checks into your ASP.NET Core application, allowing you to monitor various aspects of your Kentico application's health.

## Health Checks

### Application Initialized Health Check

The `ApplicationInitializedHealthCheck` class is an implementation of the `IHealthCheck` interface. It is used to perform a health check on the application initialization. 

### Email Health Check

The `EmailHealthCheck` class is an implementation of the `IHealthCheck` interface. It is used to perform a health check on the email service in the application.

### EventLogHealthCheck

The `EventLogHealthCheck` class is an implementation of the `IHealthCheck` interface. It is used to perform a health check on the event log by investigating the last 100 event log entries for errors. 

### WebFarmHealthCheck

The `WebFarmHealthCheck` class is an implementation of the `IHealthCheck` interface provided by the `Microsoft.Extensions.Diagnostics.HealthChecks` namespace. It is used to perform health checks on the Kentico web farm servers.

### WebFarmTaskHealthCheck

The `WebFarmTaskHealthCheck` class is an implementation of the `IHealthCheck` interface. It is responsible for checking the health of the web farm server tasks. 

## Built With

* [Microsoft.AspNetCore.Health](https://www.nuget.org/packages/Microsoft.AspNetCore.Diagnostics.HealthChecks/) - The web framework used
* [Xperience By Kentico](https://www.kentico.com) - Kentico Xperience
* [NuGet](https://nuget.org/) - Dependency Management

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **Brandon Henricks** - *Initial work* - [Brandon Henricks](https://github.com/brandonhenricks)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* [Mike Wills](https://github.com/heywills)
