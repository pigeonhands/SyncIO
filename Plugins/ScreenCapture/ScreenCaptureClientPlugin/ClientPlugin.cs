namespace ScreenCaptureClientPlugin
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    using ScreenCaptureClientPlugin.Input;
    using ScreenCaptureClientPlugin.Input.Keyboard;
    using ScreenCaptureClientPlugin.Input.Mouse;

    using SyncIO.Client;
    using SyncIO.ClientPlugin;
    using SyncIO.Common.Packets;
    using SyncIO.Network;
    using SyncIO.Transport.Packets;

    public class ClientPlugin : ISyncIOClientPlugin
    {
        #region Variables

        private ISyncIOClient _clientHost;
        private readonly INetHost _netHost;
        private readonly ILoggingHost _loggingHost;

        private byte[] _desktopData = null;
        private bool _stopRemoteDesktop = true;
        private Thread _rdpThread = null;
        private ScreenCapture _capture;

        private readonly IMouse _mouseInput;
        private readonly IKeyboard _keyboardInput;

        #endregion

        #region Constructor

        public ClientPlugin(INetHost netHost, ILoggingHost loggingHost)
        {
            _netHost = netHost;
            _loggingHost = loggingHost;

            _capture = new ScreenCapture();
            _mouseInput = GetMouseInput();
            _keyboardInput = GetKeyboardInput();

            // Remote desktop screen share packet handlers
            _netHost.SetHandler<StartDesktopPacket>((c, p) => StartRemoteDesktop(p));
            _netHost.SetHandler<StopDesktopPacket>((c, p) => StopRemoteDesktop());
            _netHost.SetHandler<DesktopImagePacket>((c, p) => UpdateScreen());
            _netHost.SetHandler<DesktopSettingsPacket>((c, p) => UpdateDesktopSettings(p));
            _netHost.SetHandler<MouseClickPacket>((c, p) => HandleMouseClick(p));
            _netHost.SetHandler<MouseMovePacket>((c, p) => HandleMouseMove(p));
            _netHost.SetHandler<KeyPacket>((c, p) => HandleKeyPress(p));
        }

        #endregion

        #region Events

        public void OnPluginReady(SyncIOClient client)
        {
            _loggingHost.Trace($"OnPluginReady [ClientHost={client.Id}]");

            _clientHost = client;
        }

        public void OnConnect()
        {
            _loggingHost.Trace("OnConnect");
        }

        public void OnDisconnect(Exception error)
        {
            _loggingHost.Trace($"OnDisconnect [Error={error}]");
            _stopRemoteDesktop = true;
        }

        public void OnPacketReceived(IPacket packet)
        {
            _loggingHost.Trace($"OnPacketReceived [Packet={packet}]");
        }

        #endregion

        #region Remote Desktop

        private void StartRemoteDesktop(StartDesktopPacket packet)
        {
            _stopRemoteDesktop = false;

            if (_rdpThread != null && _rdpThread.IsAlive)
            {
                _rdpThread.Abort();
            }

            if (_capture == null)
            {
                _capture = new ScreenCapture();
            }

            RemoteSettings.DesktopQuality = packet.Quality;
            RemoteSettings.DesktopFramesPerSecond = packet.MaxFPS;

            _rdpThread = new Thread(StartRemoteDesktopHandler);
            _rdpThread.Start();
        }

        private delegate void UpdateScreenEventHandler();
        private void StartRemoteDesktopHandler(object o)
        {
            //_rdpService.CaptureAllScreens = deviceName == "All Screens";
            if (_stopRemoteDesktop)
                return;

            var updateScreen = new UpdateScreenEventHandler(UpdateScreen);
            if (updateScreen != null)
            {
                while (!_stopRemoteDesktop)
                {
                    try
                    {
                        updateScreen();
                        Thread.Sleep(1000 / RemoteSettings.DesktopFramesPerSecond); // 30fps is max speed we can get...
                    }
                    catch { }
                }
            }
        }

        private void StopRemoteDesktop()
        {
            _stopRemoteDesktop = true;

            if (_rdpThread != null && _rdpThread.IsAlive)
            {
                //_rdpThread.Abort();
                _rdpThread.Interrupt();
            }

            if (_capture != null)
            {
                _capture.Dispose();
            }

            _rdpThread = null;
            _capture = null;
        }

        private void UpdateScreen()
        {
            _desktopData = _capture.UpdateScreenImage(RemoteSettings.DesktopQuality);
            if (_desktopData == null)
                return;

            _clientHost.Send(new DesktopImagePacket(_desktopData));
            _desktopData = null;
        }

        private void UpdateDesktopSettings(DesktopSettingsPacket packet)
        {
            RemoteSettings.DesktopQuality = packet.ImageQuality;
            RemoteSettings.DesktopFramesPerSecond = packet.MaxFPS;
        }

        #endregion

        #region Mouse Input

        private void HandleMouseMove(MouseMovePacket packet)
        {
            if (_mouseInput == null)
                return;

            _mouseInput.Move(packet.X, packet.Y);
        }

        private void HandleMouseClick(MouseClickPacket packet)
        {
            if (_mouseInput == null)
                return;

            if (packet.IsLeftClick)
                _mouseInput.LeftClick(packet.X, packet.Y, packet.WheelDelta, packet.IsKeyUp);
            else
                _mouseInput.RightClick(packet.X, packet.Y, packet.WheelDelta, packet.IsKeyUp);
        }

        private IMouse GetMouseInput()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new MouseWindows();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return new MouseMacOS();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // TODO: Implement linux mouse input
                throw new PlatformNotSupportedException();
            }

            throw new PlatformNotSupportedException();
        }

        #endregion

        #region Keyboard Input

        private void HandleKeyPress(KeyPacket packet)
        {
            if (_keyboardInput == null)
                return;

            if (packet.IsKeyDown)
                _keyboardInput.KeyDown(packet.Key, packet.Modifiers);
            else
                _keyboardInput.KeyUp(packet.Key, packet.Modifiers);
        }

        private IKeyboard GetKeyboardInput()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new KeyboardWindows();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return new KeyboardMacOS();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // TODO: Implement linux keyboard input
                throw new PlatformNotSupportedException();
            }

            throw new PlatformNotSupportedException();
        }

        #endregion
    }
}