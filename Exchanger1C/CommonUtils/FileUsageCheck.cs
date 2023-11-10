using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Exchanger1C.CommonUtils
{
    internal class FileUsageCheck
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr _lopen(string lpPathName, int iReadWrite);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private const int OF_READWRITE = 2;
        private const int OF_SHARE_DENY_NONE = 0x40;        

        public enum State { NONE, BUSY, FREE}

        public static State CheckState(string vFileName) {

            if (!File.Exists(vFileName)) return State.NONE;

            IntPtr HFILE_ERROR = new IntPtr(-1);
            IntPtr vHandle = _lopen(vFileName, OF_READWRITE | OF_SHARE_DENY_NONE);
            if (vHandle == HFILE_ERROR) return State.BUSY;

            CloseHandle(vHandle);
            return State.FREE;
        }

        public static async Task UntilBusyAsync(string vFileName) {
            if (CheckState(vFileName) != State.NONE)
            {
                do
                {
                    Debug.WriteLine("WaitUntilBusy");
                    await Task.Delay(100);
                } while (CheckState(vFileName) == State.FREE);
            }
        }

        public static void DeleteWhenFree(string vFileName) {
            Debug.WriteLine("DeleteWhenFree - start");
            while(true)
            {
                State state = CheckState(vFileName);
                switch (state) {
                    case State.NONE:
                        Debug.WriteLine($"DeleteWhenFree None - exit");
                        return;
                    case State.BUSY:
                        Debug.WriteLine("DeleteWhenFree BUSY -- sleeping 1 sec");
                        Thread.Sleep(1000);
                        break;
                    case State.FREE:
                        Debug.WriteLine($"DeleteWhenFree FREE -- deleting file {vFileName}");
                        File.Delete(vFileName);
                        break;
                }                
            }
        }

    }
}