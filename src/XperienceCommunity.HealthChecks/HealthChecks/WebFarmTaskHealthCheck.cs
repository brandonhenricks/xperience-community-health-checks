using System.Collections.ObjectModel;
using CMS.Base;
using CMS.Base.Internal;
using CMS.DataEngine;
using CMS.WebFarmSync;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using XperienceCommunity.HealthChecks.Extensions;

namespace XperienceCommunity.HealthChecks.HealthChecks
{
    /// <summary>
    /// Web Farm Server Task Health Check
    /// </summary>
    public sealed class WebFarmTaskHealthCheck : BaseKenticoHealthCheck<WebFarmServerTaskInfo>, IHealthCheck
    {
        private static readonly string[] s_columnNames =
        [
            nameof(WebFarmServerTaskInfo.ErrorMessage),
            nameof(WebFarmServerTaskInfo.TaskID)
        ];

        public WebFarmTaskHealthCheck(IInfoProvider<WebFarmServerTaskInfo> infoProvider) : base(infoProvider)
        {
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var result = HealthCheckResult.Healthy();

            if (!CMSApplication.ApplicationInitialized.HasValue)
            {
                return result;
            }

            try
            {
                var data = await GetDataForTypeAsync(cancellationToken);

                if (data.Count != 0)
                {
                    result = HealthCheckResult.Degraded("Web Farm Tasks Contain Errors.", null, GetErrorData(data));
                }

                return result;
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        protected override async Task<List<WebFarmServerTaskInfo>> GetDataForTypeAsync(
            CancellationToken cancellationToken = default)
        {
            ContextUtils.ResetCurrent();

            using (new CMSConnectionScope(true))
            {
                var query = Provider
                    .Get()
                    .Columns(s_columnNames)
                    .WhereNotNullOrEmpty(nameof(WebFarmServerTaskInfo.ErrorMessage))
                    .TopN(50);

                return await query.ToListAsync(cancellationToken: cancellationToken);
            }
        }

        protected override IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<WebFarmServerTaskInfo> objects)
        {
            var dictionary = objects.ToDictionary<WebFarmServerTaskInfo, string, object>(
                webFarmTask => webFarmTask.TaskID.ToString(), webFarmTask => webFarmTask.ErrorMessage);

            return new ReadOnlyDictionary<string, object>(dictionary);
        }
    }
}
