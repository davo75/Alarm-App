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
using Newtonsoft.Json;

namespace CustomListView
{
    [Activity(Label = "NextAlarm")]
    public class NextAlarm : Activity
    {
        Alarm alarmToShow;
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
            TextView remainingTime = FindViewById<TextView>(Resource.Id.txtRemainingTime);

            if (Intent.GetStringExtra("Alarm") != "No Alarms Set")
            {
                alarmToShow = JsonConvert.DeserializeObject<Alarm>(Intent.GetStringExtra("Alarm"));                
                name.Text = alarmToShow.AlarmName;               
                time.Text = (alarmToShow.AlarmTime).ToString(@"hh\:mm");
                //show countdown until next alarm

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

        private void AlarmList_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ReorderToFront);
            StartActivity(intent);
        }
    }
}