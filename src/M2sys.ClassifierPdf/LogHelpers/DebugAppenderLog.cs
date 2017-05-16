namespace M2sys.ClassifierPdf.LogHelpers
{
    using System.ComponentModel;
    using System.Diagnostics;
    using log4net.Appender;
    using log4net.Layout;

    public class DebugAppenderLog : FrameworkAppender
    {
        public DebugAppenderLog(LogConfiguration configuration) : base(configuration)
        {
            var debugAppender = new DebugAppender
            {
                Name = "TraceAppender",
                Layout = new PatternLayout(this.configuration.ConsolePattern),
                Threshold = this.configuration.Level
            };

            debugAppender.ActivateOptions();

            this.appender = new SqlAppender();
            this.appender.AddAppender(debugAppender);
        }

        public override bool ShouldBeEnable
        {
            get
            {
                return this.IsInDesignMode();
            }
        }

        /// <summary>
        /// TODO: mover para outra classe
        /// </summary>
        public bool IsInDesignMode()
        {
            bool designMode = 
                LicenseManager.UsageMode == LicenseUsageMode.Designtime || Debugger.IsAttached;

            if (designMode == false)
            {
                using (var process = Process.GetCurrentProcess())
                {
                    return process.ProcessName.ToLowerInvariant().Contains("devenv");
                }
            }

            return true;
        }
    }
}
