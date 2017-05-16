namespace M2sys.ClassifierPdf.LogHelpers
{
    using log4net.Repository;
    using log4net.Repository.Hierarchy;

    internal class FrameworkLogger : BaseLogger
    {
        public FrameworkLogger(LogConfiguration configuration, ILoggerRepository loggerRepository)
            : base(configuration, loggerRepository)
        {
            this.Logger = (Logger) loggerRepository.GetLogger("Framework");
        }

        public override bool ShouldBeEnable
        {
            get
            {
                return false;
            }
        }
    }
}