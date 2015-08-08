using System;

namespace Veeam.IntroductoryAssignment.Common
{
    interface ILoggable
    {
        void Log(string message);
    }

    class ConsoleLoggable : ILoggable
    {
        public virtual void Log(string message)
        {
            Console.WriteLine("[{0}] {1}: {2}", DateTime.Now.ToString("HH:mm:ss"), this, message);
        }
    }
}
