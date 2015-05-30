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

namespace MessangeroTracking
{
	public class adapterVouchers : ArrayAdapter<VoucherInfo>
	{
		public delegate void ItemSelectedChangedDelegate();
		public event ItemSelectedChangedDelegate ItemSelectedChanged;
		public ListView parent;

		Activity context;

		public adapterVouchers(Activity context, List<VoucherInfo> list)
			: base(context, Resource.Layout.row_voucher_status, list)
		{
			this.context = context;
		}

		public void SelectAll()
		{
			for (int i = 0; i < Count; i++)
			{
				VoucherInfo vi = GetItem(i);
				vi.IsSelected = true;
				View v = parent.GetChildAt (i);
				if (v != null)
				{
					((VoucherViewHolder)v.Tag).chbSelected.Checked = true;
				}
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			VoucherViewHolder holder;
			//var item = this.TransHedList[position];
			var item = this.GetItem (position);
			View view = convertView;
			if (view == null) 
			{
				view = context.LayoutInflater.Inflate (Resource.Layout.row_voucher_info, null);
				holder = new VoucherViewHolder ();

				holder.tvName = view.FindViewById<TextView> (Resource.Id.tvName);
				holder.tvNumber = view.FindViewById<TextView> (Resource.Id.tvNumber);
				holder.tvDate = view.FindViewById<TextView> (Resource.Id.tvDate);
				holder.tvLastStatus = view.FindViewById<TextView> (Resource.Id.tvLastStatus);
				holder.tvProvider = view.FindViewById<TextView> (Resource.Id.tvProvider);
				holder.chbSelected = view.FindViewById<CheckBox> (Resource.Id.chbSelect);
				holder.ivProvider = view.FindViewById<ImageView> (Resource.Id.ivProvider);
				holder.tvDelivered = view.FindViewById<TextView> (Resource.Id.tvDelivered);

				view.Tag = holder;

				holder.chbSelected.CheckedChange += new EventHandler<CompoundButton.CheckedChangeEventArgs> ((o, e) => {

					VoucherInfo vi = ((VoucherInfo)((CheckBox)o).Tag);
					vi.IsSelected = e.IsChecked;

					if(ItemSelectedChanged != null)
					{
						ItemSelectedChanged();	
					}
				});
			} else 
			{
				holder = ((VoucherViewHolder)view.Tag);
			}
			holder.Voucher = item;

			holder.chbSelected.Tag = item;
			holder.chbSelected.Checked = item.IsSelected;
			holder.tvName.Text = item.Name;
			holder.tvNumber.Text = "(" + item.Number + ")" ;
			holder.tvDate.Text = item.LastStatusDate + " " + item.LastStatusTime;
			holder.tvLastStatus.Text = item.LastStatusArea + " " + item.LastStatus;
			holder.tvProvider.Text = item.ProviderName;
			holder.ivProvider.SetImageURI (Android.Net.Uri.Parse (item.GetImagePath ()));
			if(item.IsDelivered)
				holder.tvDelivered.Visibility = ViewStates.Visible;
			else
				holder.tvDelivered.Visibility = ViewStates.Gone;

			try
			{
				if(!string.IsNullOrEmpty(item.ProviderColor))
					holder.tvProvider.SetTextColor (Android.Graphics.Color.ParseColor("#" + item.ProviderColor));
				else
					holder.tvProvider.SetTextColor (new Android.Graphics.Color (231,169,71));
			}
			catch {
				holder.tvProvider.SetTextColor (new Android.Graphics.Color (231,169,71));
			}

			return view;
		}
	}

	public class VoucherViewHolder: Java.Lang.Object
	{
		public VoucherInfo Voucher{get;set;}
		public TextView tvName{ get; set;}
		public TextView tvNumber{ get; set;}
		public TextView tvLastStatus{ get; set;}
		public TextView tvDate{ get; set;}
		public TextView tvProvider{ get; set;}
		public CheckBox chbSelected{ get; set;}
		public ImageView ivProvider{ get; set;}
		public TextView tvDelivered{ get; set;}

		public void UpdateLastStatusFields()
		{
			tvDate.Text = Voucher.LastStatusDate + " " + Voucher.LastStatusTime;
			tvLastStatus.Text = Voucher.LastStatusArea + " " + Voucher.LastStatus;
		}

	}
}

