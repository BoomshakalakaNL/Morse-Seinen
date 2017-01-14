using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MorseServer.Classes;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using System.Diagnostics;
using Windows.Devices.Gpio;

namespace MorseServer
{

    public sealed partial class MainPage : Page
    {
        private SocketServer socket;

        private HD44780Controller lcdHD44780;
        private LED led;
        private Pushbutton btnInvoer;
        private Pushbutton btnBevestig;
        private Stopwatch stopwatch;
        private Stopwatch stopwatchTryDecode;
        private DispatcherTimer timer;


        private string[,] characters = new string[36, 2] { { "A", ".-" }, { "B", "-..." }, { "C", "-.-." }, { "D", "-.." }, { "E", "." },
                                                       { "F", "..-." }, { "G", "--." }, { "H", "...." }, { "I", ".." }, { "J", ".---" },
                                                       { "K", "-.-" }, { "L", ".-.." }, { "M", "--" }, { "N", "-." }, { "O", "---" },
                                                       { "P", ".--." }, { "Q", "--.-" }, { "R", ".-." }, { "S", "..." }, { "T", "-" },
                                                       { "U", "..-" }, { "V", "...-" }, { "W", ".--" }, { "X", "-..-" }, { "Y", "-.--" },
                                                       { "Z", "--.." }, { "0","-----"}, { "1", ".----" }, { "2", "..---"},{ "3", "...--"},
                                                       { "4", "....-"}, { "5", "....."}, { "6", "-...."}, { "7", "--..."}, { "8", "---.."},
                                                       { "9", "----."}};

        private int tijdPunt = 50; // in miliseconden

        private string code;
        private string bericht;
        private int knopDownTijd; // vastleggen tijd wanneer knop is ingedrukt
        private int knopDownLengte; //vastleggen tijd hoelang de knop is ingehouden
        private int knopUpTijd; //vastleggen tijd wanneer los gelaten


        public MainPage()
        {
            this.InitializeComponent();
            Init();
            RunServer();
        }



        public void RunServer()
        {
            socket = new SocketServer(2048);
            ThreadPool.RunAsync(x => {
                socket.OnError += socket_OnError;
                socket.OnDataRecived += Socket_OnDataRecived;
                socket.Star();
            });
        }

        private void Socket_OnDataRecived(string data)
        {
            lcdHD44780.ClearDisplay();
            lcdHD44780.Write(data);
        }

        private void Socket_SendMessage()
        {
            this.lcdHD44780.ClearDisplay();
            this.lcdHD44780.Write("Message sent:");
            this.lcdHD44780.SetCursorPosition(1, 8);
            this.lcdHD44780.Write(bericht);
            socket.Send(bericht);
            bericht = "";
        }

        private void socket_OnError(string message)
        {

        }

        private void Init()
        {
            this.lcdHD44780 = new HD44780Controller();
            this.lcdHD44780.Init(26, 19, 13, 6, 5, 22, 27, 17, 4, 20, 21);

            this.led = new LED(24, GpioPinDriveMode.Output);
            this.btnInvoer = new Pushbutton(23, GpioPinDriveMode.InputPullUp);
            this.btnBevestig = new Pushbutton(25, GpioPinDriveMode.InputPullUp);

            this.stopwatch = new Stopwatch();
            this.stopwatchTryDecode = new Stopwatch();

            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromMilliseconds(10);
            this.timer.Tick += timerTick;
            this.timer.Start();
        }

        private void timerTick(object sender, object e)
        {
            if (btnBevestig.Pressed())
            {
                Socket_SendMessage();
            }
            if (btnInvoer.Pressed())
            {
                stopwatch.Start();
                stopwatchTryDecode.Restart();

                led.Aan();
            }
            if (!btnInvoer.Pressed())
            {
                stopwatch.Stop();
                led.Uit();
                knopDownTijd = 0;
                knopUpTijd = unchecked((int)stopwatch.ElapsedMilliseconds);
                knopDownLengte = knopUpTijd - knopDownTijd;

                if (knopDownLengte >= tijdPunt * 10 && bericht != null && bericht != "")
                {
                    bericht = bericht.Remove(bericht.Length - 1);
                    stopwatch.Reset();
                    printBericht();
                }

                else if (knopDownLengte >= tijdPunt * 3)
                {
                    code += "-";
                    stopwatch.Reset();
                    printCode();
                }
                else if (knopDownLengte > 0)
                {
                    code += ".";
                    stopwatch.Reset();
                    printCode();
                }

                if (unchecked((int)stopwatchTryDecode.ElapsedMilliseconds) >= tijdPunt * 15)
                {
                    if (code != "")
                    {
                        ControleerCode();
                        code = "";
                        printBericht();
                        stopwatchTryDecode.Restart();
                    }

                    else if (unchecked((int)stopwatchTryDecode.ElapsedMilliseconds) >= tijdPunt * 50 && bericht != null && bericht != "")
                    {
                        char last = bericht[bericht.Length - 1];
                        if (last.ToString() != " ")
                        {
                            bericht += " ";
                            printBericht();
                            stopwatchTryDecode.Reset();
                        }

                    }
                }
            }
        }

        private void printBericht()
        {
            this.lcdHD44780.ClearDisplay();
            this.lcdHD44780.SetCursorPosition(0, 0);
            this.lcdHD44780.WriteLine(bericht);
            printCode();
            Debug.WriteLine("Bericht: " + bericht);
        }

        private void printCode()
        {
            this.lcdHD44780.SetCursorPosition(1, 8);
            this.lcdHD44780.Write("Code: " + this.code);
            Debug.WriteLine("Code: " + this.code);
        }

        private void ControleerCode()
        {
            int match = -1;
            for (int i = 0; i < this.characters.Length / 2; i++)
            {
                if (this.code == this.characters[i, 1])
                {
                    match = i;
                }
            }
            if (match != -1)
            {
                this.bericht += this.characters[match, 0];
            }
            else
            {
                printInvalidCode();
            }
        }

        private void printInvalidCode()
        {
            this.lcdHD44780.SetCursorPosition(1, 8);
            this.lcdHD44780.Write("Code invalid    ");
            Debug.WriteLine("Code invalid    ");
        }

    }


}
