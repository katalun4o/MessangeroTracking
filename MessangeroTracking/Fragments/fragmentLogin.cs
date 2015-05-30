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
using Android.Support.V4.App;
using Facebook;
using Microsoft.WindowsAzure.MobileServices;
using Android.Gms.Common;
using Android.Gms.Plus;

namespace MessangeroTracking
{
	public class fragmentLogin : Android.Support.V4.App.Fragment, Android.Gms.Common.IGooglePlayServicesClientConnectionCallbacks, 
	Android.Gms.Common.IGooglePlayServicesClientOnConnectionFailedListener
	{
	
		private ConnectionResult mConnectionResult;
		bool mIntentInProgress = false;
		bool mSignInClicked = false;
		public const int RC_SIGN_IN = 534234;
		Android.Gms.Plus.PlusClient mGoogleApiClient;

		/* A helper method to resolve the current ConnectionResult error. */
		private void resolveSignInError()
		{
			if (mConnectionResult != null && mConnectionResult.HasResolution)
			{
				try
				{
					mIntentInProgress = true;
					// startIntentSenderForResult(mConnectionResult.getIntentSender(),
					// RC_SIGN_IN, null, 0, 0, 0);
					mConnectionResult.StartResolutionForResult (this.Activity, RC_SIGN_IN);
				} catch (Exception e)
				{
					// The intent was canceled before it was sent. Return to the
					// default
					// state and attempt to connect to get an updated
					// ConnectionResult.
					mIntentInProgress = false;
					mGoogleApiClient.Connect ();
				}
			}
		}

		public void ConnectGoogle(Result resultCode, Intent data)
		{
			if (resultCode != Result.Ok)
			{
				mSignInClicked = false;
			}

			mIntentInProgress = false;

			if (!mGoogleApiClient.IsConnecting)
			{
				mGoogleApiClient.Connect();
			}
		}

		public void OnConnected (Bundle p0)
		{
			//mSignInClicked = false;

			if (mGoogleApiClient.CurrentPerson != null)
		{
				//Android.Gms.Plus.Model.People.IPerson Person currentPerson = Plus.PeopleApi
				//	.getCurrentPerson(mGoogleApiClient);

				String personName = mGoogleApiClient.CurrentPerson.DisplayName;
			// String personGooglePlusProfile = currentPerson.getUrl();
				String email = mGoogleApiClient.AccountName;
				String socID = mGoogleApiClient.CurrentPerson.Id;

				((MainActivity)this.Activity).Authenticate(email, "2", socID);
				//CreateAccount(personName, email, socID, "2");
		}
		}

		public void OnDisconnected ()
		{

		}

		public void OnConnectionFailed (Android.Gms.Common.ConnectionResult result)
		{
if (!mIntentInProgress)
		{
			// Store the ConnectionResult so that we can use it later when the
			// user clicks
			// 'sign-in'.
			mConnectionResult = result;

			if (mSignInClicked)
			{
				// The user has already clicked 'sign-in' so we attempt to
				// resolve all
				// errors until the user is signed in, or they cancel.
				resolveSignInError();
			}
		}
		}

		private const string AppId = "277340355776573";
		private const string ExtendedPermissions = "email";
		FacebookClient fb;

		EditText etUsername;
		EditText etPassword;
		Button btnLogin;
		Button btnFacebookLogin;
		Button btnRegister;
		Button btnForgotPassword;

		private void InitFacebookButton(Button btnFacebookLogin)
		{
			btnFacebookLogin.Gravity = GravityFlags.Center;
			btnFacebookLogin.SetTextColor(Resources.GetColor(Resource.Color.com_facebook_loginview_text_color));
			btnFacebookLogin.SetTextSize(ComplexUnitType.Px,
				Resources.GetDimension(Resource.Dimension.com_facebook_loginview_text_size));
			btnFacebookLogin.Typeface = Android.Graphics.Typeface.DefaultBold;
			btnFacebookLogin.SetBackgroundResource (Resource.Drawable.com_facebook_button_blue);
			btnFacebookLogin.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.com_facebook_inverse_icon, 0, 0, 0);
			btnFacebookLogin.CompoundDrawablePadding = Resources.GetDimensionPixelSize(Resource.Dimension.com_facebook_loginview_compound_drawable_padding);

			btnFacebookLogin.SetPadding(Resources.GetDimensionPixelSize(Resource.Dimension.com_facebook_loginview_padding_left),
				Resources.GetDimensionPixelSize(Resource.Dimension.com_facebook_loginview_padding_top),
				Resources.GetDimensionPixelSize(Resource.Dimension.com_facebook_loginview_padding_right),
				Resources.GetDimensionPixelSize(Resource.Dimension.com_facebook_loginview_padding_bottom));

		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View v = inflater.Inflate (Resource.Layout.fragment_login, container, false);

			etUsername = v.FindViewById<EditText> (Resource.Id.etUsername);
			etPassword = v.FindViewById<EditText> (Resource.Id.etPasswrod);
			btnLogin = v.FindViewById<Button> (Resource.Id.btnLogin);
			btnFacebookLogin = v.FindViewById<Button> (Resource.Id.btnFacebookLogin);
			btnForgotPassword = v.FindViewById<Button> (Resource.Id.btnForgotPassword);
			InitFacebookButton (btnFacebookLogin);
			btnRegister = v.FindViewById<Button> (Resource.Id.btnRegister);
			SignInButton sign_in_button  = v.FindViewById<SignInButton> (Resource.Id.sign_in_button);

			sign_in_button.Click += sign_in_button_Click;
			btnLogin.Click += btnLogin_Click;
			btnFacebookLogin.Click += btnFacebookLogin_Click;
			btnRegister.Click += btnRegister_Click;

			mGoogleApiClient = new Android.Gms.Plus.PlusClient.Builder(this.Activity, this, this).
				SetScopes(Android.Gms.Common.Scopes.PlusLogin).Build();

			btnForgotPassword.Click +=	(object sender, EventArgs e) => {

				//show mail dialog
				ShowForgotPasswordDialog();
			};


			return v;
		}


		private void ShowForgotPasswordDialog()
		{
			Dialog dialogProviders = new Dialog (this.Activity,Resource.Style.noTitleDialog);
			dialogProviders.SetContentView (Resource.Layout.dialog_forgot_password);
			EditText tbMail = dialogProviders.FindViewById<EditText> (Resource.Id.tbEmail);
			Button btnOK = dialogProviders.FindViewById<Button> (Resource.Id.btnOK);
			Button btnCancel = dialogProviders.FindViewById<Button> (Resource.Id.btnCancel);

			btnOK.Click += (object sender, EventArgs e) => {
				string mail = tbMail.Text.ToString();

				if(string.IsNullOrEmpty(mail))
				{
					//toast error
					Toast.MakeText(Activity, GetString(Resource.String.EmailIsRequired),ToastLength.Short).Show();
					return;
				}

				WebServiceHelper.ForgotPassword (this.Activity, mail, (errorID) => {
					//toast email sent
					if(errorID == 0)
						Toast.MakeText(Activity, GetString(Resource.String.EmailSent),ToastLength.Short).Show();
					else
						Toast.MakeText(Activity,GetString(Resource.String.EmailNotExist),ToastLength.Short).Show();
				});

				dialogProviders.Dismiss();
			};
			btnCancel.Click += (object sender, EventArgs e) => {dialogProviders.Dismiss();};

			dialogProviders.Show ();
		}

		public static MobileServiceClient client = new MobileServiceClient( "https://trackingservice.azure-mobile.net/", "uknxLzRvJxVdzPgwmvDPSxoAfaLjBD20" );

		private void sign_in_button_Click(object sender, EventArgs e)
		{
			((MainActivity)this.Activity).ShowProgressDialog (GetString(Resource.String.PleasWait), GetString(Resource.String.LoggingIn));

			mSignInClicked = true;

			mGoogleApiClient.Connect ();

			if (!mGoogleApiClient.IsConnecting)
			{

				mGoogleApiClient.Connect ();
			}
		}

		private void btnLogin_Click(object sender, EventArgs e)
		{
			//Android.Gms.Plus.PlusClient

			string user = etUsername.Text.ToString ();
			string pass = etPassword.Text.ToString ();

			Android.Views.InputMethods.InputMethodManager manager = 
				(Android.Views.InputMethods.InputMethodManager)this.Activity.GetSystemService(Android.InputMethodServices.InputMethodService.InputMethodService);
			manager.HideSoftInputFromWindow(etUsername.WindowToken, 0);
			manager.HideSoftInputFromWindow(etPassword.WindowToken, 0);

			if (string.IsNullOrEmpty (user) || string.IsNullOrEmpty (pass))
			{
				Toast.MakeText (this.Activity, GetString(Resource.String.msgEmptyFields), ToastLength.Short).Show();
				return;
			}

			progressDialog = ProgressDialog.Show(this.Activity, GetString(Resource.String.PleasWait), GetString(Resource.String.LoggingIn), true);

			//start login dialog
			WebServiceHelper.LoginUser(etUsername.Text.ToString(), etPassword.Text.ToString(),this.Activity, new Action(()=>{
				if(PreferencesUtil.UserID != 0)
				{

					((MainActivity)this.Activity).StartMainMenu();
				}
				else
				{
					//show login error message
					Toast.MakeText (this.Activity, GetString(Resource.String.msgWrongCredentials), ToastLength.Short).Show();
				}
				progressDialog.Hide();
			}));
		}

		ProgressDialog progressDialog;
		private void btnFacebookLogin_Click(object sender, EventArgs e)
		{
			//progressDialog = ProgressDialog.Show(this.Activity, "Please wait...", "Logging in...", true);
			((MainActivity)this.Activity).ShowProgressDialog (GetString(Resource.String.PleasWait), GetString(Resource.String.LoggingIn));
			AuthenticateFacebook();
		}

		private async void AuthenticateFacebook()
		{
			var webAuth = new Intent (this.Activity, typeof (FBWebViewAuthActivity));
			webAuth.PutExtra ("AppId", AppId);
			webAuth.PutExtra ("ExtendedPermissions", ExtendedPermissions);
			StartActivityForResult (webAuth, 0);

			/*try
			{
				object oUser = await client.LoginAsync(this.Activity, MobileServiceAuthenticationProvider.Facebook);

				MobileServiceUser user = await client.LoginAsync(this.Activity, MobileServiceAuthenticationProvider.Facebook);
				var facebookId = user.UserId.Substring(user.UserId.IndexOf(':') + 1);
				var httpClient = new System.Net.Http.HttpClient();
				var url = "https://graph.facebook.com/me?access_token=" + user.MobileServiceAuthenticationToken;
				//var fbUser = await httpClient.GetAsync("https://graph.facebook.com/" + facebookId);
				var fbUser = await httpClient.GetAsync(url);
				var response = await fbUser.Content.ReadAsStringAsync();
				var jo = System.Json.JsonObject.Parse(response);
				var userName = jo["name"].ToString();
				WebServiceHelper.SocialLogin("1",user.UserId,this.Activity, ()=>{
					progressDialog.Dismiss();
					if(PreferencesUtil.UserID != 0)
					{
						((MainActivity)this.Activity).StartMainMenu();
					}
					else
					{
						//toast error login
					}
				});
			}
			catch (Exception ex)
			{
				progressDialog.Dismiss();
			}*/
		}



		private void btnRegister_Click(object sender, EventArgs e)
		{
			((MainActivity)Activity).StartRegistration ();
		}
	}
}

