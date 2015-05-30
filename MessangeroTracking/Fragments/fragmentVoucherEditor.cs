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
using Xamarin.ActionbarSherlockBinding.App;
using Xamarin.ActionbarSherlockBinding.Views;
using IMenuItem = Xamarin.ActionbarSherlockBinding.Views.IMenuItem;
using ZXing;
using ZXing.Mobile;

namespace MessangeroTracking
{
	public class fragmentVoucherEditor : Xamarin.ActionbarSherlockBinding.App.SherlockFragment
	{
		public int VoucherID{ get; set;}
		EditText etName;
		EditText etDescription;
		EditText etNumber;
		Button btnProviders;
		Button btnScan;
		VoucherInfo voucher;
		ProgressBar pbLoading;


		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View v = inflater.Inflate (Resource.Layout.fragment_voucher_editor, container, false);
			HasOptionsMenu = true;
			etName = v.FindViewById<EditText> (Resource.Id.etName);
			etDescription = v.FindViewById<EditText> (Resource.Id.etDescription);
			etNumber = v.FindViewById<EditText> (Resource.Id.etNumber);
			btnProviders = v.FindViewById<Button> (Resource.Id.btnProviders);
			btnScan = v.FindViewById<Button> (Resource.Id.btnScan);
			pbLoading = v.FindViewById<ProgressBar> (Resource.Id.pbLoading);

			voucher = new VoucherInfo();
			if (VoucherID != -1) {
				voucher.Load (VoucherID);

				etName.Text = voucher.Name;
				etDescription.Text = voucher.Description;
				etNumber.Text = voucher.Number;
				btnProviders.Text = voucher.ProviderName;
				((MainActivity)Activity).SetActionbarTitle (voucher.Name);
			}
			else 
			{
				((MainActivity)Activity).SetActionbarTitle (GetString(Resource.String.AddVoucher));
				//init voucher
				//default 1st provider
				voucher.ProviderID = 1;
			}

			btnProviders.Click += btnProviders_Click;

			btnScan.Click += btnScan_Click;

			return v;
		}

		Dialog dialogProviders;
		List<Provider> lstProviders;
		public void btnProviders_Click(object sender, EventArgs e)
		{
			lstProviders = Provider.GetProviders ();

			dialogProviders = new Dialog (this.Activity,Resource.Style.noTitleDialog);
			dialogProviders.SetContentView (Resource.Layout.dialog_providers);
			ListView lvProviders = dialogProviders.FindViewById<ListView> (Resource.Id.lvProviders);
			lvProviders.Adapter = new adapterDialogProviders (this.Activity, lstProviders);

			lvProviders.ItemClick +=new EventHandler<AdapterView.ItemClickEventArgs>((o,args)=>
				{
					Provider current = lstProviders[args.Position] ;
					btnProviders.Text = current.Name;
					voucher.ProviderID = current.ProviderID;
					dialogProviders.Dismiss();
				});

			dialogProviders.Show ();
		}

		MobileBarcodeScanner scanner;
		public async void btnScan_Click(object sender, EventArgs e)
		{
			scanner = new MobileBarcodeScanner(this.Activity);

			scanner.UseCustomOverlay = false;

			//We can customize the top and bottom text of the default overlay
			scanner.TopText = "Hold the camera up to the barcode\nAbout 6 inches away";
			scanner.BottomText = "Wait for the barcode to automatically scan!";

			//Start scanning
			var result = await scanner.Scan();

			HandleScanResult(result);

		}

		void HandleScanResult (ZXing.Result result)
		{
			if (result == null)
				return;

			string msg = "";

			/*if (result != null && !string.IsNullOrEmpty(result.Text))
				msg = "Found Barcode: " + result.Text;
			else
				msg = "Scanning Canceled!";*/

			this.Activity.RunOnUiThread(() => etNumber.Text = result.Text);
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId)
			{
			case 1:
				//save
				Save ();
				return true;
			case 2:
				//delete
				//show dialog before delete !

				Android.Views.InputMethods.InputMethodManager manager = (Android.Views.InputMethods.InputMethodManager) 
					this.Activity.GetSystemService(Android.InputMethodServices.InputMethodService.InputMethodService);
				manager.HideSoftInputFromWindow(etName.WindowToken, 0);
				manager.HideSoftInputFromWindow(etDescription.WindowToken, 0);
				manager.HideSoftInputFromWindow(etNumber.WindowToken, 0);

				AlertDialog.Builder b = new AlertDialog.Builder (this.Activity);
				b.SetMessage (Resource.String.msgDeleteVoucher);
				b.SetPositiveButton (Resource.String.btnYes, (o,e) => {
					if(PreferencesUtil.IsPremium)
					{
					WebServiceHelper.DeleteVoucher(voucher.VoucherID,this.Activity, ()=>{
						voucher.Delete();
						this.FragmentManager.PopBackStack(MainActivity.MainFragmentTransactionID, 0);
					});
					}
					else
					{
						voucher.Delete();
						this.FragmentManager.PopBackStack(MainActivity.MainFragmentTransactionID, 0);
					}

				});
				b.SetNegativeButton (Resource.String.btnNo, (o,e) => {
				});
				AlertDialog dialogDelete = b.Create ();
				dialogDelete.Show ();


				return true;
			}

			return base.OnOptionsItemSelected (item);
		}

		private void Save()
		{
			voucher.Name = etName.Text.ToString ();
			voucher.Description = etDescription.Text.ToString ();
			voucher.Number = etNumber.Text.ToString ();

			if (string.IsNullOrEmpty (voucher.Number)) 
			{
				Toast.MakeText (this.Activity, Resource.String.msgNumberRequired, ToastLength.Short).Show ();
				return;
			}


			Android.Views.InputMethods.InputMethodManager manager = (Android.Views.InputMethods.InputMethodManager) 
				this.Activity.GetSystemService(Android.InputMethodServices.InputMethodService.InputMethodService);
			manager.HideSoftInputFromWindow(etName.WindowToken, 0);
			manager.HideSoftInputFromWindow(etDescription.WindowToken, 0);
			manager.HideSoftInputFromWindow(etNumber.WindowToken, 0);

			//voucher.Save();
			if (PreferencesUtil.IsPremium)
			{
				WebServiceHelper.AddVoucher (voucher.VoucherID, voucher.Number, voucher.ProviderID, voucher.Name, voucher.Description, voucher.IsDelivered, Activity, (v) => {
					if (v == null)
						voucher.Save ();
					else
						v.Save ();
					this.FragmentManager.PopBackStack ();
				});
			} else
			{
				//not premium
				voucher.Save();
				this.FragmentManager.PopBackStack ();
			}
		}

		public override void OnCreateOptionsMenu (Xamarin.ActionbarSherlockBinding.Views.IMenu menu, Xamarin.ActionbarSherlockBinding.Views.MenuInflater inflater)
		{
			//IMenuItem mItem = menu.Add(0,1,0,Resource.String.miSave);
			IMenuItem mItem = menu.Add(0,1,0,"").SetIcon(Resource.Drawable.ic_action_save);
			mItem.SetShowAsAction (MenuItem.ShowAsActionAlways | MenuItem.ShowAsActionWithText);

			IMenuItem mItem1 = menu.Add (0, 2, 1, "").SetIcon (Resource.Drawable.ic_action_discard);
			mItem1.SetShowAsAction (MenuItem.ShowAsActionAlways | MenuItem.ShowAsActionWithText);
		}
	}
}

