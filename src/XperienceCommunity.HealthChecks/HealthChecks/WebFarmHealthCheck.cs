using System.Collections.ObjectModel;
using CMS.DataEngine;
using CMS.WebFarmSync;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using XperienceCommunity.HealthChecks.Extensions;

namespace XperienceCommunity.HealthChecks.HealthChecks
{
    /// <summary>
    /// Web Farm Health Check
    /// </summary>
    public sealed class WebFarmHealthCheck : BaseKenticoHealthCheck<WebFarmServerInfo>, IHealthCheck
    {
        private readonly IInfoProvider<WebFarmServerInfo> _webFarmServerInfoProvider;

        public WebFarmHealthCheck(IInfoProvider<WebFarmServerInfo> webFarmServerInfoProvider)
        {
            _webFarmServerInfoProvider = webFarmServerInfoProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            if (!CMSApplication.ApplicationInitialized.HasValue)
            {
                return HealthCheckResult.Healthy("Application is not Initialized.");
            }

            try
            {
                var webFarmServers = await GetDataForTypeAsync(cancellationToken);

                if (webFarmServers.Count == 0)
                {
                    return HealthCheckResult.Degraded("No Web Farm Info Returned");
                }

                foreach (var server in webFarmServers)
                {
                    if (server.Status == WebFarmServerStatusEnum.NotResponding)
                    {
                        return HealthCheckResult.Degraded($"Server {server.ServerName} is not responding.", null,
                            GetData(webFarmServers));
                    }
                }

                // If all servers are running, return a healthy status
                return HealthCheckResult.Healthy("All servers in the web farm are running.", GetData(webFarmServers));
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        protected override IEnumerable<WebFarmServerInfo> GetDataForType()
        {
            throw new NotImplementedException();
        }

        protected override async Task<List<WebFarmServerInfo>> GetDataForTypeAsync(
            CancellationToken cancellationToken = default)
        {
            using (new CMSConnectionScope())
            {
                var query = _webFarmServerInfoProvider
                    .Get();

                return await query.ToListAsync(cancellationToken: cancellationToken);
            }
        }

        protected override IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<WebFarmServerInfo> objects)
        {
            var dictionary = objects.ToDictionary<WebFarmServerInfo, string, object>(webFarm => webFarm.ServerName,
                webFarmTask => webFarmTask.Status.ToString());

            return new ReadOnlyDictionary<string, object>(dictionary);
        }

        private static IReadOnlyDictionary<string, object> GetData(IEnumerable<WebFarmServerInfo> objects)
        {
            var dictionary = objects.ToDictionary<WebFarmServerInfo, string, object>(webfarm => webfarm.ServerName,
                webfarm => webfarm.Status.ToString());

            return new ReadOnlyDictionary<string, object>(dictionary);
        }
    }
}
