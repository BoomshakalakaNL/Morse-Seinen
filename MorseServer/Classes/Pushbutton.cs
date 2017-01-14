using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace MorseServer.Classes
{
    class Pushbutton : Hardware
    {
        public Pushbutton(int RPiPin, GpioPinDriveMode Drivemode) : base(RPiPin, Drivemode)
        {

        }

        public bool Pressed()
        {
            return myGpioPin.Read() == GpioPinValue.Low;
        }
    }
}
