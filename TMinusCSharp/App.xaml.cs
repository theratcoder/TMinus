﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.WindowManagement.Preview;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections;
using Windows.Storage;
using Windows.ApplicationModel.Core;

namespace TMinusCSharp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e) {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // tell the main page to load any countdowns
                    rootFrame.Navigate(typeof(MainPage), true);
                }

                rootFrame.CacheSize = 1;

                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when the app is launched on start up or in some other way which is not by the end user
        /// </summary>
        /// <param name="args">Details about the launch request and process</param>
        protected override async void OnActivated(IActivatedEventArgs args) {
            if (args.Kind == ActivationKind.StartupTask) {
                try {
                    Windows.Storage.StorageFolder folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("countdowns");
                    if ((await folder.GetFilesAsync())?.Any() != true) {
                        // nothing to load so just close the app
                        CoreApplication.Exit();
                    }
                }
                catch (FileNotFoundException) {
                    // again, nothing to load so close the app
                    CoreApplication.Exit();
                }

                Frame rootFrame = Window.Current.Content as Frame;

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (rootFrame == null) {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame = new Frame();

                    rootFrame.NavigationFailed += OnNavigationFailed;

                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;
                }

                if (rootFrame.Content == null) {
                    // tell the main page to load any countdowns
                    rootFrame.Navigate(typeof(MainPage), false);
                }

                rootFrame.CacheSize = 1;

                // Ensure the current window is active
                Window.Current.Activate();
            }
            base.OnActivated(args);
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e) {
            try {
                // clear any files that do not have associated countdowns
                StorageFolder folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("countdowns");

                foreach (Windows.Storage.StorageFile file in await folder.GetFilesAsync()) {
                    int fileId;
                    if (!int.TryParse(Path.GetFileNameWithoutExtension(file.Path), out fileId)) {
                        // clear any files that don't conform to the naming scheme ({file id}.ctdn)
                        await file.DeleteAsync();
                        continue;
                    }

                    bool found = false;
                    foreach (Countdown countdown in Countdown.countdowns.Values) {
                        if (fileId == countdown.fileId) {
                            found = true;
                            break;
                        }
                    }
                    if (!found) {
                        await file.DeleteAsync();
                    }
                }
            }
            catch (FileNotFoundException) { } // folder doesn't exist so don't bother

            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}
