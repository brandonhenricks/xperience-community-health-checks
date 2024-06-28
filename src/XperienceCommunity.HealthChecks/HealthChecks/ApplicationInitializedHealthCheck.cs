using CMS.DataEngine;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.HealthChecks.HealthChecks
{
    public sealed class ApplicationInitializedHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            if (!CMSApplication.ApplicationInitialized.HasValue)
            {
                return Task.FromResult(HealthCheckResult.Degraded("Application is not Initialized."));
            }

            if (CMSApplication.ApplicationInitialized.Value)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Application is Initialized."));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy(CMSApplication.ApplicationErrorMessage));
        }
    }
}
