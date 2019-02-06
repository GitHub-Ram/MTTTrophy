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
using Android.Support.V7.App;

namespace MTTrophy
{
    [Activity(Theme = "@style/Theme.AppCompat.Light.NoActionBar", ScreenOrientation = ScreenOrientation.Unspecified, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout, UiOptions = UiOptions.SplitActionBarWhenNarrow)]
    public class TrophyCameraActivity : AppCompatActivity
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
            Window.SetFormat(Format.Translucent);
            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            mPreview = new Preview(this);
            SetContentView(mPreview);
            fmanager = FragmentManager;

            Bundle extras = Intent.Extras;
            if (extras != null)
            {
                TrophyName = extras.GetString("TrophyName");
            }

            trophyFragment = TrophyFragment.newInstance(TrophyName, this);
            trophyFragment.Show(fmanager, "fragment_edit_name");
            trophyFragment.Cancelable = false;
            trophyFragment.OnCAptureClick += (Bitmap obj, LinearLayout linearButton) => {
                if(obj!=null){
                    try{
                        bitmaptrophy = obj;
                        mCamera.TakePicture(null, null, mPreview);
                    }catch(System.Exception ex){
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
