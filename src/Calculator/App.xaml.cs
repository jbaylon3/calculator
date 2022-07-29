using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using CalculatorApp;
using Windows.UI.ViewManagement;
using CalculatorApp.ViewModel.Common;
using Windows.Storage;
using Windows.ApplicationModel.Core;
using CalculatorApp.ViewModel.Common.Automation;
using Windows.UI.Core;
using Windows.UI.StartScreen;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Windowing;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CalculatorApp
{
    namespace ApplicationResourceKeys
    {
        static public partial class Globals
        {
            public static readonly string AppMinWindowHeight = "AppMinWindowHeight";
            public static readonly string AppMinWindowWidth = "AppMinWindowWidth";
        }
    }
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            m_preLaunched = false;

            RegisterDependencyProperties();

            // TODO: MSFT 14645325: Set this directly from XAML.
            // Currently this is bugged so the property is only respected from code-behind.
            HighContrastAdjustment = ApplicationHighContrastAdjustment.None;

            //Suspending += OnSuspending;
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            /*if (args.UWPLaunchActivatedEventArgs.PrelaunchActivated)
            {
                // If the app got pre-launch activated, then save that state in a flag
                m_preLaunched = true;
            }*/

            //NavCategoryStates.SetCurrentUser(args.UWPLaunchActivatedEventArgs.User.NonRoamableId);

            // It takes time to check GraphingMode at the 1st time. So, do it in a background thread
            //Task.Run(() => NavCategoryStates.IsViewModeEnabled(ViewMode.Graphing));

            //OnAppLaunch(args, args.Arguments);
            OnAppLaunch(args, null); //Temporary fix, nou used anymore
        }

        protected void OnActivated(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            if (args.UWPLaunchActivatedEventArgs.Kind == ActivationKind.Protocol)
            {
                // We currently don't pass the uri as an argument,
                // and handle any protocol launch as a normal app launch.
                OnAppLaunch(args, null);
            }
        }

        internal void RemoveWindow(WindowFrameService frameService)
        {
            // Shell does not allow killing the main window.
            if (m_mainViewId != frameService.GetViewId())
            {
                _ = HandleViewReleaseAndRemoveWindowFromMap(frameService);
            }
        }

        internal void RemoveSecondaryWindow(WindowFrameService frameService)
        {
            // Shell does not allow killing the main window.
            if (m_mainViewId != frameService.GetViewId())
            {
                RemoveWindowFromMap(frameService.GetViewId());
            }
        }

        private static Frame CreateFrame()
        {
            var frame = new Frame();
            frame.FlowDirection = LocalizationService.GetInstance().GetFlowDirection();
            return frame;
        }

        private static void SetMinWindowSizeAndThemeAndActivate(Frame rootFrame, Size minWindowSize)
        {
            // SetPreferredMinSize should always be called before Window.Activate
           // ApplicationView appView = ApplicationView.GetForCurrentView();
            //appView.SetPreferredMinSize(minWindowSize);

            // Place the frame in the current Window
            App.Window.Content = rootFrame;
            CalculatorApp.Utils.ThemeHelper.InitializeAppTheme();
            App.Window.Activate();
        }

        private void OnAppLaunch(Microsoft.UI.Xaml.LaunchActivatedEventArgs args, string argument)
        {
            // Uncomment the following lines to display frame-rate and per-frame CPU usage info.
            //#if _DEBUG
            //    if (IsDebuggerPresent())
            //    {
            //        DebugSettings->EnableFrameRateCounter = true;
            //    }
            //#endif

            //args.SplashScreen.Dismissed += DismissedEventHandler;
            Window = new MainWindow();

            var rootFrame = (Window.Content as Frame);
            WeakReference weak = new WeakReference(this);

            float minWindowWidth = (float)((double)Resources[ApplicationResourceKeys.Globals.AppMinWindowWidth]);
            float minWindowHeight = (float)((double)Resources[ApplicationResourceKeys.Globals.AppMinWindowHeight]);
            Size minWindowSize = SizeHelper.FromDimensions(minWindowWidth, minWindowHeight);

            //ApplicationView appView = ApplicationView.GetForCurrentView();
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            // For very first launch, set the size of the calc as size of the default standard mode
            /*if (!localSettings.Values.ContainsKey("VeryFirstLaunch"))
            {
                localSettings.Values["VeryFirstLaunch"] = false;
                appView.SetPreferredMinSize(minWindowSize);
                appView.TryResizeView(minWindowSize);
            }
            else
            {
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
            }*/

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                if (!Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons")) // PC Family
                {
                    // Disable the system view activation policy during the first launch of the app
                    // only for PC family devices and not for phone family devices
                    /*try
                    {
                        ApplicationViewSwitcher.DisableSystemViewActivationPolicy();
                    }
                    catch (Exception)
                    {
                        // Log that DisableSystemViewActionPolicy didn't work
                    }*/
                }

                // Create a Frame to act as the navigation context
                rootFrame = App.CreateFrame();
                Window.Content = rootFrame;
                bool nav = false;

                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    // TODO Raname this MainPage type in case your app MainPage has a different name
                    nav = rootFrame.Navigate(typeof(MainPage), args.Arguments);
                }

                Window.Activate();
                WindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(Window);
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                //if (!rootFrame.Navigate(typeof(MainPage), argument))
                if (!nav)
                {
                    // We couldn't navigate to the main page, kill the app so we have a good
                    // stack to debug
                    throw new SystemException();
                }

                SetMinWindowSizeAndThemeAndActivate(rootFrame, minWindowSize);
                //m_mainViewId = ApplicationView.GetForCurrentView().Id;
                //AddWindowToMap(WindowFrameService.CreateNewWindowFrameService(rootFrame, false, weak));
            }
            else
            {
                // For first launch, LaunchStart is logged in constructor, this is for subsequent launches.

                // !Phone check is required because even in continuum mode user interaction mode is Mouse not Touch
                if ((UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Mouse)
                    && (!Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons")))
                {
                    // If the pre-launch hasn't happened then allow for the new window/view creation
                    if (!m_preLaunched)
                    {
                        var newCoreAppView = CoreApplication.CreateNewView();
                        _ = newCoreAppView.Dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal, async () =>
                            {
                                var that = weak.Target as App;
                                if (that != null)
                                {
                                    var newRootFrame = App.CreateFrame();

                                    SetMinWindowSizeAndThemeAndActivate(newRootFrame, minWindowSize);

                                    if (!newRootFrame.Navigate(typeof(MainPage), argument))
                                    {
                                        // We couldn't navigate to the main page, kill the app so we have a good
                                        // stack to debug
                                        throw new SystemException();
                                    }

                                    var frameService = WindowFrameService.CreateNewWindowFrameService(newRootFrame, true, weak);
                                    that.AddWindowToMap(frameService);

                                    //var dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

                                    // CSHARP_MIGRATION_ANNOTATION:
                                    // class SafeFrameWindowCreation is being interpreted into a IDisposable class
                                    // in order to enhance its RAII capability that was written in C++/CX
                                    using (var safeFrameServiceCreation = new SafeFrameWindowCreation(frameService, that))
                                    {
                                        //int newWindowId = ApplicationView.GetApplicationViewIdForWindow(CoreWindow.GetForCurrentThread());

                                        ActivationViewSwitcher activationViewSwitcher = null;
                                        /*var activateEventArgs = (args as IViewSwitcherProvider);
                                        if (activateEventArgs != null)
                                        {
                                            activationViewSwitcher = activateEventArgs.ViewSwitcher;
                                        }

                                        if (activationViewSwitcher != null)
                                        {
                                            _ = activationViewSwitcher.ShowAsStandaloneAsync(newWindowId, ViewSizePreference.Default);
                                            safeFrameServiceCreation.SetOperationSuccess(true);
                                        }
                                        else
                                        {
                                            var activatedEventArgs = (args as IApplicationViewActivatedEventArgs);
                                            if ((activatedEventArgs != null) && (activatedEventArgs.CurrentlyShownApplicationViewId != 0))
                                            {
                                                // CSHARP_MIGRATION_ANNOTATION:
                                                // here we don't use ContinueWith() to interpret origin code because we would like to 
                                                // pursue the design of class SafeFrameWindowCreate whichi was using RAII to ensure
                                                // some states get handled properly when its instance is being destructed.
                                                //
                                                // To achieve that, SafeFrameWindowCreate has been reinterpreted using IDisposable
                                                // pattern, which forces we use below way to keep async works being controlled within
                                                // a same code block.
                                                var viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(
                                                                frameService.GetViewId(),
                                                                ViewSizePreference.Default,
                                                                activatedEventArgs.CurrentlyShownApplicationViewId,
                                                                ViewSizePreference.Default);
                                                // SafeFrameServiceCreation is used to automatically remove the frame
                                                // from the list of frames if something goes bad.
                                                safeFrameServiceCreation.SetOperationSuccess(viewShown);
                                            }
                                        }*/
                                    }
                                }
                            });
                    }
                    else
                    {
                        ActivationViewSwitcher activationViewSwitcher = null;
                        /*var activateEventArgs = (args as IViewSwitcherProvider);
                        if (activateEventArgs != null)
                        {
                            activationViewSwitcher = activateEventArgs.ViewSwitcher;
                        }

                        if (activationViewSwitcher != null)
                        {
                            _ = activationViewSwitcher.ShowAsStandaloneAsync(
                                ApplicationView.GetApplicationViewIdForWindow(CoreWindow.GetForCurrentThread()), ViewSizePreference.Default);
                        }
                        else
                        {
                            TraceLogger.GetInstance().LogError(ViewMode.None, "App.OnAppLaunch", "Null_ActivationViewSwitcher");
                        }*/
                    }
                    // Set the preLaunched flag to false
                    m_preLaunched = false;
                }
                else // for touch devices
                {
                    if (rootFrame.Content == null)
                    {
                        // When the navigation stack isn't restored navigate to the first page,
                        // configuring the new page by passing required information as a navigation
                        // parameter
                        if (!rootFrame.Navigate(typeof(MainPage), argument))
                        {
                            // We couldn't navigate to the main page,
                            // kill the app so we have a good stack to debug
                            throw new SystemException();
                        }
                    }
                    /*if (ApplicationView.GetForCurrentView().ViewMode != ApplicationViewMode.CompactOverlay)
                    {
                        if (!Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                        {
                            // for tablet mode: since system view activation policy is disabled so do ShowAsStandaloneAsync if activationViewSwitcher exists in
                            // activationArgs
                            ActivationViewSwitcher activationViewSwitcher = null;
                            /*var activateEventArgs = (args as IViewSwitcherProvider);
                            if (activateEventArgs != null)
                            {
                                activationViewSwitcher = activateEventArgs.ViewSwitcher;
                            }
                            if (activationViewSwitcher != null)
                            {
                                var viewId = (args as IApplicationViewActivatedEventArgs).CurrentlyShownApplicationViewId;
                                if (viewId != 0)
                                {
                                    _ = activationViewSwitcher.ShowAsStandaloneAsync(viewId);
                                }
                            }
                        }
                        // Ensure the current window is active
                        App.Window.Activate();
                    }*/
                }
            }
        }

        private void DismissedEventHandler(SplashScreen sender, object e)
        {
            _ = SetupJumpList();
        }

        private void RegisterDependencyProperties()
        {
            NarratorNotifier.RegisterDependencyProperties();
        }

        private void OnSuspending(object sender, SuspendingEventArgs args)
        {
            TraceLogger.GetInstance().LogButtonUsage();
        }

        private sealed class SafeFrameWindowCreation : IDisposable
        {
            public SafeFrameWindowCreation(WindowFrameService frameService, App parent)
            {
                m_frameService = frameService;
                m_frameOpenedInWindow = false;
                m_parent = parent;
            }

            public void SetOperationSuccess(bool success)
            {
                m_frameOpenedInWindow = success;
            }

            public void Dispose()
            {
                if (!m_frameOpenedInWindow)
                {
                    // Close the window as the navigation to the window didn't succeed
                    // and this is not visible to the user.
                    m_parent.RemoveWindowFromMap(m_frameService.GetViewId());
                }

                GC.SuppressFinalize(this);
            }

            ~SafeFrameWindowCreation()
            {
                Dispose();
            }

            private WindowFrameService m_frameService;
            private bool m_frameOpenedInWindow;
            private App m_parent;
        };

        private async Task SetupJumpList()
        {
            try
            {
                var calculatorOptions = NavCategoryStates.CreateCalculatorCategoryGroup();

                var jumpList = await JumpList.LoadCurrentAsync();
                jumpList.SystemGroupKind = JumpListSystemGroupKind.None;
                jumpList.Items.Clear();

                foreach (NavCategory option in calculatorOptions.Categories)
                {
                    if (!NavCategoryStates.IsViewModeEnabled(option.ViewMode))
                    {
                        continue;
                    }
                    ViewMode mode = option.ViewMode;
                    var item = JumpListItem.CreateWithArguments(((int)mode).ToString(), "ms-resource:///Resources/" + NavCategoryStates.GetNameResourceKey(mode));
                    item.Description = "ms-resource:///Resources/" + NavCategoryStates.GetNameResourceKey(mode);
                    item.Logo = new Uri("ms-appx:///Assets/" + mode.ToString() + ".png");

                    jumpList.Items.Add(item);
                }

                await jumpList.SaveAsync();
            }
            catch
            {
            }
        }

        private async Task HandleViewReleaseAndRemoveWindowFromMap(WindowFrameService frameService)
        {
            WeakReference weak = new WeakReference(this);

            // Unregister the event handler of the Main Page
            var frame = (App.Window.Content as Frame);
            var mainPage = (frame.Content as MainPage);

            mainPage.UnregisterEventHandlers();

            await frameService.HandleViewRelease();
            await Task.Run(() =>
            {
                var that = weak.Target as App;
                that.RemoveWindowFromMap(frameService.GetViewId());
            }).ConfigureAwait(false /* task_continuation_context::use_arbitrary() */);
        }

        private void AddWindowToMap(WindowFrameService frameService)
        {
            m_windowsMapLock.EnterWriteLock();
            try
            {
                m_secondaryWindows[frameService.GetViewId()] = frameService;
                TraceLogger.GetInstance().UpdateWindowCount(Convert.ToUInt64(m_secondaryWindows.Count));
            }
            finally
            {
                m_windowsMapLock.ExitWriteLock();
            }
        }

        private WindowFrameService GetWindowFromMap(int viewId)
        {
            m_windowsMapLock.EnterReadLock();
            try
            {
                if (m_secondaryWindows.TryGetValue(viewId, out var windowMapEntry))
                {
                    return windowMapEntry;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                m_windowsMapLock.ExitReadLock();
            }
        }

        private void RemoveWindowFromMap(int viewId)
        {
            m_windowsMapLock.EnterWriteLock();
            try
            {
                bool removed = m_secondaryWindows.Remove(viewId);
                Debug.Assert(removed != false, "Window does not exist in the list");
            }
            finally
            {
                m_windowsMapLock.ExitWriteLock();
            }
        }

        private readonly ReaderWriterLockSlim m_windowsMapLock = new ReaderWriterLockSlim();
        private Dictionary<int, WindowFrameService> m_secondaryWindows = new Dictionary<int, WindowFrameService>();
        private int m_mainViewId;
        private bool m_preLaunched;

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        /*protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs e)
        {
            // TODO This code defaults the app to a single instance app. If you need multi instance app, remove this part.
            // Read: https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/guides/applifecycle#single-instancing-in-applicationonlaunched
            // If this is the first instance launched, then register it as the "main" instance.
            // If this isn't the first instance launched, then "main" will already be registered,
            // so retrieve it.
            var mainInstance = Microsoft.Windows.AppLifecycle.AppInstance.FindOrRegisterForKey("main");
            var activatedEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();

            // If the instance that's executing the OnLaunched handler right now
            // isn't the "main" instance.
            if (!mainInstance.IsCurrent)
            {
                // Redirect the activation (and args) to the "main" instance, and exit.
                await mainInstance.RedirectActivationToAsync(activatedEventArgs);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                return;
            }

            // TODO This code handles app activation types. Add any other activation kinds you want to handle.
            // Read: https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/guides/applifecycle#file-type-association
            if (activatedEventArgs.Kind == ExtendedActivationKind.File)
            {
                OnFileActivated(activatedEventArgs);
            }


            // Initialize MainWindow here
            Window = new MainWindow();

            Frame rootFrame = Window.Content as Frame;
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                // Place the frame in the current Window
                Window.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                // TODO Raname this MainPage type in case your app MainPage has a different name
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }

            Window.Activate();
            WindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(Window);
        }

        // TODO This is an example method for the case when app is activated through a file.
        // Feel free to remove this if you do not need this.
        public void OnFileActivated(AppActivationArguments activatedEventArgs)
        {

        }*/

        public static MainWindow Window { get; private set; }

        public static IntPtr WindowHandle { get; private set; }
    }
}
