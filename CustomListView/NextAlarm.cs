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
    [Activity(Label = "NextAlarm", ScreenOrientation = ScreenOrientation.Portrait)]
    public class NextAlarm : Activity
    {
        Alarm alarmToShow;
        private System.Timers.Timer alarmTimer;
        
        TextView remainingTime;
        TimeSpan duration;
        int daysFromNow;
       

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.NextAlarm);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            ActionBar.Title = "Next Alarm";

            TextView name = FindViewById<TextView>(Resource.Id.txtAlarmName);
            TextView time = FindViewById<TextView>(Resource.Id.txtAlarmTime);
            TextView txtRemaining = FindViewById<TextView>(Resource.Id.txtRemaining);
            remainingTime = FindViewById<TextView>(Resource.Id.txtRemainingTime);

            



            if (Intent.GetStringExtra("Alarm") != "No Alarms Set")
            {
                alarmToShow = JsonConvert.DeserializeObject<Alarm>(Intent.GetStringExtra("Alarm"));
                daysFromNow = Intent.GetIntExtra("DaysFromNow", -1);
                              
                name.Text = alarmToShow.AlarmName;               
                time.Text = (alarmToShow.AlarmTime).ToString(@"hh\:mm");

                setUpTimer();
                

            } else
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

        private void setUpTimer()
        {
            alarmTimer = new System.Timers.Timer();
            //Trigger event every second
            alarmTimer.Interval = 1000;
            alarmTimer.Elapsed += OnTimedEvent;

            alarmTimer.Enabled = true;
        }

        private void getTimeRemaning()
        {
            Console.WriteLine("In countdown");
            //show countdown until next alarm
            Calendar now = Calendar.GetInstance(Java.Util.TimeZone.Default);
            //get a calendar instance for today and set the required alarm hour and minute
            Calendar alarm = Calendar.GetInstance(Java.Util.TimeZone.Default);

            alarm.Set(CalendarField.HourOfDay, alarmToShow.AlarmTime.Hours);
            alarm.Set(CalendarField.Minute, alarmToShow.AlarmTime.Minutes);
            alarm.Set(CalendarField.Second, 0);

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
            duration = TimeSpan.FromMilliseconds(alarmMillis - now.TimeInMillis);
            remainingTime.Text = duration.Days + "d " + duration.Hours + "h " + duration.Minutes + "m " + duration.Seconds + "s";
        }

        private void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
           

            //Update visual representation here
            //Remember to do it on UI thread
            RunOnUiThread(() => {
                getTimeRemaning();

            });

            
        }

        private void AlarmList_Click(object sender, EventArgs e)
        {
            
            Intent intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ReorderToFront);
            StartActivity(intent);
        }

        protected override void OnResume()
        {
            base.OnResume();
            Toast.MakeText(this, "Next Alrm Resuming..", ToastLength.Short).Show();
            if (alarmTimer != null)
            {
                alarmTimer.Start();
            }
        }

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