using System;
using System.Collections.Generic;

namespace Bedtime
{
    /// <summary>
    /// Alarm class for holding details of alarms
    /// </summary>
    /// <remarks>
    /// author: David Pyle 041110777
    /// version: 1.0
    /// date: 18/11/2016
    /// </remarks
    class Alarm
    { 
        //alarmID
        public int AlarmID { get; set; }
        //Alarm name
        public string AlarmName { get; set; }
        //Alarm time
        public TimeSpan AlarmTime { get; set; }
        //Alarm active state - on or off
        public Boolean AlarmActive { get; set; }
        //Alarm reminder time
        public int AlarmReminder { get; set; }
        //Alarm ringtone
        public string AlarmSound { get; set; }
        //Days alarm will repeat on
        public List<int> AlarmDays { get; set; }
       
    }
} 