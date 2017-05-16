namespace M2sys.ClassifierPdf.LogHelpers
{
    using log4net.Appender;
    using log4net.Layout;

    public class ConsoleAppenderLog : FrameworkAppender
    {
        public ConsoleAppenderLog(LogConfiguration configuration) : base(configuration)
        {
            var consoleAppender = new ConsoleAppender
            {
                Name = "ConsoleAppender",
                Layout = new PatternLayout(this.configuration.ConsolePattern),
                Threshold = this.configuration.Level,
            };

            consoleAppender.ActivateOptions();

            this.appender = new SqlAppender();
            this.appender.AddAppender(consoleAppender);
        }

        public override bool ShouldBeEnable
        {
            get
            {
                return this.configuration.ConsoleAtivo;
            }
        }
    }
}
