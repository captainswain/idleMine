using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Timers;
using System.Diagnostics;

namespace idleMine
{
    class Program
    {
        //--------- Configuration ------------------------

        static string processName = "marlin";
        // full path start command or path to batch file.
        static string startCommand = @"C:\Users\shane\Desktop\marlin\marlin.exe -H us-west.siamining.com:3333 -u 4142ee6512c3b77702ada0e038a529cbb034ba5da69cb58fe31638daaf03d668499323da6634.shanedesktop -I 24";
        // Amount of idle minutes before we start mining.
        static uint minutesIdle = 15;


        static void Main(string[] args)
        {
            Console.WriteLine("IdleMiner v0.0.1");
            Console.WriteLine("Current Status: Not idle.");

            Timer timer = new Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Start();
            //Console.Write("Press any key to exit... \n");
            Console.ReadKey();
        }


        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            //Console.WriteLine("Current Idle time: " + IdleTime() + " Seconds.");
            if (IdleTime() >= minutesIdle && !Running("marlin"))
            {
                Console.WriteLine("Current Status: idle.");
                Process process = new Process();

                ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", @"/c " + startCommand);

                process.StartInfo = processInfo;

                process.Start();

                Console.WriteLine("Started mining.");
            }else if (IdleTime() < minutesIdle && Running("marlin"))
            {
                Console.WriteLine("Current Status: not idle.");
                try
                {
                    foreach (Process proc in Process.GetProcessesByName("marlin"))
                    {
                        proc.Kill();
                        Console.WriteLine("Killed miner.");
                    }
                    }
                    catch (Exception ex)
                    {
                    Console.WriteLine(ex.Message);
                    }
            }

        }
        public static int IdleTime() //In minutes
        {
            TimeSpan timespentIdle = TimeSpan.FromMilliseconds(IdleTimeFinder.GetIdleTime());
            return timespentIdle.Minutes;
        }
        public static void print(string output)
        {
                Console.Write("\r{0}", output);

        }

        public static bool Running(string process)
        {
            Process[] processname = Process.GetProcessesByName(process);
            if (processname.Length == 0)
                return false;

            return true; 
        }

    }

    // credits https://goo.gl/VYDfZz
    internal struct LASTINPUTINFO
    {
        public uint cbSize;

        public uint dwTime;
    }

    /// <summary>
    /// Helps to find the idle time, (in milliseconds) spent since the last user input
    /// </summary>
    public class IdleTimeFinder
    {
        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("Kernel32.dll")]
        private static extern uint GetLastError();

        public static uint GetIdleTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);

            return ((uint)Environment.TickCount - lastInPut.dwTime);
        }
        /// <summary>
        /// Get the Last input time in milliseconds
        /// </summary>
        /// <returns></returns>
        public static long GetLastInputTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            if (!GetLastInputInfo(ref lastInPut))
            {
                throw new Exception(GetLastError().ToString());
            }
            return lastInPut.dwTime;
        }
    }
}
