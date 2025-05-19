using System;
using System.Threading;

namespace WindowLocker.Managers
{
    public class SingleInstanceManager
    {
        private static Mutex mutex = new Mutex(true, "WindowLockerSingleInstance");

        public static bool IsAlreadyRunning()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                return false;
            }
            return true;
        }

        public static void Initialize()
        {
            try
            {
                mutex = new Mutex(true, "WindowLockerSingleInstance");
            }
            catch (Exception)
            {
                // If mutex creation fails, try to acquire existing one
                mutex = Mutex.OpenExisting("WindowLockerSingleInstance");
                mutex.WaitOne();
            }
        }

        public static void Release()
        {
            try
            {
                if (mutex != null)
                {
                    mutex.ReleaseMutex();
                }
            }
            catch (Exception)
            {
                // Ignore any errors during release
            }
        }
    }
}