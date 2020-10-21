using Device.Net;
using Hid.Net.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Threading;

namespace G560Led
{
    public class G560LedController : IDisposable
    {
        byte[] usb_buf = new byte[20];
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
            usb_buf[0] = 0x11;
            usb_buf[01] = 0xFF;
            usb_buf[02] = 0x04;
            usb_buf[03] = 0x3C;
            //usb_buf[0x04] = 0x00; //Canal
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
            _sendTimer = new Timer(new TimerCallback(SendColors), null, 50, 50);
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
            for (byte i = 0; i < 4; i++)
            {
                if (_newColors[i] == _currentColors[i])
                {
                    return;
                }

                usb_buf[04] = i;
                usb_buf[06] = _newColors[i].R;
                usb_buf[07] = _newColors[i].G;
                usb_buf[08] = _newColors[i].B;
                bool pending = true;
                while (pending)
                {
                    try
                    {
                        _device.WriteAsync(usb_buf).Wait(40);
                        _currentColors[i] = _newColors[i];
                        pending = false;
                    }
                    catch { }
                    Thread.Sleep(1);
                }
            }
        }
        public void Dispose()
        {
            _device.Dispose();
        }
    }
}
