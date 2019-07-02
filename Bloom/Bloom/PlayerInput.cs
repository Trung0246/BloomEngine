using System;
using System.Numerics;
using Dapplo.Windows.Input.Keyboard;
using Dapplo.Windows.Input.Enums;

namespace Bloom
{
    public class PlayerInput : IDisposable, IObserver<KeyboardHookEventArgs>
    {
        private IDisposable Unsubscriber;

        public (VirtualKeyCode, VirtualKeyCode) Axis0XKey { get; }
        public (VirtualKeyCode, VirtualKeyCode) Axis0YKey { get; }
        public VirtualKeyCode Action0Key { get; }
        public VirtualKeyCode Action1Key { get; }
        public VirtualKeyCode Action2Key { get; }
        public VirtualKeyCode Action3Key { get; }
        public VirtualKeyCode Action4Key { get; }
        public VirtualKeyCode Action5Key { get; }
        public VirtualKeyCode PauseKey { get; }

        public bool Axis0RightState { get; private set; }

        public bool Axis0LeftState { get; private set; }

        public bool Axis0UpState { get; private set; }

        public bool Axis0DownState { get; private set; }

        public bool Action0State { get; private set; }

        public bool Action1State { get; private set; }

        public bool Action2State { get; private set; }

        public bool Action3State { get; private set; }

        public bool Action4State { get; private set; }

        public bool Action5State { get; private set; }

        public bool PauseState { get; private set; }

        public bool Disposed { get; private set; }

        public PlayerInput()
        {
            // Create control settings
            // TODO: load control settings from file instead of using hardcoded ones
            Axis0XKey = (VirtualKeyCode.Right, VirtualKeyCode.Left);
            Axis0YKey = (VirtualKeyCode.Up, VirtualKeyCode.Down);
            Action0Key = VirtualKeyCode.KeyZ;
            Action1Key = VirtualKeyCode.KeyX;
            Action2Key = VirtualKeyCode.KeyC;
            Action3Key = VirtualKeyCode.LeftShift;
            Action4Key = VirtualKeyCode.LeftControl;
            Action5Key = VirtualKeyCode.Space;
            PauseKey = VirtualKeyCode.Escape;
            // Subscribe to key events
            Unsubscriber = KeyboardHook.KeyboardEvents.Subscribe(this);
        }

        ~PlayerInput()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Disposed)
                return;
            Unsubscriber.Dispose();
        }

        public void OnCompleted()
        {
            Console.WriteLine("No longer receiving keyboard input");
        }

        public void OnError(Exception error)
        {
            throw error;
        }

        public void OnNext(KeyboardHookEventArgs keyArgs)
        {
            if (keyArgs.Key == Axis0XKey.Item1)
            {
                Axis0RightState = keyArgs.IsKeyDown;
                return;
            }
            if (keyArgs.Key == Axis0XKey.Item2)
            {
                Axis0LeftState = keyArgs.IsKeyDown;
                return;
            }
            if (keyArgs.Key == Axis0YKey.Item1)
            {
                Axis0UpState = keyArgs.IsKeyDown;
                return;
            }
            if (keyArgs.Key == Axis0YKey.Item2)
            {
                Axis0DownState = keyArgs.IsKeyDown;
                return;
            }
            if (keyArgs.Key == Action0Key)
            {
                Action0State = keyArgs.IsKeyDown;
                return;
            }
            if (keyArgs.Key == Action1Key)
            {
                Action1State = keyArgs.IsKeyDown;
                return;
            }
            if (keyArgs.Key == Action2Key)
            {
                Action2State = keyArgs.IsKeyDown;
                return;
            }
            if (keyArgs.Key == Action3Key)
            {
                Action3State = keyArgs.IsKeyDown;
                return;
            }
            if (keyArgs.Key == Action4Key)
            {
                Action4State = keyArgs.IsKeyDown;
                return;
            }
            if (keyArgs.Key == Action5Key)
            {
                Action5State = keyArgs.IsKeyDown;
                return;
            }
            if (keyArgs.Key == PauseKey)
            {
                PauseState = keyArgs.IsKeyDown;
                return;
            }
        }
    }
}
