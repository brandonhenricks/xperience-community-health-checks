using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.HealthChecks
{
    /// <summary>
    /// Health Check Response Writer.
    /// </summary>
    /// <remarks>Provides a more detailed output than Health, Unhealthy, Degraded.</remarks>
    public static class HealthCheckResponseWriter
    {
        private static JsonWriterOptions s_Options = new JsonWriterOptions() { Indented = true };
        private const string JsonContentType = "application/json; charset=utf-8";

        /// <summary>
        /// Create Health Check Response.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static async Task WriteResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = JsonContentType;

            await using (var writer = new Utf8JsonWriter(context.Response.Body, s_Options))
            {
                writer.WriteStartObject();
                writer.WriteString("status", result.Status.ToString());
                writer.WriteStartObject("results");

                foreach (var entry in result.Entries)
                {
                    writer.WriteStartObject(entry.Key);
                    writer.WriteString("status", entry.Value.Status.ToString());
                    writer.WriteString("description", entry.Value.Description);
                    writer.WriteStartObject("data");

                    foreach (var item in entry.Value.Data)
                    {
                        writer.WritePropertyName(item.Key);
                        JsonSerializer.Serialize(writer, item.Value, item.Value?.GetType() ?? typeof(object));
                    }

                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }

                writer.WriteEndObject();
                writer.WriteEndObject();
                await writer.FlushAsync();
            }
        }
    }
}
