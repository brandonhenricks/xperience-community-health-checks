using CMS.Base;
using CMS.DataEngine;
using CMS.Websites;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using XperienceCommunity.HealthChecks.Extensions;

namespace XperienceCommunity.HealthChecks.HealthChecks
{
    public sealed class WebsiteChannelHealthCheck : BaseKenticoHealthCheck<WebsiteChannelInfo>, IHealthCheck
    {
        public WebsiteChannelHealthCheck(IInfoProvider<WebsiteChannelInfo> infoProvider) : base(infoProvider)
        {
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (!CMSApplication.ApplicationInitialized.HasValue)
            {
                return HealthCheckResult.Healthy("Application is not Initialized.");
            }

            var data = await GetDataForTypeAsync(cancellationToken);

            if (data.Count == 0)
            {
                return HealthCheckResult.Unhealthy("There are no Website Channels Configured.");
            }

            return HealthCheckResult.Healthy($"There are {data.Count} Website Channels configured.");
        }

        protected override async Task<List<WebsiteChannelInfo>> GetDataForTypeAsync(
            CancellationToken cancellationToken = default)
        {
            ContextUtils.ResetCurrent();

            using (new CMSConnectionScope(true))
            {
                var query = Provider.Get()
                    .TopN(100);

                return await query.ToListAsync(cancellationToken: cancellationToken);
            }
        }

        protected override IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<WebsiteChannelInfo> objects)
        {
            return new Dictionary<string, object>(0);
        }
    }
}
