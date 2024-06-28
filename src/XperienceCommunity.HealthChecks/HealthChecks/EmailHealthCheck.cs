using System.Collections.ObjectModel;
using CMS.DataEngine;
using CMS.EmailEngine;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using XperienceCommunity.HealthChecks.Extensions;

namespace XperienceCommunity.HealthChecks.HealthChecks
{
    public sealed class EmailHealthCheck : BaseKenticoHealthCheck<EmailInfo>, IHealthCheck
    {
        private static readonly string[] s_columnNames =
        [
            nameof(EmailInfo.EmailStatus),
            nameof(EmailInfo.EmailLastSendAttempt),
            nameof(EmailInfo.EmailID)
        ];

        private readonly IInfoProvider<EmailInfo> _emailInfoProvider;

        public EmailHealthCheck(IInfoProvider<EmailInfo> emailInfoProvider)
        {
            _emailInfoProvider = emailInfoProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (!CMSApplication.ApplicationInitialized.HasValue)
            {
                return HealthCheckResult.Healthy("Application is not Initialized.");
            }

            try
            {
                var currentTimePlusTwoHours = DateTime.UtcNow.AddHours(-1);

                var data = await GetDataForTypeAsync(cancellationToken);

                var filtered = data.Where(email =>
                        email.EmailLastSendAttempt < currentTimePlusTwoHours &&
                        email.EmailStatus == EmailStatusEnum.Waiting)
                    .ToList();

                if (filtered.Count > 0)
                {
                    return HealthCheckResult.Degraded("Email Items are not being sent.", data: GetErrorData(filtered));
                }

                return HealthCheckResult.Healthy("Email Items Appear to be Healthy.");
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        protected override IEnumerable<EmailInfo> GetDataForType()
        {
            throw new NotImplementedException();
        }

        protected override async Task<List<EmailInfo>> GetDataForTypeAsync(
            CancellationToken cancellationToken = default)
        {
            using (new CMSConnectionScope(true))
            {
                var query = _emailInfoProvider.Get()
                    .Columns(s_columnNames)
                    .WhereEquals(nameof(EmailInfo.EmailStatus), EmailStatusEnum.Waiting)
                    .TopN(100);

                return await query.ToListAsync(cancellationToken: cancellationToken);
            }
        }

        protected override IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<EmailInfo> objects)
        {
            var dictionary = objects.ToDictionary<EmailInfo, string, object>(email => email.EmailID.ToString(),
                email => $"{email.EmailStatus} - {email.EmailLastSendAttempt}");

            return new ReadOnlyDictionary<string, object>(dictionary);
        }
    }
}
