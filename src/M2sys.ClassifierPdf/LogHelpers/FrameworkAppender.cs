namespace M2sys.ClassifierPdf.LogHelpers
{
    using log4net.Appender;

    public abstract class FrameworkAppender
    {
        protected readonly LogConfiguration configuration;
        protected SqlAppender appender;

        protected FrameworkAppender(LogConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public abstract bool ShouldBeEnable
        {
            get;
        }

        public IAppender Get()
        {
            return this.appender;
        }
    }
}