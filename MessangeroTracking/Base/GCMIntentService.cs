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
using Android.Support.V4.App;

namespace MessangeroTracking
{
	[Service]
	public class GCMIntentService : IntentService
{
		public static int VoucherUpdatedNotoficationID = 34345;
    static PowerManager.WakeLock sWakeLock;
    static object LOCK = new object();

		public static void RunIntentInService(Context context, Intent intent)
		{
			lock (LOCK) {
				if (sWakeLock == null) {
					// This is called from BroadcastReceiver, there is no init.
					var pm = PowerManager.FromContext (context);
					sWakeLock = pm.NewWakeLock (
						WakeLockFlags.Partial, "My WakeLock Tag");
				}
			}

			sWakeLock.Acquire ();
			intent.SetClass (context, typeof(GCMIntentService));
			context.StartService (intent);

		}

    protected override void OnHandleIntent(Intent intent)
		{
			try {
				Context context = this.ApplicationContext;
				string action = intent.Action;

				if (action.Equals ("com.google.android.c2dm.intent.REGISTRATION")) {
					string registration_id = intent.GetStringExtra("registration_id");
					//save received id to azure database
					WebServiceHelper.RegisterCloudMessage(PreferencesUtil.UserID,registration_id,null);

				} else if (action.Equals ("com.google.android.c2dm.intent.RECEIVE")) {
					//show notification in notification bar
					string numbersToUpdate = intent.GetStringExtra("numbers");
					int userID = int.Parse(intent.GetStringExtra("usr"));
					//string numbersToUpdate = "nv013834374gr,H8892906928,010454357023";
					ShowNotification(numbersToUpdate,userID);
				}
			} finally {
				lock (LOCK) {
					//Sanity check for null as this is a public method
					if (sWakeLock != null)
						sWakeLock.Release ();
				}
			}
		}

		private void ShowNotification(string numbers, int userID)
		{
			Context context = this.ApplicationContext;
			PreferencesUtil.LoadSettings(context);
			if (PreferencesUtil.IsPremium == false || PreferencesUtil.UserID == 0 || PreferencesUtil.UserID != userID)
				//if (PreferencesUtil.UserID == 0)
				return;
			Intent resultIntent = new Intent(this, typeof(MainActivity));
			resultIntent.PutExtra("Numbers",numbers); // Pass some values to SecondActivity.

			int count = numbers.Split (',').Length;

			TaskStackBuilder stackBuilder = TaskStackBuilder.Create(context);
			stackBuilder.AddParentStack (Java.Lang.Class.FromType(typeof(MainActivity)));
			stackBuilder.AddNextIntent(resultIntent);

			PendingIntent resultPendingIntent = stackBuilder.GetPendingIntent(0, (int)PendingIntentFlags.UpdateCurrent);

			// Build the notification
			NotificationCompat.Builder builder = new NotificationCompat.Builder(this)
				.SetAutoCancel(true) // dismiss the notification from the notification area when the user clicks on it
				.SetContentIntent(resultPendingIntent) // start up this activity when the user clicks the intent.
				.SetContentTitle("Vouchers updated") // Set the title
				.SetNumber(count) // Display the count in the Content Info
				.SetSmallIcon(Resource.Drawable.ic_launcher) // This is the icon to display
				.SetContentText(String.Format("There are {0} new vouchers.", count)); // the message to display.

			//builder.SetSound(Android.Media.RingtoneManager.GetDefaultUri(Android.Media.RingtoneType.Notification));
			builder.SetDefaults((int)NotificationDefaults.All);
			// Finally publish the notification
			NotificationManager notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
			notificationManager.Notify(VoucherUpdatedNotoficationID, builder.Build());
		}

	}
}

