namespace M2sys.ClassifierPdf.LogHelpers
{
    using System;
    using log4net.Core;

    public class LogConfiguration
    {
        public LogConfiguration()
        {
            this.ConsoleAtivo = false;
            this.ArquivoAtivo = false;
            this.Level = Level.Error;
            this.ConsolePattern = "%message %newline";
            ////this.ConsolePattern = "[%5.5thread] %message %newline";
            this.NomeDoArquivo = AppDomain.CurrentDomain.FriendlyName;
        }

        public bool ConsoleAtivo
        {
            get;
            set;
        }

        public bool TraceAtivo
        {
            get;
            set;
        }

        public string ConsolePattern
        {
            get;
            set;
        }

        public bool ArquivoAtivo
        {
            get;
            set;
        }
        
        public Level Level
        {
            get;
            set;
        }

        public string NomeDoArquivo
        {
            get;
            set;
        }

        public void Configurar()
        {
            this.ConsoleAtivo = SettingsConfig.LogConsole;
            this.ArquivoAtivo = SettingsConfig.LogFile;
            this.Level = SettingsConfig.LogLevel;
            this.ConsolePattern = SettingsConfig.LogConsolePattern;
            this.NomeDoArquivo = AppDomain.CurrentDomain.FriendlyName;
        }

        public LogConfiguration MergeWithSettingsConfig()
        {
            this.ConsoleAtivo = this.ConsoleAtivo || SettingsConfig.LogConsole;
            this.TraceAtivo = this.TraceAtivo || SettingsConfig.LogTrace;
            this.ArquivoAtivo = this.ArquivoAtivo || SettingsConfig.LogFile;
            this.Level = SettingsConfig.LogLevel <= this.Level ? SettingsConfig.LogLevel : this.Level;
            this.ConsolePattern = this.ConsolePattern;

            return this;
        }
    }
}