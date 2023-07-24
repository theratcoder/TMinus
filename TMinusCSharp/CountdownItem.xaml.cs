using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.Input.ForceFeedback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TMinusCSharp {
    public sealed partial class CountdownItem : Page {
        bool ended;
        int CDid;
        DispatcherTimer timer;

        public CountdownItem() {
            this.InitializeComponent();
        }

        public void timerTick(object sender, object e) {
            CDTxt.Text = Countdown.countdowns[CDid].displayRemaining();

            // timer is done
            if (Countdown.countdowns[CDid].time <= DateTimeOffset.Now) {
                timer.Stop();
                ended = true;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            CDid = (int)e.Parameter;

            titleTxt.Text = Countdown.countdowns[CDid].title;

            CDTxt.Text = Countdown.countdowns[CDid].displayRemaining();

            Countdown.countdowns[CDid].delete += delegate {
                // ensure the timer is stopped when the countdown is deleted
                timer.Stop();
            };

            timer = new DispatcherTimer();
            timer.Tick += timerTick;
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();

            base.OnNavigatedTo(e);
        }

        private void OnClick(object sender, PointerRoutedEventArgs e) {
            if (!ended) {
                timer.Stop();
                Frame.Navigate(typeof(ModifyPage), CDid);
            }
        }

        private void OnTap(object sender, TappedRoutedEventArgs e) {
            if (!ended) {
                timer.Stop();
                Frame.Navigate(typeof(ModifyPage), CDid);
            }
        }

        private void pointerIn(object sender, PointerRoutedEventArgs e) {
            page.Background = new SolidColorBrush(Windows.UI.Colors.LightGray);
        }

        private void pointerOut(object sender, PointerRoutedEventArgs e) {
            page.Background = null;
        }
    }
}
