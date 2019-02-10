using System;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;

namespace MTTrophy
{
    public class HorizontalTrouphyAdaptor : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;
        public PhotoAlbum mPhotoAlbum;
        public HorizontalTrouphyAdaptor(PhotoAlbum photoAlbum)
        {
            mPhotoAlbum = photoAlbum;
        }
        public override int ItemCount
        {
            get { return mPhotoAlbum.numPhoto; }
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            PhotoViewHolder vh = holder as PhotoViewHolder;
            if(mPhotoAlbum[position].mPhotoID ==0)
                vh.imageView.SetImageResource(mPhotoAlbum[position].mPhotoID);
            else
                vh.imageView.SetImageBitmap(mPhotoAlbum[position].bitmap);
            if(mPhotoAlbum[position].mPhotoID!=0){
                vh.imageView.SetBackgroundResource(Resource.Drawable.circle_background_trophy);
            }else{
                vh.imageView.SetBackgroundResource(Resource.Drawable.circle_background_trophy_clear);
            }
            if(mPhotoAlbum[position].SelectedId != 0){
                vh.imageView.SetBackgroundResource(Resource.Drawable.circle_background_trophy_clear);
            }
            switch (TrophyCameraActivity.tempOrientRounded)
            {
                case 1:
                    vh.imageView.Rotation = 0;
                    break;
                case 2:
                    vh.imageView.Rotation = -90;
                    break;
                case 3:
                    vh.imageView.Rotation = -180;
                    break;
                case 4:
                    vh.imageView.Rotation = 90;
                    break;
            }
        }
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.trophy_image_templet, parent, false);
            PhotoViewHolder vh = new PhotoViewHolder(itemView, OnClick);
            return vh;
        }
        private void OnClick(int obj)
        {
            if (ItemClick != null)
                ItemClick(this, obj);
        }
    }
    public class PhotoViewHolder : RecyclerView.ViewHolder
    {
        public LinearLayout linearLayout { set; get; }
        public ImageView imageView { set; get; } 

        public PhotoViewHolder(View itemview,Action<int> listener) : base(itemview)
        {
            linearLayout = itemview.FindViewById<LinearLayout>(Resource.Id.circleBackground);
            imageView = itemview.FindViewById<ImageView>(Resource.Id.trouphyImage);
            itemview.Click += (sender, e) => listener(base.Position);
        }
    }
    public class Photo  
    {  
        public int mPhotoID { get; set; }
        public Bitmap bitmap { get; set; }
        public string mCaption { get; set; }  
        public int SelectedId { get; set; } 
        public string Hex { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float H { get; set; }
        public float W { get; set; }
        public float Roation { get; set; }
    }  
    public class PhotoAlbum  
    {
        int offset;
        Context context;
        string TrophyName;
        Photo[] listPhoto =
        {  
            new Photo() {mPhotoID = Resource.Mipmap.trophy_1, mCaption = "trophy 1",SelectedId=0,X=128,Y=486,W=490,H=112,Hex="#411809"},  

            new Photo() {mPhotoID = Resource.Mipmap.trophy_2, mCaption = "trophy 2",SelectedId=0,X=130,Y=450,W=485,H=104,Hex="#FFFFFF"},  
            new Photo() {mPhotoID = Resource.Mipmap.trophy_3, mCaption = "trophy 3",SelectedId=0,X=223,Y=333,W=312,H=150,Hex="#444445"},  
            new Photo() {mPhotoID = Resource.Mipmap.trophy_4, mCaption = "trophy 4",SelectedId=0,X=128,Y=811,W=496,H=110,Hex="#282829"} 
        };  
        private Photo[] photos; 

        public PhotoAlbum(int offset,Context context,string trophyName)
        {
            this.offset = offset;
            this.context = context;
            this.TrophyName = trophyName;
            Photo[] listPhotoOff = new Photo[offset];
            for (int i = 0; i < offset; i++){
                listPhotoOff[i] = new Photo() { mPhotoID = 0, mCaption = "trophy 0", SelectedId = 0 };
            }
            Photo[] listPhotoOffLast = new Photo[offset-1];
            for (int i = 0; i < offset-1; i++)
            {
                listPhotoOffLast[i] = new Photo() { mPhotoID = 0, mCaption = "trophy 0", SelectedId = 0 };
            }
            listPhoto[0].bitmap = drawTextToBitmap( listPhoto[0]);
            listPhoto[1].bitmap = drawTextToBitmap( listPhoto[1]);
            listPhoto[2].bitmap = drawTextToBitmap( listPhoto[2]);
            listPhoto[3].bitmap = drawTextToBitmap( listPhoto[3]);

            Photo[] combined = listPhotoOff.Concat(listPhoto).ToArray();
            Photo[] combinedLast = combined.Concat(listPhotoOffLast).ToArray();
            this.photos = combinedLast;  
        }  

        public void PhotoAlbumUpdate(int offset)
        {
            this.offset = offset;
            Photo[] listPhotoOff = new Photo[offset];
            for (int i = 0; i < offset; i++){
                listPhotoOff[i] = new Photo() { mPhotoID = 0, mCaption = "trophy 0", SelectedId = 0 };
            }
            Photo[] listPhotoOffLast = new Photo[offset-1];
            for (int i = 0; i < offset-1; i++)
            {
                listPhotoOffLast[i] = new Photo() { mPhotoID = 0, mCaption = "trophy 0", SelectedId = 0 };
            }
            Photo[] combined = listPhotoOff.Concat(listPhoto).ToArray();
            Photo[] combinedLast = combined.Concat(listPhotoOffLast).ToArray();
            this.photos = combinedLast; 
        }  

        public int numPhoto  
        {  
            get  
            {  
                return photos.Length;  
            }  
        }  
        public Photo this[int i]  
        {  
            get { return photos[i]; }  
        }  

        public Bitmap drawTextToBitmap(Photo photo)
        {
            Android.Content.Res.Resources resources = context.Resources;
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InScaled = false; 
            Bitmap bitmap = BitmapFactory.DecodeResource(resources, photo.mPhotoID,options);
            try
            {
                float scale = resources.DisplayMetrics.Density;
                Bitmap.Config bitmapConfig = bitmap.GetConfig();
                // set default bitmap config if none
                if (bitmapConfig == null)
                {
                    bitmapConfig = Bitmap.Config.Argb8888;
                }
                // resource bitmaps are imutable,
                // so we need to convert it to mutable one
                bitmap = bitmap.Copy(bitmapConfig, true);
                Console.WriteLine("Height:" + bitmap.Height + " Width:" + bitmap.Width);
                Canvas canvas = new Canvas(bitmap);
                Console.WriteLine("Canvas Height:" + canvas.Height + " Width:" + canvas.Width);
                TextPaint mTextPaint = new TextPaint(PaintFlags.AntiAlias);
                mTextPaint.Color = Color.ParseColor(photo.Hex);
                mTextPaint.SetTypeface(Typeface.Create(Typeface.DefaultBold, TypefaceStyle.Bold));
                int size = 30;
                mTextPaint.TextSize = ((int)(size * scale));
                StaticLayout mTextLayout = new StaticLayout(TrophyName, mTextPaint, (int)photo.W, Layout.Alignment.AlignCenter, 1.0f, 0.0f, false);
                while(true){
                    if (mTextLayout.Height > photo.H)
                    {
                        mTextPaint.TextSize = (((size --) * scale));
                        mTextLayout = new StaticLayout(TrophyName, mTextPaint, (int)photo.W, Layout.Alignment.AlignCenter, 1.0f, 0.0f, false);
                    }
                    else break;
                }
                canvas.Save();
                canvas.Translate(photo.X, photo.Y+(photo.H - mTextLayout.Height)/2);
                mTextLayout.Draw(canvas);
                canvas.Restore();
            }
            catch (Exception e)
            {
                Console.WriteLine("Bitmap Text Merge Exception:"+e.ToString());
            }
            return bitmap;
        }
    }  
}
