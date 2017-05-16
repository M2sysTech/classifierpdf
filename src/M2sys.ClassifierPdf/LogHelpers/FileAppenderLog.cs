namespace M2sys.ClassifierPdf.LogHelpers
{
    using System.IO;
    using log4net.Appender;
    using log4net.Layout;

    public class FileAppenderLog : FrameworkAppender
    {
        public FileAppenderLog(LogConfiguration configuration)
            : base(configuration)
        {
            var fileAppender = new RollingFileAppender
            {
                Name = "FileAppender",
                AppendToFile = true,
                File = Path.Combine("Logs", this.configuration.NomeDoArquivo + ".log"),
                Layout = new PatternLayout("%date [%5.5thread] [%-5.5level] > %message %newline"),
                Threshold = this.configuration.Level,
                RollingStyle = RollingFileAppender.RollingMode.Size,
                StaticLogFileName = true,
                MaxSizeRollBackups = 10,
                MaximumFileSize = "30MB"
            };

            fileAppender.ActivateOptions();

            this.appender = new SqlAppender();
            this.appender.AddAppender(fileAppender);
        }

        public override bool ShouldBeEnable
        {
            get
            {
                return this.configuration.ArquivoAtivo;
            }
        }
    }
}
