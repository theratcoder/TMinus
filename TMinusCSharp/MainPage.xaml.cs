using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.UI.ViewManagement;
using Windows.Storage;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.UI.Popups;

namespace TMinusCSharp {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        public MainPage() {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            if (e.Parameter is int) {
                int id = (int)e.Parameter;

                Frame item = new Frame();
                CDList.Children.Add(item);
                item.Navigate(typeof(CountdownItem), id);

                Countdown.countdowns[id].delete += delegate {
                    CDList.Children.RemoveAt(Countdown.countdowns.Keys.ToList().IndexOf(id));
                    // this has to be here or else it doesn't work :(
                    Countdown.countdowns.Remove(id);
                };
            }
            else if (e.Parameter is bool) {
                try {
                    Windows.Storage.StorageFolder folder = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync("countdowns");

                    foreach (Windows.Storage.StorageFile file in await folder.GetFilesAsync()) {
                        string json = await Windows.Storage.FileIO.ReadTextAsync(file);
                        CountdownInfo info = JsonConvert.DeserializeObject<CountdownInfo>(json);

                        int fileId;
                        if (!int.TryParse(Path.GetFileNameWithoutExtension(file.Path), out fileId)) {
                            fileId = Countdown.nextId++;
                        }

                        Countdown countdown = new Countdown(info, fileId);
                        countdown.window = await AppWindow.TryCreateAsync();
                        countdown.window.Title = countdown.title;

                        Frame frame = new Frame();
                        frame.MinHeight = frame.MinWidth = 0;
                        frame.Navigate(typeof(TimerPage), countdown);
                        ElementCompositionPreview.SetAppWindowContent(countdown.window, frame);

                        WindowManagementPreview.SetPreferredMinSize(countdown.window, new Size(0, 0));
                        countdown.window.RequestSize(info.windowSize);

                        DisplayRegion curDR = countdown.window.GetDisplayRegions()[0];
                        countdown.window.RequestMoveRelativeToDisplayRegion(curDR, info.windowPos);

                        await countdown.window.TryShowAsync();

                        countdown.window.Closed += delegate {
                            frame.Content = null;
                            countdown.window = null;

                            // ensure this is only run when the user closes the countdown window
                            if (Countdown.countdowns.ContainsKey(countdown.id)) {
                                Countdown.countdowns[countdown.id].delete();
                            }
                        };

                        Countdown.countdowns.Add(countdown.id, countdown);

                        Frame item = new Frame();
                        CDList.Children.Add(item);
                        item.Navigate(typeof(CountdownItem), countdown.id);

                        Countdown.countdowns[countdown.id].delete += delegate {
                            CDList.Children.RemoveAt(Countdown.countdowns.Keys.ToList().IndexOf(countdown.id));
                            // this has to be here or else it doesn't work :(
                            Countdown.countdowns.Remove(countdown.id);
                        };
                    }
                }
                catch (FileNotFoundException) { }

                // app was launched on startup so minimise the window
                if (!(bool)e.Parameter) {
                    await ApplicationView.GetForCurrentView().TryConsolidateAsync();
                }
                else {  // app was launched normally. If this was the first time, the startup task will be disabled so try to enable it
                    StartupTask startupTask = await StartupTask.GetAsync("StId");
                    if (startupTask.State == StartupTaskState.Disabled) {
                        // Task is disabled but can be enabled.
                        StartupTaskState newState = await startupTask.RequestEnableAsync();
                        if (newState == StartupTaskState.DisabledByUser) {
                            ContentDialog msgDlg = new ContentDialog() {
                                Title = "Run on startup disabled",
                                Content = "If you want you can enable this feature later in the \"Startup\" tab in TaskManager.",
                                CloseButtonText = "OK"
                            };
                            await msgDlg.ShowAsync();
                        }
                        else {
                            ContentDialog msgDlg = new ContentDialog() {
                                Title = "Run on startup enabled",
                                Content = "Any saved countdowns will now be opened on startup. You can disable this later in the \"Startup\" tab in TaskManager.",
                                CloseButtonText = "OK"
                            };
                            await msgDlg.ShowAsync();
                        }
                    }
                }
            }
            
            base.OnNavigatedTo(e);
        }

        private void onAdd(object sender, RoutedEventArgs e) {
            Frame.Navigate(typeof(AddPage), titleTxt.Text);
        }
    }
}
