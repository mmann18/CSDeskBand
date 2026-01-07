#pragma warning disable 1591
#if DESKBAND_AVALONIA
namespace CSDeskBand
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Platform;
    using Avalonia.Threading;
    using Avalonia.Win32;
    using CSDeskBand.Interop;

    /// <summary>
    /// Avalonia implementation of <see cref="ICSDeskBand"/>.
    /// The deskband should also have these attributes <see cref="ComVisibleAttribute"/>, <see cref="GuidAttribute"/>, <see cref="CSDeskBandRegistrationAttribute"/>.
    /// </summary>
    public abstract class CSDeskBandAvalonia : ICSDeskBand, IDeskBandProvider
    {
        private readonly CSDeskBandImpl _impl;
        private readonly ManualResetEventSlim _hostReady = new ManualResetEventSlim(false);
        private readonly object _hostLock = new object();
        private CancellationTokenSource _dispatcherCancellation;
        private Dispatcher _dispatcher;
        private Thread _uiThread;
        private Window _window;
        private IntPtr _handle;
        private volatile bool _hasFocus;

        /// <summary>
        /// Initializes a new instance of the <see cref="CSDeskBandAvalonia"/> class.
        /// </summary>
        public CSDeskBandAvalonia()
        {
            Options.Title = RegistrationHelper.GetToolbarName(GetType());
            _impl = new CSDeskBandImpl(this);
            _impl.Closed += (o, e) => DeskbandOnClosed();
            TaskbarInfo = _impl.TaskbarInfo;
        }

        [ComRegisterFunction]
        private static void Register(Type t)
        {
            RegistrationHelper.Register(t);
        }

        [ComUnregisterFunction]
        private static void Unregister(Type t)
        {
            RegistrationHelper.Unregister(t);
        }

        /// <summary>
        /// Gets the taskbar information.
        /// </summary>
        protected TaskbarInfo TaskbarInfo { get; }

        /// <summary>
        /// Gets the main control for the deskband.
        /// </summary>
        protected abstract Control Control { get; }

        /// <summary>
        /// Gets the options for this deskband.
        /// </summary>
        /// <seealso cref="CSDeskBandOptions"/>
        public CSDeskBandOptions Options { get; } = new CSDeskBandOptions();

        /// <summary>
        /// Gets the handle.
        /// </summary>
        public IntPtr Handle
        {
            get
            {
                EnsureAvaloniaHost();
                return _handle;
            }
        }

        /// <summary>
        /// Gets the deskband guid.
        /// </summary>
        public Guid Guid => GetType().GUID;

        public bool HasFocus
        {
            get => _hasFocus;
            set
            {
                if (value)
                {
                    EnsureAvaloniaHost();
                    _ = _dispatcher?.InvokeAsync(() => Control?.Focus());
                }
            }
        }

        /// <summary>
        /// Updates the focus on this deskband.
        /// </summary>
        /// <param name="focused"><see langword="true"/> if focused.</param>
        public void UpdateFocus(bool focused)
        {
            _impl.UpdateFocus(focused);
        }

        /// <summary>
        /// Handle closing of the deskband.
        /// </summary>
        protected virtual void DeskbandOnClosed()
        {
            ShutdownAvalonia();
        }

        private void EnsureAvaloniaHost()
        {
            if (_uiThread != null)
            {
                _hostReady.Wait();
                return;
            }

            lock (_hostLock)
            {
                if (_uiThread != null)
                {
                    _hostReady.Wait();
                    return;
                }

                _dispatcherCancellation = new CancellationTokenSource();
                _uiThread = new Thread(AvaloniaThreadStart)
                {
                    IsBackground = true,
                    Name = "CSDeskBand Avalonia UI",
                };
                _uiThread.SetApartmentState(ApartmentState.STA);
                _uiThread.Start();
            }

            _hostReady.Wait();
        }

        private void AvaloniaThreadStart()
        {
            AppBuilder.Configure<CSDeskBandAvaloniaApp>()
                .UseWin32()
                .UseSkia()
                .SetupWithoutStarting();

            _dispatcher = Dispatcher.UIThread;
            _dispatcher.Post(CreateAvaloniaWindow);
            _dispatcher.MainLoop(_dispatcherCancellation.Token);
        }

        private void CreateAvaloniaWindow()
        {
            _window = new Window
            {
                SystemDecorations = SystemDecorations.None,
                ShowInTaskbar = false,
                CanResize = false,
                Content = Control,
            };

            _window.Closed += (sender, args) => _dispatcherCancellation?.Cancel();
            _window.GotFocus += (sender, args) => _hasFocus = true;
            _window.LostFocus += (sender, args) => _hasFocus = false;

            _window.Show();

            if (_window.TryGetPlatformHandle() is IPlatformHandle platformHandle)
            {
                _handle = platformHandle.Handle;
            }

            _hostReady.Set();
        }

        private void ShutdownAvalonia()
        {
            if (_dispatcher == null)
            {
                return;
            }

            _ = _dispatcher.InvokeAsync(() =>
            {
                _window?.Close();
                _window = null;
            });

            _dispatcherCancellation?.Cancel();
        }

        public int GetWindow(out IntPtr phwnd)
        {
            return _impl.GetWindow(out phwnd);
        }

        public int ContextSensitiveHelp(bool fEnterMode)
        {
            return _impl.ContextSensitiveHelp(fEnterMode);
        }

        public int ShowDW([In] bool fShow)
        {
            return _impl.ShowDW(fShow);
        }

        public int CloseDW([In] uint dwReserved)
        {
            return _impl.CloseDW(dwReserved);
        }

        public int ResizeBorderDW(RECT prcBorder, [In, MarshalAs(UnmanagedType.IUnknown)] IntPtr punkToolbarSite, bool fReserved)
        {
            return _impl.ResizeBorderDW(prcBorder, punkToolbarSite, fReserved);
        }

        public int GetBandInfo(uint dwBandID, DESKBANDINFO.DBIF dwViewMode, ref DESKBANDINFO pdbi)
        {
            return _impl.GetBandInfo(dwBandID, dwViewMode, ref pdbi);
        }

        public int CanRenderComposited(out bool pfCanRenderComposited)
        {
            return _impl.CanRenderComposited(out pfCanRenderComposited);
        }

        public int SetCompositionState(bool fCompositionEnabled)
        {
            return _impl.SetCompositionState(fCompositionEnabled);
        }

        public int GetCompositionState(out bool pfCompositionEnabled)
        {
            return _impl.GetCompositionState(out pfCompositionEnabled);
        }

        public int SetSite([In, MarshalAs(UnmanagedType.IUnknown)] object pUnkSite)
        {
            return _impl.SetSite(pUnkSite);
        }

        public int GetSite(ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out IntPtr ppvSite)
        {
            return _impl.GetSite(ref riid, out ppvSite);
        }

        public int QueryContextMenu(IntPtr hMenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, QueryContextMenuFlags uFlags)
        {
            return _impl.QueryContextMenu(hMenu, indexMenu, idCmdFirst, idCmdLast, uFlags);
        }

        public int InvokeCommand(IntPtr pici)
        {
            return _impl.InvokeCommand(pici);
        }

        public int GetCommandString(ref uint idcmd, uint uflags, ref uint pwReserved, [MarshalAs(UnmanagedType.LPTStr)] out string pcszName, uint cchMax)
        {
            return _impl.GetCommandString(ref idcmd, uflags, ref pwReserved, out pcszName, cchMax);
        }

        public int HandleMenuMsg(uint uMsg, IntPtr wParam, IntPtr lParam)
        {
            return _impl.HandleMenuMsg(uMsg, wParam, lParam);
        }

        public int HandleMenuMsg2(uint uMsg, IntPtr wParam, IntPtr lParam, out IntPtr plResult)
        {
            return _impl.HandleMenuMsg2(uMsg, wParam, lParam, out plResult);
        }

        public int GetClassID(out Guid pClassID)
        {
            return _impl.GetClassID(out pClassID);
        }

        public int GetSizeMax(out ulong pcbSize)
        {
            return _impl.GetSizeMax(out pcbSize);
        }

        public int IsDirty()
        {
            return _impl.IsDirty();
        }

        public int Load(object pStm)
        {
            return _impl.Load(pStm);
        }

        public int Save(IntPtr pStm, bool fClearDirty)
        {
            return _impl.Save(pStm, fClearDirty);
        }

        public int UIActivateIO(int fActivate, ref MSG msg)
        {
            return _impl.UIActivateIO(fActivate, ref msg);
        }

        public int HasFocusIO()
        {
            return _impl.HasFocusIO();
        }

        public int TranslateAcceleratorIO(ref MSG msg)
        {
            return _impl.TranslateAcceleratorIO(ref msg);
        }

        private sealed class CSDeskBandAvaloniaApp : Application
        {
        }
    }
}
#endif
