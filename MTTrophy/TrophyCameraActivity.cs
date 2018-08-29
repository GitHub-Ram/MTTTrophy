using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V4.App;
using static Android.Views.View;
using Android.Views;
using Android.Graphics;
using Android.Runtime;
using Java.Lang;
using Android.Util;
using Android.Hardware;
using System;
using Android.Content.Res;
using Java.IO;
using System.IO;
using System.Collections.Generic;
using Android.Content.PM;
using Android.Support.V7.Widget;
using System.Threading.Tasks;
using Android.Content;

namespace MTTrophy
{
    [Activity(Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = ScreenOrientation.Unspecified, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout, UiOptions = UiOptions.SplitActionBarWhenNarrow)]
    public class TrophyCameraActivity : Activity
    {
        internal Matrix matrix = new Matrix();
        Preview mPreview;
        private PointF start = new PointF();
        private PointF mid = new PointF();
        //private RelativeLayout relativeLayout;
        //LinearLayout linearButton;
        Bitmap bitmaptrophy = null;
        private Android.Hardware.Camera mCamera = null;
        //private SurfaceView cameraSurfaceView = null;
        //private ISurfaceHolder cameraSurfaceHolder = null;
        internal bool previewing = false;
        private Button btnCapture = null;
        public int LastSelectedIndex = -1;
        Android.Hardware.CameraFacing currentCameraId = Android.Hardware.Camera.CameraInfo.CameraFacingBack;

        Android.App.FragmentManager fmanager;
        internal TrophyFragment trophyFragment;
        public string TrophyName = string.Empty;


        int numberOfCameras;
        int cameraCurrentlyLocked;

        // The first rear facing camera
        int defaultCameraId;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //GameApplication.Current.Trophycameraactivity = this;
            Window.SetFormat(Format.Translucent);
            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            mPreview = new Preview(this);
            SetContentView(mPreview);
            //SetContentView(Resource.Layout.mytrophies);
            //relativeLayout = (RelativeLayout)FindViewById(Resource.Id.relativeLayout);
            fmanager = FragmentManager;

            //save.Click += delegate
            //{
            //    if (newBitmap == null)
            //        return;
            //    Java.IO.File storagePath = new Java.IO.File(Android.OS.Environment.ExternalStorageDirectory + "/PhotoAR/");
            //    storagePath.Mkdirs();

            //    Java.IO.File myImage = new Java.IO.File(storagePath, Long.ToString(JavaSystem.CurrentTimeMillis()) + ".jpg");

            //    try
            //    {
            //        //string path = System.IO.Path.Combine(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures).AbsolutePath, "newProdict.png");
            //        string path = myImage.AbsolutePath;
            //        var fs = new FileStream(path, FileMode.OpenOrCreate);
            //        if (fs != null)
            //        {
            //            newBitmap.Compress(Bitmap.CompressFormat.Jpeg, 80, fs);
            //            fs.Close();
            //        }
            //        //Stream outs = new FileOutputStream(myImage);
            //        //newBitmap.Compress(Bitmap.CompressFormat.Jpeg, 80, outs);

            //        //out.flush();
            //        //out.close();
            //    }
            //    catch (System.Exception e)
            //    {
            //        Log.Debug("In Saving File", e + "");
            //    }
            //    newBitmap.Recycle();
            //    newBitmap.Dispose();
            //    newBitmap = null;
            //    System.GC.Collect();
            //};

            Bundle extras = Intent.Extras;
            if (extras != null)
            {
                TrophyName = extras.GetString("TrophyName");
            }
            //cameraSurfaceView = (SurfaceView)FindViewById(Resource.Id.surfaceView);
            //cameraSurfaceHolder = cameraSurfaceView.Holder;
            //cameraSurfaceHolder.AddCallback(this);


            trophyFragment = TrophyFragment.newInstance(TrophyName, this);
            trophyFragment.Show(fmanager, "fragment_edit_name");
            trophyFragment.Cancelable = false;
            trophyFragment.OnCAptureClick += (Bitmap obj, LinearLayout linearButton) => {
                if(obj!=null){
                    try{
                        bitmaptrophy = obj;
                        //this.linearButton = linearButton;
                        mCamera.TakePicture(null, null, mPreview);
                    }catch(System.Exception ex){
                        //trophyFragment.linearButton.Enabled = true;
                        trophyFragment.progressBarLL.Visibility = ViewStates.Gone;
                        System.Console.WriteLine("Take Picture Exception:"+ex.ToString());
                    }
                }
            };

            trophyFragment.ResetAcitivity += (bool obj) => {
                System.GC.Collect();
            };

            trophyFragment.BackPressEvent += (bool obj) => {
                trophyFragment.Dismiss();
                //try
                //{
                //    ActivityManager activityMgr = (ActivityManager)ApplicationContext.GetSystemService(Context.ActivityService);
                //    IList<ActivityManager.RunningTaskInfo> tasks = activityMgr.GetRunningTasks(1);
                //    if (tasks.Count > 2)
                //    { }
                //    else
                //    {
                //        this.StartActivity(typeof(MainActivity));
                //    }
                //}
                //catch (System.Exception ex)
                //{
                //    System.Console.WriteLine("Get Activity Stack:" + ex.ToString());
                //}
                //GameApplication.Current.MinimizedActivity = string.Empty;
                Finish();
            };

            trophyFragment.ChangeCameraFace += (bool obj) => {
                if (previewing)
                {
                    mCamera.StopPreview();
                }
                //NB: if you don't release the current camera before switching, you app will crash
                mCamera.Release();

                //swap the id of the camera to be used
                if (currentCameraId == Android.Hardware.Camera.CameraInfo.CameraFacingBack)
                {
                    currentCameraId = Android.Hardware.Camera.CameraInfo.CameraFacingFront;
                }
                else
                {
                    currentCameraId = Android.Hardware.Camera.CameraInfo.CameraFacingBack;
                }
                mCamera = Android.Hardware.Camera.Open((int)currentCameraId);

                SetCameraDisplayOrientation(this, (int)currentCameraId, mCamera);
                try
                {

                    mCamera.SetPreviewDisplay(mPreview.mHolder);
                }
                catch (System.Exception exx)
                {
                    System.Console.WriteLine("Exception Changeing Camera:" + exx.ToString());
                }
                mCamera.StartPreview();
            };

            numberOfCameras = Android.Hardware.Camera.NumberOfCameras;

            // Find the ID of the default camera
            Android.Hardware.Camera.CameraInfo cameraInfo = new Android.Hardware.Camera.CameraInfo();
            for (int i = 0; i < numberOfCameras; i++)
            {
                Android.Hardware.Camera.GetCameraInfo(i, cameraInfo);
                if (cameraInfo.Facing == CameraFacing.Back)
                {
                    defaultCameraId = i;
                }
            }
        }

        /*public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            if (previewing)
            {
                camera.StopPreview();
                previewing = false;
            }
            try
            {
                float w = 0;
                float h = 0;
                if (this.Resources.Configuration.Orientation != Android.Content.Res.Orientation.Landscape)
                {
                    camera.SetDisplayOrientation(90);
                    Android.Hardware.Camera.Size cameraSize = camera.GetParameters().PictureSize;
                    int wr = relativeLayout.Width;
                    int hr = relativeLayout.Height;
                    float ratio = relativeLayout.Width * 1f / cameraSize.Height;
                    w = cameraSize.Width * ratio;
                    h = cameraSize.Height * ratio;
                    //RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams((int)h, (int)w);
                    //cameraSurfaceView.LayoutParameters = (lp);
                }
                else
                {
                    camera.SetDisplayOrientation(0);
                    Android.Hardware.Camera.Size cameraSize = camera.GetParameters().PictureSize;
                    float ratio = relativeLayout.Height * 1f / cameraSize.Height;
                    w = cameraSize.Width * ratio;
                    h = cameraSize.Height * ratio;
                    //RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams((int)w, (int)h);
                    //cameraSurfaceView.LayoutParameters = (lp);
                }
                camera.SetPreviewDisplay(cameraSurfaceHolder);
                camera.StartPreview();
                previewing = true;
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("SurfaceChanged:" + e.ToString());
            }
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            try
            {
                camera = Android.Hardware.Camera.Open();
                Android.Hardware.Camera.Parameters param = camera.GetParameters();

                // Check what resolutions are supported by your camera
                IList<Android.Hardware.Camera.Size> sizes = param.SupportedPictureSizes;

                // setting small image size in order to avoid OOM error
                Android.Hardware.Camera.Size cameraSize = null;
                foreach (Android.Hardware.Camera.Size size in sizes)
                {
                    //set whatever size you need
                    //if(size.height<500) {
                    cameraSize = size;
                    break;
                    //}
                }

                if (cameraSize != null)
                {
                    param.SetPictureSize(cameraSize.Width, cameraSize.Height);
                    camera.SetParameters(param);

                    float ratio = relativeLayout.Height * 1f / cameraSize.Height;
                    float w = cameraSize.Width * ratio;
                    float h = cameraSize.Height * ratio;
                    //RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams((int)w, (int)h);
                    //cameraSurfaceView.LayoutParameters = (lp);
                }
            }
            catch (RuntimeException e)
            {
                Toast.MakeText(
                        ApplicationContext,
                        "Device camera  is not working properly, please try after sometime.",
                    ToastLength.Long).Show();
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            // TODO Auto-generated method stub
            camera.StopPreview();
            camera.Release();
            camera = null;
            previewing = false;
        }*/

        internal async Task<Bitmap> ProcessImage(byte[] data, Android.Hardware.Camera camera)
        {
            Bitmap newBitmap = null;
            try
            {
                // TODO Auto-generated method stub
                BitmapFactory.Options options = new BitmapFactory.Options();
                //o.inJustDecodeBounds = true;
                Bitmap cameraBitmapNull = BitmapFactory.DecodeByteArray(data, 0, data.Length, options);

                int wid = options.OutWidth;
                int hgt = options.OutHeight;
                Matrix nm = new Matrix();

                Android.Hardware.Camera.Size cameraSize = camera.GetParameters().PictureSize;
                float ratio = mPreview.Height * 1f / cameraSize.Height;
                if (Resources.Configuration.Orientation != Android.Content.Res.Orientation.Landscape)
                {
                    nm.PostRotate(90);
                    nm.PostTranslate(hgt, 0);
                    wid = options.OutHeight;
                    hgt = options.OutWidth;
                    ratio = mPreview.Width * 1f / cameraSize.Height;

                }
                else
                {
                    wid = options.OutWidth;
                    hgt = options.OutHeight;
                    ratio = mPreview.Height * 1f / cameraSize.Height;
                }

                float[] f = new float[9];
                matrix.GetValues(f);
                f[0] = f[0] / ratio;
                f[4] = f[4] / ratio;
                f[5] = f[5] / ratio;
                f[2] = f[2] / ratio;


                matrix.SetValues(f);
                newBitmap = Bitmap.CreateBitmap(wid, hgt, Bitmap.Config.Argb8888);

                Canvas canvas = new Canvas(newBitmap);
                Bitmap cameraBitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length, options);

                canvas.DrawBitmap(cameraBitmap, nm, null);


                canvas.DrawBitmap(bitmaptrophy, matrix, null);
                camera.StartPreview();
                //bitmaptrophy.Recycle();
                //bitmaptrophy.Dispose();
                //bitmaptrophy = null;
                //System.GC.Collect();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("ON PICTURE Ex:" + ex.ToString());
            }
            return newBitmap;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            trophyFragment.Dismiss();
            //try
            //{
            //    ActivityManager activityMgr = (ActivityManager)ApplicationContext.GetSystemService(Context.ActivityService);
            //    IList<ActivityManager.RunningTaskInfo> tasks = activityMgr.GetRunningTasks(1);
            //    if (tasks.Count > 2)
            //    { }
            //    else
            //    {
            //        this.StartActivity(typeof(MainActivity));
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    System.Console.WriteLine("Get Activity Stack:" + ex.ToString());
            //}
            //GameApplication.Current.MinimizedActivity = string.Empty;
            Finish();

        }

        protected override void OnResume()
        {
            base.OnResume();
            mCamera = Android.Hardware.Camera.Open();
            cameraCurrentlyLocked = defaultCameraId;
            mPreview.PreviewCamera = mCamera;
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        protected override void OnPause()
        {
            base.OnPause();
            // Because the Camera object is a shared resource, it's very
            // important to release it when the activity is paused.
            if (mCamera != null)
            {
                mPreview.PreviewCamera = null;
                mCamera.Release();
                mCamera = null;
            }
        }

        public static void SetCameraDisplayOrientation(Activity activity, int cameraId, Android.Hardware.Camera camera)
        {
            Android.Hardware.Camera.CameraInfo info = new Android.Hardware.Camera.CameraInfo();
            Android.Hardware.Camera.GetCameraInfo(cameraId, info);
            //IWindowManager windowManager = activity.GetSystemService(activity.WindowManager).JavaCast<IWindowManager>();
            SurfaceOrientation rotation = activity.WindowManager.DefaultDisplay.Rotation;
            int degrees = 0;
            switch (rotation)
            {
                case SurfaceOrientation.Rotation0: degrees = 0; break;
                case SurfaceOrientation.Rotation90: degrees = 90; break;
                case SurfaceOrientation.Rotation180: degrees = 180; break;
                case SurfaceOrientation.Rotation270: degrees = 270; break;
            }

            int result;
            if (info.Facing == Android.Hardware.Camera.CameraInfo.CameraFacingFront)
            {
                result = (info.Orientation + degrees) % 360;
                result = (360 - result) % 360;  // compensate the mirror
            }
            else
            {  // back-facing
                result = (info.Orientation - degrees + 360) % 360;
            }
            camera.SetDisplayOrientation(result);
        }

        /*private Android.Hardware.Camera.Size getOptimalPreviewSize(List<Android.Hardware.Camera.Size> sizes, int w, int h)
        {
            double ASPECT_TOLERANCE = 0.1;
            double targetRatio = (double)h / w;

            if (sizes == null)
                return null;

            Android.Hardware.Camera.Size optimalSize = null;
            double minDiff = System.Double.MaxValue;

            int targetHeight = h;

            foreach (Android.Hardware.Camera.Size size in sizes)
            {
                double ratio = (double)size.Height / size.Width;
                if (Java.Lang.Math.Abs(ratio - targetRatio) > ASPECT_TOLERANCE)
                    continue;

                if (Java.Lang.Math.Abs(size.Height - targetHeight) < minDiff)
                {
                    optimalSize = size;
                    minDiff = Java.Lang.Math.Abs(size.Height - targetHeight);
                }
            }

            if (optimalSize == null)
            {
                minDiff = Java.Lang.Double.MaxValue;
                foreach (Android.Hardware.Camera.Size size in sizes)
                {
                    if (Java.Lang.Math.Abs(size.Height - targetHeight) < minDiff)
                    {
                        optimalSize = size;
                        minDiff = Java.Lang.Math.Abs(size.Height - targetHeight);
                    }
                }
            }

            return optimalSize;
        }*/
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

    class Preview : ViewGroup, ISurfaceHolderCallback, Android.Hardware.Camera.IPictureCallback
    {
        string TAG = "Preview";
        TrophyCameraActivity _Context;
        SurfaceView mSurfaceView;
        internal ISurfaceHolder mHolder;
        Android.Hardware.Camera.Size mPreviewSize;
        IList<Android.Hardware.Camera.Size> mSupportedPreviewSizes;
        internal Android.Hardware.Camera _camera;

        public Android.Hardware.Camera PreviewCamera
        {
            get { return _camera; }
            set
            {
                _camera = value;
                if (_camera != null)
                {
                    mSupportedPreviewSizes = PreviewCamera.GetParameters().SupportedPreviewSizes;
                    RequestLayout();
                }
            }
        }

        public Preview(TrophyCameraActivity context) : base(context.ApplicationContext)
        {
            _Context = context;
            mSurfaceView = new SurfaceView(context);
            //RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            //mSurfaceView.LayoutParameters = lp;
            AddView(mSurfaceView);

            // Install a SurfaceHolder.Callback so we get notified when the
            // underlying surface is created and destroyed.
            mHolder = mSurfaceView.Holder;
            mHolder.AddCallback(this);
            mHolder.SetType(SurfaceType.PushBuffers);
        }

        public void SwitchCamera(Android.Hardware.Camera camera)
        {
            PreviewCamera = camera;

            try
            {
                camera.SetPreviewDisplay(mHolder);
            }
            catch (Java.IO.IOException exception)
            {
                Log.Error(TAG, "IOException caused by setPreviewDisplay()", exception);
            }

            Android.Hardware.Camera.Parameters parameters = camera.GetParameters();
            parameters.SetPreviewSize(mPreviewSize.Width, mPreviewSize.Height);
            RequestLayout();

            camera.SetParameters(parameters);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            if (changed && ChildCount > 0)
            {
                View child = GetChildAt(0);

                int width = r - l;
                int height = b - t;

                int previewWidth = width;
                int previewHeight = height;
                if (mPreviewSize != null)
                {
                    previewWidth = mPreviewSize.Width;
                    previewHeight = mPreviewSize.Height;
                }

                // Center the child SurfaceView within the parent.
                if (width * previewHeight > height * previewWidth)
                {
                    int scaledChildWidth = previewWidth * height / previewHeight;
                    child.Layout((width - scaledChildWidth) / 2, 0,
                                 (width + scaledChildWidth) / 2, height);
                }
                else
                {
                    int scaledChildHeight = previewHeight * width / previewWidth;
                    child.Layout(0, (height - scaledChildHeight) / 2,
                                 width, (height + scaledChildHeight) / 2);
                }
            }
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            // The Surface has been created, acquire the camera and tell it where
            // to draw.
            try
            {
                if (PreviewCamera != null)
                {
                    PreviewCamera.SetPreviewDisplay(holder);
                }
            }
            catch (Java.IO.IOException exception)
            {
                Log.Error(TAG, "IOException caused by setPreviewDisplay()", exception);
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            // Surface will be destroyed when we return, so stop the preview.
            if (PreviewCamera != null)
            {
                PreviewCamera.StopPreview();
            }
        }

        private Android.Hardware.Camera.Size GetOptimalPreviewSize(IList<Android.Hardware.Camera.Size> sizes, int w, int h)
        {
            double ASPECT_TOLERANCE = 0.1;
            double targetRatio = (double)h / w;

            if (sizes == null)
                return null;

            Android.Hardware.Camera.Size optimalSize = null;
            double minDiff = System.Double.MaxValue;

            int targetHeight = h;

            foreach (Android.Hardware.Camera.Size size in sizes)
            {
                double ratio = (double)size.Height / size.Width;
                if (Java.Lang.Math.Abs(ratio - targetRatio) > ASPECT_TOLERANCE)
                    continue;

                if (Java.Lang.Math.Abs(size.Height - targetHeight) < minDiff)
                {
                    optimalSize = size;
                    minDiff = Java.Lang.Math.Abs(size.Height - targetHeight);
                }
            }
            if (optimalSize == null)
            {
                minDiff = Java.Lang.Double.MaxValue;
                foreach (Android.Hardware.Camera.Size size in sizes)
                {
                    if (Java.Lang.Math.Abs(size.Height - targetHeight) < minDiff)
                    {
                        optimalSize = size;
                        minDiff = Java.Lang.Math.Abs(size.Height - targetHeight);
                    }
                }
            }
            return optimalSize;
        }

        public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int w, int h)
        {
            // Now that the size is known, set up the camera parameters and begin
            // the preview.
            //Android.Hardware.Camera.Parameters parameters = PreviewCamera.GetParameters();
            //parameters.SetPreviewSize(mPreviewSize.Width, mPreviewSize.Height);
            //RequestLayout();

            //PreviewCamera.SetParameters(parameters);
            //PreviewCamera.StartPreview();


            if (_Context.previewing)
            {
                _camera.StopPreview();
                _Context.previewing = false;
            }
            try
            {
                //float w = 0;
                //float h = 0;
                if (this.Resources.Configuration.Orientation != Android.Content.Res.Orientation.Landscape)
                {
                    _camera.SetDisplayOrientation(90);
                    //Android.Hardware.Camera.Size cameraSize = _camera.GetParameters().PictureSize;
                    //int wr = relativeLayout.Width;
                    //int hr = relativeLayout.Height;
                    //float ratio = relativeLayout.Width * 1f / cameraSize.Height;
                    //w = cameraSize.Width * ratio;
                    //h = cameraSize.Height * ratio;
                    //RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams((int)h, (int)w);
                    //cameraSurfaceView.LayoutParameters = (lp);
                }
                else
                {
                    _camera.SetDisplayOrientation(0);
                    //Android.Hardware.Camera.Size cameraSize = camera.GetParameters().PictureSize;
                    //float ratio = relativeLayout.Height * 1f / cameraSize.Height;
                    //w = cameraSize.Width * ratio;
                    //h = cameraSize.Height * ratio;
                    //RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams((int)w, (int)h);
                    //cameraSurfaceView.LayoutParameters = (lp);
                }
                //camera.SetPreviewDisplay(cameraSurfaceHolder);
                //camera.StartPreview();
                Android.Hardware.Camera.Parameters parameters = PreviewCamera.GetParameters();
                parameters.SetPreviewSize(mPreviewSize.Width, mPreviewSize.Height);
                RequestLayout();

                PreviewCamera.SetParameters(parameters);
                PreviewCamera.StartPreview();
                _Context.previewing = true;
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("SurfaceChanged:" + e.ToString());
            }
        }
    
        public async void OnPictureTaken(byte[] data, Android.Hardware.Camera camera)
        {
            Bitmap newBitmap = await _Context.ProcessImage(data, camera);
            //linearButton.Enabled = true;
            System.GC.Collect();
            _Context.trophyFragment.CapturedBitmap(newBitmap);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            // We purposely disregard child measurements because act as a
            // wrapper to a SurfaceView that centers the camera preview instead
            // of stretching it.
            //int width = ResolveSize(SuggestedMinimumWidth, widthMeasureSpec);
            //int height = ResolveSize(SuggestedMinimumHeight, heightMeasureSpec);
            //SetMeasuredDimension(width, height);

            //if (mSupportedPreviewSizes != null)
            //{
            //    mPreviewSize = GetOptimalPreviewSize(mSupportedPreviewSizes, width, height);
            //}

            int width = ResolveSize(SuggestedMinimumWidth, widthMeasureSpec);
            int height = ResolveSize(SuggestedMinimumHeight, heightMeasureSpec);
            SetMeasuredDimension(width, height);

            if (mSupportedPreviewSizes != null)
            {
                mPreviewSize = GetOptimalPreviewSize(mSupportedPreviewSizes, width, height);
            }

            if (mPreviewSize != null)
            {
                float ratio;
                if (mPreviewSize.Height >= mPreviewSize.Width)
                    ratio = (float)mPreviewSize.Height / (float)mPreviewSize.Width;
                else
                    ratio = (float)mPreviewSize.Width / (float)mPreviewSize.Height;

                // One of these methods should be used, second method squishes preview slightly
                //SetMeasuredDimension(width, (int)(width * ratio));
                //        setMeasuredDimension((int) (width * ratio), height);
                float camHeight = (int)(width * ratio);
                float newCamHeight;
                float newHeightRatio;

                if (camHeight < height)
                {
                    newHeightRatio = (float)height / (float)mPreviewSize.Height;
                    newCamHeight = (newHeightRatio * camHeight);
                    System.Console.WriteLine(camHeight + " " + height + " " + mPreviewSize.Height + " " + newHeightRatio + " " + newCamHeight);
                    SetMeasuredDimension((int)(width * newHeightRatio), (int)newCamHeight);
                    System.Console.WriteLine(mPreviewSize.Width + " | " + mPreviewSize.Height + " | ratio - " + ratio + " | H_ratio - " + newHeightRatio + " | A_width - " + (width * newHeightRatio) + " | A_height - " + newCamHeight);
                }
                else
                {
                    newCamHeight = camHeight;
                    SetMeasuredDimension(width, (int)newCamHeight);
                    System.Console.WriteLine( mPreviewSize.Width + " | " + mPreviewSize.Height + " | ratio - " + ratio + " | A_width - " + (width) + " | A_height - " + newCamHeight);
                }
            
            }
        }
    }
}
