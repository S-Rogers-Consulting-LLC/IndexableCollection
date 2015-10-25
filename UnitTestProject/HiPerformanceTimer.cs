using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;

namespace UnitTestProject {
    internal class HiPerformanceTimer {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        private long startTime;
        private long stopTime;
        private long freq;

        public HiPerformanceTimer() {
            startTime = 0;
            stopTime = 0;

            if (QueryPerformanceFrequency(out freq) == false)
                throw new Win32Exception();
        }

        public void Start() {
            Thread.Sleep(0);
            QueryPerformanceCounter(out startTime);
        }

        public void Stop() { QueryPerformanceCounter(out stopTime); }

        public Decimal ElapsedMilliseconds {
            get {
                var result = Decimal.Parse(Convert.ToDecimal(((Double)(stopTime - startTime) / (double)freq) * 1000).ToString("0.00000"));
                if (result < 0) result = 0;
                return result;
            }
        }
    }
}
