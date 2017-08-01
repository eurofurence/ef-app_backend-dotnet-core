using System;
using System.Diagnostics;

namespace Eurofurence.App.Common.Utility
{
    public class TimeTrap : IDisposable
    {
        private readonly Action<TimeSpan> _evaluate;
        private readonly Stopwatch _stopwatch;

        public TimeTrap(Action<TimeSpan> evaluate)
        {
            _evaluate = evaluate;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _evaluate.Invoke(_stopwatch.Elapsed);
        }
    }
}
