using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Log4net.Http.Appender
{
    public class HttpBufferedAppender: HttpAppender
    {
        private readonly BlockingCollection<LogEntry> _messages;

        /// <summary>
        /// Specifies how many items to keep in memory
        /// </summary>
        /// <remarks>Default: 10240</remarks>
        public int MaxItemsInMemory { get; set; } = 10240;

        /// <summary>
        /// Specifies how many events to send in one request
        /// </summary>
        /// <remarks>Default: 10</remarks>
        public int BatchMaxSize { get; set; } = 10;

        /// <summary>
        /// Specifies how log to sleep before checking for new log entries
        /// </summary>
        /// <remarks>Default: 200 milliseconds</remarks>
        public TimeSpan BatchSleepTime { get; set; } = TimeSpan.FromMilliseconds(200);

        public HttpBufferedAppender()
        {
            Task.Factory.StartNew(Send, TaskCreationOptions.LongRunning);

            if (MaxItemsInMemory <= 0)
                MaxItemsInMemory = 10240;

            _messages = new BlockingCollection<LogEntry>(MaxItemsInMemory);
        }

        protected override Task EnqueueAsync(LogEntry entry)
        {
            _messages.Add(entry);
            return Task.CompletedTask;
        }

        private async Task Send()
        {
            var batch = new List<LogEntry>();
            while (true)
            {
                for (int i = 0; i < BatchMaxSize; ++i)
                {
                    if (_messages.TryTake(out var entry))
                    {
                        batch.Add(entry);
                    }
                    else
                    {
                        break;
                    }
                }

                await SendEvents(batch.ToArray()).ConfigureAwait(false);

                batch.Clear();

                if (_messages.Count == 0)
                {
                    await Task.Delay(BatchSleepTime).ConfigureAwait(false);
                }
            }
        }
    }
}
