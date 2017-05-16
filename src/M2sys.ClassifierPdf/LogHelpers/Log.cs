namespace M2sys.ClassifierPdf.LogHelpers
{
    using System.Collections.Generic;
    using System.IO;
    using log4net;
    using log4net.Repository.Hierarchy;
    using System;
    using System.Configuration;

    public class Log
    {
        private static ILog applicationLog;
        private static ILog frameworkLog;
        private static Logger root;
        private static FileSystemWatcher fileSystemWatcher;

        public static ILog Application
        {
            get
            {
                return applicationLog ?? (applicationLog = LogManager.GetLogger("Application"));
            }
        }

        public static LogConfiguration Configuracao
        {
            get;
            private set;
        }

        internal static ILog Framework
        {
            get
            {
                return frameworkLog ?? (frameworkLog = LogManager.GetLogger("Framework"));
            }
        }

        private static Logger Root
        {
            get
            {
                return root ?? (root = ((Hierarchy)LogManager.GetRepository()).Root);
            }
        }

        public static void Configurar(LogConfiguration configuracao = null)
        {
            if (configuracao == null)
            {
                configuracao = new LogConfiguration();
            }

            AssistirArquivoDeConfiguracao();
            Root.Repository.ResetConfiguration();
            configuracao = configuracao.MergeWithSettingsConfig();

            var appenders = GetAppenders(configuracao);
            var loggers = GetLoggers(configuracao);

            foreach (var appender in appenders)
            {
                if (appender.ShouldBeEnable)
                {
                    foreach (var logger in loggers)
                    {
                        if (logger.ShouldBeEnable)
                        {
                            logger.Logger.AddAppender(appender.Get());
                        }
                    }
                }
            }

            Configuracao = configuracao;
            Root.Repository.Configured = true;
        }

        private static IEnumerable<BaseLogger> GetLoggers(LogConfiguration configuracao)
        {
            yield return new ApplicationLogger(configuracao, Root.Repository);
            yield return new FrameworkLogger(configuracao, Root.Repository);
        }

        private static IEnumerable<FrameworkAppender> GetAppenders(LogConfiguration configuracao)
        {
            yield return new FileAppenderLog(configuracao);
            yield return new DebugAppenderLog(configuracao);
            yield return new ConsoleAppenderLog(configuracao);
        }

        private static void AssistirArquivoDeConfiguracao()
        {
            if (fileSystemWatcher != null)
            {
                return;
            }

            const NotifyFilters NotifyFilters = NotifyFilters.LastWrite;

            fileSystemWatcher = new FileSystemWatcher
            { 
                Path = AppDomain.CurrentDomain.BaseDirectory, 
                NotifyFilter = NotifyFilters, 
                Filter = "settings.config"
            };

            fileSystemWatcher.Changed += (sender, args) =>
            { 
                ConfigurationManager.RefreshSection("appSettings");
                Configurar();
            };

            fileSystemWatcher.EnableRaisingEvents = true;
        }
    }
}