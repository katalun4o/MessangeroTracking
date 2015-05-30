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
	public class fragmentSlideMenu :  Android.Support.V4.App.Fragment
	{
		public delegate void ProviderSelectedDelegate (object sender, ProviderSelectedEventArgs e);
		public event ProviderSelectedDelegate ProviderSelected;

		List<Provider> lstProviders;
		ListView lvProviders;
		CheckBox chbShowDelivered;
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View v = inflater.Inflate (Resource.Layout.fragment_slide_menu, container, false);

			lvProviders = v.FindViewById<ListView> (Resource.Id.lstProviders);
			chbShowDelivered = v.FindViewById<CheckBox> (Resource.Id.chbShowDelivered);

			chbShowDelivered.Checked = PreferencesUtil.ShowDelivered;

			lstProviders = Provider.GetProviders();
			string all = GetString (Resource.String.All);
			lstProviders.Insert (0, new Provider (){ ProviderID = -1, Name = all, IsDefault = true });
			lvProviders.Adapter = new adapterProviders(this.Activity,lstProviders);

			lvProviders.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>((o,e) => 
				{
					Provider currentProvider = lstProviders[e.Position];
					if(ProviderSelected != null)
					{
						ProviderSelected(this,new ProviderSelectedEventArgs(currentProvider, chbShowDelivered.Checked));
					}
				});

			chbShowDelivered.CheckedChange += (object sender, CompoundButton.CheckedChangeEventArgs e) => {
				int provPos = lvProviders.SelectedItemPosition;
				if(provPos == -1)
					provPos = 0;

				PreferencesUtil.ShowDelivered = chbShowDelivered.Checked;
				PreferencesUtil.SavePreferences(this.Activity);

				Provider currentProvider = lstProviders[provPos];
				if(ProviderSelected != null)
				{
					ProviderSelected(this,new ProviderSelectedEventArgs(currentProvider, chbShowDelivered.Checked));
				}
			};

			return v;
		}

		public void ReloadProviders()
		{
			string all = GetString (Resource.String.All);
			lstProviders = Provider.GetProviders();
			lstProviders.Insert (0, new Provider (){ ProviderID = -1, Name = all, IsDefault = true });
			lvProviders.Adapter = new adapterProviders(this.Activity,lstProviders);
		}
	}
}

