using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Net.NetworkInformation;
using Bedtime.au.edu.wa.central.mydesign.student;
using Android.Views.InputMethods;
using Android.Media;
using Android.Content.PM;

namespace Bedtime
{
    [Activity(Label = "EditAlarm", ScreenOrientation = ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden)]
    public class EditAlarm : Activity
    {

        Alarm alarmToEdit;
        EditText alarmName;
        EditText alarmTime;
        EditText alarmSound;
        TimeSpan timeOfAlarm;
        Spinner alarmReminderSpinner;
        Android.Net.Uri uriToRingTone;
      

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
            alarmTime.Text = alarmToEdit.AlarmTime.ToString(@"hh\:mm");

            alarmTime.Click += AlarmTime_Click;

            alarmSound = FindViewById<EditText>(Resource.Id.txtAlarmSound);

            if (alarmToEdit.AlarmSound != null && alarmToEdit.AlarmSound != "")
            {
                alarmSound.Text = RingtoneManager.GetRingtone(this, Android.Net.Uri.Parse(alarmToEdit.AlarmSound)).GetTitle(this);
            } else
            {
                uriToRingTone = null;
            }

            alarmSound.Click += AlarmSound_Click;

            alarmReminderSpinner = FindViewById<Spinner>(Resource.Id.reminderList);
            alarmReminderSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(alarmReminderSpinner_ItemSelected);

            var adapter = ArrayAdapter.CreateFromResource(
                this, Resource.Array.reminder_array, Resource.Drawable.spinner_style);

            adapter.SetDropDownViewResource(Resource.Drawable.spinner_item_style);
            alarmReminderSpinner.Adapter = adapter;

            string[] str = Resources.GetStringArray(Resource.Array.reminder_array);
            int pos = Array.IndexOf(str, alarmToEdit.AlarmReminder.ToString() + " min");
            
            alarmReminderSpinner.SetSelection(pos); 

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

        private void AlarmSound_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(RingtoneManager.ActionRingtonePicker);
            this.StartActivityForResult(intent, 3);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 3 && resultCode == Result.Ok)
            {
                uriToRingTone = (Android.Net.Uri)data.GetParcelableExtra(RingtoneManager.ExtraRingtonePickedUri);
                string alarmTitle = RingtoneManager.GetRingtone(this, uriToRingTone).GetTitle(this);
                if (uriToRingTone != null)
                {
                    // RingtoneManager.SetActualDefaultRingtoneUri(this, RingtoneManager.TYPE_ALARM, uri);
                    alarmSound.Text = alarmTitle;
                    Toast.MakeText(this, uriToRingTone.ToString(), ToastLength.Short).Show();
                }
            }
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
            TimePickerDialog tpd = new TimePickerDialog(this, tdpCallback, DateTime.Now.Hour, DateTime.Now.Minute, true);
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
            string reminderTime = alarmReminderSpinner.SelectedItem.ToString();
            alarmToEdit.AlarmReminder = int.Parse(reminderTime.Substring(0, reminderTime.Length - 4));
            //turn on
            alarmToEdit.AlarmActive = true;

            string alarmSound = "";
            if (uriToRingTone != null)
            {
                alarmToEdit.AlarmSound = uriToRingTone.ToString();
                alarmSound = uriToRingTone.ToString();
            }


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
                

                client.UpdateAlarmAsync(alarmToEdit.AlarmID, alarmToEdit.AlarmName, alarmToEdit.AlarmTime.ToString(), "y", alarmToEdit.AlarmReminder, alarmSound, daysSelected);

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