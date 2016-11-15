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
using System.Net.NetworkInformation;
using CustomListView.au.edu.wa.central.mydesign.student;
using Android.Content.PM;
using Android.Views.InputMethods;

namespace CustomListView
{
    [Activity(Label = "Login", MainLauncher = true, WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden, ScreenOrientation = ScreenOrientation.Portrait)]
    public class Login : Activity
    {
        EditText theUsername;
        EditText thePassword;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            // Create your application here
            SetContentView(Resource.Layout.Login);

            Button login = FindViewById<Button>(Resource.Id.btnLogin);
            login.Click += login_Click;
        }
       

        private void login_Click(object sender, EventArgs e)
        {
            

            

            // get the username and password
            theUsername = FindViewById<EditText>(Resource.Id.editUsername);
            thePassword = FindViewById<EditText>(Resource.Id.editPassword);

            hideSoftKeyboard();

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                Service1 client = new Service1();
                client.CheckPasswordAsync(theUsername.Text, thePassword.Text);

                client.CheckPasswordCompleted += Client_CheckPasswordCompleted;
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

