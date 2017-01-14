using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace MorseServer.Classes
{
    public class LED : Hardware
    {
        public LED(int RPiPin, GpioPinDriveMode Drivemode) : base(RPiPin, Drivemode)
        {

        }

        public void Aan()
        {
            myGpioPin.Write(GpioPinValue.Low);
        }
        public void Uit()
        {
            myGpioPin.Write(GpioPinValue.High);
        }
        public void Knipper(int aantalKeer, int tijdAan, int tijdUit, int tijdDelay)
        {
            for (int i = 0; i < aantalKeer; i++)
            {
                Aan();
                Task.Delay(tijdAan).Wait();
                Uit();
                Task.Delay(tijdUit).Wait();
            }
            Task.Delay(tijdDelay).Wait();
        }
    }
}
