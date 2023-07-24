using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace TMinusCSharp {
    [Serializable]
    internal class CountdownInfo {
        public string title;
        public DateTimeOffset dateTime;

        public Windows.Foundation.Size windowSize;
        public Windows.Foundation.Point windowPos;

        public CountdownInfo() { }

        public CountdownInfo(Countdown countdown) {
            title = countdown.title;
            dateTime = countdown.time;

            Windows.UI.WindowManagement.AppWindowPlacement windowPlacement = countdown.window.GetPlacement();
            windowSize = windowPlacement.Size;
            windowPos = windowPlacement.Offset;
        }
    }
}
