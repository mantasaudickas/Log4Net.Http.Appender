using System;
using System.Collections.Generic;

namespace Log4net.Http.Appender
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string RenderedMessage { get; set; }
        public string MachineName { get; set; }
        public string SourceContext { get; set; }
        public IDictionary<string, object> Properties { get; set; }
        public string Exception { get; set; }
    }
}
