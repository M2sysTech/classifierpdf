namespace M2sys.ClassifierPdf.LogHelpers
{
    using System;
    using log4net;

    public static class LogExtensions
    {
        public static void ErrorFormat(this ILog log, Exception exception, string message, params object[] args)
        {
            log.Error(string.Format(message, args), exception);
        }
    }
}