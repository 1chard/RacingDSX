using System;
using static RacingDSX.RacingDSXWorker;

namespace RacingDSX
{
    public class CLI
    {
        private Core Core;
        private System.Threading.ManualResetEvent exitEvent;

        public CLI(Core core)
        {
            this.Core = core;

            Initialize();
            Await();
        }

        private void Await()
        {
            using (exitEvent = new System.Threading.ManualResetEvent(false))
            {
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    Core.close();
                    exitEvent.Set();
                };

                exitEvent.WaitOne();
            }
        }

        private void Dispose()
        {
            exitEvent.Set();
        }

        private void Initialize()
        {
            Core.Initialize(WorkerThreadReporter, AppCheckReporter, Dispose);
        }

        protected void WorkerThreadReporter(RacingDSXReportStruct value)
        {
        }

        protected void AppCheckReporter(AppCheckReportStruct value)
        {
        }

    }
}
