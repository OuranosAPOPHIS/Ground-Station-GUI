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
using System.Windows.Input;
using SharpDX.XInput;
using System.Windows.Threading;


namespace WpfApplication2
{
    //
    // Definition for the xinput controller class.
    class XInputController
    {
        Controller controller;
        Gamepad gamepad;
        public bool connected = false;
        public int deadband = 2500;
        public Point leftThumb, rightThumb = new Point(0, 0);
        public float leftTrigger, rightTrigger;
        public GamepadButtonFlags buttonState = 0x0000;

        //
        // XInputController Constructor.
        public XInputController()
        {
            controller = new Controller(UserIndex.One);
            connected = controller.IsConnected;
        }

        // Call this method to update all class values
        public void Update()
        {
            if (!connected)
                return;

            gamepad = controller.GetState().Gamepad;

            //
            // Analog stick positions.
            leftThumb.X = (Math.Abs((float)gamepad.LeftThumbX) < deadband) ? 0 : (float)gamepad.LeftThumbX / short.MinValue * -1;
            leftThumb.Y = (Math.Abs((float)gamepad.LeftThumbY) < deadband) ? 0 : (float)gamepad.LeftThumbY / short.MaxValue * 1;
            rightThumb.Y = (Math.Abs((float)gamepad.RightThumbX) < deadband) ? 0 : (float)gamepad.RightThumbX / short.MaxValue * 1;
            rightThumb.X = (Math.Abs((float)gamepad.RightThumbY) < deadband) ? 0 : (float)gamepad.RightThumbY / short.MaxValue * 1;

            //
            // Bitwise mapping of the buttons pressed.
            buttonState = gamepad.Buttons;

            leftTrigger = gamepad.LeftTrigger;
            rightTrigger = gamepad.RightTrigger;            
        }
    }

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

        internal delegate void dispatcherTimer_Tick(object sender, EventArgs e);

        delegate void SetTextCallback(string text);

        //
        // True will be autonomous mode. False will be manual mode.
        // Initialize this to manual mode. 'M' for manual. 'A' for autonomous.
        char controlState = 'M';

        public MainWindow()
        {
            InitializeComponent();

            InitializeComPort();

            ComPort.DataReceived +=
                new System.IO.Ports.SerialDataReceivedEventHandler(port_DataReceived_1);

            // add handler to call closed function upon program exit
            this.Closed += new EventHandler(MainWindow_Closed);
        }

        //
        // My stuff.

        //
        // Payload release indicator.
        public bool payloadRelease = false;

        //
        // Flag to indicate when user connects to the radio.
        public bool radioConnected = false;

        //
        // Flag to indicate when a valid gps target coordinate has been set.
        public bool targetSet = false;

        //
        // Latitude and longitude of target position.
        public float latitude = 0.0f;
        public float longitude = 0.0f;

        //
        // Set up a controller.
        XInputController controller = new XInputController();

        //
        // Set up a timer for the controller.
        DispatcherTimer controllerTimer = new DispatcherTimer();         

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

        //
        // Global variable to store all th data.
        dataPacket inputData = new dataPacket();

        //
        // Output data struct.
        public struct outDataPacket
        {
            public char type;          // Target or Control command? T or C?
            public char flyordrive;     // vehicle flying or driving?
            public char fdConfirm;      // fly or drive confirmation;
            public float targetLat;    // Target latitude (only in auto mode)
            public float targetLong;   // Target longitude (only in auto mode)
            public float throttle;   // Desired throttle level.
            public float roll;         // Desired roll angle.
            public float pitch;        // Desired pitch angle.
            public float yaw;          // Desired yaw angle.
            public bool payloadRelease;    // Release the payload command.
            public bool prConfirm;         // Confirmation to release payload command.
        }

        //
        // Global variable to store output data.
        outDataPacket outData = new outDataPacket();

        //
        // Function to initialize the serial port on the machine.
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

        //
        // Com port received data event handler. Called by the operating system when
        // there is data available in the rx buffer.
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

        //
        // Updates the GUI with the new data values in the struct dataPacket.
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

        //
        // Send data packet to the platform.
        public void WriteData()
        {
            byte[] data = new byte[21]; // max data packet size is 23 bytes.
            string dataString;

            if (controlState == 'A')
            {
                //
                // Build a buffer to send to the platform.
                data[0] = Convert.ToByte(outData.type);
                data[1] = Convert.ToByte(outData.targetLat);
                data[5] = Convert.ToByte(outData.targetLong);
            }
            else if (controlState == 'M')
            {
                //
                // Build a buffer to send to the platform.
                data[0] = Convert.ToByte(outData.type);
                data[1] = Convert.ToByte(outData.flyordrive);
                data[2] = Convert.ToByte(outData.fdConfirm);
                data[3] = Convert.ToByte(outData.throttle);
                data[7] = Convert.ToByte(outData.roll);
                data[11] = Convert.ToByte(outData.pitch);
                data[14] = Convert.ToByte(outData.yaw);
                data[18] = Convert.ToByte(outData.payloadRelease);
                data[19] = Convert.ToByte(outData.prConfirm);
            }

            //
            // Convert to string to write to the radio.
            dataString = System.Text.Encoding.Default.GetString(data);

            //
            // Write the data.
            ComPort.WriteLine(dataString);
        }


        //
        // Connect to the Com Port button.
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
                radioConnected = true;
            }
            else
            {
                btnPortState.Content = "Connect";
                ComPort.Close();

                //
                // Connection terminated.
                this.Radio.Background = Brushes.Red;
                this.Radio.Text = "Not Connected";
                radioConnected = false;
            }
        }

        //
        // Controller timer event handler. Called every 250 ms to check the 
        // state of the controller. 
        private void controllerTimerTick(object sender, EventArgs e)
        {
            if (controlState == 'A')
            {
                //
                // Check if user has input coordinates yet or not.
                if (targetSet)
                {
                    //
                    // Autonomous control. Just send lat and long and T for type to platform.
                    outData.type = 'T';
                    outData.targetLat = this.latitude;
                    outData.targetLong = this.longitude;
                }
                else
                {
                    //
                    // Send a '0', indicating bad data, ignore the target lat and long.
                    outData.type = '0';
                }
            }
            else // Manual mode, control the platform with the controller.
            {
                //
                // Refresh the button presses on the controller.
                controller.Update();

                //
                // Check the throttle level. Ignore any x value on the right stick.
                // This will be a % from 0.0 to 1.0.
                outData.throttle = (Single)controller.rightThumb.Y;

                //
                // Check if we are driving or flying.
                if (inputData.movement == 'G')
                {
                    //
                    // Travelling on the ground. Ignore pitch and roll.
                    outData.yaw = (Single)controller.leftThumb.X;
                    outData.pitch = 0.0F;
                    outData.roll = 0.0F;
                }
                else if (inputData.movement == 'A')
                {
                    //
                    // We are flying.
                    // Calculate the values of the left analog stick.
                    outData.pitch = (Single)controller.leftThumb.Y;
                    outData.roll = (Single)controller.leftThumb.X;

                    //
                    // Use the left and right triggers to calaculate yaw "rate". 
                    // Value ranges from 0 to 255 for triggers. 
                    // TODO
                }

                //
                // Proof that controller works.
                if (controller.leftTrigger > 1.0)
                {
                    this.Timer.Text = controller.leftThumb.X.ToString();
                }
                else if (controller.rightTrigger > 1.0)
                {
                    this.Timer.Text = controller.rightTrigger.ToString();
                }

                //
                // Check the state of the buttons.
                if (Convert.ToInt16(controller.buttonState) == Convert.ToInt16(GamepadButtonFlags.Start))
                {
                    //
                    // Start button is pressed, change from gnd travel to air travel.
                    if (outData.flyordrive == 'D')
                    {
                        outData.flyordrive = 'F';
                        outData.fdConfirm = 'F';
                        this.FlyorDrive.Text = "FLYING";
                    }
                    else
                    {
                        outData.flyordrive = 'D';
                        outData.fdConfirm = 'D';
                        this.FlyorDrive.Text = "DRIVING";
                    }
                }

                //
                // Check if payload has been deployed.
                if (payloadRelease)
                {
                    outData.payloadRelease = true;
                    outData.prConfirm = true;
                }

                //
                // Trigger a data packet send over the com port.
                if (radioConnected)
                {
                    WriteData();
                }
            }
        }

        //
        // Change the operational mode of the platform.
        private void ModeBtn_Click(object sender, RoutedEventArgs e)
        {
            //
            // Get the color of the autotonomous box.
            if (controlState == 'A')
            {
                //
                // Currently in auto mode. Send command to switch to manual.
                AutoControl.Background = Brushes.Red;
                ManualControl.Background = Brushes.GreenYellow;

                //
                // Send command to platform.
                controlState = 'M';
            }
            else if (controlState == 'M')
            {
                //
                // Currently in manual mode. Send comand to switch to auto.
                ManualControl.Background = Brushes.Red;
                AutoControl.Background = Brushes.GreenYellow;

                //
                // Send command to platform.
                controlState = 'A';
            }
        }

        //
        // Send the target location to the platform.
        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            string targetLat; string targetLong;

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
            else
            {
                //
                // Signal that a target location has been set.
                targetSet = true;

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
        }

        private void btnConnectController_Click(object sender, RoutedEventArgs e)
        {
            //
            // Initialize a controller using XINPUT.
            controller.Update();

            //
            // Initialize the controller timer.
            controllerTimer.Interval = TimeSpan.FromMilliseconds(250);
            controllerTimer.Tick += controllerTimerTick;

            //
            // Start the timer.
            controllerTimer.Start();

            //
            // Change the button to say disconnect.
            this.btnConnectController.Content = "Connected!";
        }

        //
        // When button is pressed, manual deploy the payload. 
        private void DeployPayload_Click(object sender, RoutedEventArgs e)
        {
            //
            // Deploy the payload.
            if (payloadRelease == false)
            {
                //
                // Set it to true.
                payloadRelease = true;

                //
                // Change the GUI.
                this.DeployPayload.Content = "Deployed";
                this.PayloadDeployed.Background = Brushes.GreenYellow;
                this.PayloadDeployed.Text = "Deployed";
            }
            //
            // else do nothing. It is already deployed.
        }

        //
        // End of program.
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

    }
}

