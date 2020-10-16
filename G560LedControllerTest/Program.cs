using System.Drawing;
using System.Threading;

namespace G560Led
{
    public class Program
    {
        static void Main(string[] args)
        {
            G560LedControllerLoader.InitDevice(2).Wait();
           
            G560LedControllerLoader.Devices[2].SetColor(Color.Blue, 0); //Front Left
            G560LedControllerLoader.Devices[2].SetColor(Color.Green, 1); //Front Right
            G560LedControllerLoader.Devices[2].SetColor(Color.Yellow, 2); // Rear Left
            G560LedControllerLoader.Devices[2].SetColor(Color.Violet, 3); // Rear Right
           

            for (int i = 0; i < 10000; i++)
            {
                G560LedControllerLoader.Devices[2].SetColor(Color.FromArgb(255,0, 0, ((i % 255))), 0); //Front Left
                Thread.Sleep(5);
            }


        }
    }
}
