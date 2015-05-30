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
	public class fragmentMainMenu :  Xamarin.ActionbarSherlockBinding.App.SherlockFragment
	{
		VoucherList listVouchers;
		ListView lvVouchers;
		Xamarin.ActionbarSherlockBinding.Views.IMenu actionMenu;
		ProgressBar activity_bar;
		bool loadStatusesFromPush = false;
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View v = inflater.Inflate (Resource.Layout.fragment_main_menu, container, false);
			HasOptionsMenu = true;
			activity_bar = v.FindViewById<ProgressBar> (Resource.Id.activity_bar);
			lvVouchers = v.FindViewById<ListView> (Resource.Id.lstVouchers);
			HasOptionsMenu = true;

			//PreferencesUtil.IsPremium == false ||
			if (PreferencesUtil.IsPremium)
			{
				LoadPremiumVouchers();
			} else
			{
				LoadFreeVouchers();
			}

			lvVouchers.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>((o,e) => 
				{
					VoucherInfo currentVoucher = listVouchers[e.Position];
					//ShowVoucherEditor(currentVoucher.VoucherID);
					ShowVoucherDetails(currentVoucher.VoucherID);
				});

			((MainActivity)Activity).SetActionbarTitle ("Messangero Tracking");

			return v;
		}

		private void LoadFreeVouchers()
		{
			listVouchers = new VoucherList ();
			listVouchers.Load ();

			adapterVouchers adapter = new adapterVouchers(this.Activity,listVouchers);
			adapter.ItemSelectedChanged += adapterVouchers_ItemSelectedChanged;
			lvVouchers.Adapter = adapter;
		}

		private void LoadPremiumVouchers()
		{
			if ( MainActivity.IsFirstTime) {
				MainActivity.IsFirstTime = false;
				WebServiceHelper.GetVouchers (this.Activity, new Action<VoucherInfo[]> ((o) => {

					listVouchers = new VoucherList ();
					listVouchers.Load ();

					adapterVouchers adapter = new adapterVouchers(this.Activity,listVouchers);
					adapter.ItemSelectedChanged +=	adapterVouchers_ItemSelectedChanged;
					lvVouchers.Adapter = adapter;


					if (Arguments != null && Arguments.ContainsKey ("Numbers")) 
					{
						string numbers = Arguments.GetString ("Numbers");
						if (!string.IsNullOrEmpty (numbers)) 
						{
							loadStatusesFromPush = true;
							RefreshVouchers(numbers.Split(','));
						}
					}
				}));
			} else {
				listVouchers = new VoucherList ();
				listVouchers.Load ();

				adapterVouchers adapter = new adapterVouchers(this.Activity,listVouchers);
				adapter.ItemSelectedChanged +=	adapterVouchers_ItemSelectedChanged;
				lvVouchers.Adapter = adapter;
			}
		}

		static bool raiseVoucherSelectedChangedEvent = true;
		private void adapterVouchers_ItemSelectedChanged()
		{
			if (raiseVoucherSelectedChangedEvent == false)
				return;
			//change actionbar if needed
			bool res = (from i in listVouchers where i.IsSelected select i).Any();
			if(res)
				CreateItemsEditActionBar();
			else
				CreateDefaultActionBar();
		}

		public void ReloadList()
		{
			ReloadList (null);
		}

		public void ReloadList(Provider provider)
		{
			listVouchers.Clear ();
			if(provider == null || provider.IsDefault)
				listVouchers.Load();
			else
				listVouchers.Load(provider.ProviderID);

			adapterVouchers adapter = new adapterVouchers(this.Activity,listVouchers);
			adapter.ItemSelectedChanged +=	adapterVouchers_ItemSelectedChanged;
			lvVouchers.Adapter = adapter;
		}

		private void ShowVoucherDetails(int voucherID)
		{
			fragmentVoucherDetails fMainMenu = new fragmentVoucherDetails();
			fMainMenu.VoucherID = voucherID;
			var ft = this.Activity.SupportFragmentManager.BeginTransaction();
			ft.Replace(Resource.Id.mainLayout, fMainMenu);
			ft.SetTransition(Android.Support.V4.App.FragmentTransaction.TransitFragmentFade);
			ft.AddToBackStack("VoucherDetails");
			ft.Commit();
		}


		private void ShowVoucherEditor(int voucherID)
		{
			//if is free version - check if vouchers count is <=5 in database
			if (PreferencesUtil.IsPremium == false)
			{
				if (VoucherInfo.GetVouchersCount () >= 5)
				{

					AlertDialog.Builder b = new AlertDialog.Builder (this.Activity);
					b.SetMessage (Resource.String.msgUpgradeToPremium);
					b.SetPositiveButton (Resource.String.btnYes, (o,e) => 
						{
							//upgrade to pro version
					});
					b.SetNegativeButton (Resource.String.btnNo, (o,e) => {
					});
					AlertDialog dialogPremium = b.Create ();
					dialogPremium.Show ();
					return;
				}
			}

			fragmentVoucherEditor fMainMenu = new fragmentVoucherEditor();
			fMainMenu.VoucherID = -1;
			var ft = this.Activity.SupportFragmentManager.BeginTransaction();
			ft.Replace(Resource.Id.mainLayout, fMainMenu);
			ft.SetTransition(Android.Support.V4.App.FragmentTransaction.TransitFragmentFade);
			ft.AddToBackStack("VoucherEditor");
			ft.Commit();
		}

		private void ShowSettings()
		{
			fragmentSettings fSettings = new fragmentSettings();
			var ft = this.Activity.SupportFragmentManager.BeginTransaction();
			ft.Replace(Resource.Id.mainLayout, fSettings);
			ft.SetTransition(Android.Support.V4.App.FragmentTransaction.TransitFragmentFade);
			ft.AddToBackStack("Settings");
			ft.Commit();
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId) {
			case 2:
				//select all
				//raiseVoucherSelectedChangedEvent = false;
				/*foreach (VoucherInfo voucher in listVouchers)
				{
					voucher.IsSelected = true; 
				}*/
				if (lvVouchers.Count > 0)
				{
					((adapterVouchers)lvVouchers.Adapter).parent = lvVouchers;
					((adapterVouchers)lvVouchers.Adapter).SelectAll ();
					CreateItemsEditActionBar ();
				}
				//((adapterVouchers)lvVouchers.Adapter).NotifyDataSetChanged ();
				//CreateItemsEditActionBar();
				//raiseVoucherSelectedChangedEvent = true;
				return true;
			case 3:
				//open settings
				ShowSettings ();
				return true;
			case 7:
				//toggle slide menu
				((MainActivity)this.Activity).ToggleMenu ();
				return true;
			case 6:
				//show edit voucher fragment
				ShowVoucherEditor (-1);
				return true;
			case 12:
				//delete selected items
				AlertDialog.Builder b = new AlertDialog.Builder (this.Activity);
				b.SetMessage (Resource.String.msgDeleteVouchers);
				b.SetPositiveButton (Resource.String.btnYes, (o,e) => {

					foreach (VoucherInfo voucher in listVouchers) 
					{
						if (voucher.IsSelected) 
						{
							if(PreferencesUtil.IsPremium)
							{
							WebServiceHelper.DeleteVoucher(voucher.VoucherID, this.Activity, ()=>{
								voucher.Delete ();
								((adapterVouchers)lvVouchers.Adapter).Remove(voucher);
							});
							}
							else
							{
								voucher.Delete ();
								((adapterVouchers)lvVouchers.Adapter).Remove(voucher);
							}
						}
					}

					CreateItemsEditActionBar();
					//ReloadList();

				});
				b.SetNegativeButton (Resource.String.btnNo, (o,e) => {
				});
				AlertDialog dialogDelete = b.Create ();
				dialogDelete.Show ();

				return true;
			case 13:
				//refresh selected vouchers
				RefreshVouchersStatus ();
				return true;

			}

			return base.OnOptionsItemSelected (item);
		}

		int currentVoucher = 0;
		int selectedVouchersCount = 0;
		private void RefreshVouchersStatus()
		{
			activity_bar.Visibility = ViewStates.Visible;
			currentVoucher = 0;
			selectedVouchersCount = 0;
			//((MainActivity)this.Activity).SetProgressBarIndeterminateVisibility (true);

			if (loadStatusesFromPush)
			{
				foreach (VoucherInfo voucher in listVouchers)
				{
					if (voucher.IsSelected == false)
						continue;
					selectedVouchersCount++;

					WebServiceHelper.GetVoucherStatusFromDBAsync (this.Activity, voucher, (o) => {
						LoadVoucherStatuses (voucher, o);
					});
				}
			} else
			{
				if (PreferencesUtil.IsPremium)
				{
					List<decimal> ids = new List<decimal> ();
					foreach (VoucherInfo voucher in listVouchers)
					{
						if (voucher.IsSelected == false)
							continue;
						selectedVouchersCount++;

						ids.Add (voucher.VoucherID);
					}
					//get voucher status by voucher id
					WebServiceHelper.GetUserVouchersStatuses (ids, this.Activity, (o) => {

						foreach (VoucherInfo voucher in o)
						{
							LoadVoucherStatuses (voucher, voucher.Statuses.ToArray ());
						}
					});
				} else
				{
					//if not premium

					foreach(VoucherInfo voucher in listVouchers)
					{
						if (voucher.IsSelected == false)
							continue;
						selectedVouchersCount++;

						WebServiceHelper.GetVoucherStatusAsync (this.Activity, voucher, (o) => {
								LoadVoucherStatuses (voucher, o);
							});
					}

				}
			}

			/*foreach(VoucherInfo voucher in listVouchers)
			{
				if (voucher.IsSelected == false)
					continue;
				selectedVouchersCount++;

				if (loadStatusesFromPush)
				{
					WebServiceHelper.GetVoucherStatusFromDBAsync (this.Activity, voucher, (o) => {
						LoadVoucherStatuses (voucher, o);
					});
				} else
				{
					WebServiceHelper.GetVoucherStatusAsync (this.Activity, voucher, (o) => {
						LoadVoucherStatuses (voucher, o);
					});
				}
			}*/
			loadStatusesFromPush = false;
			//((MainActivity)this.Activity).SetProgressBarIndeterminateVisibility (false);
		}

		private void LoadVoucherStatuses(VoucherInfo voucher, VoucherStatus[] o)
		{
			if(this.IsVisible)
			{
				if(o == null)
				{
					currentVoucher++;
					for(int i = 0; i < listVouchers.Count; i++)
					{
						View view = lvVouchers.GetChildAt(i);
						if(view == null)
							continue;

						VoucherViewHolder holder = ((VoucherViewHolder)view.Tag);

						if(holder.Voucher == null)
							continue;

						if(holder.Voucher.VoucherID == voucher.VoucherID)
						{
							holder.chbSelected.Checked = false;
						}
					}
					if(currentVoucher >= selectedVouchersCount)
						activity_bar.Visibility = ViewStates.Gone;

					return;
				}

				if(o.Count() > 0)
				{
					voucher.LastStatus = o[o.Count() - 1].Status;
					voucher.LastStatusArea = o[o.Count() - 1].Area;
					voucher.LastStatusDate = o[o.Count() - 1].Date;
					voucher.LastStatusTime = o[o.Count() - 1].Time;
				}

				for(int i = 0; i < listVouchers.Count; i++)
				{
					if (listVouchers[i].VoucherID == voucher.VoucherID)
					{
						listVouchers[i].IsSelected = false;

						listVouchers[i].IsSelected = false;
						listVouchers[i].LastStatus = voucher.LastStatus;
						listVouchers[i].LastStatusArea = voucher.LastStatusArea;
						listVouchers[i].LastStatusDate = voucher.LastStatusDate;
						listVouchers[i].LastStatusTime = voucher.LastStatusTime;
					}

					View view = lvVouchers.GetChildAt(i);
					if(view == null)
						continue;

					VoucherViewHolder holder = ((VoucherViewHolder)view.Tag);

					if(holder.Voucher == null)
						continue;

					if(holder.Voucher.VoucherID == voucher.VoucherID)
					{
						holder.Voucher.IsSelected = false;
						holder.Voucher.LastStatus = voucher.LastStatus;
						holder.Voucher.LastStatusArea = voucher.LastStatusArea;
						holder.Voucher.LastStatusDate = voucher.LastStatusDate;
						holder.Voucher.LastStatusTime = voucher.LastStatusTime;

						holder.UpdateLastStatusFields ();

						holder.chbSelected.Checked = false;
					}
				}
			}
			currentVoucher++;
			if(currentVoucher >= selectedVouchersCount)
				activity_bar.Visibility = ViewStates.Gone;
			//((MainActivity)this.Activity).SetProgressBarIndeterminateVisibility (false);
		}

		private void CreateItemsEditActionBar()
		{
			actionMenu.Clear();

			//IMenuItem mItem = actionMenu.Add(0,12,0,Resource.String.miDelete);
			IMenuItem mItem = actionMenu.Add(0,12,0,"").SetIcon(Resource.Drawable.ic_action_discard);
			mItem.SetShowAsAction (MenuItem.ShowAsActionAlways | MenuItem.ShowAsActionWithText);
			//IMenuItem mFilter = actionMenu.Add(0,13,0,Resource.String.miRefresh);
			IMenuItem mFilter = actionMenu.Add(0,13,0,"").SetIcon(Resource.Drawable.ic_action_refresh);
			mFilter.SetShowAsAction (MenuItem.ShowAsActionAlways | MenuItem.ShowAsActionWithText);

			actionMenu.Add(0,2,1,GetString(Resource.String.SelectAll));
			actionMenu.Add(0,3,2,GetString(Resource.String.Settings));
		}

		private void CreateDefaultActionBar()
		{
			actionMenu.Clear ();

			//IMenuItem mItem = actionMenu.Add(0,6,0,Resource.String.miAdd);
			IMenuItem mItem = actionMenu.Add(0,6,0,"").SetIcon(Resource.Drawable.ic_action_new);
			mItem.SetShowAsAction (MenuItem.ShowAsActionAlways | MenuItem.ShowAsActionWithText);
			//IMenuItem mFilter = actionMenu.Add(0,7,0,Resource.String.miFilter);
			IMenuItem mFilter = actionMenu.Add (0, 7, 0, "").SetIcon (Resource.Drawable.ic_action_overflow);
			mFilter.SetShowAsAction (MenuItem.ShowAsActionAlways | MenuItem.ShowAsActionWithText);

			actionMenu.Add(0,2,1,GetString(Resource.String.SelectAll));
			actionMenu.Add(0,3,2,GetString(Resource.String.Settings));
		}

		public override void OnCreateOptionsMenu (Xamarin.ActionbarSherlockBinding.Views.IMenu menu, Xamarin.ActionbarSherlockBinding.Views.MenuInflater inflater)
		{
			this.actionMenu = menu;
			//IMenuItem mItem = menu.Add(0,6,0,Resource.String.miAdd);
			IMenuItem mItem = actionMenu.Add(0,6,0,"").SetIcon(Resource.Drawable.ic_action_new);
			mItem.SetShowAsAction (MenuItem.ShowAsActionAlways | MenuItem.ShowAsActionWithText);
			//IMenuItem mFilter = menu.Add(0,7,0,Resource.String.miFilter);
			IMenuItem mFilter = actionMenu.Add (0, 7, 0, "").SetIcon (Resource.Drawable.ic_action_overflow);
			mFilter.SetShowAsAction (MenuItem.ShowAsActionAlways | MenuItem.ShowAsActionWithText);

			menu.Add(0,2,1,GetString(Resource.String.SelectAll));
			menu.Add(0,3,2,GetString(Resource.String.Settings));

			base.OnCreateOptionsMenu (menu, inflater);
		}

		private void RefreshVouchers(string[] numbers)
		{
			foreach (string num in numbers) 
			{
				foreach (VoucherInfo voucher in listVouchers) 
				{
					if (voucher.Number == num) 
					{
						voucher.IsSelected = true;
						break;
					}
				}
			}
			RefreshVouchersStatus();

			//this.Arguments = null;
		}

	}


}

