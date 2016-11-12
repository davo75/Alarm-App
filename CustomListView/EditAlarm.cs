using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Net.NetworkInformation;
using CustomListView.au.edu.wa.central.mydesign.student;

namespace CustomListView
{
    [Activity(Label = "EditAlarm")]
    public class EditAlarm : Activity
    {

        Alarm alarmToEdit;
        EditText alarmName;
        EditText alarmTime;
        EditText alarmReminder;
        TimeSpan timeOfAlarm;
        Spinner alarmReminderSpinner;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.EditAlarm);

            alarmToEdit = JsonConvert.DeserializeObject<Alarm>(Intent.GetStringExtra("Alarm"));

            alarmName = FindViewById<EditText>(Resource.Id.txtAlarmName);
            alarmName.Text = alarmToEdit.AlarmName;

            alarmTime = FindViewById<EditText>(Resource.Id.txtAlarmTime);
            alarmTime.Text = alarmToEdit.AlarmTime.ToString();

            alarmTime.Click += AlarmTime_Click;

            alarmReminder = FindViewById<EditText>(Resource.Id.txtReminder);
            alarmReminder.Text = alarmToEdit.AlarmReminder.ToString();

            alarmReminderSpinner = FindViewById<Spinner>(Resource.Id.reminderList);
            alarmReminderSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(alarmReminderSpinner_ItemSelected);

            var adapter = ArrayAdapter.CreateFromResource(
                this, Resource.Array.reminder_array, Android.Resource.Layout.SimpleSpinnerItem);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            alarmReminderSpinner.Adapter = adapter;

            List<int> days = new List<int>();

            days = alarmToEdit.AlarmDays;

            foreach (var day in days)
            {
                switch (day)
                {
                    case 1:
                        CheckBox cbSun = (CheckBox)FindViewById(Resource.Id.cBoxSun);
                        cbSun.Checked = true;
                        break;
                    case 2:
                        CheckBox cbMon = (CheckBox)FindViewById(Resource.Id.cBoxMon);
                        cbMon.Checked = true;
                        break;
                    case 3:
                        CheckBox cbTue = (CheckBox)FindViewById(Resource.Id.cBoxTue);
                        cbTue.Checked = true;
                        break;
                    case 4:
                        CheckBox cbWed = (CheckBox)FindViewById(Resource.Id.cBoxWed);
                        cbWed.Checked = true;
                        break;
                    case 5:
                        CheckBox cbThu = (CheckBox)FindViewById(Resource.Id.cBoxThu);
                        cbThu.Checked = true;
                        break;
                    case 6:
                        CheckBox cbFri = (CheckBox)FindViewById(Resource.Id.cBoxFri);
                        cbFri.Checked = true;
                        break;
                    case 7:
                        CheckBox cbSat = (CheckBox)FindViewById(Resource.Id.cBoxSat);
                        cbSat.Checked = true;
                        break;
                }
            }

            Button saveBtn = FindViewById<Button>(Resource.Id.btnSaveAlarm);
            saveBtn.Click += SaveBtn_Click;

            Button cancelBtn = FindViewById<Button>(Resource.Id.btnCancelEditAlarm);
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

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            alarmToEdit.AlarmName = alarmName.Text;
            string[] aTime = alarmTime.Text.Split(':');
            TimeSpan aTimSpan = new TimeSpan(int.Parse(aTime[0]), int.Parse(aTime[1]), 0);
            alarmToEdit.AlarmTime = aTimSpan;
            //alarmToEdit.AlarmReminder = int.Parse(alarmReminder.Text);
            alarmToEdit.AlarmReminder = int.Parse(alarmReminderSpinner.SelectedItem.ToString());
            //turn on
            alarmToEdit.AlarmActive = true;

            List<int> days = new List<int>();
            //get days for alarm
            if (FindViewById<CheckBox>(Resource.Id.cBoxSun).Checked) days.Add(1);
            if (FindViewById<CheckBox>(Resource.Id.cBoxMon).Checked) days.Add(2);
            if (FindViewById<CheckBox>(Resource.Id.cBoxTue).Checked) days.Add(3);
            if (FindViewById<CheckBox>(Resource.Id.cBoxWed).Checked) days.Add(4);
            if (FindViewById<CheckBox>(Resource.Id.cBoxThu).Checked) days.Add(5);
            if (FindViewById<CheckBox>(Resource.Id.cBoxFri).Checked) days.Add(6);
            if (FindViewById<CheckBox>(Resource.Id.cBoxSat).Checked) days.Add(7);
            alarmToEdit.AlarmDays = days;

            if (days.Count == 0)
            {
                days.Add(0);
            }

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                Service1 client = new Service1();

                int[] daysSelected = days.ToArray();

                client.UpdateAlarmAsync(alarmToEdit.AlarmID, alarmToEdit.AlarmName, alarmToEdit.AlarmTime.ToString(), "y", alarmToEdit.AlarmReminder, daysSelected);

                client.UpdateAlarmCompleted += (object sender1, UpdateAlarmCompletedEventArgs e1) =>
                {
                    if (e1.Result == 1)
                    {
                        Intent intent = new Intent();
                        intent.PutExtra("EditedAlarm", JsonConvert.SerializeObject(alarmToEdit));
                        SetResult(Result.Ok, intent);
                        Finish();
                    } else
                    {
                        Toast.MakeText(this, "Failed to Update DB", ToastLength.Short).Show();
                    }
                };

            } 
        }

        public override void OnBackPressed()
        {
            Finish();
        }
    }
}