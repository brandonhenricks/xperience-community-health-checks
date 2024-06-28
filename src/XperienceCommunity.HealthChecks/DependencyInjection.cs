using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.HealthChecks.HealthChecks;

namespace XperienceCommunity.HealthChecks
{
    /// <summary>
    /// Dependency Injection Helper Methods.
    /// </summary>
    public static class DependencyInjection
    {
        private const string Kentico = "Kentico";
        private static readonly string[] s_tags = [Kentico];

        /// <summary>
        /// Adds Kentico Health Checks to the specified <see cref="IHealthChecksBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add the health checks to.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/> instance.</returns>

        [return: NotNull]
        public static IHealthChecksBuilder AddKenticoHealthChecks(this IHealthChecksBuilder builder)
        {
            return builder
                .AddCheck<ApplicationInitializedHealthCheck>("Application Initialized Health Check", tags: s_tags)
                .AddCheck<EventLogHealthCheck>("Event Log Health Check", tags: s_tags)
                .AddCheck<WebFarmHealthCheck>("Web Farm Health Check", tags: s_tags)
                .AddCheck<WebFarmTaskHealthCheck>("Web Farm Task Health Check", tags: s_tags)
                .AddCheck<EmailHealthCheck>("Email Health Check", tags: s_tags);
        }

    }
}
