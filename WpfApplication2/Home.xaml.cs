using System;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using SharpDX.XInput;
using APOPHIS.GroundStation.Packet;

namespace APOPHIS.GroundStation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        // Declare the serial com port
        SerialPort comPort = new SerialPort();

        //
        // Set up a controller.
        XInputController controller = new XInputController();

        //
        // True will be autonomous mode. False will be manual mode.
        // Initialize this to manual mode. 'M' for manual. 'A' for autonomous.
        char ControlState { get; set; } = 'M';
        
        //
        // My stuff.

        //
        // Payload release indicator.
        public bool PayloadRelease { get; set; }

        //
        // Flag to indicate when user connects to the radio.
        public bool RadioConnected { get; set; }

        //
        // Flag to indicate when a valid gps target coordinate has been set.
        public bool TargetSet { get; set; }

        //
        // Latitude and Longitude of target position.
        public float Latitude { get; set; }

        public float Longitude { get; set; }
        
        //
        // Set up a timer for the controller.
        DispatcherTimer PacketUpdate { get; set; } = new DispatcherTimer();

        //
        // Setup a system clock to count the time between data packets.
        DateTime Millisecond { get; set; } = DateTime.Now;
        int DeltaT { get; set; } = 0;
        int PreviousMillisecond { get; set; }
        int DataStreamSize { get; set; } = 0;

        //
        // Open a log file to write the data to.
        // using (System.IO.StreamWriter sw = File.AppendText(" c:\\test.txt"));
        //string lines = "First Line.\nSecond Line.\nThird Line.\n";

        //
        // Global variable to store all th data.
        DataPacket InputData { get; set; } = new DataPacket();

        //
        // Global variable to store output data.
        ControlOutDataPacket ControlOutData { get; set; } = new ControlOutDataPacket();

        TargetOutDataPacket TargetOutData { get; set; } = new TargetOutDataPacket();

        internal delegate void SerialDataReceivedEventHandlerDelegate(object sender, SerialDataReceivedEventArgs e);

        internal delegate void DispatcherTimerTick(object sender, EventArgs e);

        delegate void SetTextCallback(string text);

        public MainWindow()
        {
            InitializeComponent();

            comPort.DataReceived += new SerialDataReceivedEventHandler(COMPortDataReceived);

            // add handler to call closed function upon program exit
            Closed += new EventHandler(MainWindowClosed);
        }
        
        //
        // Com port received data event handler. Called by the operating system when
        // there is data available in the rx buffer.
        private void COMPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int size;
            byte[] rawData;
            rawData = new byte[100];
            int currentMillisecond;

            //
            // Packet received! Get the current time.
            Millisecond = DateTime.Now;
            currentMillisecond = Millisecond.Millisecond;
            DeltaT = currentMillisecond - PreviousMillisecond;
            if (DeltaT < 0) DeltaT = DeltaT + 1000;

            PreviousMillisecond = currentMillisecond;

            //
            // Get the size of the incoming buffer.
            size = comPort.BytesToRead;
            DataStreamSize = size;

            //
            // Make sure we have a full packet, before updating.
            if (size > 77)
            {
                if (size > 100)
                {
                    comPort.DiscardInBuffer();
                }
                else
                {
                    //
                    // Read the data from the incoming buffer.
                    comPort.Read(rawData, 0, size);

                    InputData.FromBytes(rawData);
                    Dispatcher?.Invoke(() => SetText());
                }
            }
        }

        //
        // Updates the GUI with the new data values in the struct DataPacket.
        private void SetText()
        {
            //
            // Write to the log file.
            // file.WriteLine(lines);

            //
            // Set the update rate.
            updateRate.Text = DeltaT.ToString();
            dataStream.Text = DataStreamSize.ToString();

            //
            // Update the GUI.
            UTC.Text = InputData.UTC.ToString();
            GPSLatitude.Text = InputData.Latitude.ToString();
            GPSLongitude.Text = InputData.Longitude.ToString();
            AltitudeASL.Text = InputData.Altitude.ToString();
            AltitudeAGL.Text = "0.000"; // TODO: Add a ground level feature.
            accelX.Text = InputData.AccelX.ToString();
            accelY.Text = InputData.AccelY.ToString();
            accelZ.Text = InputData.AccelZ.ToString();
            velX.Text = InputData.VelX.ToString();
            velY.Text = InputData.VelY.ToString();
            velZ.Text = InputData.VelZ.ToString();
            posX.Text = InputData.PosX.ToString();
            posY.Text = InputData.PosY.ToString();
            posZ.Text = InputData.PosZ.ToString();
            FlyOrDrive.Text = (InputData.Movement == 'D') ? "DRIVING" : "FLYING";
            Roll.Text = InputData.Roll.ToString();
            Pitch.Text = InputData.Pitch.ToString();
            Yaw.Text = InputData.Yaw.ToString();
            GndMtr1.Background = InputData.GroundMeter1 ? Brushes.GreenYellow : Brushes.Red;
            GndMtr2.Background = InputData.GroundMeter2 ? Brushes.GreenYellow : Brushes.Red;
            AirMtr1.Background = InputData.AirMotor1 ? Brushes.GreenYellow : Brushes.Red;
            AirMtr2.Background = InputData.AirMotor2 ? Brushes.GreenYellow : Brushes.Red;
            AirMtr3.Background = InputData.AirMotor3 ? Brushes.GreenYellow : Brushes.Red;
            AirMtr4.Background = InputData.AirMotor4 ? Brushes.GreenYellow : Brushes.Red;
            USensor1.Background = InputData.uS1 ? Brushes.GreenYellow : Brushes.Red;
            USensor2.Background = InputData.uS2 ? Brushes.GreenYellow : Brushes.Red;
            USensor3.Background = InputData.uS3 ? Brushes.GreenYellow : Brushes.Red;
            USensor4.Background = InputData.uS4 ? Brushes.GreenYellow : Brushes.Red;
            USensor5.Background = InputData.uS5 ? Brushes.GreenYellow : Brushes.Red;
            USensor6.Background = InputData.uS6 ? Brushes.GreenYellow : Brushes.Red;
            if (InputData.PayloadBay)
            {
                PayloadDeployed.Background = Brushes.GreenYellow;
                PayloadDeployed.Text = "Deployed";
            }
            else
            {
                PayloadDeployed.Background = Brushes.Red;
            }
        }

        //
        // Send data packet to the platform.
        public void WriteData()
        {
            switch (ControlState)
            {
                case 'A':
                    {
                        byte[] data = TargetOutData.GetBytes();

                        //
                        // Write the data.
                        comPort.Write(data, 0, data.Length);

                        break;
                    }
                case 'M':
                    {
                        byte[] datam = ControlOutData.GetBytes();

                        //
                        // Write the data.
                        comPort.Write(datam, 0, datam.Length);

                        break;
                    }
            }
        }

        //
        // controller timer event handler. Called every 250 ms to check the 
        // state of the controller. 
        private void UpdateTimerTick(object sender, EventArgs e)
        {
            switch (ControlState)
            {
                case 'A':
                    { // Autonomous mode
                      //
                      // Check if user has input coordinates yet or not.
                        if (TargetSet)
                        {
                            //
                            // Autonomous control. Just send lat and long and T for type to platform.
                            TargetOutData.Type = 'T';
                            TargetOutData.TargetLat = Latitude;
                            TargetOutData.TargetLong = Longitude;
                        }
                        else
                        {
                            //
                            // Send a '0', indicating bad data, ignore the target lat and long.
                            TargetOutData.Type = '0';
                        }
                        break;
                    }
                case 'M':
                    { // Manual mode, control the platform with the controller.    
                      //
                      // Set the type of command.
                        ControlOutData.Type = 'C';

                        //
                        // Check the throttle level. Ignore any x value on the right stick.
                        // This will be a % from 0.0 to 1.0.
                        ControlOutData.Throttle = (float)controller.RightThumb.Y;

                        //
                        // Check if we are driving or flying.
                        switch (InputData.Movement)
                        {
                            case 'D':
                                {
                                    //
                                    // Travelling on the ground. Ignore pitch and roll.
                                    ControlOutData.Throttle2 = (float)controller.LeftThumb.Y;
                                    ControlOutData.Pitch = 0.0F;
                                    ControlOutData.Roll = 0.0F;
                                    ControlOutData.Yaw = 0.0F;
                                    break;
                                }
                            case 'F':
                                {
                                    //
                                    // We are flying.
                                    // Calculate the values of the left analog stick.
                                    ControlOutData.Pitch = (float)controller.LeftThumb.Y;
                                    ControlOutData.Roll = (float)controller.LeftThumb.X;

                                    //
                                    // Use the left and right triggers to calaculate yaw "rate". 
                                    // Value ranges from 0 to 255 for triggers. 
                                    if (controller.RightTrigger > 0)
                                    {
                                        ControlOutData.Yaw = controller.RightTrigger;
                                    }
                                    else if (controller.LeftTrigger > 0)
                                    {
                                        ControlOutData.Yaw = controller.LeftTrigger * -1;
                                    }
                                    else
                                    {
                                        ControlOutData.Yaw = 0.0F;
                                    }
                                    break;
                                }
                        }

                        //
                        // Check the state of the buttons.
                        if (Convert.ToInt16(controller.ButtonState) == Convert.ToInt16(GamepadButtonFlags.Start))
                        {
                            //
                            // Start button is pressed, change from gnd travel to air travel.
                            if (ControlOutData.FlyOrDrive == 'D')
                            {
                                ControlOutData.FlyOrDrive = 'F';
                                ControlOutData.FDConfirm = 'F';
                                FlyOrDrive.Text = "FLYING";
                            }
                            else
                            {
                                ControlOutData.FlyOrDrive = 'D';
                                ControlOutData.FDConfirm = 'D';
                                FlyOrDrive.Text = "DRIVING";
                            }
                        }

                        //
                        // Check if payload has been deployed.
                        if (PayloadRelease)
                        {
                            ControlOutData.PayloadRelease = true;
                            ControlOutData.PRConfirm = true;
                        }
                    }
                    break;
            }
            //
            // Trigger a data packet send over the com port.
            if (RadioConnected && (comPort.BytesToRead == 0)) WriteData();
        }

        //
        // Function to initialize the serial port on the machine.
        private void btnCOMPortRefresh_Click(object sender, RoutedEventArgs e)
        {
            cbPorts.ItemsSource = SerialPort.GetPortNames().OrderBy(x => x).ToArray();
            if (cbPorts.Items.Count > 0) cbPorts.SelectedIndex = 0;
            cbBaudRate.SelectedIndex = 0;
        }

        //
        // Connect to the Com Port button.
        private void btnCOMPortConnect_Click(object sender, RoutedEventArgs e)
        {
            if (btnCOMPortConnect.Content.ToString() == "Connect")
            {
                btnCOMPortConnect.Content = "Disconnect";

                comPort.PortName = cbPorts.Text;
                comPort.BaudRate = Convert.ToInt32(cbBaudRate.Text);
                comPort.DataBits = 8;
                comPort.StopBits = StopBits.One;
                comPort.Handshake = Handshake.None;
                comPort.Parity = Parity.None;

                //
                // Check if port is open already
                if (comPort.IsOpen)
                {
                    comPort.Close();
                    System.Windows.MessageBox.Show(string.Concat(comPort.PortName, " failed to open."));
                }
                else
                {
                    comPort.Open();
                }

                //
                // COM Port is booted. Start keeping track of time.
                Millisecond = DateTime.Now;
                PreviousMillisecond = Millisecond.Millisecond;

                //
                // We have connection! 
                Radio.Background = Brushes.GreenYellow;
                Radio.Text = "Connected";
                RadioConnected = true;
            }
            else
            {
                btnCOMPortConnect.Content = "Connect";
                comPort.Close();

                //
                // Connection terminated.
                Radio.Background = Brushes.Red;
                Radio.Text = "Not Connected";
                RadioConnected = false;
            }
        }

        //
        // Change the operational mode of the platform.
        private void btnMode_Click(object sender, RoutedEventArgs e)
        {
            //
            // Get the color of the autotonomous box.
            switch (ControlState)
            {
                case 'A':
                    {
                        //
                        // Currently in auto mode. Send command to switch to manual.
                        AutoControl.Background = Brushes.Red;
                        ManualControl.Background = Brushes.GreenYellow;

                        //
                        // Send command to platform.
                        ControlState = 'M';
                        break;
                    }
                case 'M':
                    {
                        //
                        // Currently in manual mode. Send comand to switch to auto.
                        ManualControl.Background = Brushes.Red;
                        AutoControl.Background = Brushes.GreenYellow;

                        //
                        // Send command to platform.
                        ControlState = 'A';
                        break;
                    }
            }
        }

        //
        // Changes the update rate of the system (packets sent per second)
        private void btnSetUpdateRate_Click(object sender, RoutedEventArgs e)
        {
            //
            // Initialize the controller timer.
            PacketUpdate.Interval = TimeSpan.FromMilliseconds(250);
            PacketUpdate.Tick += UpdateTimerTick;

            //
            // Start the timer.
            PacketUpdate.Start();
        }

        //
        // Send the target location to the platform.
        private void btnSendPacket_Click(object sender, RoutedEventArgs e)
        {
            string targetLat; string targetLong;

            //
            // Get the data in the two text boxes.
            targetLat = TargetLatitude.Text;
            targetLong = TargetLongitude.Text;

            if (targetLat == String.Empty || targetLong == String.Empty)
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
                TargetSet = true;

                //
                // This program assumes the user will enter valid data.
                // Convert this data to floating point.
                Latitude = Convert.ToSingle(targetLat);
                Longitude = Convert.ToSingle(targetLong);

                //
                // Send over the COM port.
                // I don't know how to do that...

                //
                // Set the current target position strings.
                CurrentTargetLatitude.Text = targetLat;
                CurrentTargetLongitude.Text = targetLong;
            }
        }

        //
        // Connect the selected controller
        private async void btnConnectController_Click(object sender, RoutedEventArgs e)
        {
            if (controller.IsConnected)
            {
                await controller.Disconnect();
                btnConnectController.Content = "Connect";
            }
            else
            {
                //
                // Initialize a controller using XINPUT.
                switch (cbController.Text)
                {
                    case "1":
                        {
                            await controller.Connect(UserIndex.One);
                            break;
                        }
                    case "2":
                        {
                            await controller.Connect(UserIndex.Two);
                            break;
                        }
                    case "3":
                        {
                            await controller.Connect(UserIndex.Three);
                            break;
                        }
                    case "4":
                        {
                            await controller.Connect(UserIndex.Four);
                            break;
                        }
                    default:
                        {
                            await controller.Connect(UserIndex.Any);
                            break;
                        }
                }
                if (controller.IsConnected)
                {
                    //
                    // Change the button to say disconnect.
                    btnConnectController.Content = $"Controller {controller.UserIndex}";
                }
                else
                {
                    System.Windows.MessageBox.Show("Could not connect to controller!");
                }
            }
        }

        //
        // When button is pressed, manual deploy the payload. 
        private void DeployPayload_Click(object sender, RoutedEventArgs e)
        {
            //
            // Deploy the payload.
            if (!PayloadRelease)
            {
                //
                // Set it to true.
                PayloadRelease = true;

                //
                // Change the GUI.
                DeployPayload.Content = "Deployed";
                PayloadDeployed.Background = Brushes.GreenYellow;
                PayloadDeployed.Text = "Deployed";
            }
        }

        //
        // End of program.
        void MainWindowClosed(object sender, EventArgs e)
        {
            //
            // Close the COM port before ending the program.
            if (!comPort.IsOpen) comPort.Close();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    comPort.Dispose();
                    controller.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}

