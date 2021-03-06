using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Android.Content.PM;
using Java.Util;

namespace Bedtime
{
    /// <summary>
    ///Screen that shows the next alarm that is due.
    /// </summary>
    /// <remarks>
    /// author: David Pyle 041110777
    /// version: 1.0
    /// date: 18/11/2016
    /// </remarks>
    
    [Activity(Label = "NextAlarm", ScreenOrientation = ScreenOrientation.Portrait)]
    public class NextAlarm : Activity
    {
        //next due alarm
        private Alarm alarmToShow;
        //timer for time remaining
        private System.Timers.Timer alarmTimer;
        //text for time remaining until alarm sounds
        private TextView remainingTime;
        //duration until next alarm
        private TimeSpan duration;
        //days from now until alarm
        private int daysFromNow;
       
        /// <summary>
        /// Sets up main UI for showing next alarm info.
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.NextAlarm);

            //display the toolbar
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            ActionBar.Title = "Next Alarm";

            //get references to the alarm name, alarm time, time remaining fields
            TextView name = FindViewById<TextView>(Resource.Id.txtAlarmName);
            TextView time = FindViewById<TextView>(Resource.Id.txtAlarmTime);
            TextView txtRemaining = FindViewById<TextView>(Resource.Id.txtRemaining);
            remainingTime = FindViewById<TextView>(Resource.Id.txtRemainingTime);

            //if alarms have been set then display the alarm info
            if (Intent.GetStringExtra("Alarm") != "No Alarms Set")
            {
                //get the alarm passed in
                alarmToShow = JsonConvert.DeserializeObject<Alarm>(Intent.GetStringExtra("Alarm"));
                daysFromNow = Intent.GetIntExtra("DaysFromNow", -1);
                
                //set the text views      
                name.Text = alarmToShow.AlarmName;               
                time.Text = (alarmToShow.AlarmTime).ToString(@"hh\:mm");
                //set up the timer
                setUpTimer();
                

            } else //no alarms are on so just display a msg
            {
                time.Text = "No Alarms Set";
                name.Visibility = ViewStates.Invisible;
                txtRemaining.Visibility = ViewStates.Invisible;
                remainingTime.Visibility = ViewStates.Invisible;

            }

            //button for navigating to  alarm list screen
            ImageButton alarmList = FindViewById<ImageButton>(Resource.Id.btnAlarmList);
            alarmList.Click += AlarmList_Click;
        }

        /// <summary>
        /// Timer to countdown remaining time
        /// </summary>
        private void setUpTimer()
        {
            alarmTimer = new System.Timers.Timer();
            //Trigger event every second
            alarmTimer.Interval = 1000;
            alarmTimer.Elapsed += OnTimedEvent;

            alarmTimer.Enabled = true;
        }

        /// <summary>
        /// Calculates the time remaining until the alarm is triggered
        /// </summary>
        private void getTimeRemaning()
        {           
            //show countdown until next alarm
            Calendar now = Calendar.GetInstance(Java.Util.TimeZone.Default);
            //get a calendar instance for today and set the required alarm hour and minute
            Calendar alarm = Calendar.GetInstance(Java.Util.TimeZone.Default);
            //set the alarm time
            alarm.Set(CalendarField.HourOfDay, alarmToShow.AlarmTime.Hours);
            alarm.Set(CalendarField.Minute, alarmToShow.AlarmTime.Minutes);
            alarm.Set(CalendarField.Second, 0);

            //calculate the alarm time
            long alarmMillis = alarm.TimeInMillis + (86400000L * daysFromNow);
            if (alarm.Before(now) && daysFromNow == 0 && alarmToShow.AlarmDays[0] == 0)
            {
                alarmMillis += 86400000L;
            }
            //if alarm days have been set and the alarm time is before now and the days are 0 then it must be a week from now
            else if (alarm.Before(now) && daysFromNow == 0 && alarmToShow.AlarmDays.Count > 0)
            {
                alarmMillis += 7 * 86400000L;
            }

            //show the time remaining
            duration = TimeSpan.FromMilliseconds(alarmMillis - now.TimeInMillis);
            remainingTime.Text = duration.Days + "d " + duration.Hours + "h " + duration.Minutes + "m " + duration.Seconds + "s";
        }

        /// <summary>
        /// Updates the time remaining field every second
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {    
            //Update visual representation here
            RunOnUiThread(() => {
                getTimeRemaning();
            });           
        }

        /// <summary>
        /// Navigates back to main alarm screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlarmList_Click(object sender, EventArgs e)
        {
            //go to main alarm screen   
            Intent intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ReorderToFront);
            StartActivity(intent);
        }

        /// <summary>
        /// Restart the timer when the activity is resumed
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();
            if (alarmTimer != null)
            {
                alarmTimer.Start();
            }
        }

        /// <summary>
        /// Stop the timer when the activity is paused
        /// </summary>
        protected override void OnPause()
        {
            base.OnPause();
            if (alarmTimer != null)
            {
                alarmTimer.Stop();
            }
        }

    }
}