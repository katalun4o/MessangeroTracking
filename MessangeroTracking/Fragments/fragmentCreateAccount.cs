using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace MessangeroTracking
{
	public class fragmentCreateAccount : Android.Support.V4.App.Fragment
	{
		EditText etUsername;
		EditText etPassword;
		EditText etPassword2;
		Button btnRegister;
		string soc_net_id;
		string soc_user_id;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View v = inflater.Inflate (Resource.Layout.fragment_register, container, false);

			string mail = this.Arguments.GetString ("Mail");
			soc_net_id = this.Arguments.GetString ("soc_net_id");
			soc_user_id = this.Arguments.GetString ("soc_user_id");

			etUsername = v.FindViewById<EditText> (Resource.Id.etUsername);
			etPassword = v.FindViewById<EditText> (Resource.Id.etPasswrod);
			etPassword2 = v.FindViewById<EditText> (Resource.Id.etPasswrod2);
			btnRegister = v.FindViewById<Button> (Resource.Id.btnRegister);
			etUsername.Text = mail;

			btnRegister.Click += btnRegister_Click;

			return v;
		}

		private void btnRegister_Click(object sender, EventArgs e)
		{
			string email = etUsername.Text.ToString();
			string pass1 = etPassword.Text.ToString();
			string pass2 = etPassword2.Text.ToString();

			if (string.IsNullOrEmpty (email) || string.IsNullOrEmpty (pass1) || string.IsNullOrEmpty (pass2))
			{
				Toast.MakeText (this.Activity, this.Activity.GetString(Resource.String.msgEmptyFields), ToastLength.Short).Show();
				return;
			}


			if (pass1 != pass2) {
				Toast.MakeText (this.Activity, this.Activity.GetString(Resource.String.msgPasswordsDifferent), ToastLength.Short).Show();
				return;
			}

			WebServiceHelper.RegisterUser (email, pass1, this.Activity, (user)=>{
				if(user == null)
				{
					return;
				}
				if(user.UserID > 0)
				{
					PreferencesUtil.UserID = user.UserID;
					PreferencesUtil.Mail = user.Email;
					PreferencesUtil.Password = pass1;
					PreferencesUtil.SavePreferences(this.Activity);
					((MainActivity)this.Activity).StartMainMenu();
				}
				else
				{
					//show toast - already exsit username
					Toast.MakeText (this.Activity, this.Activity.GetString(Resource.String.msgEmailAlreadyExists), ToastLength.Short).Show();
				}
			});
		}
	}
}

