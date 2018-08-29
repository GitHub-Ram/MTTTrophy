using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util.Jar;
using static Android.Views.View;

namespace MTTrophy
{
    public class TrophyFragment : DialogFragment, IOnTouchListener
    {
        public Action<Bitmap, LinearLayout> OnCAptureClick { get; set; }
        public Action<bool> ResetAcitivity { get; set; }
        public Action<bool> ChangeCameraFace { get; set; }
        public Action<bool> BackPressEvent { get; set; }
        //private Matrix matrix = new Matrix();
        private Matrix ReuseMatrix = new Matrix();
        private Matrix savedMatrix = new Matrix();
        private const int NONE = 0;
        private const int DRAG = 1;
        private const int ZOOM = 2;
        private int mode = NONE;
        private PointF start = new PointF();
        private PointF mid = new PointF();
        private float oldDist = 1f;
        private float d = 0f;
        private float newRot = 0f;
        private float[] lastEvent = null;
        private int screenWidth = 0;
        private int screenHeight = 0;
        private Handler mHandler ;

        FrameLayout ActionLayout;
        public LinearLayout progressBarLL;
        RelativeLayout relativeLayout, imageLayout,containerImg;
        int currentCameraId = 0;
        private Button btnCapture = null;
        ImageButton useOtherCamera = null;
        ImageView logoImageView, takenImage;// frameImageView;
        View ViewImage;
        Button back, save, buttonFlipcamera, BackLobby, buttonRetake;
        int ExtraItem = 0;


        public RecyclerView mRecycleView;
        RecyclerView.LayoutManager mLayoutManager;
        PhotoAlbum mPhotoAlbum;
        HorizontalTrouphyAdaptor mAdapter;
        public LinearLayout linearButton;
        public int selectedIndex = 0;

        Bitmap bitmap = null;
        internal TrophyCameraActivity context;
        public string TrophyName = string.Empty;

        public TrophyFragment(String trophyName, TrophyCameraActivity context)
        {
            this.context = context;
            this.TrophyName = trophyName;
        }

        public static TrophyFragment newInstance(String trophyName, TrophyCameraActivity context)
        {
            TrophyFragment frag = new TrophyFragment(trophyName, context);
            return frag;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            RelativeLayout root = new RelativeLayout(context);
            root.LayoutParameters = (new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
            DialogCustom dialog = new DialogCustom(this);
            dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
            dialog.SetContentView(root);
            dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
            dialog.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            dialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            return dialog;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            SetStyle(DialogFragmentStyle.Normal, Android.Resource.Style.ThemeTranslucentNoTitleBarFullScreen);
            Dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
            var view = inflater.Inflate(Resource.Layout.trophy_fragment, container);
            view.SetBackgroundColor(Color.Transparent);
            return view;
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            int ScreenHeight = (int)(Resources.DisplayMetrics.HeightPixels);
            int ScreenWidth = (int)(Resources.DisplayMetrics.WidthPixels);
            screenWidth = ScreenWidth;
            screenHeight = ScreenHeight;

            int actual = Resources.GetDimensionPixelSize(Resource.Dimension.dimen90);
            int numberofImages = ScreenWidth / actual;
            int increase = 0;
            if (numberofImages % 2 == 0)
            {
                increase = (numberofImages / 2) + 1;
            }
            else
            {
                increase = (numberofImages / 2) + 2;
            }
            ExtraItem = increase;
            mPhotoAlbum.PhotoAlbumUpdate(increase);
            mAdapter.NotifyDataSetChanged();

            mRecycleView.PostDelayed(() =>
            {
                mRecycleView.SmoothScrollToPosition(increase + context.LastSelectedIndex);
            }, 500);

            if (context.LastSelectedIndex == -1)
            {
                linearButton.SetBackgroundResource(Resource.Drawable.trophy_center_selection_circle);
                //logoImageView.SetImageResource(0);
                containerImg.RemoveAllViews();
            }
        }

        public override void OnStart()
        {
            base.OnStart();
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            mHandler = new Handler();
            progressBarLL = view.FindViewById<LinearLayout>(Resource.Id.LLPleaseWait);
            progressBarLL.Visibility = ViewStates.Gone;
            ActionLayout = view.FindViewById<FrameLayout>(Resource.Id.ActionLayout);
            ActionLayout.Visibility = ViewStates.Gone;
            mRecycleView = view.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            linearButton = view.FindViewById<LinearLayout>(Resource.Id.linearButton);
            linearButton.Enabled = true;
            mLayoutManager = new CenterLayoutManager(context, LinearLayoutManager.Horizontal, false);
            mRecycleView.SetLayoutManager(mLayoutManager);
            mRecycleView.AddOnScrollListener(new CustomScrollListener(this));
            buttonFlipcamera = view.FindViewById<Button>(Resource.Id.buttonFlipcamera);
            BackLobby = view.FindViewById<Button>(Resource.Id.buttonGoLobby);
            buttonRetake = view.FindViewById<Button>(Resource.Id.buttonRetake);
            SnapHelper helper = new LinearSnapHelpers();
            helper.AttachToRecyclerView(mRecycleView);
            //logoImageView = (ImageView)view.FindViewById(Resource.Id.logoImageView);
            //logoImageView.SetOnTouchListener(this);
            //ViewImage = (View)view.FindViewById(Resource.Id.logoImageView);
            takenImage = (ImageView)view.FindViewById(Resource.Id.ImageViewtaken);
            imageLayout = (RelativeLayout)view.FindViewById(Resource.Id.containerTaken);
            containerImg = (RelativeLayout)view.FindViewById(Resource.Id.containerImg);
            imageLayout.Visibility = ViewStates.Gone;
            back = (Button)view.FindViewById(Resource.Id.buttonBack);
            save = (Button)view.FindViewById(Resource.Id.buttonSave);

            linearButton.Click += delegate
            {
                if (bitmap != null)
                {
                    ReuseMatrix = new Matrix(context.matrix);
                    savedMatrix = new Matrix(context.matrix);
                    progressBarLL.Visibility = ViewStates.Visible;
                    OnCAptureClick(bitmap, linearButton);
                    //linearButton.Enabled = false;
                }
            };

            buttonFlipcamera.Click += (object sender, EventArgs e) =>
            {
                ChangeCameraFace(true);
            };

            BackLobby.Click += (object sender, EventArgs e) =>
            {
                BackPressEvent(true);
            };

            buttonRetake.Click += (object sender, EventArgs e) =>
            {
                imageLayout.Visibility = ViewStates.Gone;
                ActionLayout.Visibility = ViewStates.Visible;
                ResetAcitivity(true);
                //if (bitmap != null)
                //    logoImageView.SetImageBitmap(bitmap);
                //logoImageView.ImageMatrix = ReuseMatrix;
                containerImg.RemoveAllViews();
                containerImg.AddView(new SandboxView(Context, bitmap));
            };

            back.Click += delegate
            {
                BackPressEvent(true);
            };

            save.Click += delegate
            {
                //if (newBitmap == null)
                //    return;
                //Java.IO.File storagePath = new Java.IO.File(Android.OS.Environment.ExternalStorageDirectory + "/PhotoAR/");
                //storagePath.Mkdirs();
                //Java.IO.File myImage = new Java.IO.File(storagePath, Long.ToString(JavaSystem.CurrentTimeMillis()) + ".jpg");
                //try
                //{
                //    //string path = System.IO.Path.Combine(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures).AbsolutePath, "newProdict.png");
                //    string path = myImage.AbsolutePath;
                //    var fs = new FileStream(path, FileMode.OpenOrCreate);
                //    if (fs != null)
                //    {
                //        newBitmap.Compress(Bitmap.CompressFormat.Jpeg, 80, fs);
                //        fs.Close();
                //    }
                //    //Stream outs = new FileOutputStream(myImage);
                //    //newBitmap.Compress(Bitmap.CompressFormat.Jpeg, 80, outs);
                //    //out.flush();
                //    //out.close();
                //}
                //catch (System.Exception e)
                //{
                //    Log.Debug("In Saving File", e + "");
                //}
                //newBitmap.Recycle();
                //newBitmap.Dispose();
                //newBitmap = null;
                //System.GC.Collect();
            };

            Dialog dialog = Dialog;
            if (dialog != null)
            {
                int ScreenHeight = (int)(Resources.DisplayMetrics.HeightPixels);
                int ScreenWidth = (int)(Resources.DisplayMetrics.WidthPixels);
                screenWidth = ScreenWidth;
                screenHeight = ScreenHeight;

                Window window = Dialog.Window;
                WindowManagerLayoutParams windowParams = window.Attributes;
                windowParams.DimAmount = 0.0f;
                window.Attributes = (windowParams);

                int actual = Resources.GetDimensionPixelSize(Resource.Dimension.dimen90);
                int numberofImages = ScreenWidth / actual;
                int increase = 0;
                if (numberofImages % 2 == 0)
                {
                    increase = (numberofImages / 2) + 1;
                }
                else
                {
                    increase = (numberofImages / 2) + 2;
                }
                ExtraItem = increase;
                mPhotoAlbum = new PhotoAlbum(increase, context.BaseContext, TrophyName);
                mAdapter = new HorizontalTrouphyAdaptor(mPhotoAlbum);
                ActionLayout.Visibility = ViewStates.Visible;
                mRecycleView.SetAdapter(mAdapter);
                //if (context.LastSelectedIndex != -1)
                //{
                //    mRecycleView.PostDelayed(() => {
                //        mRecycleView.SmoothScrollToPosition(increase + context.LastSelectedIndex);
                //    }, 500);
                //    //mRecycleView.PostDelayed(() => {
                //    //    mRecycleView.FindViewHolderForAdapterPosition(increase + context.LastSelectedIndex).ItemView.PerformClick();
                //    //}, 1000);
                //    bitmap = mPhotoAlbum[increase + context.LastSelectedIndex].bitmap;
                //    logoImageView.SetImageBitmap(bitmap);
                //    logoImageView.ImageMatrix = context.matrix;
                //}
                mAdapter.ItemClick += (sender, e) =>
                {
                    if (e > ExtraItem - 1 && e - 1 < mPhotoAlbum.numPhoto - ExtraItem)
                    {
                        mRecycleView.SmoothScrollToPosition(e);
                        for (int i = 0; i < mPhotoAlbum.numPhoto; i++)
                        {
                            if (mPhotoAlbum[i].SelectedId != 0)
                            {
                                mPhotoAlbum[i].SelectedId = 0;
                                mAdapter.NotifyItemChanged(i);
                            }

                        }
                        mPhotoAlbum[e].SelectedId = e;
                        linearButton.SetBackgroundResource(Resource.Drawable.solid_circle_background_trophy);
                        mAdapter.NotifyItemChanged(e);
                        context.LastSelectedIndex = e - ExtraItem;
                        bitmap = mPhotoAlbum[e].bitmap; //BitmapFactory.DecodeResource(Resources, mPhotoAlbum[e].mPhotoID);

                        //bitmap = Bitmap.CreateScaledBitmap(bitmap, (int)context.Resources.GetDimension(Resource.Dimension.dimen120), (int)context.Resources.GetDimension(Resource.Dimension.dimen150), true);
                        //logoImageView.SetImageBitmap(bitmap);
                        containerImg.RemoveAllViews();
                        containerImg.AddView(new SandboxView(Context, bitmap));
                        //ViewImage = new SandboxView(Context,bitmap);
                        //RectF drawableRect = new RectF(0, 0, (int)context.Resources.GetDimension(Resource.Dimension.dimen120), (int)context.Resources.GetDimension(Resource.Dimension.dimen150));
                        //RectF viewRect = new RectF(0, 0, logoImageView.Width, logoImageView.Height);
                        //context.matrix.SetRectToRect(drawableRect, viewRect, Matrix.ScaleToFit.Center);

                        //logoImageView.ImageMatrix = context.matrix;
                    }
                };
            }
        }

        internal void AfterScroll()
        {
            CenterLayoutManager layoutManager = ((CenterLayoutManager)mRecycleView.GetLayoutManager());
            int lastVisisble = layoutManager.FindLastCompletelyVisibleItemPosition();
            int firstVisible = layoutManager.FindFirstCompletelyVisibleItemPosition();
            if ((lastVisisble - firstVisible) / 2 + firstVisible > ExtraItem - 1)
            {
                int e = (lastVisisble - firstVisible) / 2 + firstVisible;
                //mRecycleView.SmoothScrollToPosition(e);
                for (int i = 0; i < mPhotoAlbum.numPhoto; i++)
                {
                    if (mPhotoAlbum[i].SelectedId != 0)
                    {
                        mPhotoAlbum[i].SelectedId = 0;
                        mAdapter.NotifyItemChanged(i);
                    }

                }
                mPhotoAlbum[e].SelectedId = e;
                linearButton.SetBackgroundResource(Resource.Drawable.solid_circle_background_trophy);
                mAdapter.NotifyItemChanged(e);
                context.LastSelectedIndex = e - ExtraItem;
                bitmap = mPhotoAlbum[e].bitmap;
                //bitmap = Bitmap.CreateScaledBitmap(bitmap, (int)context.Resources.GetDimension(Resource.Dimension.dimen120), (int)context.Resources.GetDimension(Resource.Dimension.dimen150), true);
                //logoImageView.SetImageBitmap(bitmap);
                //ViewImage = new SandboxView(Context, bitmap);
                containerImg.RemoveAllViews();
                containerImg.AddView(new SandboxView(Context, bitmap));
                //RectF drawableRect = new RectF(0, 0, (int)context.Resources.GetDimension(Resource.Dimension.dimen120), (int)context.Resources.GetDimension(Resource.Dimension.dimen150));
                //RectF viewRect = new RectF(0, 0, logoImageView.Width, logoImageView.Height);
                //context.matrix.SetRectToRect(drawableRect, viewRect, Matrix.ScaleToFit.Center);

                //logoImageView.ImageMatrix = context.matrix;
            }
            else
            {
                for (int i = 0; i < mPhotoAlbum.numPhoto; i++)
                {
                    if (mPhotoAlbum[i].SelectedId != 0)
                    {
                        mPhotoAlbum[i].SelectedId = 0;
                        mAdapter.NotifyItemChanged(i);
                    }

                }
                linearButton.SetBackgroundResource(Resource.Drawable.trophy_center_selection_circle);
                //logoImageView.SetImageResource(0);
                containerImg.RemoveAllViews();
                //ViewImage = new SandboxView(Context, BitmapFactory.DecodeResource(context.Resources,0));
                context.LastSelectedIndex = -1;
                if (bitmap != null && !bitmap.IsRecycled)
                {
                    //bitmap.Recycle();
                    bitmap = null;
                    GC.Collect();
                }
            }
        }

        public void CapturedBitmap(Bitmap bitmap)
        {
            //ActionLayout.Visibility = ViewStates.Gone;
            progressBarLL.Visibility = ViewStates.Gone;
            imageLayout.Visibility = ViewStates.Visible;
            takenImage.SetImageBitmap(bitmap);
        }

        #region Motion
        /**
         * Determine the space between the first two fingers
         */
        private float Spacing(MotionEvent e)
        {
            try{
                float x = e.GetX(0) - e.GetX(1);
                float y = e.GetY(0) - e.GetY(1);
                return FloatMath.Sqrt(x * x + y * y);
            }catch(Exception ex){
                float x = e.GetX(0);
                float y = e.GetY(0);
                return FloatMath.Sqrt(x * x + y * y);
            }
        }

        /**
         * Calculate the mid point of the first two fingers
         */
        private void MidPoint(PointF point, MotionEvent e)
        {
            try{
                float x = e.GetX(0) + e.GetX(1);
                float y = e.GetY(0) + e.GetY(1);
                point.Set(x / 2, y / 2);
            }catch(Exception ex){
                float x = e.GetX(0) ;
                float y = e.GetY(0) ;
                point.Set(x / 2, y / 2);
            }

        }

        /**
         * Calculate the degree to be rotated by.
         *
         * @param event
         * @return Degrees
         */
        private float Rotation(MotionEvent e)
        {
            try{
                double delta_x = (e.GetX(0) - e.GetX(1));
                double delta_y = (e.GetY(0) - e.GetY(1));
                double radians = Java.Lang.Math.Atan2(delta_y, delta_x);
                return (float)Java.Lang.Math.ToDegrees(radians);
            }
            catch(Exception ex){
                double delta_x = (e.GetX(0) );
                double delta_y = (e.GetY(0) );
                double radians = Java.Lang.Math.Atan2(delta_y, delta_x);
                return (float)Java.Lang.Math.ToDegrees(radians);
            }
        }
        bool NOEvent = false;
        public bool OnTouch(View v, MotionEvent e)
        {
            if (NOEvent)
                return false;
            ImageView view = (ImageView)v;
            switch (e.Action & MotionEventActions.Mask)
            {
                case MotionEventActions.Down:
                    savedMatrix.Set(context.matrix);
                    start.Set(e.GetX(), e.GetY());
                    mode = DRAG;
                    lastEvent = null;
                    break;
                case MotionEventActions.PointerDown:
                    oldDist = Spacing(e);
                    if (oldDist > 10f)
                    {
                        savedMatrix.Set(context.matrix);
                        MidPoint(mid, e);
                        mode = ZOOM;
                    }
                    lastEvent = new float[4];
                    lastEvent[0] = e.GetX(0);
                    lastEvent[1] = e.GetX(1);
                    lastEvent[2] = e.GetY(0);
                    lastEvent[3] = e.GetY(1);
                    d = Rotation(e);
                    break;
                case MotionEventActions.Up:
                case MotionEventActions.PointerUp:
                    mode = NONE;
                    lastEvent = null;
                    break;
                case MotionEventActions.Move:
                    if (mode == DRAG)
                    {
                        Drawable drawable = view.Drawable;
                        RectF imageBounds = new RectF();

                        if (drawable != null)
                        {
                            context.matrix.MapRect(imageBounds, new RectF(drawable.Bounds));
                        }
                        if (e.GetX() < imageBounds.Left + imageBounds.Width() && e.GetX() > imageBounds.Left && e.GetY() < imageBounds.Top + imageBounds.Height() && e.GetY() > imageBounds.Top)
                        {
                            context.matrix.Set(savedMatrix);
                            float dx = e.GetX() - start.X;
                            float dy = e.GetY() - start.Y;
                            context.matrix.PostTranslate(dx, dy);
                        }else{
                            return true;
                        }
                    }
                    else if (mode == ZOOM)
                    {
                        float newDist = Spacing(e);

                        if (newDist > 10f)
                        {
                            int widthScrn = screenWidth > screenHeight ? screenHeight : screenWidth;
                            RectF rect = GetRect(view.Drawable, context.matrix);
                            context.matrix.Set(savedMatrix);
                            float scale = (newDist / oldDist);
                            context.matrix.PostScale(scale, scale, rect.CenterX(), rect.CenterY());

                            rect = GetRect(view.Drawable, context.matrix);
                            if (rect.Height() < 200)
                            {
                                //RectF rr = new RectF((int)rect.Left, (int)rect.Top, 200,200);
                                //context.matrix.SetRectToRect(rect,rr,Matrix.ScaleToFit.Center);
                                NOEvent = true;
                                mHandler.PostDelayed(() => { NOEvent = false; },200);
                                while (rect.Height() < 200)
                                {
                                    scale = scale + 0.05f;
                                    context.matrix.PostScale(scale, scale, rect.CenterX(), rect.CenterY());
                                    rect = GetRect(view.Drawable, context.matrix);
                                    //Console.WriteLine("Smaller Scale:" + scale+ " Height:"+rect.Height());
                                }
                                //Console.WriteLine("Smaller Scale break:" + scale);
                            
                            }else if(rect.Width() > widthScrn){
                                //RectF rr = new RectF((int)rect.Left, (int)rect.Top, widthScrn, widthScrn);
                                //context.matrix.SetRectToRect(rect, rr, Matrix.ScaleToFit.Center);
                                NOEvent = true;
                                mHandler.PostDelayed(() => { NOEvent = false; }, 200);
                                scale = 1;
                                while (rect.Width() > widthScrn){
                                    scale = scale - 0.05f;
                                    context.matrix.PostScale(scale , scale, rect.CenterX(), rect.CenterY());
                                    rect = GetRect(view.Drawable, context.matrix);
                                    //Console.WriteLine("Bigger Scale:" + scale+ " Height:" + rect.Height());
                                }
                                //Console.WriteLine("Bigger Scale break:" + scale);
                            }
                        }
                        if (lastEvent != null && e.PointerCount == 3)
                        {
                            newRot = Rotation(e);
                            float r = newRot - d;
                            float[] values = new float[9];
                            context.matrix.GetValues(values);
                            float tx = values[2];
                            float ty = values[5];
                            float sx = values[0];
                            float xc = (view.Width / 2) * sx;
                            float yc = (view.Height / 2) * sx;
                            context.matrix.PostRotate(r, tx + xc, ty + yc);
                        }
                    }
                    break;
            }
            view.ImageMatrix = (context.matrix);
            return true;
        }

        private RectF GetRect(Drawable drawable ,Matrix matrix){
            RectF rect = new RectF();
            if (drawable != null)
            {
                matrix.MapRect(rect, new RectF(drawable.Bounds));
            }
            return rect;
        }
        #endregion
    }

    public class CenterLayoutManager : LinearLayoutManager
    {

        public CenterLayoutManager(Context context) : base(context)
        {

        }

        public CenterLayoutManager(Context context, int orientation, bool reverseLayout) : base(context, orientation, reverseLayout)
        {

        }

        public CenterLayoutManager(Context context, Android.Util.IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {

        }

        public override void SmoothScrollToPosition(RecyclerView recyclerView, RecyclerView.State state, int position)
        {
            RecyclerView.SmoothScroller smoothScroller = new CenterSmoothScroller(recyclerView.Context);
            smoothScroller.TargetPosition = (position);
            StartSmoothScroll(smoothScroller);
        }

        private class CenterSmoothScroller : LinearSmoothScroller
        {

            public CenterSmoothScroller(Context context) : base(context)
            {

            }

            public override int CalculateDtToFit(int viewStart, int viewEnd, int boxStart, int boxEnd, int snapPreference)
            {
                return (boxStart + (boxEnd - boxStart) / 2) - (viewStart + (viewEnd - viewStart) / 2);
            }
        }
    }


    public class CustomScrollListener : RecyclerView.OnScrollListener
    {
        TrophyFragment trophyFragment;
        bool hasStartedScroll;
        public CustomScrollListener(TrophyFragment trophyFragment)
        {
            this.trophyFragment = trophyFragment;
        }

        public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
        {
            switch (newState)
            {
                case RecyclerView.ScrollStateIdle:
                    Console.WriteLine("The RecyclerView is not scrolling");
                    if (hasStartedScroll)
                    {
                        trophyFragment.AfterScroll();
                    }
                    hasStartedScroll = false;
                    break;
                case RecyclerView.ScrollStateDragging:
                    Console.WriteLine("Scrolling now");
                    hasStartedScroll = true;
                    break;
                case RecyclerView.ScrollStateSettling:
                    Console.WriteLine("Scroll Settling");
                    break;
            }
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
        }
    }
    public class DialogCustom : Dialog
    {
        TrophyFragment trophyFragment;
        public DialogCustom(TrophyFragment trophyFragment) : base(trophyFragment.context)
        {
            this.trophyFragment = trophyFragment;
        }

        public override void OnBackPressed()
        {
            trophyFragment.BackPressEvent(true);
        }
    }
}
