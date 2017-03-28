using System;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using SharpDX.XInput;
using APOPHIS.GroundStation.Input.Xbox;
using APOPHIS.GroundStation.Packet.Data;
using APOPHIS.GroundStation.Helpers;
using APOPHIS.GroundStation.Packet;

namespace APOPHIS.GroundStation.GUI {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, IDisposable {
    // Declare the serial com port
    SerialPort _COMPort = new SerialPort();
    
    //
    // Set up a controller.
    XboxController _controller = new XboxController();

    //
    // Set up a log writer.
    PacketLogger _logWriter = new PacketLogger();
    string fileName = "GSDataLog";
    
    //
    // Setup a system clock to count the time between data packets.
    DateTime Millisecond { get; set; } = DateTime.Now;
    int DeltaT { get; set; } = 0;
    int PreviousMillisecond { get; set; }
    int DataStreamSize { get; set; } = 0;

    //
    // True will be autonomous mode. False will be manual mode.
    // Initialize this to manual mode. 'M' for manual. 'A' for autonomous.
    char ControlState { get; set; } = 'M';
    
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
    // Indicator for when to log data.
    bool LogData = false;

    //
    // Set up a timer for the controller.
    DispatcherTimer PacketUpdate { get; set; } = new DispatcherTimer();
        
    //
    // Global variable to store all th data.
    DataPacket InputData { get; set; } = new DataPacket();

    //
    // Global variable to store output data.
    ControlOutDataPacket ControlOutData { get; set; } = new ControlOutDataPacket();

    TargetOutDataPacket TargetOutData { get; set; } = new TargetOutDataPacket();

    public MainWindow() {
      InitializeComponent();
      
      // Initialize COM Port GUI options and add handler for COM Port changes
      cbPorts.ItemsSource = SerialPortService.GetAvailableSerialPorts();
      SerialPortService.PortsChanged += (sender, eventArgs) => {
        cbPorts.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => {
          cbPorts.ItemsSource = SerialPortService.GetAvailableSerialPorts();
        }));
      };
      
      _COMPort.DataReceived += COMPortDataReceived;

      _controller.Connected += (sender, eventArgs) => {
        btnConnectController.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => {
          btnConnectController.Content = $"Disconnect";
          Controller.Text = $"Controller {_controller.UserIndex}";
          Controller.Background = Brushes.GreenYellow;
        }));        
      };

      _controller.ConnectedFailed += (sender, eventArgs) => {      
          MessageBox.Show("Could not connect to controller!");
      };

      _controller.Disconnected += (sender, eventArgs) => {
        btnConnectController.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => {
          btnConnectController.Content = "Connect";
          Controller.Text = "Not Connected";
          Controller.Background = Brushes.Red;
        }));
        MessageBox.Show("The controller has been disconnected!");
      };

      // Add handler to call closed function upon program exit
      Closed += new EventHandler(OnMainWindowClosed);
    }

    //
    // Com port received data event handler. Called by the operating system when
    // there is data available in the rx buffer.
    private void COMPortDataReceived(object sender, SerialDataReceivedEventArgs e) {
            byte[] rawData = new byte[84];
            byte[] Magic = new byte[4] { 0, 0, 0, 0};
            int MagicCount = 0;
            int currentMillisecond;
            int Index = 0;
            bool bValidData = false;
            byte rxChar;

        //TODO : FIX TO UTILIZE MAGIC PACKET ON ROLLING BUFFER
        rawData = new byte[84];

        DataStreamSize = _COMPort.BytesToRead;

        //
        // Search for the magic packet.
        while (_COMPort.BytesToRead > 0)
        {
                rxChar = Convert.ToByte(_COMPort.ReadByte());

                if (Index >= 84) Index = 0;
                if (bValidData)
                {
                    //
                    // Read the data from the incoming buffer.
                    rawData[Index++] = rxChar;

                    if (((rawData[3] == Convert.ToByte('D')) || (rawData[3] == Convert.ToByte('F'))) && (Index >= 84))
                    {

                        //
                        // Packet received! Get the current time.
                        Millisecond = DateTime.Now;
                        currentMillisecond = Millisecond.Millisecond;
                        DeltaT = currentMillisecond - PreviousMillisecond;
                        if (DeltaT < 0) DeltaT = DeltaT + 1000;

                        PreviousMillisecond = currentMillisecond;

                        InputData.Bytes = rawData;

                        Dispatcher?.Invoke(() => UpdateGUI());

                        //
                        // Log all the received data to a log file.
                        if (LogData)
                            _logWriter.LogWrite(InputData.CSVData);
                              break;
                    }
                }
                else
                {
                    Magic[Index] = rxChar;
                    Index = (Index + 1) % 4;
                    if (MagicCount >= 3)
                    {
                        if (Magic[Index % 4] == 0xFF && Magic[(Index + 1) % 4] == 0xFF && Magic[(Index + 2) % 4] == 0xFF &&
                            (Magic[(Index + 3) % 4] == Convert.ToByte('D') || Magic[(Index + 3) % 4] == Convert.ToByte('F')))
                        {
                            rawData[3] = Magic[(Index + 3) % 4];
                            Index = 4;
                            bValidData = true;
                            MagicCount = 0;
                        }
                    }
                    else
                    {
                        MagicCount++;
                    }
                }
        }
    }

    //
    // Updates the GUI with the new data values in the struct DataPacket.
    private void UpdateGUI() {
      //
      // Set the update rate.
      updateRate.Text = DeltaT.ToString();
      dataStream.Text = DataStreamSize.ToString();

      //
      // Update the GUI.
      txtUTC.Text = InputData.UTC.ToString();
      txtGPSLatitude.Text = InputData.Latitude.ToString();
      txtGPSLongitude.Text = InputData.Longitude.ToString();
      txtAltitudeASL.Text = InputData.Altitude.ToString();
      txtAltitudeAGL.Text = "0.000"; // TODO: Add a ground level feature.
      txtAccelX.Text = InputData.AccelX.ToString();
      txtAccelY.Text = InputData.AccelY.ToString();
      txtAccelZ.Text = InputData.AccelZ.ToString();
      txtVelX.Text = InputData.GyroX.ToString();
      txtVelY.Text = InputData.GyroY.ToString();
      txtVelZ.Text = InputData.GyroZ.ToString();
      txtPosX.Text = InputData.MagX.ToString();
      txtPosY.Text = InputData.MagY.ToString();
      txtPosZ.Text = InputData.MagZ.ToString();
      txtFlyOrDrive.Text = (InputData.Movement == 'D') ? "DRIVING" : "FLYING";
      txtRoll.Text = InputData.Roll.ToString();
      txtPitch.Text = InputData.Pitch.ToString();
      txtYaw.Text = InputData.Yaw.ToString();
      txtGndMtr1.Background = InputData.GroundMeter1 ? Brushes.GreenYellow : Brushes.Red;
      txtGndMtr2.Background = InputData.GroundMeter2 ? Brushes.GreenYellow : Brushes.Red;
      txtAirMtr1.Background = InputData.AirMotor1 ? Brushes.GreenYellow : Brushes.Red;
      txtAirMtr2.Background = InputData.AirMotor2 ? Brushes.GreenYellow : Brushes.Red;
      txtAirMtr3.Background = InputData.AirMotor3 ? Brushes.GreenYellow : Brushes.Red;
      txtAirMtr4.Background = InputData.AirMotor4 ? Brushes.GreenYellow : Brushes.Red;
      txtUSensor1.Background = InputData.uS1 ? Brushes.GreenYellow : Brushes.Red;
      txtUSensor2.Background = InputData.uS2 ? Brushes.GreenYellow : Brushes.Red;
      txtUSensor3.Background = InputData.uS3 ? Brushes.GreenYellow : Brushes.Red;
      txtUSensor4.Background = InputData.uS4 ? Brushes.GreenYellow : Brushes.Red;
      txtUSensor5.Background = InputData.uS5 ? Brushes.GreenYellow : Brushes.Red;
      txtUSensor6.Background = InputData.uS6 ? Brushes.GreenYellow : Brushes.Red;
      if (InputData.PayloadBay) {
        txtPayloadDeployed.Background = Brushes.GreenYellow;
        txtPayloadDeployed.Text = "Deployed";
      } else {
        txtPayloadDeployed.Background = Brushes.Red;
      }
    }

    //
    // Send data packet to the platform.
    public void SendPacket() {
      switch (ControlState) {
        case 'A': {
            byte[] data = TargetOutData.Bytes;

            //
            // Write the data.
            _COMPort.Write(data, 0, data.Length);

            break;
          }
        case 'M': {
            byte[] data = ControlOutData.Bytes;

            //
            // Write the data.
            _COMPort.Write(data, 0, data.Length);

            break;
          }
      }
    }

    //
    // controller timer event handler. Called every 250 ms to check the 
    // state of the controller. 
    private void UpdateTimerTick(object sender, EventArgs e) {
      switch (ControlState) {
        case 'A': { // Autonomous mode
                    //
                    // Check if user has input coordinates yet or not.
            if (TargetSet) {
              //
              // Autonomous control. Just send lat and long and T for type to platform.
              TargetOutData.Type = 'T';
              TargetOutData.TargetLat = Latitude;
              TargetOutData.TargetLong = Longitude;
            } else {
              //
              // Send a '0', indicating bad data, ignore the target lat and long.
              TargetOutData.Type = '0';
            }
            break;
          }
        case 'M': { 
            // Manual mode, control the platform with the controller.    
            //
            // Set the type of command.
            ControlOutData.Type = 'C';

            //
            // Check if we are driving or flying.
            switch (InputData.Movement) {
              case 'D': {
                    //
                    // Travelling on the ground. Ignore pitch and roll.
                    // Check the throttle level. Ignore any x value on the right stick.
                    // This will be a % from 0.0 to 1.0.
                    ControlOutData.Throttle = (float)_controller.RightThumb.Y;
                    ControlOutData.Throttle2 = (float)_controller.LeftThumb.Y;
                    ControlOutData.Pitch = 0.0F;
                    ControlOutData.Roll = 0.0F;
                    ControlOutData.Yaw = 0.0F;
                  break;
                }
              case 'F': {
                  //
                  // We are flying.
                  // Calculate the values of the left analog stick.
                  ControlOutData.Pitch = (float)_controller.LeftThumb.Y;
                  ControlOutData.Roll = (float)_controller.LeftThumb.X;

                  //
                  // Use the left and right triggers to calaculate throttle. 
                  // Value ranges from 0 to 255 for triggers. 
                  /*
                  if (_controller.RightTrigger > 0) {
                    ControlOutData.Throttle = Convert.ToSingle(_controller.RightTrigger);
                  } else if (_controller.LeftTrigger > 0) {
                    ControlOutData.Throttle = Convert.ToSingle(_controller.LeftTrigger * -1);
                  } else {
                    ControlOutData.Throttle = 0.0F;
                  }*/

                  ControlOutData.Throttle = (Single)_controller.RightThumb.Y;

                 //
                 // Get the yaw direction using the controller bumpers. 
                 if (_controller.IsRightShoulder)
                    ControlOutData.Yaw = 1;
                else if (_controller.IsLeftShoulder)
                    ControlOutData.Yaw = -1;
                else
                    ControlOutData.Yaw = 0;
                break;
                }
            }

            //
            // Check the state of the buttons.
            if (Convert.ToInt16(_controller.ButtonState) == Convert.ToInt16(GamepadButtonFlags.Start)) {
              //
              // Start button is pressed, change from gnd travel to air travel.
              if (ControlOutData.FlyOrDrive == 'D') {
                ControlOutData.FlyOrDrive = 'F';
                ControlOutData.FDConfirm = 'F';
                txtFlyOrDrive.Text = "FLYING";
              } else {
                ControlOutData.FlyOrDrive = 'D';
                ControlOutData.FDConfirm = 'D';
                txtFlyOrDrive.Text = "DRIVING";
              }
            }

            //
            // Check if payload has been deployed.
            if (PayloadRelease) {
              ControlOutData.PayloadRelease = Convert.ToByte(true);
              ControlOutData.PRConfirm = Convert.ToByte(true);
            }
          }
          break;
      }
      //
      // Trigger a data packet send over the com port.
      if ((RadioConnected && _controller.IsConnected) || (RadioConnected && (ControlState == 'A'))) 
                SendPacket();
    }

    //
    // Function to initialize the serial port on the machine.
    private void OnBtnCOMPortRefreshClick(object sender, RoutedEventArgs e) {
      cbPorts.ItemsSource = SerialPort.GetPortNames().OrderBy(x => x).ToArray();
      if (cbPorts.Items.Count > 0) cbPorts.SelectedIndex = 0;
      cbBaudRate.SelectedIndex = 0;
    }

    //
    // Connect to the Com Port button.
    private void OnBtnCOMPortConnectClick(object sender, RoutedEventArgs e) {
      if (btnCOMPortConnect.Content.ToString() == "Connect") {
        btnCOMPortConnect.Content = "Disconnect";

        _COMPort.PortName = cbPorts.Text;
        _COMPort.BaudRate = Convert.ToInt32(cbBaudRate.Text);
        _COMPort.DataBits = 8;
        _COMPort.StopBits = StopBits.One;
        _COMPort.Handshake = Handshake.None;
        _COMPort.Parity = Parity.None;

        //
        // Check if port is open already
        if (_COMPort.IsOpen) {
          _COMPort.Close();
          System.Windows.MessageBox.Show(string.Concat(_COMPort.PortName, " failed to open."));
        } else {
          _COMPort.Open();
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
      } else {
        btnCOMPortConnect.Content = "Connect";
        _COMPort.Close();

        //
        // Connection terminated.
        Radio.Background = Brushes.Red;
        Radio.Text = "Not Connected";
        RadioConnected = false;
      }
    }

    //
    // Change the operational mode of the platform.
    private void OnBtnModeClick(object sender, RoutedEventArgs e) {
      //
      // Get the color of the autotonomous box.
      switch (ControlState) {
        case 'A': {
            //
            // Currently in auto mode. Send command to switch to manual.
            AutoControl.Background = Brushes.Red;
            ManualControl.Background = Brushes.GreenYellow;

            //
            // Send command to platform.
            ControlState = 'M';
            break;
          }
        case 'M': {
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
    private void OnBtnSetUpdateRateClick(object sender, RoutedEventArgs e) {
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
    private void OnBtnSendPacketClick(object sender, RoutedEventArgs e) {
      string targetLat; string targetLong;

      //
      // Get the data in the two text boxes.
      targetLat = TargetLatitude.Text;
      targetLong = TargetLongitude.Text;

      if (targetLat == String.Empty || targetLong == String.Empty) {
        //
        // User didn't input any data.
        // Maybe add a pop-up to say that...
        System.Windows.Forms.MessageBox.Show("You must input the target coordinates first!");
        return;
      } else {
        //
        // Signal that a target location has been set.
        TargetSet = true;

        //
        // This program assumes the user will enter valid data.
        // Convert this data to floating point.
        Latitude = Convert.ToSingle(targetLat);
        Longitude = Convert.ToSingle(targetLong);
        
        //
        // Set the current target position strings.
        CurrentTargetLatitude.Text = targetLat;
        CurrentTargetLongitude.Text = targetLong;
      }
    }

    //
    // Connect the selected controller
    private async void OnBtnConnectControllerClick(object sender, RoutedEventArgs e) {
      if (_controller.IsConnected) {
        await _controller.Disconnect();
      } else {
        UserIndex controllerIndex;
        if (!Enum.TryParse(cbController.Text, out controllerIndex)) controllerIndex = UserIndex.Any;
        await _controller.Connect(controllerIndex);
      }
    }

    //
    // When button is pressed, manual deploy the payload. 
    private void OnBtnDeployPayloadClick(object sender, RoutedEventArgs e) {
      //
      // Deploy the payload.
      if (!PayloadRelease) {
        //
        // Set it to true.
        PayloadRelease = true;

        //
        // Change the GUI.
        DeployPayload.Content = "Deployed";
        txtPayloadDeployed.Background = Brushes.GreenYellow;
        txtPayloadDeployed.Text = "Deployed";
      }
    }

    private void LogFileBtn_Click(object sender, RoutedEventArgs e)
    {
      if (!LogData)
      {
        //
        // Create the log file.
        _logWriter.LogCreator(fileName);
        _logWriter.LogWrite(InputData.CSVHeader);

        LogData = true;

        this.LogFileBtn.Content = "Stop Logging Data";
      }
      else
      {
        //
        // Stop the logger.
        LogData = false;

        this.LogFileBtn.Content = "Log Data";
      }
    }

    //
    // End of program.
    private void OnMainWindowClosed(object sender, EventArgs e) {
      Dispose();
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing) {
      if (!disposedValue) {
        if (disposing) {
          SerialPortService.CleanUp();
          _COMPort.Dispose();
          _controller.Dispose();
          SerialPortService.CleanUp();
        }

        disposedValue = true;
      }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose() {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
    }
    #endregion

  }
}
