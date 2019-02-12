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
    [Activity(Theme = "@style/Theme.AppCompat.Light.NoActionBar", ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout, UiOptions = UiOptions.SplitActionBarWhenNarrow)]
    public class TrophyCameraActivity : AppCompatActivity, ISensorEventListener
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
        internal static bool IsFrontCamera = false;
        Android.App.FragmentManager fmanager;
        internal TrophyFragment trophyFragment;
        public string TrophyName = string.Empty;


        int numberOfCameras;
        int cameraCurrentlyLocked;

        // The first rear facing camera
        int defaultCameraId;

        SensorManager mSensorManager;
        Sensor accSensor;
        Sensor magnetSensor;

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
                    }
                    catch (System.Exception ex){
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
                    IsFrontCamera = true;
                }
                else
                {
                    currentCameraId = Android.Hardware.Camera.CameraInfo.CameraFacingBack;
                    IsFrontCamera = false;
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
                //Android.Hardware.Camera.Parameters parameters = mCamera.GetParameters();
                //parameters.SetPreviewSize(mPreview.mPreviewSize.Width, mPreview.mPreviewSize.Height);
                //System.Console.WriteLine("Param mPreviewSize.Width:" + mPreview.mPreviewSize.Width + " mPreviewSize.height:" + mPreview.mPreviewSize.Height);
                //parameters.SetPictureSize(mPreview.mPreviewSize.Width, mPreview.mPreviewSize.Height);
                //parameters.JpegQuality = (100);
                //parameters.PictureFormat = (ImageFormat.Jpeg);
                //parameters.FocusMode = Android.Hardware.Camera.Parameters.FocusModeAuto;
                //if (parameters.IsZoomSupported)
                //    parameters.Zoom = (0);
                //mCamera.SetParameters(parameters);
                //mCamera.StartPreview();
                previewing = false;
                mPreview.Dispose();
                mPreview = new Preview(this);
                SetContentView(mPreview);
                mPreview.PreviewCamera = mCamera;
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
            mSensorManager = (SensorManager)GetSystemService(SensorService);
            accSensor = mSensorManager.GetDefaultSensor(SensorType.Accelerometer);
            mSensorManager.RegisterListener(this, accSensor, SensorDelay.Normal);
        }

        internal Bitmap ProcessImage(byte[] data, Android.Hardware.Camera camera)
        {
            Bitmap newBitmap = null;
            try
            {
                // TODO Auto-generated method stub
                int orientation = Exif.GetExifOrientations(data);
                BitmapFactory.Options options = new BitmapFactory.Options();
                //o.inJustDecodeBounds = true;
                Bitmap cameraBitmapNull = BitmapFactory.DecodeByteArray(data, 0, data.Length,options);
                Bitmap bitmapPicture = null;
                int extraangle = 180;
                switch (orientation)
                {
                    case 90:
                        bitmapPicture = RotateImage(cameraBitmapNull, 180);

                        break;
                    case 180:
                        bitmapPicture = RotateImage(cameraBitmapNull, 270);

                        break;
                    case 270:
                        bitmapPicture = RotateImage(cameraBitmapNull, 360);

                        break;
                    case 0:
                        bitmapPicture = RotateImage(cameraBitmapNull, 90);
                        break;
                    // if orientation is zero we don't need to rotate this 

                    default:
                        break;
                }
                int wid = options.OutWidth;
                int hgt = options.OutHeight;
                Matrix nm = new Matrix();

                Android.Hardware.Camera.Size cameraSize = camera.GetParameters().PictureSize;
                float ratio = mPreview.Height * 1f / cameraSize.Height;
                if (Resources.Configuration.Orientation != Android.Content.Res.Orientation.Landscape)
                {
                    //nm.PostRotate(90);
                    //nm.PostTranslate(hgt, 0);
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
                //Bitmap cameraBitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length, options);

                canvas.DrawBitmap(bitmapPicture, nm, null);


                canvas.DrawBitmap(bitmaptrophy, matrix, null);
                camera.StartPreview();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("ON PICTURE Ex:" + ex.ToString());
            }
            return newBitmap;
        }

        public static Bitmap RotateImage(Bitmap source, float angle)
        {
            Matrix matrix = new Matrix();
            matrix.PostRotate(IsFrontCamera?180+angle:angle);
            var result = Bitmap.CreateBitmap(source, 0, 0, source.Width, source.Height, matrix, true);
            if (IsFrontCamera)
            {
                matrix = new Matrix();
                matrix.SetValues(new float[] { -1, 0, 0, 0, 1, 0, 0, 0, 1 });
                result = Bitmap.CreateBitmap(result, 0, 0, result.Width, result.Height, matrix, true);
            }
            return result;
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
            cameraCurrentlyLocked = defaultCameraId;
            mCamera = Android.Hardware.Camera.Open(defaultCameraId);

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
            mSensorManager.UnregisterListener(this, accSensor);
            //mSensorManager.UnregisterListener(this, magnetSensor);
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

        internal Android.Hardware.Camera.Size GetOptimalPreviewSize(IList<Android.Hardware.Camera.Size> sizes)
        {
            const double ASPECT_TOLERANCE = 0.05;
            string TAG = "GetOptimalPreviewSize";
            if (sizes == null)
                return null;

            Android.Hardware.Camera.Size optimalSize = null;
            double minDiff = System.Double.MaxValue;
            Point display_size = new Point();
            //AppCompatActivity activity = (AppCompatActivity)this.BaseContext;
            {
                Display display = this.WindowManager.DefaultDisplay;
                display.GetSize(display_size);

                Log.Debug(TAG, "display_size: " + display_size.X + " x " + display_size.Y);
            }
            double targetRatio = CalculateTargetRatioForPreview(display_size);
            int targetHeight = System.Math.Min(display_size.Y, display_size.X);
            if (targetHeight <= 0)
            {
                targetHeight = display_size.Y;
            }
            // Try to find the size which matches the aspect ratio, and is closest match to display height
            foreach (Android.Hardware.Camera.Size size in sizes)
            {
                Log.Debug(TAG, "    supported preview size: " + size.Width + ", " + size.Height);
                double ratio = (double)size.Width / size.Height;
                if (System.Math.Abs(ratio - targetRatio) > ASPECT_TOLERANCE)
                    continue;
                if (System.Math.Abs(size.Height - targetHeight) < minDiff)
                {
                    optimalSize = size;
                    minDiff = System.Math.Abs(size.Height - targetHeight);
                }
            }
            if (optimalSize == null)
            {
                // can't find match for aspect ratio, so find closest one
                Log.Debug(TAG, "no preview size matches the aspect ratio");
                optimalSize = GetClosestSize(sizes, targetRatio);
            }
            optimalSize = sizes[0];

            Log.Debug(TAG, "chose optimalSize: " + optimalSize.Width + " x " + optimalSize.Height);
            Log.Debug(TAG, "optimalSize ratio: " + ((double)optimalSize.Width / optimalSize.Height));
            return optimalSize;
        }

        public Android.Hardware.Camera.Size GetClosestSize(IList<Android.Hardware.Camera.Size> sizes,double targetRatio)
        {
            Log.Debug("GetClosestSize", "getClosestSize()");
            Android.Hardware.Camera.Size optimalSize = null;
            double minDiff = System.Double.MaxValue;
            foreach (Android.Hardware.Camera.Size size in sizes)
            {
                double ratio = (double)size.Width / size.Height;
                if (System.Math.Abs(ratio - targetRatio) < minDiff)
                {
                    optimalSize = size;
                    minDiff = System.Math.Abs(ratio - targetRatio);
                }
            }
            return optimalSize;
        }

        private double CalculateTargetRatioForPreview(Point display_size)
        {
            double targetRatio = 0.0f;

                targetRatio = ((double)mCamera.GetParameters().PictureSize.Width) / (double)mCamera.GetParameters().PictureSize.Height;
                //targetRatio = ((double)display_size.x) / (double)display_size.y;
            return targetRatio;
        }

        internal Android.Hardware.Camera.Size GetOptimalPreviewSizeNotinUse(IList<Android.Hardware.Camera.Size> sizes, int w, int h)
        {
            double ASPECT_TOLERANCE = 0.1;
            double targetRatio = (double)h / w;
            if (Resources.Configuration.Orientation == Android.Content.Res.Orientation.Portrait)
            {
                targetRatio = (double)h / (double)w;
            }
            else if (Resources.Configuration.Orientation == Android.Content.Res.Orientation.Landscape)
            {
                targetRatio = (double)w / (double)h;
            }

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
                    //Console.WriteLine("FIXED HEIGHT:"+size.Height+" WIDTH:"+size.Width);
                    if (Java.Lang.Math.Abs(size.Height - targetHeight) < minDiff)
                    {
                        optimalSize = size;
                        minDiff = Java.Lang.Math.Abs(size.Height - targetHeight);
                    }
                }
            }
            return optimalSize;
        }


        public static int UPSIDE_DOWN = 3;
        public static  int LANDSCAPE_RIGHT = 4;
        public static  int PORTRAIT = 1;
        public static  int LANDSCAPE_LEFT = 2;
        public int mOrientationDeg; //last rotation in degrees
        public int mOrientationRounded; //last orientation int from above 
        private static  int _DATA_X = 0;
        private static  int _DATA_Y = 1;
        private static  int _DATA_Z = 2;
        private int ORIENTATION_UNKNOWN = -1;
        internal static int tempOrientRounded = 0;
        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {

        }

        static int neglectCount = 0;
        public void OnSensorChanged(SensorEvent e)
        {
            Log.Debug("TROPHY", "Sensor Changed");
            neglectCount++;
            if (neglectCount < 7)
                return;
            neglectCount = 0;
            var values = e.Values;
            int orientation = ORIENTATION_UNKNOWN;
            float X = -values[_DATA_X];
            float Y = -values[_DATA_Y];
            float Z = -values[_DATA_Z];
            float magnitude = X * X + Y * Y;
            // Don't trust the angle if the magnitude is small compared to the y value
            if (magnitude* 4 >= Z* Z) {
                float OneEightyOverPi = 57.29577957855f;
                float angle = (float)Java.Lang.Math.Atan2(-Y, X) * OneEightyOverPi;
                orientation = 90 - (int)Java.Lang.Math.Round(angle);
                // normalize to 0 - 359 range
                while (orientation >= 360) {
                    orientation -= 360;
                } 
                while (orientation< 0) {
                    orientation += 360;
                }
            }
            //^^ thanks to google for that code
            //now we must figure out which orientation based on the degrees
            Log.Debug("Oreination", ""+orientation);
            if (orientation != mOrientationDeg) 
            {
                mOrientationDeg = orientation;
                //figure out actual orientation
                if(orientation == -1){//basically flat

                }
                else if(orientation <= 45 || orientation > 315){//round to 0
                    tempOrientRounded = 1;//portrait
                }
                else if(orientation > 45 && orientation <= 135){//round to 90
                    tempOrientRounded = 2; //lsleft
                }
                else if(orientation > 135 && orientation <= 225){//round to 180
                    tempOrientRounded = 3; //upside down
                }
                else if(orientation > 225 && orientation <= 315){//round to 270
                    tempOrientRounded = 4;//lsright
                }

            }

            if(mOrientationRounded != tempOrientRounded){
                mOrientationRounded = tempOrientRounded;
                int viewsIds = TrophyFragment.layoutManager.FindLastCompletelyVisibleItemPosition();
                int angle = 0;
                switch (TrophyCameraActivity.tempOrientRounded)
                {
                    case 1:
                        trophyFragment.back.Rotation = trophyFragment.save.Rotation = trophyFragment.buttonFlipcamera.Rotation = trophyFragment.BackLobby.Rotation = trophyFragment.buttonRetake.Rotation = trophyFragment.BackLobby.Rotation = 0;
                        angle = 0;
                        break;
                    case 2:
                        trophyFragment.back.Rotation = trophyFragment.save.Rotation = trophyFragment.buttonFlipcamera.Rotation = trophyFragment.BackLobby.Rotation = trophyFragment.buttonRetake.Rotation = trophyFragment.BackLobby.Rotation = -90;
                        angle = -90;
                        break;
                    case 3:
                        trophyFragment.back.Rotation = trophyFragment.save.Rotation = trophyFragment.buttonFlipcamera.Rotation = trophyFragment.BackLobby.Rotation = trophyFragment.buttonRetake.Rotation = trophyFragment.BackLobby.Rotation = -180;
                        angle = -180;
                        break;
                    case 4:
                        trophyFragment.back.Rotation = trophyFragment.save.Rotation = trophyFragment.buttonFlipcamera.Rotation = trophyFragment.BackLobby.Rotation = trophyFragment.buttonRetake.Rotation = trophyFragment.BackLobby.Rotation = 90;
                        angle = 90;
                        break;
                }
                for (int i = 0; i <= viewsIds+1; i++)
                {
                    PhotoViewHolder firstViewHolder = (PhotoViewHolder)trophyFragment.mRecycleView.FindViewHolderForLayoutPosition(i);
                    if(firstViewHolder!=null)
                        firstViewHolder.imageView.Rotation = angle;
                }
                if (TrophyFragment.sandboxView != null)
                {
                    MatrixConfig matrixConfig = TrophyFragment.sandboxView.matrixConfig;
                    System.Console.WriteLine("matrixConfig.angle:" + matrixConfig.angle + " angle:" + angle);
                    if (System.Math.Abs(matrixConfig.angle - (float)angle) > 0)
                    {
                        matrixConfig.angle = (float)(angle * (System.Math.PI / 180));
                        TrophyFragment.sandboxView.Invalidate();
                    }
                }
            }
        }
    }

}
