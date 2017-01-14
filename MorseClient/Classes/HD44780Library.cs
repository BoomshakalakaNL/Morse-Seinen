using MorseClient.Classes;
using System;
using System.Collections;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace MorseClient.Classes
{
	public class HD44780Controller
	{
		private GpioPin rsPin;
		private GpioPin rwPin;
		private GpioPin enablePin;
		private GpioPin d0Pin;
		private GpioPin d1Pin;
		private GpioPin d2Pin;
		private GpioPin d3Pin;
		private GpioPin d4Pin;
		private GpioPin d5Pin;
		private GpioPin d6Pin;
		private GpioPin d7Pin;

		private bool isConfigured = false;

		private bool isDisplayOn = true;
		private bool isCursorOn = true;
		private bool isCursorBlinking = false;

		private bool is8BitMode = true;
		private bool isDoubleLines = true;
		private bool isFont5x8 = true;

		public HD44780Controller() { }

		/// <summary>
		/// The initialisation method, this needs to be called before the screen is ready for use.
		/// </summary>
		/// <param name="rs">The number of the GPIO pin assigned to the RS pin on the display</param>
		/// <param name="rw">The number of the GPIO pin assigned to the RW pin on the display</param>
		/// <param name="enable">The number of the GPIO pin assigned to the Enable pin on the display</param>
		/// <param name="d7">The number of the GPIO pin assigned to the D7 pin on the display</param>
		/// <param name="d6">The number of the GPIO pin assigned to the D6 pin on the display</param>
		/// <param name="d5">The number of the GPIO pin assigned to the D5 pin on the display</param>
		/// <param name="d4">The number of the GPIO pin assigned to the D4 pin on the display</param>
		/// <param name="d3">The number of the GPIO pin assigned to the D3 pin on the display</param>
		/// <param name="d2">The number of the GPIO pin assigned to the D2 pin on the display</param>
		/// <param name="d1">The number of the GPIO pin assigned to the D1 pin on the display</param>
		/// <param name="d0">The number of the GPIO pin assigned to the D0 pin on the display</param>
		/// <returns>A notification of when the task completes</returns>
		public void Init(int rs, int rw, int enable, int d7, int d6, int d5, int d4, int d3, int d2, int d1, int d0)
		{
			var gpio = GpioController.GetDefault();

			rsPin = gpio.OpenPin(rs);
			rsPin.SetDriveMode(GpioPinDriveMode.Output);
			rwPin = gpio.OpenPin(rw);
			rwPin.SetDriveMode(GpioPinDriveMode.Output);
			enablePin = gpio.OpenPin(enable);
			enablePin.SetDriveMode(GpioPinDriveMode.Output);
			enablePin.Write(GpioPinValue.Low);
			d0Pin = gpio.OpenPin(d0);
			d0Pin.SetDriveMode(GpioPinDriveMode.Output);
			d1Pin = gpio.OpenPin(d1);
			d1Pin.SetDriveMode(GpioPinDriveMode.Output);
			d2Pin = gpio.OpenPin(d2);
			d2Pin.SetDriveMode(GpioPinDriveMode.Output);
			d3Pin = gpio.OpenPin(d3);
			d3Pin.SetDriveMode(GpioPinDriveMode.Output);
			d4Pin = gpio.OpenPin(d4);
			d4Pin.SetDriveMode(GpioPinDriveMode.Output);
			d5Pin = gpio.OpenPin(d5);
			d5Pin.SetDriveMode(GpioPinDriveMode.Output);
			d6Pin = gpio.OpenPin(d6);
			d6Pin.SetDriveMode(GpioPinDriveMode.Output);
			d7Pin = gpio.OpenPin(d7);
			d7Pin.SetDriveMode(GpioPinDriveMode.Output);

			isConfigured = true;

            TurnDisplayOff();
            TurnDisplayOn();
            ClearDisplay();
            ConfigureTextDisplay();
            ConfigureDisplay();
            SendInstruction(InstructionDefinitions.ENTRY_MODE_SET(true, false));
            SetCursorPosition(0, 0);
            Task.Delay(5).Wait();
            TurnCursorOff();
            WriteLine("Shalom chiabatta");
        }

		/// <summary>
		/// Applies the configuration for the current text display
		/// </summary>
		/// <returns>A notification of when the task completes</returns>
		private void ConfigureTextDisplay()
		{
			if (isConfigured)
			{
				SendInstruction(InstructionDefinitions.FUNCTION_SET(is8BitMode, isDoubleLines, !isFont5x8));
			}
		}

		/// <summary>
		/// Applies the configuration for the display
		/// </summary>
		/// <returns>A notification of when the task completes</returns>
		private void ConfigureDisplay()
		{
			if (isConfigured)
			{
				SendInstruction(InstructionDefinitions.DISPLAY_ONOFF_CONTROL(isDisplayOn, isCursorOn, isCursorBlinking));

			}
		}

		/// <summary>
		/// Sends an instruction to the pins then applies it
		/// </summary>
		/// <param name="instructions">The BitArray containing the instructions to seend, should be 10 items long</param>
		/// <returns>A notification of when the task completes</returns>
		private void SendInstruction(BitArray instructions)
		{
			if (instructions.Length != 10 || !isConfigured)
			{
				return;
			}

			SetPin(rsPin, instructions[0]);
			SetPin(rwPin, instructions[1]);
			SetPin(d7Pin, instructions[2]);
			SetPin(d6Pin, instructions[3]);
			SetPin(d5Pin, instructions[4]);
			SetPin(d4Pin, instructions[5]);
			SetPin(d3Pin, instructions[6]);
			SetPin(d2Pin, instructions[7]);
			SetPin(d1Pin, instructions[8]);
			SetPin(d0Pin, instructions[9]);

			enablePin.Write(GpioPinValue.Low);
			Task.Delay(10);
			enablePin.Write(GpioPinValue.High);
			Task.Delay(10);
			enablePin.Write(GpioPinValue.Low);
			Task.Delay(10);
		}

		/// <summary>
		/// Set the value of a pin
		/// </summary>
		/// <param name="pin">The pin to configure</param>
		/// <param name="value">The value to set the pin to</param>
		private void SetPin(GpioPin pin, bool value)
		{
			pin.Write(value ? GpioPinValue.High : GpioPinValue.Low);
		}

		/// <summary>
		/// Sets the position of the cursor
		/// </summary>
		/// <param name="row">The row to move the cursor to</param>
		/// <param name="column">The column to move the cursor to</param>
		/// <returns>A notification of when the task completes</returns>
		public void SetCursorPosition(int row, int column)
		{
			if (!isDoubleLines)
			{
				row = 0;
			}

			if (row > 1)
			{
				row = 1;
			}

			if (column > 32)
			{
				column = 32;
			}

			BitArray pos = new BitArray(new int[] { ((32 * row) + column) });
			SendInstruction(InstructionDefinitions.SET_DDRAM_ADDRESS(pos[6], pos[5], pos[4], pos[3], pos[2], pos[1], pos[0]));
		}

		/// <summary>
		/// Writes a string of text to the display
		/// </summary>
		/// <param name="text">The text to display</param>
		/// <returns>A notification of when the task completes</returns>
		public void Write(string text)
		{
			foreach (var c in text)
			{
				BitArray character = new BitArray(new byte[] { Convert.ToByte(c) });
                SendInstruction(InstructionDefinitions.WRITE_DATA(character[7], character[6], character[5], character[4], character[3], character[2], character[1], character[0]));                
            }
		}

		/// <summary>
		/// Writes a string of text to the display then adds a new line
		/// </summary>
		/// <param name="text">The text to display</param>
		/// <returns>A notification of when the task completes</returns>
		public void WriteLine(string text)
		{
			Write(text);
			SetCursorPosition(1, 0);
		}

		/// <summary>
		/// Turns the display on
		/// </summary>
		/// <returns>A notification of when the task completes</returns>
		public void TurnDisplayOn()
		{
			isDisplayOn = true;
			ConfigureDisplay();
		}

		/// <summary>
		/// Turns the display off
		/// </summary>
		/// <returns>A notification of when the task completes</returns>
		public void TurnDisplayOff()
		{
			isDisplayOn = false;
			ConfigureDisplay();
		}

		/// <summary>
		/// Turns the cursor on
		/// </summary>
		/// <returns>A notification of when the task completes</returns>
		public void TurnCursorOn()
		{
			isCursorOn = true;
			ConfigureDisplay();
		}

		/// <summary>
		/// Turns the cursor on
		/// </summary>
		/// <returns>A notification of when the task completes</returns>
		public void TurnCursorOff()
		{
			isCursorOn = false;
			ConfigureDisplay();
		}

		/// <summary>
		/// Turns the cursor off
		/// </summary>
		/// <returns>A notification of when the task completes</returns>
		public void StartCursorBlinking()
		{
			isCursorBlinking = true;
			ConfigureDisplay();
		}

		/// <summary>
		/// Stops the cursor blinking
		/// </summary>
		/// <returns>A notification of when the task completes</returns>
		public void StopCursorBlinking()
		{
			isCursorBlinking = false;
			ConfigureDisplay();
		}

		/// <summary>
		/// Clears the display and returns the cursor to the origin (first row, first column)
		/// </summary>
		/// <returns>A notification of when the task completes</returns>
		public void ClearDisplay()
		{
			SendInstruction(InstructionDefinitions.CLEAR_DISPLAY());
            Task.Delay(5).Wait();
        }

		/// <summary>
		/// Switch the display to 8-bit input mode
		/// </summary>
		/// <returns>A notification of when the task completes</returns>
		public void SwitchTo8BitMode()
		{
			is8BitMode = true;
			ConfigureTextDisplay();
		}

		/// <summary>
		/// Switch the display to 4-bit input mode (NOT YET IMPLEMENTED)
		/// </summary>
		/// <returns>A notification of when the task completes</returns>
		public void SwitchTo4BitMode()
		{
			//TODO: Implement 4 bit mode
			//is8BitMode = false;
			//await ConfigureTextDisplay();
		}

		/// <summary>
		/// Configure the display for two lines of text
		/// </summary>
		/// <returns>A notification of when the task completes</returns>
		public void SwitchToDoubleLines()
		{
			isDoubleLines = true;
			ConfigureTextDisplay();
		}

		/// <summary>
		/// Configure the display for a single line of text
		/// </summary>
		/// <returns>A notification of when the task completes</returns>
		public void SwitchToSingleLines()
		{
			isDoubleLines = false;
            ConfigureTextDisplay();
		}

		/// <summary>
		/// Configure the display to use a 5x8 pixel font
		/// </summary>
		/// <returns>A notification of when the task completes</returns>
		public void SwitchTo5x8Font()
		{
			isFont5x8 = true;
			ConfigureTextDisplay();
		}

		/// <summary>
		/// Configure the display to use a 5x10 pixel font
		/// </summary>
		/// <returns>A notification of when the task completes</returns>
		public void SwitchTo5x10Font()
		{
			isFont5x8 = false;
			ConfigureTextDisplay();
		}
	}
}