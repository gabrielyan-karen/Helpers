using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Data;
using System.IO.Ports;
using System.Management;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Web;
using System.Security.Cryptography;

namespace Helpers
{
    public class Helpers
    {
        public string[] keysArray = { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "A",
                                        "S", "D", "F", "G", "H", "J", "K", "L", "Z", "X", "C",
                                        "V", "B", "N", "M", "q", "w", "e", "r", "t", "y", "u",
                                        "i", "o", "p", "a", "s", "d", "f", "g", "h", "j", "k",
                                        "l", "z", "x", "c", "v", "b", "n", "m", "1", "2", "3",
                                        "4", "5", "6", "7", "8", "9", "0" };

        public string[] keys = { "{DOWN}", "{UP}", "{LEFT}", "{RIGHT}", "{END}", "{HOME}", "{PGDN}", "{PGUP}", "{ESC}", /*"{TAB}"*/ };

        private const UInt32 MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const UInt32 MOUSEEVENTF_LEFTUP = 0x0004;

        public void keyboardAction(string className, string windowName, string[] keys)
        {
            IntPtr calculatorHandle = User32.FindWindow(className, windowName);

            if (calculatorHandle == IntPtr.Zero)
            {
                MessageBox.Show(windowName + " is not running.", "Error");
                return;
            }

            User32.SetForegroundWindow(calculatorHandle);

            for (int i = 0; i < keys.Length; i++)
                SendKeys.SendWait(keys[i]);
        }

        public void mouseAction(int X, int Y)
        {
            int x = Convert.ToInt16(X);
            int y = Convert.ToInt16(Y);
            Cursor.Position = new Point(x, y);
            User32.mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            User32.mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        public int[] getScreenSize()
        {
            Size screenSize = SystemInformation.PrimaryMonitorSize;
            int[] size = { screenSize.Width, screenSize.Height };
            return size;
        }

        public void CaptureWindowToFile(IntPtr handle, string filename, ImageFormat format)
        {
            Image img = CaptureWindow(handle);
            img.Save(filename, format);
        }

        public void CaptureScreenToFile(string filename, ImageFormat format)
        {
            Image img = CaptureScreen();
            img.Save(filename, format);
        }

        public Image CaptureScreen()
        {
            return CaptureWindow(User32.GetDesktopWindow());
        }

        public Image CaptureWindow(IntPtr handle)
        {
            IntPtr hdcSrc = User32.GetWindowDC(handle);

            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);

            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;

            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);

            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            GDI32.SelectObject(hdcDest, hOld);
            GDI32.DeleteDC(hdcDest);

            User32.ReleaseDC(handle, hdcSrc);

            Image img = Image.FromHbitmap(hBitmap);

            GDI32.DeleteObject(hBitmap);

            return img;
        }

        public double[] readFile(string fileName, int size)
        {
            double[] Values = new double[size];

            try
            {
                FileStream fs = File.OpenRead(@fileName);
                BinaryReader br = new BinaryReader(fs);
                Values[0] = 0;
                for (int i = 0; i < Values.Length; i++)
                    Values[i] = br.ReadDouble();

                br.Close();
                fs.Close();
            }
            catch
            {
                MessageBox.Show("Can't read file.", "Error");
            }

            return Values;
        }

        public void writeFile(string fileName, double[] Values)
        {
            try
            {
                FileStream fs = File.OpenWrite(@fileName);
                BinaryWriter bw = new BinaryWriter(fs);
                for (int i = 0; i < Values.Length; i++)
                    bw.Write(Values[i]);
                bw.Close();
                fs.Close();
            }
            catch
            {
                MessageBox.Show("Can't write file.", "Error");
            }
        }

        public string[] hardId()
        {
            string pcID = "";
            ManagementClass mbClass = new ManagementClass("Win32_BaseBoard");
            mbClass.Options.UseAmendedQualifiers = true;
            PropertyDataCollection propertiesMB = mbClass.Properties;

            foreach (PropertyData propertyMB in propertiesMB)
            {
                if (propertyMB.Name.ToString() == "SerialNumber")
                {
                    foreach (ManagementObject c in mbClass.GetInstances())
                    {
                        pcID = c.Properties[propertyMB.Name.ToString()].Value.ToString() + "+ArkaSoft01";
                    }
                }
            }

            byte[] shaByte = GetSHA1HashDataAsArray(toByte(pcID, Encoding.UTF8));
            string shaString = Convert.ToBase64String(shaByte, 0, shaByte.Length, Base64FormattingOptions.InsertLineBreaks);
            string[] Return = { shaString, pcID };
            return Return;
        }

        public void RunCmd(string commandLine)
        {
            ProcessStartInfo PSI = new ProcessStartInfo("cmd.exe");
            PSI.RedirectStandardInput = true;
            PSI.RedirectStandardOutput = true;
            PSI.RedirectStandardError = true;
            PSI.UseShellExecute = false;
            Process p = Process.Start(PSI);
            System.IO.StreamWriter SW = p.StandardInput;
            System.IO.StreamReader SR = p.StandardOutput;
            SW.WriteLine(commandLine);
            SW.Close();
        }

        private byte[] GetSHA1HashDataAsArray(byte[] data)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            return sha.ComputeHash(data);
        }

        private byte[] toByte(String s, Encoding enc)
        {
            return enc.GetBytes(s);
        }

        private class GDI32
        {
            public const int SRCCOPY = 0x00CC0020;

            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
            [DllImport("user32.dll")]
            public static extern void mouse_event(uint dwFlags,
                uint dx, uint dy, uint dwData, uint dwExtraInf);
            [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
            public static extern IntPtr FindWindow(string lpClassName,
                string lpWindowName);

            [DllImport("USER32.DLL")]
            public static extern bool SetForegroundWindow(IntPtr hWnd);

        }
    }
}
