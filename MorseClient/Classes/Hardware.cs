using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace MorseClient.Classes
{
    public abstract class Hardware
    {
        private GpioPin GpioPin;
        private GpioController gpio = GpioController.GetDefault();

        public Hardware(int RPiPin, GpioPinDriveMode Drivemode)
        {
            this.GpioPin = gpio.OpenPin(RPiPin);
            this.GpioPin.SetDriveMode(Drivemode);
            this.GpioPin.Write(GpioPinValue.High);
        }

        public GpioPin myGpioPin
        {
            get { return this.GpioPin; }
        }


    }
}
