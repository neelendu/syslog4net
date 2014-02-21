﻿using System.IO;
using log4net.Core;
using log4net.Layout;
using syslog4net.Converters;
using System.Text;

namespace syslog4net
{
    /// <summary>
    /// Log4net layout class with default support for the Syslog message format as described in Syslog IETF 5424 standard: http://tools.ietf.org/html/rfc5424
    /// </summary>
    internal class SyslogLayout : LayoutSkeleton
    {
        private readonly PatternLayout _layout;

        // http://tools.ietf.org/html/rfc5424#section-6.1
        private const int SyslogMaxMessageLength = 2048;

        /// <summary>
        /// Instantiates a new instance of <see cref="SyslogLayout"/>
        /// </summary>
        public SyslogLayout()
        {
            StructuredDataPrefix = "UNKNOWN@11111";

            IgnoresException = false;  //TODO deal with this. sealed?

            this._layout = new PatternLayout("<%syslog-priority>1 %utcdate{yyyy-MM-ddTHH:mm:ss:FFZ} %syslog-hostname %appdomain %syslog-process-id %syslog-message-id %syslog-structured-data %message%newline");

            this._layout.AddConverter("syslog-priority", typeof(PriorityConverter));
            this._layout.AddConverter("syslog-hostname", typeof(HostnameConverter));
            this._layout.AddConverter("syslog-process-id", typeof(ProcessIdConverter));
            this._layout.AddConverter("syslog-message-id", typeof(MessageIdConverter));
            this._layout.AddConverter("syslog-structured-data", typeof(StructuredDataConverter));

            this.ActivateOptions();
        }

        /// <summary>
        /// Formats data within the event and writes the formatted data out to the provided writer instance
        /// </summary>
        /// <param name="writer">writer to output the formatted data to</param>
        /// <param name="logEvent">logging event data to use</param>
        override public void Format(TextWriter writer, LoggingEvent logEvent)
        {
            logEvent.Properties["log4net:StructuredDataPrefix"] = StructuredDataPrefix;

            using (var stringWriter = new StringWriter())
            {
                this._layout.Format(stringWriter, logEvent);

                // truncate the message to SYSLOG_MAX_MESSAGE_LENGTH or fewer bytes
                string message = stringWriter.ToString();

                var utf8 = Encoding.UTF8;

                byte[] utfBytes = utf8.GetBytes(message);
                if (utfBytes.Length > SyslogMaxMessageLength)
                {
                    message = utf8.GetString(utfBytes, 0, SyslogMaxMessageLength);
                }

                writer.Write(message);
            }
        }

        public string StructuredDataPrefix { get; set; }

        /// <summary>
        /// Activates the use of options for the converter allowing the underlying PatternLayout implmentation to behave correctly
        /// </summary>
        override public void ActivateOptions()
        {
            this._layout.ActivateOptions();
        }
    }
}