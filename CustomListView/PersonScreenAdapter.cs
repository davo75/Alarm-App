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

            view.FindViewById<TextView>(Resource.Id.Text1).Text = string.Format("{0}", item.AlarmName);
            view.FindViewById<TextView>(Resource.Id.Text2).Text = string.Format("{0}", item.AlarmTime);

            //format the alarm days from integers to days of the week
            var days = "";
            if (item.AlarmDays.Count > 0 && item.AlarmDays[0] != 0)
            {
                days = string.Join(" ",
                     from g in (string.Join(",", item.AlarmDays)).Split(new char[] { ',' })
                     select Enum.GetName(typeof(DayOfWeek), ((Convert.ToInt32(g) - 1) % 7)));
            }
            view.FindViewById<TextView>(Resource.Id.Text3).Text = string.Format("{0}", days);
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

       
    }
}