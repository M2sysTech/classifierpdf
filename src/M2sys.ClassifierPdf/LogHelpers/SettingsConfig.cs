namespace M2sys.ClassifierPdf.LogHelpers
{
    using System.Configuration;

    public class SettingsConfig
    {
        public static log4net.Core.Level LogLevel
        {
            get
            {
                var nivel = Obter("Log.Level", "error").ToLower();

                if (nivel.Equals("info"))
                {
                    return log4net.Core.Level.Info;
                }

                if (nivel.Equals("debug"))
                {
                    return log4net.Core.Level.Debug;
                }

                return log4net.Core.Level.Error;
            }
        }

        public static bool LogFile
        {
            get
            {
                return Obter("Log.File", "true") == "true";
            }
        }

        public static bool LogConsole
        {
            get
            {
                return Obter("Log.Console", "false") == "true";
            }
        }

        public static bool LogTrace
        {
            get
            {
                return Obter("Log.Trace", "false") == "true";
            }
        }

        public static string LogConsolePattern
        {
            get
            {
                return Obter("Log.Console.Pattern");
            }
        }

        public static string Obter(string chave, string padrao = "")
        {
            var valor = ConfigurationManager.AppSettings[chave];

            if (string.IsNullOrEmpty(valor))
            {
                return padrao;
            }

            return valor;
        }
    }
}