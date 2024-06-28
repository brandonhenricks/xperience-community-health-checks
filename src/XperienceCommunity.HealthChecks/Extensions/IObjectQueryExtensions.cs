using System.Data;
using System.Diagnostics.CodeAnalysis;
using CMS.DataEngine;

namespace XperienceCommunity.HealthChecks.Extensions
{
    internal static class IObjectQueryExtensions
    {
        /// <summary>
        /// Return an IEnumerable of objects from the query.
        /// </summary>
        /// <typeparam name="TQuery"></typeparam>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="query">The object query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An IEnumerable of objects from the query.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the query is null.</exception>
        [return: NotNull]
        internal static async Task<List<TObject>> ToListAsync<TQuery, TObject>(this IObjectQuery<TQuery, TObject> query, CancellationToken cancellationToken = default)
            where TQuery : IObjectQuery<TQuery, TObject>
            where TObject : BaseInfo
        {
            ArgumentNullException.ThrowIfNull(query);

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var results = await query
                    .GetTypedQuery()
                    .GetEnumerableTypedResultAsync(commandBehavior: CommandBehavior.Default, false,
                        cancellationToken: cancellationToken);

                return results?.ToList() ?? new List<TObject>(0);
            }
            catch (InvalidOperationException)
            {
                var results = await query
                    .GetTypedQuery()
                    .GetEnumerableTypedResultAsync(commandBehavior: CommandBehavior.CloseConnection, true,
                        cancellationToken: cancellationToken);

                return results?.ToList() ?? new List<TObject>(0);
            }
            catch (Exception)
            {
                return new List<TObject>(0);
            }
        }

        /// <summary>
        /// Returns the first element of the query, or a default value if the query is empty.
        /// </summary>
        /// <typeparam name="TQuery">The type of the query.</typeparam>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="query">The object query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The first element of the query, or a default value if the query is empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the query is null.</exception>
        internal static async Task<TObject?> FirstOrDefaultAsync<TQuery, TObject>(this IObjectQuery<TQuery, TObject> query, CancellationToken cancellationToken = default)
            where TQuery : IObjectQuery<TQuery, TObject>
            where TObject : BaseInfo
        {
            ArgumentNullException.ThrowIfNull(query);

            cancellationToken.ThrowIfCancellationRequested();
            
            try
            {

                var results = await query
                    .GetTypedQuery()
                    .GetEnumerableTypedResultAsync(commandBehavior: CommandBehavior.Default, true, cancellationToken: cancellationToken);

                return results?.FirstOrDefault() ?? default;
            }
            catch (InvalidOperationException)
            {
                var results = await query
                    .GetTypedQuery()
                    .GetEnumerableTypedResultAsync(commandBehavior: CommandBehavior.CloseConnection, true,
                        cancellationToken: cancellationToken);

                return results?.FirstOrDefault() ?? default;
            }
            catch (Exception)
            {
                return default;
            }
        }

        /// <summary>
        /// Filters the object query to include only the objects where the specified column is not null or empty.
        /// </summary>
        /// <typeparam name="TQuery">The type of the object query.</typeparam>
        /// <typeparam name="TObject">The type of the objects in the query.</typeparam>
        /// <param name="query">The object query.</param>
        /// <param name="columnName">The name of the column to check for null or empty values.</param>
        /// <returns>The filtered object query.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the query is null.</exception>
        internal static IObjectQuery<TQuery, TObject> WhereNotNullOrEmpty<TQuery, TObject>(this IObjectQuery<TQuery, TObject> query,
            string columnName)
            where TQuery : IObjectQuery<TQuery, TObject>
            where TObject : BaseInfo
        {
            return query
                .Where(new WhereCondition()
                    .WhereNotNull(columnName)
                    .And()
                    .WhereNotEmpty(columnName));
        }
    }
}
