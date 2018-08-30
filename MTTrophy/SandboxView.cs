using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using static Android.Views.View;

namespace MTTrophy
{
    public class MatrixConfig{
        public Vector2D position { get; set; }
        public float scale { get; set; }
        public float angle { get; set; }
        public Matrix transform { get; set; }
    }

    public class SandboxView : View , IOnTouchListener
    {
        private  Bitmap bitmap;
        private  int width;
        private  int height;
        //private Matrix transform = new Matrix();
        public MatrixConfig matrixConfig;

        private TouchManager touchManager = new TouchManager(2);
        private bool isInitialized = false;

        // Debug helpers to draw lines between the two touch points
        private Vector2D vca = null;
        private Vector2D vcb = null;
        private Vector2D vpa = null;
        private Vector2D vpb = null;

        public SandboxView(Context context, Bitmap bitmap,MatrixConfig matrixConfig):base(context)
        {
            this.bitmap = bitmap;
            this.width = bitmap.Width;
            this.height = bitmap.Height;
            this.matrixConfig = matrixConfig;
            if (matrixConfig.transform == null)
                matrixConfig.transform = new Matrix();
            SetOnTouchListener(this);
        }

        private static float getDegreesFromRadians(float angle)
        {
            return (float)(angle * 180.0 / Math.PI);
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            vca = null;
            vcb = null;
            vpa = null;
            vpb = null;
            try
            {
                touchManager.update(e);
                if (touchManager.getPressCount() == 1) {
                    vca = touchManager.getPoint(0);
                    vpa = touchManager.getPreviousPoint(0);
                    matrixConfig.position.add(touchManager.moveDelta(0));
                }
                else 
                {
                    if (touchManager.getPressCount() == 2) {
                        vca = touchManager.getPoint(0);
                        vpa = touchManager.getPreviousPoint(0);
                        vcb = touchManager.getPoint(1);
                        vpb = touchManager.getPreviousPoint(1);

                        Vector2D current = touchManager.getVector(0, 1);
                        Vector2D previous = touchManager.getPreviousVector(0, 1);
                        float currentDistance = current.getLength();
                        float previousDistance = previous.getLength();

                        if (previousDistance != 0) {
                            matrixConfig.scale *= currentDistance / previousDistance;
                        }
                        matrixConfig.angle -= Vector2D.getSignedAngleBetween(current, previous);
                    }
                }
                Invalidate();
            }
            catch(Exception t) 
            {
                Console.WriteLine("Exception OnTouch:"+t.ToString());
            }
            return true;
        }

        protected override void OnConfigurationChanged(Configuration newConfig)
        {
            //if (newConfig.Orientation == Orientation.Landscape)
            //{
            //    InLandscape = true;
            //    matrixConfig.position.set(matrixConfig.position.getY(), matrixConfig.position.getX());
            //}
            //else
            //{
            //    InLandscape = false;
            //    matrixConfig.position.set(matrixConfig.position.getX(), matrixConfig.position.getY());
            //}
            isInitialized = false;
            matrixConfig.position = null;
            base.OnConfigurationChanged(newConfig);

        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            if (!isInitialized)
            {
                if(matrixConfig.position ==null){
                    matrixConfig.position = new Vector2D();
                    matrixConfig.position.set(Width / 2, Height / 2);
                    if(matrixConfig.scale==0)
                        matrixConfig.scale = 1;
                }
                isInitialized = true;
            }

            Paint paint = new Paint();
            matrixConfig.transform.Reset();
            matrixConfig.transform.PostTranslate(-width / 2.0f, -height / 2.0f);
            matrixConfig.transform.PostRotate(getDegreesFromRadians(matrixConfig.angle));

            if(matrixConfig.scale < 0.22)
            {
                matrixConfig.scale = 0.22f;
                matrixConfig.transform.PostScale(matrixConfig.scale, matrixConfig.scale);
            }
            else if (matrixConfig.scale > 1.1)
            {
                matrixConfig.scale = 1.1f;
                matrixConfig.transform.PostScale(matrixConfig.scale, matrixConfig.scale);
            }
            else
            {
                matrixConfig.transform.PostScale(matrixConfig.scale, matrixConfig.scale);
            }

            matrixConfig.transform.PostTranslate(matrixConfig.position.getX(), matrixConfig.position.getY());
            Console.WriteLine("Pos Height:" + matrixConfig.position.getY() + " Width:" + matrixConfig.position.getX());
            canvas.DrawBitmap(bitmap, matrixConfig.transform, paint);

            /*try
            {
                paint.Color = Color.ParseColor ("#FF007F00");
                canvas.DrawCircle(vca.getX(), vca.getY(), 64, paint);
                paint.Color = Color.ParseColor("#FF7F0000");
                canvas.DrawCircle(vcb.getX(), vcb.getY(), 64, paint);

                paint.Color = Color.ParseColor("#FFFF0000");
                canvas.DrawLine(vpa.getX(), vpa.getY(), vpb.getX(), vpb.getY(), paint);
                paint.Color = Color.ParseColor("#FF00FF00");
                canvas.DrawLine(vca.getX(), vca.getY(), vcb.getX(), vcb.getY(), paint);
            }
            catch (Exception e)
            {
                // Just being lazy here...
            }*/
        }
    }
}
