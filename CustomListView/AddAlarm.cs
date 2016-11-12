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
using System.Net.NetworkInformation;
using CustomListView.au.edu.wa.central.mydesign.student;

namespace CustomListView
{
    [Activity(Label = "AddAlarm")]
    public class AddAlarm : Activity
    {
        EditText alarmName;
        EditText alarmTime;
        EditText alarmReminder;
        TimeSpan timeOfAlarm;
        Spinner alarmReminderSpinner;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //this.RequestWindowFeature(WindowFeatures.NoTitle);
            //this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.AddAlarm);

            alarmName = FindViewById<EditText>(Resource.Id.txtAlarmName);
            alarmTime = FindViewById<EditText>(Resource.Id.txtAlarmTime);
            alarmReminder = FindViewById<EditText>(Resource.Id.txtReminder);

            alarmReminderSpinner = FindViewById<Spinner>(Resource.Id.reminderList);
            alarmReminderSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(alarmReminderSpinner_ItemSelected);

            var adapter = ArrayAdapter.CreateFromResource(
                this, Resource.Array.reminder_array, Android.Resource.Layout.SimpleSpinnerItem);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            alarmReminderSpinner.Adapter = adapter;


            alarmTime.Click += AlarmTime_Click;

            Button saveBtn = FindViewById<Button>(Resource.Id.btnSaveAlarm);
            saveBtn.Click += SaveBtn_Click;

            Button cancelBtn = FindViewById<Button>(Resource.Id.btnCancelAddAlarm);
            cancelBtn.Click += (object sender, EventArgs e) =>
            {
                Finish();
            };
        }

        private void alarmReminderSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            string toast = string.Format("The reminder is {0}", spinner.GetItemAtPosition(e.Position));
            toast = spinner.SelectedItem.ToString();
            Toast.MakeText(this, toast, ToastLength.Long).Show();
        }

        private void AlarmTime_Click(object sender, EventArgs e)
        {
            TimePickerDialog tpd = new TimePickerDialog(this, tdpCallback, DateTime.Now.Hour, DateTime.Now.Minute, false);
            tpd.Show();
        }

        private void tdpCallback(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            //Toast.MakeText(this, string.Format("{0}:{1}", e.HourOfDay, e.Minute.ToString().PadLeft(2, '0')), ToastLength.Short).Show();
            //alarmTime.Text = string.Format("{0}:{1}", e.HourOfDay, e.Minute.ToString().PadLeft(2, '0'));
            timeOfAlarm = new TimeSpan(e.HourOfDay, e.Minute, 0);
            alarmTime.Text = string.Format("{0}:{1}", timeOfAlarm.Hours, timeOfAlarm.Minutes.ToString().PadLeft(2, '0'));
            Toast.MakeText(this, timeOfAlarm.ToString(), ToastLength.Short).Show();
        }

        public override void OnBackPressed()
        {
            Finish();
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            List<int> days = new List<int>();
            //get days for alarm
            if (FindViewById<CheckBox>(Resource.Id.cBoxSun).Checked) days.Add(1);
            if (FindViewById<CheckBox>(Resource.Id.cBoxMon).Checked) days.Add(2);
            if (FindViewById<CheckBox>(Resource.Id.cBoxTue).Checked) days.Add(3);
            if (FindViewById<CheckBox>(Resource.Id.cBoxWed).Checked) days.Add(4);
            if (FindViewById<CheckBox>(Resource.Id.cBoxThu).Checked) days.Add(5);
            if (FindViewById<CheckBox>(Resource.Id.cBoxFri).Checked) days.Add(6);
            if (FindViewById<CheckBox>(Resource.Id.cBoxSat).Checked) days.Add(7);

            if (days.Count == 0)
            {
                days.Add(0);
            }

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                Service1 client = new Service1();

                int[] daysSelected = days.ToArray();

                client.AddNewAlarmAsync("dave", alarmName.Text, timeOfAlarm.ToString(), "y", int.Parse(alarmReminderSpinner.SelectedItem.ToString()), daysSelected);

                client.AddNewAlarmCompleted += (object sender1, AddNewAlarmCompletedEventArgs e1) =>
                {
                    //make a new alarm object           
                    Alarm alarm = new Alarm
                    {
                        AlarmID = e1.Result,
                        AlarmName = alarmName.Text,
                        AlarmTime = timeOfAlarm,
                        AlarmActive = true,
                        AlarmReminder = int.Parse(alarmReminderSpinner.SelectedItem.ToString()),
                        AlarmDays = days
                    };

                    //pass the intent the alarm object via JSON

                    Intent intent = new Intent();
                    intent.PutExtra("NewAlarm", JsonConvert.SerializeObject(alarm));
                    SetResult(Result.Ok, intent);
                    Finish();
                };

            }

            
        }

       
    }
}