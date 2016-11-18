using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Net.NetworkInformation;
using Bedtime.au.edu.wa.central.mydesign.student;
using Android.Content.PM;
using Android.Views.InputMethods;
using Android.Graphics;

namespace Bedtime
{
    [Activity(Label = "Bedtime Alarm", MainLauncher = true, Icon = "@drawable/ic_launcher", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden, ScreenOrientation = ScreenOrientation.Portrait)]
    public class Login : Activity
    {
        EditText theUsername;
        EditText thePassword;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            // Create your application here
            SetContentView(Resource.Layout.Login);

            Typeface font = Typeface.CreateFromAsset(Assets, "fonts/Montserrat-Bold.ttf");

            Button login = FindViewById<Button>(Resource.Id.btnLogin);
            login.Typeface = font;
            login.Click += login_Click;

            TextView create = FindViewById<TextView>(Resource.Id.txtCreateLink);
            create.Typeface = font;
            create.Click += delegate {
                Intent intent = new Intent(this, typeof(CreateUser));
                StartActivity(intent);
            };

                TextView help = FindViewById<TextView>(Resource.Id.txtHelpLink);
            help.Typeface = font;
            help.Click += delegate {
                var uri = Android.Net.Uri.Parse("http://student.mydesign.central.wa.edu.au/041110777/bedtime/");
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
            };
        }

        

        private void login_Click(object sender, EventArgs e)
        {
            
            // get the username and password
            theUsername = FindViewById<EditText>(Resource.Id.editUsername);
            thePassword = FindViewById<EditText>(Resource.Id.editPassword);

            hideSoftKeyboard();

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                Service1 client = new au.edu.wa.central.mydesign.student.Service1();
                client.CheckPasswordAsync(theUsername.Text, thePassword.Text);

                client.CheckPasswordCompleted += Client_CheckPasswordCompleted;
            } else
            {
                // display toast error message
                Toast.MakeText(this, "No internet connection", ToastLength.Long).Show();
            }

        }

        private void Client_CheckPasswordCompleted(object sender, CheckPasswordCompletedEventArgs e)
        {
            if (e.Result == 1)
            {
                //display toast welcome message
                Toast.MakeText(this, "Welcome " + theUsername.Text, ToastLength.Long).Show();

                //if ok show the next screen
                var viewIntent = new Intent(this, typeof(MainActivity));
                viewIntent.PutExtra("Username", theUsername.Text);
                StartActivity(viewIntent);
                Finish();
            }
            else
            {
                //display toast error message
                Toast.MakeText(this, "Please try again..", ToastLength.Long).Show();
            }
        }

        private void hideSoftKeyboard()
        {
            InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(thePassword.WindowToken, 0);
        }
    }
}

