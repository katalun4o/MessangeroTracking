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
using Xamarin.ActionbarSherlockBinding.App;
using Xamarin.ActionbarSherlockBinding.Views;
using IMenuItem = Xamarin.ActionbarSherlockBinding.Views.IMenuItem;

namespace MessangeroTracking
{
	public class fragmentVoucherDetails : Xamarin.ActionbarSherlockBinding.App.SherlockFragment
	{
		public int VoucherID{ get; set;}
		//TextView etName;
		TextView etDescription;
		TextView etNumber;
		TextView tvProvider;
		//Button btnProviders;
		VoucherInfo voucher;
		ListView lstVoucherStatuses;
		ProgressBar pbLoading;
		CheckBox chbIsDelivered;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View v = inflater.Inflate (Resource.Layout.fragment_voucher_details, container, false);

			HasOptionsMenu = true;
			lstVoucherStatuses = v.FindViewById<ListView> (Resource.Id.lstVoucherStatuses);
			//etName = v.FindViewById<TextView> (Resource.Id.etName);
			etDescription = v.FindViewById<TextView> (Resource.Id.etDescription);
			etNumber = v.FindViewById<TextView> (Resource.Id.etNumber);
			//btnProviders = v.FindViewById<Button> (Resource.Id.btnProviders);
			tvProvider = v.FindViewById<TextView> (Resource.Id.tvProvider);
			pbLoading = v.FindViewById<ProgressBar> (Resource.Id.pbLoading);
			chbIsDelivered = v.FindViewById<CheckBox> (Resource.Id.chbIsDelivered);



			voucher = new VoucherInfo();
			if (VoucherID != -1) 
			{
				voucher.Load (VoucherID);

				//etName.Text = voucher.Name;
				if (string.IsNullOrEmpty (voucher.Description))
					etDescription.Visibility = ViewStates.Gone;
				else 
				{
					etDescription.Visibility = ViewStates.Visible;
					etDescription.Text = "\"" + voucher.Description + "\"";
				}
				etNumber.Text = voucher.Number;
				//btnProviders.Text = voucher.ProviderName;
				tvProvider.Text = voucher.Name;
				//tvProvider.SetTextColor(Android.Graphics.Color.ParseColor("#" + voucher.ProviderColor));
				chbIsDelivered.Checked = voucher.IsDelivered;

				lstVoucherStatuses.Adapter = new adapterVoucherStatuses (this.Activity, voucher.Statuses);

				/*WebServiceHelper.GetVoucherStatusAsync (this.Activity, voucher, (o) => {
					if(this.IsVisible)
					{
						lstVoucherStatuses.Adapter = new adapterVoucherStatuses (this.Activity, new List<VoucherStatus> (o));
					}
				});*/
			}
			else 
			{
				//init voucher
				//default 1st provider
				voucher.ProviderID = 1;
			}

			//btnProviders.Click += btnProviders_Click;

			chbIsDelivered.CheckedChange += (object sender, CompoundButton.CheckedChangeEventArgs e) => {
				if(((CheckBox)sender).Tag != null)
					return;
				AlertDialog.Builder b = new AlertDialog.Builder (this.Activity);
				//b.SetMessage (Resource.String.msgDeleteVouchers);
				if(voucher.IsDelivered == false)
					b.SetMessage ("Do you want to mark the voucher as delivered?");
				else
					b.SetMessage ("Do you want to mark the voucher as not delivered?");
				b.SetPositiveButton (Resource.String.btnYes, (o,pe) => {
					//mark voucher as delivered
					voucher.IsDelivered = chbIsDelivered.Checked;
					if(PreferencesUtil.IsPremium)
					{
					WebServiceHelper.AddVoucher(voucher.VoucherID, voucher.Number, voucher.ProviderID, voucher.Name, voucher.Description, voucher.IsDelivered, Activity
						, (vouch)=>{
							if(voucher.IsDelivered == true)
								voucher.MarkDelivered();
							else
								voucher.MarkNotDelivered();
					});
					}
					else
					{
						if(voucher.IsDelivered == true)
							voucher.MarkDelivered();
						else
							voucher.MarkNotDelivered();
					}

				});
				b.SetNegativeButton (Resource.String.btnNo, (o,ne) => {
					//close dialog
					chbIsDelivered.Tag = 1;
					chbIsDelivered.Checked = voucher.IsDelivered;
					chbIsDelivered.Tag = null;
				});
				AlertDialog dialogDelete = b.Create ();
				dialogDelete.Show ();
			};

			((MainActivity)Activity).SetActionbarTitle (voucher.ProviderName,"#" +voucher.ProviderColor, voucher.GetImagePath());

			return v;
		}

		private void ShowVoucherEditor(int voucherID)
		{
			fragmentVoucherEditor fMainMenu = new fragmentVoucherEditor();
			fMainMenu.VoucherID = VoucherID;
			var ft = this.Activity.SupportFragmentManager.BeginTransaction();
			ft.Replace(Resource.Id.mainLayout, fMainMenu);
			ft.SetTransition(Android.Support.V4.App.FragmentTransaction.TransitFragmentFade);
			ft.AddToBackStack("VoucherEditor");
			ft.Commit();
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
					//btnProviders.Text = current.Name;
					tvProvider.Text = voucher.ProviderName;
					voucher.ProviderID = current.ProviderID;
					dialogProviders.Dismiss();
				});

			dialogProviders.DismissEvent += dialogProviders_DismissEvent;
			dialogProviders.Show ();
		}

		public void dialogProviders_DismissEvent(object sender, EventArgs e)
		{

		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId)
			{
			case 1:
				//save
				Save ();
				this.FragmentManager.PopBackStack ();
				return true;
			case 2:
				//delete
				//show dialog before delete !

				ShowVoucherEditor (VoucherID);

				/*AlertDialog.Builder b = new AlertDialog.Builder (this.Activity);
				b.SetMessage (Resource.String.msgDeleteVoucher);
				b.SetPositiveButton (Resource.String.btnYes, (o,e) => {

					voucher.Delete();
					this.FragmentManager.PopBackStack ();
				});
				b.SetNegativeButton (Resource.String.btnNo, (o,e) => {
				});
				AlertDialog dialogDelete = b.Create ();
				dialogDelete.Show ();*/


				return true;
			case 3:
				//refresh statuses
				Refresh ();
				return true;

			}

			return base.OnOptionsItemSelected (item);
		}

		private void Refresh()
		{
			if (VoucherID != -1) 
			{
				//lstVoucherStatuses.Visibility = ViewStates.Gone;
				pbLoading.Visibility = ViewStates.Visible;

				if (PreferencesUtil.IsPremium)
				{
					WebServiceHelper.GetUserVouchersStatuses (new List<decimal> (){ (decimal)voucher.VoucherID }, this.Activity, (o) => {

						pbLoading.Visibility = ViewStates.Gone;
						if (o == null || o.Count () == 0)
						{
							Toast.MakeText (this.Activity, GetString(Resource.String.ProblemLoadingStatus), ToastLength.Short).Show ();
							return;
						}
						o [0].Statuses.Reverse ();
						lstVoucherStatuses.Adapter = new adapterVoucherStatuses (this.Activity, o [0].Statuses);
						//voucher.Statuses.ToArray();
					});
				} else
				{
					WebServiceHelper.GetVoucherStatusAsync (this.Activity, voucher, (o) => {
						if(this.IsVisible)
						{
							//lstVoucherStatuses.Visibility = ViewStates.Visible;
							pbLoading.Visibility = ViewStates.Gone;
							if(o != null)
							{
								List<VoucherStatus> statusesList = new List<VoucherStatus>(o.Reverse());
								lstVoucherStatuses.Adapter = new adapterVoucherStatuses (this.Activity, statusesList);
							}
							else
							{
								Toast.MakeText(this.Activity, GetString(Resource.String.ProblemLoadingStatus), ToastLength.Short).Show();
							}
						}
					});
				}

				/*WebServiceHelper.GetVoucherStatusAsync (this.Activity, voucher, (o) => {
					if(this.IsVisible)
					{
						//lstVoucherStatuses.Visibility = ViewStates.Visible;
						pbLoading.Visibility = ViewStates.Gone;
						if(o != null)
						{
							List<VoucherStatus> statusesList = new List<VoucherStatus>(o.Reverse());
							lstVoucherStatuses.Adapter = new adapterVoucherStatuses (this.Activity, statusesList);
						}
						else
						{
							Toast.MakeText(this.Activity,"There was a problem loading the status", ToastLength.Short).Show();
						}
					}
				});*/
			}
		}

		private void Save()
		{
			//voucher.Name = etName.Text.ToString ();
			voucher.Description = etDescription.Text.ToString ();
			voucher.Number = etNumber.Text.ToString ();

			if (string.IsNullOrEmpty (voucher.Number)) 
			{
				Toast.MakeText (this.Activity, Resource.String.msgNumberRequired, ToastLength.Short).Show ();
				return;
			}

			voucher.Save();
		}


		public override void OnCreateOptionsMenu (Xamarin.ActionbarSherlockBinding.Views.IMenu menu, Xamarin.ActionbarSherlockBinding.Views.MenuInflater inflater)
		{
			//IMenuItem mItem = menu.Add(0,1,0,Resource.String.miSave);
			//IMenuItem mItem = menu.Add(0,1,0,"").SetIcon(Resource.Drawable.ic_action_save);
			//mItem.SetShowAsAction (MenuItem.ShowAsActionAlways | MenuItem.ShowAsActionWithText);

			IMenuItem mItem1 = menu.Add (0, 2, 1, "").SetIcon (Resource.Drawable.ic_action_edit);
			mItem1.SetShowAsAction (MenuItem.ShowAsActionAlways | MenuItem.ShowAsActionWithText);

			IMenuItem mItem2 = menu.Add (0, 3, 1, "").SetIcon (Resource.Drawable.ic_action_refresh);
			mItem2.SetShowAsAction (MenuItem.ShowAsActionAlways | MenuItem.ShowAsActionWithText);

		}
	}
}

