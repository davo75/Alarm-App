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
    [Activity(Label = "NextAlarm")]
    public class NextAlarm : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.NextAlarm);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            ActionBar.Title = "Next Alarm";

            //button for navigating to  alarm list screen
            ImageButton alarmList = FindViewById<ImageButton>(Resource.Id.bt);
            alarmList.Click += AlarmList_Click;
        }

        private void AlarmList_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(MainActivity));
            StartActivity(intent);
        }
    }
}