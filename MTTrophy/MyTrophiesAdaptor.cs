
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MTTrophy
{
    public class MyTrophiesAdaptor : BaseAdapter<MyTrophyModel>
    {
        List<MyTrophyModel> items;
        Activity context;
        Typeface typefaceAvanti, typefacePaddingtonBold;
        int ItemPosition = 0;


        public MyTrophiesAdaptor(Activity context, List<MyTrophyModel> EntityList) : base()
        {
            this.context = context;
            typefaceAvanti = Typeface.CreateFromAsset(context.Assets, "fonts/AvantiProB_0.otf");
            typefacePaddingtonBold = Typeface.CreateFromAsset(context.Assets, "fonts/Paddington_Bold_0.ttf");
            this.items = EntityList;
        }

        public List<MyTrophyModel> GetRoomList()
        {
            return items;
        }

        public void SetValues(List<MyTrophyModel> EntityList)
        {
            this.items = EntityList;
            NotifyDataSetChanged();
        }

        public override MyTrophyModel this[int position]
        {
            get
            {
                MyTrophyModel result = new MyTrophyModel();
                if ((items != null) && (items.Count > 0) && (position < items.Count))
                {
                    result = items[position] == null ? null : items[position];
                }
                else
                {
                    result = items == null ? null : items[position];
                }

                return result;
            }

        }

        public override int Count
        {
            get { return items == null ? 0 : items.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }


        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            if (view == null)
            {

                view = this.context.LayoutInflater.Inflate(Resource.Layout.trophies_list_templet, null);
            }
            try
            {
                if (items != null)
                {
                    //var item = items[position] == null ? items[ItemPosition] : items[position];
                    var item = items[position] == null ? null : items[position];
                    if (item != null)
                    {
                        ViewHolderTrophy.trophyName = view.FindViewById<TextView>(Resource.Id.trophy_name);
                        //ViewHolderTrophy.cameraImage = view.FindViewById<ImageButton>(Resource.Id.camera_button);
                        ViewHolderTrophy.trophyName.SetTypeface(typefacePaddingtonBold, TypefaceStyle.Normal);
                        ViewHolderTrophy.trophyName.Text = item.TrouphyName;
                        if (position % 2 == 0)
                            view.SetBackgroundResource(Resource.Mipmap.rowEven);
                        else
                            view.SetBackgroundResource(Resource.Mipmap.rowOdd);
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
            return view;
        }

        public override void NotifyDataSetChanged()
        {
            base.NotifyDataSetChanged();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            //baseContext.Dispose();
        }

    }

    public static class ViewHolderTrophy
    {
        public static ImageButton cameraImage;
        public static TextView trophyName;

    }
}
