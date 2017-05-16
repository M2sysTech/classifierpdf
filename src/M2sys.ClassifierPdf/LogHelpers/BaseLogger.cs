namespace M2sys.ClassifierPdf.LogHelpers
{
    using log4net.Repository;
    using log4net.Repository.Hierarchy;

    public abstract class BaseLogger
    {
        protected readonly LogConfiguration configuration;
        protected readonly ILoggerRepository loggerRepository;

        protected BaseLogger(LogConfiguration configuration, ILoggerRepository loggerRepository)
        {
            this.configuration = configuration;
            this.loggerRepository = loggerRepository;
        }

        public abstract bool ShouldBeEnable
        {
            get;
        }

        public Logger Logger
        {
            get;
            protected set;
        }
    }
}