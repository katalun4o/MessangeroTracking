
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
	public class fragmentSettings : Xamarin.ActionbarSherlockBinding.App.SherlockFragment
	{
		TextView tbLanguage;
		TextView tbUser;
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View v = inflater.Inflate (Resource.Layout.fragment_settings, container, false);
			HasOptionsMenu = true;

			((MainActivity)Activity).SetActionbarTitle (Activity.GetString(Resource.String.Settings));

			Button btnLanguage = v.FindViewById<Button> (Resource.Id.btnLanguage);
			Button btnChangeLanguage  = v.FindViewById<Button> (Resource.Id.btnChangePassword);
			Button btnLogout = v.FindViewById<Button> (Resource.Id.btnLogout);

			tbLanguage = v.FindViewById<TextView> (Resource.Id.tbLanguage);
			tbLanguage.Text = PreferencesUtil.GetCurrentLanguage ().Language;

			tbUser = v.FindViewById<TextView> (Resource.Id.tbUser);
			tbUser.Text = PreferencesUtil.Mail;

			btnLanguage.Click += (object sender, EventArgs e) => {
				ShowLanguageDialog();
			};

			btnChangeLanguage.Click += (object sender, EventArgs e) => {
				//start change password dialog
				ShowChangePasswordDialog();
			};

			btnLogout.Click += (object sender, EventArgs e) => {
				((MainActivity)Activity).Logout();
			};

			return v;
		}

		private void ShowLanguageDialog()
		{
			Dialog dialogLanguage = new Dialog (this.Activity,Resource.Style.TitleDialog);
			dialogLanguage.SetContentView (Resource.Layout.dialog_languages);
			dialogLanguage.SetTitle(Activity.GetString(Resource.String.Language));
			dialogLanguage.Window.SetLayout(LinearLayout.LayoutParams.MatchParent,LinearLayout.LayoutParams.WrapContent);
			ListView lvLanguages = dialogLanguage.FindViewById<ListView> (Resource.Id.lvLanguages);

			List<LanguageInfo> langList = PreferencesUtil.LanguagesList;
			/*List<LanguageInfo> langList = new List<LanguageInfo> ();
			langList.Add (new LanguageInfo (){ ID = 1, Language = "English" });
			langList.Add (new LanguageInfo (){ ID = 2, Language = "Greek" });
			langList.Add (new LanguageInfo (){ ID = 3, Language = "Bulgarian" });*/
			adapterLanguages languagesAdapter = new adapterLanguages (Activity,langList);
			lvLanguages.Adapter = languagesAdapter;

			lvLanguages.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs> ((o, e) => {
				var item = langList[e.Position];
				if(item == null)
					return;
				PreferencesUtil.LanguageID = ((LanguageInfo)item).ID;
				PreferencesUtil.SavePreferences(Activity);
				dialogLanguage.Dismiss();

				tbLanguage.Text = ((LanguageInfo)item).Language;
				PreferencesUtil.UpdateCurrentCulture(this.Activity);
			});

			dialogLanguage.Show ();
		}

		private void ShowChangePasswordDialog()
		{
			Dialog dialogChangePass = new Dialog (this.Activity,Resource.Style.TitleDialog);
			dialogChangePass.SetContentView (Resource.Layout.dialog_change_password);
			dialogChangePass.SetTitle(GetString(Resource.String.ChangePassword));
			dialogChangePass.Window.SetLayout(LinearLayout.LayoutParams.MatchParent,LinearLayout.LayoutParams.WrapContent);
			EditText tbOldPassword = dialogChangePass.FindViewById<EditText> (Resource.Id.tbOldPassword);
			EditText tbNewPassword = dialogChangePass.FindViewById<EditText> (Resource.Id.tbNewPassword);
			EditText tbRetypeNewPassword = dialogChangePass.FindViewById<EditText> (Resource.Id.tbRepeatNewPassword);
			Button btnOK = dialogChangePass.FindViewById<Button> (Resource.Id.btnOK);
			Button btnCancel = dialogChangePass.FindViewById<Button> (Resource.Id.btnCancel);

			btnOK.Click += (object sender, EventArgs e) => {

				string oldPass = tbOldPassword.Text.ToString();
				string newPass = tbNewPassword.Text.ToString();
				string newPassRetype = tbRetypeNewPassword.Text.ToString();

				if(string.IsNullOrEmpty(oldPass) || string.IsNullOrEmpty(newPass) || string.IsNullOrEmpty(newPassRetype))
				{
					Toast.MakeText(Activity,GetString(Resource.String.msgAllFieldsRequired),ToastLength.Short).Show();
					return;
				}

				if(newPass != newPassRetype)
				{
					Toast.MakeText(Activity,GetString(Resource.String.msgPasswordsDifferent),ToastLength.Short).Show();
					return;
				}

				WebServiceHelper.ChangePassword (this.Activity, PreferencesUtil.Mail, tbOldPassword.Text.ToString(), tbNewPassword.Text.ToString(), (errorID) => {
					//toast email sent
					if(errorID == 0)
					{
						Toast.MakeText(Activity,GetString(Resource.String.msgPasswordChanged),ToastLength.Short).Show();
					}
					else
						if(errorID == 4)
					{
							Toast.MakeText(Activity,GetString(Resource.String.msgOldPasswordNotCorrect),ToastLength.Short).Show();
					}
					else
						{
							Toast.MakeText(Activity,GetString(Resource.String.msgProblemChangePassword),ToastLength.Short).Show();
						}
				});

				dialogChangePass.Dismiss();
			};
			btnCancel.Click += (object sender, EventArgs e) => {dialogChangePass.Dismiss();};

			dialogChangePass.Show ();
		}
	}
}

