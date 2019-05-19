using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Util.Jar;
using static Android.Provider.MediaStore;
using static Android.Views.View;

namespace MTTrophy
{
    public class TrophyFragment : DialogFragment
    {
        public Action<Bitmap, LinearLayout> OnCAptureClick { get; set; }
        public Action<bool> ResetAcitivity { get; set; }
        public Action<bool> ChangeCameraFace { get; set; }
        public Action<bool> BackPressEvent { get; set; }
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
        private MatrixConfig matrixConfig;

        FrameLayout ActionLayout;
        public LinearLayout progressBarLL;
        RelativeLayout relativeLayout, imageLayout,containerImg;
        int currentCameraId = 0;
        private Button btnCapture = null;
        internal ImageButton useOtherCamera = null;
        internal ImageView logoImageView, takenImage;// frameImageView;
        View ViewImage;
        internal Button back, save, buttonFlipcamera, BackLobby, buttonRetake;
        int ExtraItem = 0;
        internal static SandboxView sandboxView = null;

        public RecyclerView mRecycleView;
        RecyclerView.LayoutManager mLayoutManager;
        PhotoAlbum mPhotoAlbum;
        HorizontalTrouphyAdaptor mAdapter;
        public LinearLayout linearButton;
        public int selectedIndex = 0;

        Bitmap bitmap, bitmapToSave = null;
        internal TrophyCameraActivity context;
        public string TrophyName = string.Empty;

        public TrophyFragment(string trophyName, TrophyCameraActivity context)
        {
            this.context = context;
            this.TrophyName = trophyName;
        }

        public static TrophyFragment newInstance(string trophyName, TrophyCameraActivity context)
        {
            TrophyFragment frag = new TrophyFragment(trophyName, context);
            return frag;
        }

        //public override Dialog OnCreateDialog(Bundle savedInstanceState)
        //{
        //    RelativeLayout root = new RelativeLayout(context);
        //    root.LayoutParameters = (new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
        //    DialogCustom dialog = new DialogCustom(this);
        //    dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
        //    dialog.SetContentView(root);
        //    dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
        //    dialog.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
        //    dialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
        //    return dialog;
        //}

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            //SetStyle(DialogFragmentStyle.Normal, Android.Resource.Style.ThemeDeviceDefaultLightNoActionBarFullscreen);
            Dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
            Dialog.Window.ClearFlags(WindowManagerFlags.DimBehind);
            Dialog.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
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
                containerImg.RemoveAllViews();
            }
        }

        public override void OnStart()
        {
            base.OnStart();
            //Dialog dialog = Dialog;
            //if (dialog != null)
            //{
            //    int width = ViewGroup.LayoutParams.MatchParent;
            //    int height = ViewGroup.LayoutParams.MatchParent;
            //    dialog.Window.SetLayout(width, height);
            //}
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
            layoutManager = ((CenterLayoutManager)mRecycleView.GetLayoutManager());
            linearButton.Click += delegate
            {
                if (bitmap != null)
                {
                    progressBarLL.Visibility = ViewStates.Visible;
                    context.matrix.Set(matrixConfig.transform);
                    OnCAptureClick(bitmap, linearButton);
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
                containerImg.RemoveAllViews();
                if(matrixConfig==null){
                    matrixConfig = new MatrixConfig();
                }
                containerImg.AddView(new SandboxView(Context, bitmap,matrixConfig));
            };

            back.Click += delegate
            {
                BackPressEvent(true);
            };

            save.Click += delegate
            {
                if (bitmapToSave == null)
                    return;
                try
                {
                    MediaStore.Images.Media.InsertImage(context.ContentResolver, bitmapToSave, Long.ToString(JavaSystem.CurrentTimeMillis()) + ".jpg", "Adda52");
                }
                catch (System.Exception e)
                {
                    Log.Debug("In Saving File", e + "");
                }
                bitmapToSave.Recycle();
                bitmapToSave.Dispose();
                bitmapToSave = null;
                System.GC.Collect();
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
                        bitmap = mPhotoAlbum[e].bitmap;
                        containerImg.RemoveAllViews();
                        if (matrixConfig == null)
                        {
                            matrixConfig = new MatrixConfig();
                        }
                        sandboxView = new SandboxView(Context, bitmap, matrixConfig);
                        containerImg.AddView(sandboxView);
                    }
                };
            }
        }

        internal static CenterLayoutManager layoutManager;
        internal void AfterScroll()
        {
            int lastVisisble = layoutManager.FindLastCompletelyVisibleItemPosition();
            int firstVisible = layoutManager.FindFirstCompletelyVisibleItemPosition();
            if ((lastVisisble - firstVisible) / 2 + firstVisible > ExtraItem - 1)
            {
                int e = (lastVisisble - firstVisible) / 2 + firstVisible;
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
                containerImg.RemoveAllViews();
                if (matrixConfig == null)
                {
                    matrixConfig = new MatrixConfig();
                }
                containerImg.AddView(new SandboxView(Context, bitmap,matrixConfig));
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
                containerImg.RemoveAllViews();
                context.LastSelectedIndex = -1;
                if (bitmap != null && !bitmap.IsRecycled)
                {
                    bitmap = null;
                    GC.Collect();
                }
            }
        }

        public void CapturedBitmap(Bitmap bitmap)
        {
            progressBarLL.Visibility = ViewStates.Gone;
            imageLayout.Visibility = ViewStates.Visible;
            bitmapToSave = bitmap;
            takenImage.SetImageBitmap(bitmap);
        }

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

    class LinearSnapHelpers : LinearSnapHelper
    {

        public override int FindTargetSnapPosition(RecyclerView.LayoutManager layoutManager, int velocityX, int velocityY)
        {
            var ll = (layoutManager as RecyclerView.SmoothScroller.IScrollVectorProvider);
            if (ll == null)
            {
                return RecyclerView.NoPosition;
            }

            View centerView = FindSnapView(layoutManager);

            if (centerView == null)
            {
                return RecyclerView.NoPosition;
            }

            int currentPosition = layoutManager.GetPosition(centerView);

            if (currentPosition == RecyclerView.NoPosition)
            {
                return RecyclerView.NoPosition;
            }

            return currentPosition;
        }
    }
}
