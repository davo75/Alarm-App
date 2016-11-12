using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CustomListView
{
    class Alarm
    { 
        public int AlarmID { get; set; }
        public string AlarmName { get; set; }
        public TimeSpan AlarmTime { get; set; }
        public Boolean AlarmActive { get; set; }
        public int AlarmReminder { get; set; }
        public int ReminderState { get; set; }
        public List<int> AlarmDays { get; set; }
    }
} 