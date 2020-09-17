using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace ThreeTeneHelper
{
    public partial class frmMain : Form
    {
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hwnd);

        private IntPtr activeWindow = IntPtr.Zero;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            bool focused3tene = false;
            if (GetActiveWindowTitle() != null && GetActiveWindowTitle().Contains("3tene"))
            {
                focused3tene = true;
            }

            if (!focused3tene)
            {
                short keyState = GetAsyncKeyState((int)Keys.F2);
                bool unprocessedPress = ((keyState >> 0) & 0x0001) == 0x0001;

                if (unprocessedPress)
                {
                    activeWindow = GetForegroundWindow();
                    BringMainWindowToFront("3tene");
                }
            }
            else
            {
                foreach (Key key in Enum.GetValues(typeof(Key)))
                {
                    if (key == Key.None || key == Key.Cancel)
                    {
                        continue;
                    }

                    short keyState2 = GetAsyncKeyState((int)key);

                    bool unprocessedPress2 = ((keyState2 >> 0) & 0x0001) == 0x0001;

                    if (unprocessedPress2 && focused3tene)
                    {
                        tmrDelayedRefocus.Enabled = true;
                    }
                }
            }
        }


        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
            Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
            Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
            Restore = 9, ShowDefault = 10, ForceMinimized = 11
        };

        private void BringMainWindowToFront(string processName)
        {
            // get the process
            Process bProcess = Process.GetProcessesByName(processName).FirstOrDefault();

            // check if the process is running
            if (bProcess != null)
            {
                // check if the window is hidden / minimized
                if (bProcess.MainWindowHandle == IntPtr.Zero)
                {
                    // the window is hidden so try to restore it before setting focus.
                    ShowWindow(bProcess.Handle, ShowWindowEnum.Restore);
                }

                // set user the focus to the window
                SetForegroundWindow(bProcess.MainWindowHandle);
            }
            else
            {
                // the process is not running, so start it
                Process.Start(processName);
            }
        }

        private void tmrDelayedRefocus_Tick(object sender, EventArgs e)
        {
            if (activeWindow != IntPtr.Zero)
            {
                try
                {
                    SetForegroundWindow(activeWindow);
                }
                catch (Exception ex) { }
            }

            tmrDelayedRefocus.Enabled = false;
        }
    }
}
