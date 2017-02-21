using System;
using SharpDX.XInput;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Numerics;

namespace APOPHISGS
{
    //
    // Definition for the xinput Controller class.
    class XInputController : IDisposable
    {
        private Controller controller;
        private Gamepad gamepad;
        private int pollDelay = 10;
        private CancellationTokenSource disconnect = new CancellationTokenSource();
        private Task pollTask;

        public int PollRate {
            get {
                return 1000 / pollDelay;
            }
            set {
                if (0 < value && value < 1001) {
                    pollDelay = 1000 / value;
                } else {
                    throw new ArgumentOutOfRangeException(nameof(value), "Poll rate cannot be less than 1 or greater than 1000.");
                }
            }
        }
        
        public bool IsConnected { get { return controller != null && controller.IsConnected; } }
        public UserIndex UserIndex { get { return controller != null ? controller.UserIndex : UserIndex.Any; } }
        
        public float LeftDeadZone { get; set; } = Gamepad.LeftThumbDeadZone / short.MaxValue;
        public float RightDeadZone { get; set; } = Gamepad.LeftThumbDeadZone / short.MaxValue;
        public byte TriggerThreshold { get; set; } = Gamepad.TriggerThreshold / byte.MaxValue;

        public Vector LeftThumb { get { return IsConnected ? CalculateDeadzone(gamepad.LeftThumbX, -gamepad.LeftThumbY, LeftDeadZone) : new Vector(0, 0); } }
        public Vector RightThumb { get { return IsConnected ? CalculateDeadzone(gamepad.RightThumbX, -gamepad.RightThumbY, RightDeadZone) : new Vector(0, 0); } }
        public byte LeftTrigger { get { return ((gamepad.LeftTrigger) >= TriggerThreshold && IsConnected) ? gamepad.LeftTrigger : ((byte)0); } }
        public byte RightTrigger { get { return ((gamepad.RightTrigger) >= TriggerThreshold && IsConnected) ? gamepad.RightTrigger : ((byte)0); } }

        public GamepadButtonFlags ButtonState { get { return gamepad.Buttons; } }

        public bool IsDPadUp { get { return ((ButtonState & GamepadButtonFlags.DPadUp) != GamepadButtonFlags.None); } }
        public bool IsDPadRight { get { return ((ButtonState & GamepadButtonFlags.DPadRight) != GamepadButtonFlags.None); } }
        public bool IsDPadDown { get { return ((ButtonState & GamepadButtonFlags.DPadDown) != GamepadButtonFlags.None); } }
        public bool IsDPadLeft { get { return ((ButtonState & GamepadButtonFlags.DPadLeft) != GamepadButtonFlags.None); } }
        public bool IsBack { get { return ((ButtonState & GamepadButtonFlags.Back) != GamepadButtonFlags.None); } }
        public bool IsStart { get { return ((ButtonState & GamepadButtonFlags.Start) != GamepadButtonFlags.None); } }       
        public bool IsLeftThumb { get { return ((ButtonState & GamepadButtonFlags.LeftThumb) != GamepadButtonFlags.None); } }
        public bool IsRightThumb { get { return ((ButtonState & GamepadButtonFlags.RightThumb) != GamepadButtonFlags.None); } }
        public bool IsLeftShoulder { get { return ((ButtonState & GamepadButtonFlags.LeftShoulder) != GamepadButtonFlags.None); } }
        public bool IsRightShoulder { get { return ((ButtonState & GamepadButtonFlags.RightShoulder) != GamepadButtonFlags.None); } }
        public bool IsY { get { return ((ButtonState & GamepadButtonFlags.Y) != GamepadButtonFlags.None); } }
        public bool IsB { get { return ((ButtonState & GamepadButtonFlags.B) != GamepadButtonFlags.None); } }
        public bool IsA { get { return ((ButtonState & GamepadButtonFlags.A) != GamepadButtonFlags.None); } }
        public bool IsX { get { return ((ButtonState & GamepadButtonFlags.X) != GamepadButtonFlags.None); } }

        //
        // XInputController Constructor.
        public XInputController(int pollRate = 100)
        {
            PollRate = pollRate;
        }

        public async Task<bool> Connect(UserIndex user = UserIndex.Any) {
            await Disconnect();
            disconnect = new CancellationTokenSource();
            controller = new Controller(user);
            if (IsConnected)
            {
                pollTask = Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                    //poll HW
                    gamepad = controller.GetState().Gamepad;
                        Thread.Sleep(pollDelay);
                        if (disconnect.Token.IsCancellationRequested || !IsConnected) break;
                    }
                }, disconnect.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            return IsConnected;
        }

        public async Task Disconnect() {
            disconnect?.Cancel();
            if (pollTask != null) await pollTask;
            pollTask = null;
            disconnect = null;
            controller = null;
        }

        private Vector CalculateDeadzone(int X, int Y, double deadzone) {
            if (X == short.MinValue) X = short.MinValue + 1;
            if (Y == short.MinValue) Y = short.MinValue + 1;
                        
            var input = new Vector(X, Y);
            input.Normalize();

            if (input.Length < deadzone) {
                input = new Vector(0, 0);
            } else {
                input = input * ((input.Length - deadzone) / (1 - deadzone));
            }
            return input;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    disconnect?.Cancel();
                    if (pollTask != null) pollTask.RunSynchronously();
                    pollTask = null;
                    disconnect = null;
                    controller = null;
                }
                
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
