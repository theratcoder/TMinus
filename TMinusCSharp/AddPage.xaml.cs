using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.WindowManagement;
using Windows.UI.WindowManagement.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TMinusCSharp {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddPage : Page
    {
        public AddPage() {
            this.InitializeComponent();  
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            titleTxt.Text = e.Parameter.ToString();
            base.OnNavigatedTo(e);
        }

        private async void submit(object sender, RoutedEventArgs e) {
            if (datePicker.SelectedDate != null && timePicker.Time != null) {
                TimeSpan time = new TimeSpan(timePicker.Time.Hours, timePicker.Time.Minutes, 0);
                DateTimeOffset dt = ((DateTimeOffset)datePicker.SelectedDate).Date + time;

                if (dt > DateTimeOffset.Now) {
                    Countdown countdown = new Countdown(titleTxt.Text, dt);
                    Countdown.countdowns.Add(countdown.id, countdown);

                    countdown.window = await AppWindow.TryCreateAsync();
                    countdown.window.Title = titleTxt.Text;

                    Frame frame = new Frame();
                    frame.MinHeight = frame.MinWidth = 0;
                    frame.Navigate(typeof(TimerPage), countdown);
                    ElementCompositionPreview.SetAppWindowContent(countdown.window, frame);

                    WindowManagementPreview.SetPreferredMinSize(countdown.window, new Size(0, 0));
                    countdown.window.RequestSize(new Size(0, 0));

                    await countdown.window.TryShowAsync();

                    countdown.window.Closed += delegate {
                        frame.Content = null;
                        countdown.window = null;

                        // ensure this is only run when the user closes the countdown window
                        if (Countdown.countdowns.ContainsKey(countdown.id)) {
                            Countdown.countdowns[countdown.id].delete();
                        }
                    };

                    // ensure the size and position of the window are saved if it is moved or resized
                    countdown.window.Changed += (wnd, args) => {
                        // reduce the number of events that cause this to run
                        if (args.DidSizeChange || args.DidFrameChange) {
                            countdown.save();
                        }
                    };

                    countdown.updated += delegate {
                        countdown.window.Title = countdown.title;
                    };

                    Frame.Navigate(typeof(MainPage), countdown.id);

                    countdown.save();
                }
                else {
                    ContentDialog errorDlg = new ContentDialog() {
                        Title = "Error",
                        Content = "You must choose a time and date in the future for your countdown",
                        CloseButtonText = "OK"
                    };
                    await errorDlg.ShowAsync();
                }
            }
            else {
                ContentDialog errorDlg = new ContentDialog() {
                    Title = "Error",
                    Content = "Please choose a date and time for your countdown",
                    CloseButtonText = "OK"
                };
                await errorDlg.ShowAsync();
            }
        }

        private void cancel(object sender, RoutedEventArgs e) {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
