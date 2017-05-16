namespace M2sys.ClassifierPdf.LogHelpers
{
    using log4net.Repository;
    using log4net.Repository.Hierarchy;

    internal class ApplicationLogger : BaseLogger
    {
        public ApplicationLogger(LogConfiguration configuration, ILoggerRepository loggerRepository)
            : base(configuration, loggerRepository)
        {
            this.Logger = (Logger) loggerRepository.GetLogger("Application");
        }

        public override bool ShouldBeEnable
        {
            get
            {
                return true;
            }
        }
    }
}