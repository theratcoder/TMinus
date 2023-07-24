using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TMinusCSharp {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModifyPage : Page {
        int id;

        public ModifyPage() {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            id = (int)e.Parameter;

            titleTxt.Text = Countdown.countdowns[id].title;
            datePicker.Date = Countdown.countdowns[id].time.Date;
            timePicker.Time = Countdown.countdowns[id].time.TimeOfDay;

            base.OnNavigatedTo(e);
        }

        public async void update(object sender, RoutedEventArgs e) {
            TimeSpan time = new TimeSpan(timePicker.Time.Hours, timePicker.Time.Minutes, 0);
            DateTimeOffset dt = datePicker.Date.Date + time;

            if (dt > DateTimeOffset.Now) {
                Countdown.countdowns[id].title = titleTxt.Text;
                Countdown.countdowns[id].time = dt;

                Frame.Navigate(typeof(CountdownItem), id);

                Countdown.countdowns[id].save();
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

        public void cancel(object sender, RoutedEventArgs e) {
            Frame.Navigate(typeof(CountdownItem), id);
        }

        public async void delete(object sender, RoutedEventArgs e) {
            ContentDialog delDlg = new ContentDialog {
                Title = "Delete countdown?",
                Content = "If you delete this countdown, you won't be able to recover it. Do you want to delete it?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await delDlg.ShowAsync();

            // Delete the countdown if the user clicked the primary button.
            /// Otherwise, do nothing.
            if (result == ContentDialogResult.Primary) {
                Countdown.countdowns[id].delete();
            }
        }
    }
}
