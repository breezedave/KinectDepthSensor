using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kinect
{
    public partial class Form1 : Form
    {
        public static KinectSensor sensor;
        public static DepthImageStream dis;
        public static DepthImagePixel[] dip;
        int MAX_DEPTH = 0;
        int MIN_DEPTH = 9999;
        int DEPTH_OF_VIEW = 5000;

        public Form1()
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    sensor = potentialSensor;
                    sensor.Start();
                    dis = sensor.DepthStream;
                    dip = new DepthImagePixel[dis.FramePixelDataLength];
                    dis.Enable();
                    break;
                }
            }
            InitializeComponent();
            Application.Idle += Application_Idle;

        }

        void Application_Idle(object sender, EventArgs e)
        {
            while(IsApplicationIdle())
            {
                try
                {
                    dis.OpenNextFrame(0).CopyDepthImagePixelDataTo(dip);
                    Bitmap bmp = new Bitmap(dis.FrameWidth, dis.FrameHeight);
                    int i = 0;
                    int lastDepthCol = 0;
                    foreach (DepthImagePixel di in dip)
                    {
                        if(di.Depth >0)
                        {
                            MAX_DEPTH = Math.Min(DEPTH_OF_VIEW, di.Depth > MAX_DEPTH ? di.Depth : MAX_DEPTH);
                            MIN_DEPTH = di.Depth < MIN_DEPTH ? di.Depth : MIN_DEPTH;

                            int depthCol = (int)Math.Floor(Math.Min(255, (double)(di.Depth - MIN_DEPTH) / (double)(MAX_DEPTH - MIN_DEPTH) * 255));
                            lastDepthCol = depthCol;

                            bmp.SetPixel(i % dis.FrameWidth, (int)Math.Floor((double)((double)i / (double)dis.FrameWidth)), Color.FromArgb(depthCol,255 - depthCol, 0));
                        } else
                        {
                            int depthCol = lastDepthCol;
                            //bmp.SetPixel(i % dis.FrameWidth, (int)Math.Floor((double)((double)i / (double)dis.FrameWidth)), Color.FromArgb(depthCol, 255 - depthCol, 0));
                        }
                        i++;   
                    }
                    pictureBox1.Image = bmp;
                    
                }
                catch (Exception) { }

            }
        }

        bool IsApplicationIdle()
        {
            NativeMessage result;
            return PeekMessage(out result, IntPtr.Zero, (uint)0, (uint)0, (uint)0) == 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage
        {
            public IntPtr Handle;
            public uint Message;
            public IntPtr WParameter;
            public IntPtr LParameter;
            public uint Time;
            public Point Location;
        }

        [DllImport("user32.dll")]
        public static extern int PeekMessage(out NativeMessage message, IntPtr window, uint filterMin, uint filterMax, uint remove);

    }
}
