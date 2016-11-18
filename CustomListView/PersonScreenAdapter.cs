using System;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

namespace Bedtime
{
    class PeopleScreenAdapter : BaseAdapter<Alarm>
    {
        List<Alarm> items;
        Activity context;

        public PeopleScreenAdapter(Activity context, List<Alarm> items)
            : base()
        {
            this.context = context;
            this.items = items;
        }

        public override Alarm this[int position]
        {
            get
            {
                return items[position];
            }
        }

        public override int Count
        {
            get
            {
                return items.Count;
            }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
             
            if (view == null)
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.custom, null);
            }

            string alarmLabel = item.AlarmName.ToString();

            if (alarmLabel.Length > 17)
            {
                alarmLabel = alarmLabel.Substring(0, 16);
                alarmLabel += "...";
            }

            view.FindViewById<TextView>(Resource.Id.txtAlarmName).Text = alarmLabel;
            view.FindViewById<TextView>(Resource.Id.txtAlarmTime).Text = item.AlarmTime.ToString(@"hh\:mm");

            //format the alarm days from integers to days of the week
            var days = "";
            if (item.AlarmDays.Count > 0 && item.AlarmDays[0] != 0)
            {
                //days = string.Join(" ",
                //     from g in (string.Join(",", item.AlarmDays)).Split(new char[] { ',' })
                //     select Enum.GetName(typeof(DayOfWeek), ((Convert.ToInt32(g) - 1) % 7)));
                days = getDays(item.AlarmDays);
            }
            view.FindViewById<TextView>(Resource.Id.txtAlarmDays).Text = days;
            Switch alarmSwitch = view.FindViewById<Switch>(Resource.Id.alarmActive);
            alarmSwitch.Checked = item.AlarmActive;

            alarmSwitch.Click += (object sender, EventArgs e) =>
            {
                Switch sw = (Switch)sender;

                if (sw.Checked)
                {
                    ((MainActivity)context).turnAlarmOn(item.AlarmID);
                }
                else
                {
                    ((MainActivity)context).turnAlarmOff(item.AlarmID);
                }
            };
                //alarmSwitch.CheckedChange += (object sender, CompoundButton.CheckedChangeEventArgs e) =>
                //{
                //    if (e.IsChecked)
                //    {
                //        ((MainActivity)context).turnAlarmOn(item.AlarmID);
                //    }
                //    else
                //    {
                //        ((MainActivity)context).turnAlarmOff(item.AlarmID);
                //    }
                //};
                return view;
        }

        private string getDays(List<int> dayValues)
        {
            string days = "";

            foreach (var item in dayValues)
            {
                switch (item)
                {

                    case 1:
                        days += "SUN";
                        break;
                    case 2:
                        days += "MON";
                        break;
                    case 3:
                        days += "TUE";
                        break;
                    case 4:
                        days += "WED";
                        break;
                    case 5:
                        days += "THU";
                        break;
                    case 6:
                        days += "FRI";
                        break;
                    case 7:
                        days += "SAT";
                        break;
                }
                days += " ";
            }
            return days;
        }


    }
}