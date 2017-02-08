using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.IO;
//using Windows.Gaming.Input;


namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Form form1 = new Form();

        // Declare the serial com port
        SerialPort ComPort = new SerialPort();

        internal delegate void SerialDataReceivedEventHandlerDelegate(
            object sender, SerialDataReceivedEventArgs e);

        delegate void SetTextCallback(string text);

        //
        // Maximize the window.
        //WindowState windowState = WindowState.Maximized;

        //
        // My stuff.

        //
        // Setup a system clock to count the time between data packets.
        DateTime millisecond = DateTime.Now;
        Int32 deltaT = 0;
        Int32 previousMillisecond;
        Int32 dataStreamSize = 0;

        //
        // Open a log file to write the data to.
       // using (System.IO.StreamWriter sw = File.AppendText(" c:\\test.txt"));
        //string lines = "First Line.\nSecond Line.\nThird Line.\n";

        //
        // Define the struct for input data from the platform.
        public struct dataPacket
        {
            public float UTC;
            public float lat, lon, alt;
            public float accelX, accelY, accelZ;
            public float velX, velY, velZ;
            public float posX, posY, posZ;
            public char movement;
            public float roll, pitch, yaw;
            public bool gndmtr1, gndmtr2;
            public bool amtr1, amtr2, amtr3, amtr4;
            public bool uS1, uS2, uS3, uS4, uS5, uS6;
            public bool payBay;

            public void setValues(byte[] inputData)
            {
                //
                // set all of the struct parameters on the GUI.
                this.UTC = BitConverter.ToSingle(inputData, 0);
                this.lat = BitConverter.ToSingle(inputData, 4);
                this.lon = BitConverter.ToSingle(inputData, 8);
                this.alt = BitConverter.ToSingle(inputData, 12);
                this.accelX = BitConverter.ToSingle(inputData, 16);
                this.accelY = BitConverter.ToSingle(inputData, 20);
                this.accelZ = BitConverter.ToSingle(inputData, 24);
                this.velX = BitConverter.ToSingle(inputData, 28);
                this.velY = BitConverter.ToSingle(inputData, 32);
                this.velZ = BitConverter.ToSingle(inputData, 36);
                this.posX = BitConverter.ToSingle(inputData, 40);
                this.posY = BitConverter.ToSingle(inputData, 44);
                this.posZ = BitConverter.ToSingle(inputData, 48);
                this.roll = BitConverter.ToSingle(inputData, 52);
                this.pitch = BitConverter.ToSingle(inputData, 56);
                this.yaw = BitConverter.ToSingle(inputData, 60);
                this.movement = BitConverter.ToChar(inputData, 64);
                this.gndmtr1 = BitConverter.ToBoolean(inputData, 65);
                this.gndmtr2 = BitConverter.ToBoolean(inputData, 66);
                this.amtr1 = BitConverter.ToBoolean(inputData, 67);
                this.amtr2 = BitConverter.ToBoolean(inputData, 68);
                this.amtr3 = BitConverter.ToBoolean(inputData, 69);
                this.amtr4 = BitConverter.ToBoolean(inputData, 70);
                this.uS1 = BitConverter.ToBoolean(inputData, 71);
                this.uS2 = BitConverter.ToBoolean(inputData, 72);
                this.uS3 = BitConverter.ToBoolean(inputData, 73);
                this.uS4 = BitConverter.ToBoolean(inputData, 74);
                this.uS5 = BitConverter.ToBoolean(inputData, 75);
                this.uS6 = BitConverter.ToBoolean(inputData, 76);
                this.payBay = BitConverter.ToBoolean(inputData, 77);
            }
        }

        dataPacket inputData = new dataPacket();

        //
        // True will be autonomous mode. False will be manual mode.
        // Initialize this to manual mode.
        bool controlState = false;

        public MainWindow()
        {
            InitializeComponent();

            InitializeComPort();

            ComPort.DataReceived +=
                new System.IO.Ports.SerialDataReceivedEventHandler(port_DataReceived_1);

            // add handler to call closed function upon program exit
            this.Closed += new EventHandler(MainWindow_Closed);
        }

        private void InitializeComPort()
        {
            string[] ArrayComPortsNames = null;
            int index = -1;
            string ComPortName = null;

            ArrayComPortsNames = SerialPort.GetPortNames();
            do
            {
                index += 1;
                cbPorts.Items.Add(ArrayComPortsNames[index]);

            } while (!((ArrayComPortsNames[index] == ComPortName) ||
                    (index == ArrayComPortsNames.GetUpperBound(0))));

            // sort the COM ports
            Array.Sort(ArrayComPortsNames);

            if (index == ArrayComPortsNames.GetUpperBound(0))
            {
                ComPortName = ArrayComPortsNames[0];
            }
            cbPorts.Text = ArrayComPortsNames[0];

            //
            // Initialize the baud rate combo box.
            cbBaudRate.Items.Add(57600);
            cbBaudRate.Items.Add(115200);
            cbBaudRate.Items.ToString();
            // get first item print in text
            cbBaudRate.Text = cbBaudRate.Items[0].ToString();
        }

        private void port_DataReceived_1(object sender, SerialDataReceivedEventArgs e)
        {
            int size;
            byte[] rawData;
            rawData = new byte[100];
            Int32 currentMillisecond;

            //
            // Packet received! Get the current time.
            millisecond = DateTime.Now;
            currentMillisecond = millisecond.Millisecond;
            deltaT = currentMillisecond - previousMillisecond;
            if (deltaT < 0)
                deltaT = deltaT + 1000;

            previousMillisecond = currentMillisecond;

            //
            // Get the size of the incoming buffer.
            size = ComPort.BytesToRead;
            dataStreamSize = size;

            //
            // Make sure we have a full packet, before updating.
            if (size > 77)
            {
                if (size > 100)
                {
                    ComPort.DiscardInBuffer();
                }
                else
                {
                    //
                    // Read the data from the incoming buffer.
                    ComPort.Read(rawData, 0, size);

                    if (size >= 45)
                    {
                        inputData.setValues(rawData);
                    }
                    this.Dispatcher.Invoke(() =>
                        SetText());
                }
            }
        }

        private void SetText()
        {
            //
            // Write to the log file.
           // file.WriteLine(lines);

            //
            // Set the update rate.
            this.updateRate.Text = deltaT.ToString();
            this.dataStream.Text = dataStreamSize.ToString();

            //
            // Update the GUI.
            this.UTC.Text = inputData.UTC.ToString();
            this.GPSLatitude.Text = inputData.lat.ToString();
            this.GPSLongitude.Text = inputData.lon.ToString();
            this.AltitudeASL.Text = inputData.alt.ToString();
            this.AltitudeAGL.Text = "0.000"; // TODO: Add a ground level feature.
            this.accelX.Text = inputData.accelX.ToString();
            this.accelY.Text = inputData.accelY.ToString();
            this.accelZ.Text = inputData.accelZ.ToString();
            this.velX.Text = inputData.velX.ToString();
            this.velY.Text = inputData.velY.ToString();
            this.velZ.Text = inputData.velZ.ToString();
            this.posX.Text = inputData.posX.ToString();
            this.posY.Text = inputData.posY.ToString();
            this.posZ.Text = inputData.posZ.ToString();
            if (inputData.movement == 'G')
                this.FlyorDrive.Text = "Driving";
            else
                this.FlyorDrive.Text = "Flying";
            this.Roll.Text = inputData.roll.ToString();
            this.Pitch.Text = inputData.pitch.ToString();
            this.Yaw.Text = inputData.yaw.ToString();
            if (inputData.gndmtr1)
                this.GndMtr1.Background = Brushes.GreenYellow;
            else
                this.GndMtr1.Background = Brushes.Red;
            if (inputData.gndmtr2)
                this.GndMtr2.Background = Brushes.GreenYellow;
            else
                this.GndMtr2.Background = Brushes.Red;
            if (inputData.amtr1)
                this.AirMtr1.Background = Brushes.GreenYellow;
            else
                this.AirMtr1.Background = Brushes.Red;
            if (inputData.amtr2)
                this.AirMtr2.Background = Brushes.GreenYellow;
            else
                this.AirMtr2.Background = Brushes.Red;
            if (inputData.amtr3)
                this.AirMtr3.Background = Brushes.GreenYellow;
            else
                this.AirMtr3.Background = Brushes.Red;
            if (inputData.amtr4)
                this.AirMtr4.Background = Brushes.GreenYellow;
            else
                this.AirMtr4.Background = Brushes.Red;
            if (inputData.uS1)
                this.USensor1.Background = Brushes.GreenYellow;
            else
                this.USensor1.Background = Brushes.Red;
            if (inputData.uS2)
                this.USensor2.Background = Brushes.GreenYellow;
            else
                this.USensor2.Background = Brushes.Red;
            if (inputData.uS3)
                this.USensor3.Background = Brushes.GreenYellow;
            else
                this.USensor3.Background = Brushes.Red;
            if (inputData.uS4)
                this.USensor4.Background = Brushes.GreenYellow;
            else
                this.USensor4.Background = Brushes.Red;
            if (inputData.uS5)
                this.USensor5.Background = Brushes.GreenYellow;
            else
                this.USensor5.Background = Brushes.Red;
            if (inputData.uS6)
                this.USensor6.Background = Brushes.GreenYellow;
            else
                this.USensor6.Background = Brushes.Red;
            if (inputData.payBay)
            {
                this.PayloadDeployed.Background = Brushes.GreenYellow;
                this.PayloadDeployed.Text = "Deployed";
            }
            else
                this.PayloadDeployed.Background = Brushes.Red;
        }

        private void cbPorts_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void btnPortState_Click(object sender, RoutedEventArgs e)
        {
            if (btnPortState.Content.ToString() == "Connect")
            {
                btnPortState.Content = "Disconnect";

                ComPort.PortName = Convert.ToString(cbPorts.Text);
                ComPort.BaudRate = Convert.ToInt32(cbBaudRate.Text);
                ComPort.DataBits = 8;
                ComPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), "One");
                ComPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), "None");
                ComPort.Parity = (Parity)Enum.Parse(typeof(Parity), "None");

                //
                // Check if port is open already
                if (ComPort.IsOpen)
                {
                    ComPort.Close();
                    System.Windows.MessageBox.Show(string.Concat(ComPort.PortName, " failed to open."));
                }
                else
                {
                    ComPort.Open();
                }

                //
                // COM Port is booted. Start keeping track of time.
                millisecond = DateTime.Now;
                previousMillisecond = millisecond.Millisecond;

                //
                // We have connection! 
                this.Radio.Background = Brushes.GreenYellow;
                this.Radio.Text = "Connected";
            }
            else
            {
                btnPortState.Content = "Connect";
                ComPort.Close();

                //
                // Connection terminated.
                this.Radio.Background = Brushes.Red;
                this.Radio.Text = "Not Connected";
            }
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            //
            // Close the COM port before ending the program.
            if (!ComPort.IsOpen)
            {
                ComPort.Close();
            }
            // else do nothing, port has already been closed.

        }

        //
        // Change the operational mode of the platform.
        private void ModeBtn_Click(object sender, RoutedEventArgs e)
        {
            //
            // Get the color of the autotonomous box.
            if (controlState)
            {
                //
                // Currently in auto mode. Send command to switch to manual.
                AutoControl.Background = Brushes.Red;
                ManualControl.Background = Brushes.GreenYellow;

                //
                // Send command to platform.

                controlState = false;
            }
            else
            {
                //
                // Currently in manual mode. Send comand to switch to auto.
                ManualControl.Background = Brushes.Red;
                AutoControl.Background = Brushes.GreenYellow;

                //
                // Send command to platform.

                controlState = true;
            }
        }

        //
        // Send the target location to the platform.
        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            string targetLat; string targetLong;
            float latitude; float longitude;

            //
            // Get the data in the two text boxes.
            targetLat = TargetLatitude.Text;
            targetLong = TargetLongitude.Text;

            if(targetLat == String.Empty || targetLong == String.Empty)
            {
                //
                // User didn't input any data.
                // Maybe add a pop-up to say that...
                System.Windows.Forms.MessageBox.Show("You must input the target coordinates first!");
                return;
            }

            //
            // This program assumes the user will enter valid data.
            // Convert this data to floating point.
            latitude = Convert.ToSingle(targetLat);
            longitude = Convert.ToSingle(targetLong);

            //
            // Send over the COM port.
            // I don't know how to do that...

            //
            // Set the current target position strings.
            CurrentTargetLatitude.Text = targetLat;
            CurrentTargetLongitude.Text = targetLong;
        }

        private void btnConnectController_Click(object sender, RoutedEventArgs e)
        {
            //
            // Initialize a controller using XINPUT.

        }
    }
}
