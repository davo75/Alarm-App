using System;
using System.Collections.Generic;


namespace Bedtime
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
        public string AlarmSound { get; set; }
    }
} 