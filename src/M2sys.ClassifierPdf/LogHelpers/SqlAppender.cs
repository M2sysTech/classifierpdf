namespace M2sys.ClassifierPdf.LogHelpers
{
    using System.Text;
    using System.Text.RegularExpressions;
    using log4net.Appender;
    using log4net.Core;

    public class SqlAppender : ForwardingAppender
    {
        public static string ParameterChar = "@p";

        protected override void Append(LoggingEvent loggingEvent)
        {
            var loggingEventData = loggingEvent.GetLoggingEventData();

            if (loggingEventData.Message.Contains(ParameterChar))
            {
                var messageBuilder = new StringBuilder();

                var message = loggingEventData.Message;
                var queries = Regex.Split(message, @"command\s\d+:");

                foreach (var query in queries)
                {
                    messageBuilder.Append(ReplaceQueryParametersWithValues(query));
                }

                loggingEventData.Message = messageBuilder.ToString();
            }

            base.Append(new LoggingEvent(loggingEventData));
        }

        private static string ReplaceQueryParametersWithValues(string query)
        {
            var regex = string.Format(@"{0}\d(?=[,);\s])(?!\s*=)", ParameterChar);

            return Regex.Replace(query, regex, match =>
            {
                var parameterValueRegex = new Regex(string.Format(@".*{0}\s*=\s*(.*?)\s*[\[].*", match));
                return parameterValueRegex.Match(query).Groups[1].ToString();
            });
        }
    }
}
