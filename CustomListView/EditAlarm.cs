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
using Android.Views.InputMethods;

namespace CustomListView
{
    [Activity(Label = "EditAlarm", WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden)]
    public class EditAlarm : Activity
    {

        Alarm alarmToEdit;
        EditText alarmName;
        EditText alarmTime;
       
        TimeSpan timeOfAlarm;
        Spinner alarmReminderSpinner;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.EditAlarm2);

            //add the toolbar
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            ActionBar.Title = "Edit Alarm";


            alarmToEdit = JsonConvert.DeserializeObject<Alarm>(Intent.GetStringExtra("Alarm"));

            alarmName = FindViewById<EditText>(Resource.Id.txtAlarmName);
            alarmName.Text = alarmToEdit.AlarmName;

            alarmTime = FindViewById<EditText>(Resource.Id.txtAlarmTime);
            alarmTime.Text = alarmToEdit.AlarmTime.ToString();

            alarmTime.Click += AlarmTime_Click;

            

            alarmReminderSpinner = FindViewById<Spinner>(Resource.Id.reminderList);
            alarmReminderSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(alarmReminderSpinner_ItemSelected);

            var adapter = ArrayAdapter.CreateFromResource(
                this, Resource.Array.reminder_array, Resource.Layout.spinner_layout);

            //adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            alarmReminderSpinner.Adapter = adapter;

            //var adapter = ArrayAdapter.CreateFromResource(
               // this, Resource.Array.reminder_array, Android.Resource.Layout.SimpleSpinnerItem);

           // adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
           // alarmReminderSpinner.Adapter = adapter;


            alarmReminderSpinner.SetSelection(2); // alarmToEdit.AlarmReminder.ToString();

            List<int> days = new List<int>();

            days = alarmToEdit.AlarmDays;

            foreach (var day in days)
            {
                switch (day)
                {
                    case 1:
                        ToggleButton cbSun = (ToggleButton)FindViewById(Resource.Id.cBoxSun);
                        cbSun.Checked = true;
                        break;
                    case 2:
                        ToggleButton cbMon = (ToggleButton)FindViewById(Resource.Id.cBoxMon);
                        cbMon.Checked = true;
                        break;
                    case 3:
                        ToggleButton cbTue = (ToggleButton)FindViewById(Resource.Id.cBoxTue);
                        cbTue.Checked = true;
                        break;
                    case 4:
                        ToggleButton cbWed = (ToggleButton)FindViewById(Resource.Id.cBoxWed);
                        cbWed.Checked = true;
                        break;
                    case 5:
                        ToggleButton cbThu = (ToggleButton)FindViewById(Resource.Id.cBoxThu);
                        cbThu.Checked = true;
                        break;
                    case 6:
                        ToggleButton cbFri = (ToggleButton)FindViewById(Resource.Id.cBoxFri);
                        cbFri.Checked = true;
                        break;
                    case 7:
                        ToggleButton cbSat = (ToggleButton)FindViewById(Resource.Id.cBoxSat);
                        cbSat.Checked = true;
                        break;
                }
            }

            ImageButton saveBtn = FindViewById<ImageButton>(Resource.Id.btnSaveAlarm);
            saveBtn.Click += SaveBtn_Click;

            ImageButton cancelBtn = FindViewById<ImageButton>(Resource.Id.btnCancelEditAlarm);
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
            hideSoftKeyboard();
            TimePickerDialog tpd = new TimePickerDialog(this, tdpCallback, DateTime.Now.Hour, DateTime.Now.Minute, false);
            tpd.Show();
        }

        private void hideSoftKeyboard()
        {
            InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(alarmName.WindowToken, 0);
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
            if (FindViewById<ToggleButton>(Resource.Id.cBoxSun).Checked) days.Add(1);
            if (FindViewById<ToggleButton>(Resource.Id.cBoxMon).Checked) days.Add(2);
            if (FindViewById<ToggleButton>(Resource.Id.cBoxTue).Checked) days.Add(3);
            if (FindViewById<ToggleButton>(Resource.Id.cBoxWed).Checked) days.Add(4);
            if (FindViewById<ToggleButton>(Resource.Id.cBoxThu).Checked) days.Add(5);
            if (FindViewById<ToggleButton>(Resource.Id.cBoxFri).Checked) days.Add(6);
            if (FindViewById<ToggleButton>(Resource.Id.cBoxSat).Checked) days.Add(7);
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