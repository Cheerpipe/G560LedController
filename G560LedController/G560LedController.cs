using Device.Net;
using Hid.Net.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Threading;

namespace G560Led
{
    public class G560LedController : IDisposable
    {
        private byte[] usb_buf = new byte[20];
        private Dictionary<byte, Color> _currentColors = new Dictionary<byte, Color>();
        private Dictionary<byte, Color> _newColors = new Dictionary<byte, Color>();
        private WindowsHidDevice _device;
        public G560LedController(IDevice device)
        {
            Initialize(device);
        }

        private Timer _sendTimer;
        public void Initialize(IDevice device)
        {
            _device = (WindowsHidDevice)device;
            _device.InitializeAsync().Wait();

            //Initialize Base
            usb_buf[00] = 0x11;
            usb_buf[01] = 0xFF;
            usb_buf[02] = 0x04;

            //Initialize 3E
            usb_buf[03] = 0x3E;
            usb_buf[04] = 0x00;
            _device.WriteAsync(usb_buf).Wait();
            Thread.Sleep(20);
            usb_buf[04] = 0x01;
            _device.WriteAsync(usb_buf).Wait();
            Thread.Sleep(20);
            usb_buf[04] = 0x02;
            _device.WriteAsync(usb_buf).Wait();
            Thread.Sleep(20);
            usb_buf[04] = 0x03;
            _device.WriteAsync(usb_buf).Wait();
            Thread.Sleep(20);


            //Initialize CE
            usb_buf[03] = 0xCE;
            usb_buf[04] = 0x02;
            _device.WriteAsync(usb_buf).Wait();
            Thread.Sleep(20);
            usb_buf[04] = 0x01;
            _device.WriteAsync(usb_buf).Wait();
            Thread.Sleep(20);
            usb_buf[04] = 0x03;
            _device.WriteAsync(usb_buf).Wait();
            Thread.Sleep(20);
            usb_buf[04] = 0x00;
            _device.WriteAsync(usb_buf).Wait();
            Thread.Sleep(20);

            //Initialize Send Lights
            usb_buf[03] = 0x3E;
            usb_buf[05] = 0x01; //Mode
            usb_buf[09] = 0x02;


            _currentColors[0] = Color.Black;
            _currentColors[1] = Color.Black;
            _currentColors[2] = Color.Black;
            _currentColors[3] = Color.Black;

            _newColors[0] = Color.Black;
            _newColors[1] = Color.Black;
            _newColors[2] = Color.Black;
            _newColors[3] = Color.Black;
            _sendTimer = new Timer(new TimerCallback(SendColors), null, 55, 55);
        }

        public void SetColor(Color color, byte zone)
        {
            if (zone > 3)
                return;
            _newColors[zone] = color;
        }
        public void ForceApply()
        {
            SendColors(null);
        }

        private void SendColors(object state)
        {
            byte retryCount;
            const byte maxRetryCount = 2;
            bool changeOk;

            for (byte i = 0; i < 4; i++)
            {
                usb_buf[04] = i;
                usb_buf[06] = _newColors[i].R;
                usb_buf[07] = _newColors[i].G;
                usb_buf[08] = _newColors[i].B;
                changeOk = false;
                retryCount = 0;
                do
                {

                    try
                    {
                        if (_newColors[i] == _currentColors[i])
                        {
                            break;
                        }
                        _device.WriteAsync(usb_buf).Wait();
                        _currentColors[i] = _newColors[i];
                        changeOk = true;
                    }
                    catch
                    {
                        retryCount++;
                        if (retryCount >= maxRetryCount)
                            break;
                    }
                    finally
                    {
                        Thread.Sleep(1);
                    }

                } while (!changeOk);
            }
        }
        public void Dispose()
        {
            _device.Dispose();
        }
    }
}
