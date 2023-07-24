using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TMinusCSharp {
    public sealed partial class TimerPage : Page {
        Countdown countdown;
        DispatcherTimer timer;
        DispatcherTimer endTimer;
        Windows.UI.Color BgColor = Windows.UI.Colors.White;

        public TimerPage() {
            this.InitializeComponent();
        }

        public void endTimerTick(object sender, object e) {
            // fade background color from white to green and then jump back to white
            BgColor.R--;
            BgColor.B--;
            page.Background = new SolidColorBrush(BgColor);
        }

        void timerTick(object sender, object e) {
            CDTxt.Text = countdown.displayRemaining();

            // timer is done
            if (countdown.time <= DateTimeOffset.Now) {
                timer.Stop();
                countdown.deleteFile(); // ensure that the the countdown does not presist once it has finished

                endTimer = new DispatcherTimer();
                endTimer.Tick += endTimerTick;
                endTimer.Interval = new TimeSpan(10);
                endTimer.Start();

                // show a notification when the countdown completes
                ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();
                Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
                Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
                toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode($"\"{countdown.title}\" just finished!"));
                toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode("Hooray!"));
                Windows.Data.Xml.Dom.IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
                Windows.Data.Xml.Dom.XmlElement audio = toastXml.CreateElement("audio");
                audio.SetAttribute("src", "ms-winsoundevent:Notification.Reminder");

                ToastNotification toast = new ToastNotification(toastXml);
                ToastNotifier.Show(toast);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            countdown = e.Parameter as Countdown;

            titleTxt.Text = countdown.title;
            CDTxt.Text = countdown.displayRemaining();

            timer = new DispatcherTimer();
            timer.Tick += timerTick;
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();

            base.OnNavigatedTo(e);
        }
    }
}
