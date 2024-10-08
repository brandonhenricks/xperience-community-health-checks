﻿using CMS.DataEngine;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.HealthChecks.HealthChecks
{
    /// <summary>
    /// Represents a base class for Kentico health checks.
    /// </summary>
    /// <typeparam name="T">The type of the Kentico object.</typeparam>
    public abstract class BaseKenticoHealthCheck<T> where T : AbstractInfoBase<T>, new()
    {
        private readonly IInfoProvider<T> _infoProvider;

        protected BaseKenticoHealthCheck(IInfoProvider<T> infoProvider)
        {
            _infoProvider = infoProvider;
        }

        /// <summary>
        /// Gets the provider for the Kentico object.
        /// </summary>
        protected IInfoProvider<T> Provider => _infoProvider;

        /// <summary>
        /// Handles the exception and returns the health check result.
        /// </summary>
        /// <param name="ex">The exception to handle.</param>
        /// <returns>The health check result.</returns>
        protected static HealthCheckResult HandleException(Exception ex)
        {
            if (ex is OperationCanceledException or TaskCanceledException)
            {
                return HealthCheckResult.Healthy("Operation Cancelled.");
            }

            return ex switch
            {
                InvalidOperationException ioe when
                    ioe.Message.Contains("open DataReader", StringComparison.OrdinalIgnoreCase) ||
                    ioe.Message.Contains("current state", StringComparison.OrdinalIgnoreCase) ||
                    ioe.Message.Contains("reader is closed", StringComparison.OrdinalIgnoreCase) ||
                    ioe.Message.Contains("connection is closed", StringComparison.OrdinalIgnoreCase) =>
                    HealthCheckResult.Healthy(ioe.Message),
                InvalidOperationException ioe => HealthCheckResult.Degraded(ioe.Message, ioe),
                DataClassNotFoundException de => HealthCheckResult.Healthy(de.Message),
                LinqExpressionCannotBeExecutedException lec => HealthCheckResult.Healthy(lec.Message),
                _ => HealthCheckResult.Unhealthy(ex.Message, ex)
            };
        }

        /// <summary>
        /// Gets the data for the specified Kentico object type asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation and contains an enumerable collection of Kentico objects.</returns>
        protected abstract Task<List<T>> GetDataForTypeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the error data for the specified Kentico objects.
        /// </summary>
        /// <param name="objects">The Kentico objects.</param>
        /// <returns>A read-only dictionary containing the error data.</returns>
        protected abstract IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<T> objects);
    }
}
