using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using log4net.Appender;
using log4net.Core;
using Newtonsoft.Json;

namespace Log4net.Http.Appender
{
    public class HttpAppender: AppenderSkeleton
    {
        protected static readonly string MachineName = Dns.GetHostName();

        protected static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        protected static readonly HttpClient Client = new HttpClient();

        /// <summary>
        /// Specifies HTTP endpoint to which send a POST request with collected events
        /// </summary>
        /// <remarks>Default: http://localhost:30001</remarks>
        public string HttpEndpoint { get; set; } = "http://localhost:30001";

        /// <summary>
        /// When HTTP endpoint is not available - how many retries to do before throwing events away
        /// </summary>
        /// <remarks>Default: 100</remarks>
        public int ErrorMaxRetries { get; set; } = 100;

        /// <summary>
        /// Specifies how long to sleep between retries
        /// </summary>
        /// <remarks>Default: 100 milliseconds</remarks>
        public TimeSpan ErrorSleepTime { get; set; } = TimeSpan.FromMilliseconds(100);

        protected override void Append(LoggingEvent loggingEvent)
        {
            IDictionary loggingProperties = loggingEvent.GetProperties();

            Dictionary<string, object> properties = null;
            string machineName = null;
            if (loggingProperties != null && loggingProperties.Count > 0)
            {
                properties = new Dictionary<string, object>();
                foreach (var property in loggingProperties.Keys)
                {
                    var key = property.ToString();
                    if (key == "log4net:HostName")
                    {
                        machineName = loggingProperties[property].ToString();
                        continue;
                    }

                    properties[key] = loggingProperties[property];
                }
            }

            if (string.IsNullOrWhiteSpace(machineName))
                machineName = MachineName;

            var exception = loggingEvent.ExceptionObject?.ToString();

            var entry = new LogEntry
            {
                Timestamp = loggingEvent.TimeStamp.ToUniversalTime(),
                RenderedMessage = loggingEvent.RenderedMessage,
                Level = loggingEvent.Level.Name,
                MachineName = machineName,
                SourceContext = loggingEvent.LoggerName,
                Properties = properties,
                Exception = exception
            };

            if (string.IsNullOrWhiteSpace(entry.RenderedMessage) && loggingEvent.ExceptionObject != null)
                entry.RenderedMessage = loggingEvent.ExceptionObject.Message;

            Task.Run(() => EnqueueAsync(entry)).GetAwaiter().GetResult();
        }

        protected virtual Task EnqueueAsync(LogEntry entry)
        {
            return SendEvents(entry);
        }

        protected async Task<bool> SendEvents(params LogEntry [] entries)
        {
            if (entries == null || entries.Length == 0)
                return true;

            var entriesAreSent = false;
            var reties = 0;
            while (true)
            {
                var content = FormatContent(entries);

                HttpResponseMessage response = null;
                try
                {
                    response = await Client
                        .PostAsync(HttpEndpoint, new StringContent(content, Encoding.UTF8, "application/json"))
                        .ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        entriesAreSent = true;
                        break;
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.ToString());
                }

                if (response == null || !response.IsSuccessStatusCode)
                {
                    reties += 1;
                    if (reties == ErrorMaxRetries)
                    {
                        if (response?.Content != null)
                        {
                            System.Diagnostics.Trace.WriteLine(response.Content.ToString());
                        }
                        break;
                    }

                    if (ErrorSleepTime > TimeSpan.Zero)
                    {
                        await Task.Delay(ErrorSleepTime).ConfigureAwait(false);
                    }
                }
            }

            return entriesAreSent;
        }

        private static string FormatContent(LogEntry[] entries)
        {
            var content = "{\"events\":" + JsonConvert.SerializeObject(entries, Formatting.None, SerializerSettings) + "}";
            return content;
        }
    }
}
