using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Facebook;
using Xamarin.ActionbarSherlockBinding.App;
using Xamarin.ActionbarSherlockBinding.Views;
using Com.Jeremyfeinstein.Slidingmenu.Lib;
using IMenuItem = Xamarin.ActionbarSherlockBinding.Views.IMenuItem;
//using Microsoft.WindowsAzure.MobileServices;

namespace MessangeroTracking
{

	[Activity (Label = "Messangero Tracking", MainLauncher = true,Theme = "@style/Theme.Sherlock.Light.DarkActionBar", Icon = "@drawable/ic_launcher")]
	public class MainActivity : SherlockFragmentActivity
	{

		//public static MobileServiceClient MobileService = new MobileServiceClient( "https://mtracking.azure-mobile.net/", "SqvpUfSlpXcUNPtzVoZHuNXGPFYLCB36" );
		public static int MainFragmentTransactionID = 0;
		public static bool IsFirstTime = true;
		RelativeLayout mainLayout;
		Com.Jeremyfeinstein.Slidingmenu.Lib.SlidingMenu menu;
		fragmentSlideMenu fragmentMenu1;
		TextView tvActionBarTitle;

		ProgressDialog progressDialog;

		public void ShowProgressDialog(string title, string message)
		{
			progressDialog = ProgressDialog.Show(this, title, message, true);
		}

		public void HideProgressDialog()
		{
			if (progressDialog != null)
				progressDialog.Dismiss ();
		}

		protected override void OnSaveInstanceState (Bundle outState)
		{
			//base.OnSaveInstanceState (outState);
		}

		protected override void OnCreate (Bundle bundle)
		{
			SetTheme(Resource.Style.Theme_Sherlock_Light);
			//RequestWindowFeature (Xamarin.ActionbarSherlockBinding.Views.Window.FeatureIndeterminateProgress);
			base.OnCreate (bundle);

			Tracking.CrashRep.CrashReporting.StartWithMonoHook (this, true);

			PreferencesUtil.LoadSettings(this);
			PreferencesUtil.UpdateCurrentCulture(this);

 			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			ShowLoadingFragment();

			SupportActionBar.SetDisplayShowTitleEnabled(false);
			SupportActionBar.SetDisplayShowCustomEnabled (true);
			SupportActionBar.SetCustomView (Resource.Layout.actionbar_title);
			tvActionBarTitle = SupportActionBar.CustomView.FindViewById<TextView> (Resource.Id.action_custom_title);

			DBManager.CreateDatabase();

			mainLayout = FindViewById<RelativeLayout> (Resource.Id.mainLayout);


			if (PreferencesUtil.Password == "") {
				if (PreferencesUtil.Mail != "" && PreferencesUtil.SocID != "" && PreferencesUtil.SocUserID != "")
				{
					this.Authenticate (PreferencesUtil.Mail,PreferencesUtil.SocID, PreferencesUtil.SocUserID);
					/*WebServiceHelper.SocialLogin (PreferencesUtil.Mail, PreferencesUtil.SocID, PreferencesUtil.SocUserID, this, new Action (() => {
						if(PreferencesUtil.UserID != 0)
						{
							StartMainMenu ();
						}
						else{
							//show login failure message
							StartLogin();
						}
					}));*/
				}
				else
					StartLogin();
			} else {

				WebServiceHelper.LoginUser (PreferencesUtil.Mail, PreferencesUtil.Password, this, new Action (() => {
					//do login
					if (PreferencesUtil.UserID == 0) {
						StartLogin();
					} else {
						StartMainMenu ();
					}
				}));
			}


			//SetProgressBarIndeterminateVisibility (false);
		}

		private void ShowLoadingFragment()
		{
			fragmentLoading fLoading = new fragmentLoading ();
			var ft = SupportFragmentManager.BeginTransaction ();
			ft.Replace (Resource.Id.mainLayout, fLoading);
			ft.SetTransition (Android.Support.V4.App.FragmentTransaction.TransitFragmentFade);
			ft.Commit();
		}

		public void StartLogin()
		{
			fragmentLogin fLogin = new fragmentLogin ();
			var ft = SupportFragmentManager.BeginTransaction ();
			ft.Replace (Resource.Id.mainLayout, fLogin);
			ft.SetTransition (Android.Support.V4.App.FragmentTransaction.TransitFragmentFade);
			ft.AddToBackStack("Login");
			ft.CommitAllowingStateLoss ();
		}

		public void StartRegistration()
		{
			StartRegistration ("","","");
		}

		public void StartRegistration(string mail, string soc_net_id, string soc_user_id)
		{
			fragmentCreateAccount fRegistration = new fragmentCreateAccount ();
			fRegistration.Arguments = new Bundle();
			fRegistration.Arguments.PutString ("Mail", mail);
			fRegistration.Arguments.PutString ("soc_net_id", soc_net_id);
			fRegistration.Arguments.PutString ("soc_user_id", soc_user_id);
			var ft = SupportFragmentManager.BeginTransaction ();
			ft.Replace (Resource.Id.mainLayout, fRegistration);
			ft.AddToBackStack("Register");
			ft.SetTransition (Android.Support.V4.App.FragmentTransaction.TransitFragmentFade);
			ft.CommitAllowingStateLoss ();
		}

		public void StartMainMenu()
		{
			RegisterGCMService();

			fragmentMainMenu fMainMenu = new fragmentMainMenu();

			if (this.Intent != null && this.Intent.HasExtra ("Numbers")) 
			{
				IsFirstTime = true;
				string numbers = this.Intent.GetStringExtra ("Numbers");
				//refresh these vouchers
				fMainMenu.Arguments = new Bundle();
				fMainMenu.Arguments.PutString ("Numbers",numbers);
			}

			var ft = SupportFragmentManager.BeginTransaction();
			ft.Replace(Resource.Id.mainLayout, fMainMenu);
			ft.SetTransition(Android.Support.V4.App.FragmentTransaction.TransitFragmentFade);
			ft.AddToBackStack("Main");
			MainFragmentTransactionID = ft.Commit();

			InitSlideMenu();

		}

		private void RegisterGCMService()
		{
			//530675710628
			//62105108905
			string senders = "62105108905";
			Intent intent = new Intent("com.google.android.c2dm.intent.REGISTER");
			intent.SetPackage("com.google.android.gsf");
			intent.PutExtra("app", PendingIntent.GetBroadcast(this, 0, new Intent(), 0));
			intent.PutExtra("sender", senders);
			this.StartService(intent);
		}

		private void InitSlideMenu()
		{
			menu = new Com.Jeremyfeinstein.Slidingmenu.Lib.SlidingMenu(this);
			menu.Mode = 1;
			menu.TouchModeAbove = SlidingMenu.TouchmodeNone;
			menu.SetShadowWidthRes(Resource.Dimension.shadow_width);
			menu.SetShadowDrawable(Resource.Drawable.Shadow);
			menu.SetBehindOffsetRes(Resource.Dimension.slidingmenu_offset);
			menu.SetFadeDegree(0.35f);
			menu.AttachToActivity(this, Com.Jeremyfeinstein.Slidingmenu.Lib.SlidingMenu.SlidingContent);
			menu.SetMenu(Resource.Layout.slide_menu_content);

			fragmentMenu1 = new fragmentSlideMenu();
			fragmentMenu1.ProviderSelected += new fragmentSlideMenu.ProviderSelectedDelegate (fragmentMenu1_ProviderSelected);
			var ft1 = SupportFragmentManager.BeginTransaction();
			ft1.Replace(Resource.Id.content, fragmentMenu1);
			ft1.SetTransition(Android.Support.V4.App.FragmentTransaction.TransitFragmentFade);
			ft1.Commit();
			//content
			WebServiceHelper.GetProvidersAsync (this, (o) => {
				//save providers in database
				fragmentMenu1.ReloadProviders();
			});


		}

		public void SetActionbarTitle(string title)
		{
			SetActionbarTitle (title, "", "");
		}

		public void SetActionbarTitle(string title, string color, string logo)
		{
			tvActionBarTitle.Text = title;
			if (string.IsNullOrEmpty (logo))
				SupportActionBar.SetIcon (Resource.Drawable.ic_launcher);
			else {
				SupportActionBar.SetIcon (Android.Graphics.Drawables.Drawable.CreateFromPath (logo));
			}

			if (string.IsNullOrEmpty (color)) 
			{
				tvActionBarTitle.SetTextColor (Android.Graphics.Color.ParseColor ("#737373"));
			} 
			else 
			{
				tvActionBarTitle.SetTextColor (Android.Graphics.Color.ParseColor (color));
			}
		}

		private void fragmentMenu1_ProviderSelected(object sender, ProviderSelectedEventArgs e)
		{
			Android.Support.V4.App.Fragment f = this.SupportFragmentManager.FindFragmentById (Resource.Id.mainLayout);
			if (f is fragmentMainMenu) 
			{
				PreferencesUtil.ShowDelivered = e.IsDelivered;
				((fragmentMainMenu)f).ReloadList(e.Provider );
			}
			menu.Toggle();
		}

		public void ToggleMenu()
		{
			menu.Toggle ();
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			if (requestCode == fragmentLogin.RC_SIGN_IN)
			{
				Android.Support.V4.App.Fragment f = this.SupportFragmentManager.FindFragmentById (Resource.Id.mainLayout);
				if (f is fragmentLogin) 
				{
					((fragmentLogin)f).ConnectGoogle(resultCode, data);
				}

				return;
			}
		
			switch (resultCode) {
			case Result.Ok:

				string accessToken = data.GetStringExtra ("AccessToken");
				string userId = data.GetStringExtra ("UserId");
				string error = data.GetStringExtra ("Exception");

				FacebookClient fb = new FacebookClient (accessToken);

				fb.GetTaskAsync ("me").ContinueWith( t => {
					if (!t.IsFaulted) {

						var result = (IDictionary<string, object>)t.Result;

						string profileName = (string)result["name"];
						string email = (string)result["email"];
						Authenticate(email, "1", userId);
						//try to login with this email

					} else {
						//Alert ("Failed to Log In", "Reason: " + error, false, (res) => {} );
					}
				});

				break;
			case Result.Canceled:
				//Alert ("Failed to Log In", "User Cancelled", false, (res) => {} );
				HideProgressDialog();
				break;
			default:
				break;
			}

		}

		public void Authenticate(string email, string soc_net_id, string soc_user_id)
		{
			try
			{
				WebServiceHelper.SocialLogin(email,soc_net_id, soc_user_id,this, ()=>{
					HideProgressDialog();
					if(PreferencesUtil.UserID != 0)
					{
						PreferencesUtil.Mail = email;
						PreferencesUtil.SocID = soc_net_id;
						PreferencesUtil.SocUserID = soc_user_id;
						PreferencesUtil.SavePreferences(this);

						this.StartMainMenu();
					}
					else
					{
						//toast error login
						StartRegistration(email, soc_net_id, soc_user_id);
					}
				});
			}
			catch (Exception ex)
			{
				HideProgressDialog();
			}
		}

		public override bool OnMenuItemSelected(int featureId, Xamarin.ActionbarSherlockBinding.Views.IMenuItem item){

			/*switch (item.ItemId)
			{
			case Android.Resource.Id.Home:
				menu.Toggle ();
				return true;
			}*/
			return base.OnMenuItemSelected(featureId,item);
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			bool handled = false;
			/*switch (item.ItemId)
			{
			case Android.Resource.Id.Home:
				menu.Toggle ();
				handled = true;
				break;
			}*/
			if (handled)
				return true;
			return base.OnOptionsItemSelected (item);
		}

		public override void OnBackPressed ()
		{

			var l = SupportFragmentManager.FindFragmentById (Resource.Id.mainLayout);
			if(l  is fragmentLogin || l  is fragmentMainMenu)
			{
				this.Finish ();
				return;
			}

			base.OnBackPressed ();
		}

		public void Logout()
		{
			AlertDialog.Builder b = new AlertDialog.Builder (this);
			b.SetMessage (Resource.String.msgAreYouSureLogout);
			b.SetPositiveButton (Resource.String.btnYes, (o,e) => {
				PreferencesUtil.ClearUserData ();
				PreferencesUtil.SavePreferences (this);
				StartLogin ();
			});
			b.SetNegativeButton (Resource.String.btnNo, (o,e) => {
			});
			AlertDialog dialogDelete = b.Create ();
			dialogDelete.Show ();

		}

	}


}


