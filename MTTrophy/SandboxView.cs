using System;
using Android.Content;
using Android.Graphics;
using Android.Views;
using static Android.Views.View;

namespace MTTrophy
{
    public class SandboxView : View , IOnTouchListener
    {
        private  Bitmap bitmap;
        private  int width;
        private  int height;
        private Matrix transform = new Matrix();

        private Vector2D position = new Vector2D();
        private float scale = 1;
        private float angle = 0;

        private TouchManager touchManager = new TouchManager(2);
        private bool isInitialized = false;

        // Debug helpers to draw lines between the two touch points
        private Vector2D vca = null;
        private Vector2D vcb = null;
        private Vector2D vpa = null;
        private Vector2D vpb = null;

        public SandboxView(Context context, Bitmap bitmap):base(context)
        {
            this.bitmap = bitmap;
            this.width = bitmap.Width;
            this.height = bitmap.Height;
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
                    position.add(touchManager.moveDelta(0));
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
                            scale *= currentDistance / previousDistance;
                        }
                        angle -= Vector2D.getSignedAngleBetween(current, previous);
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

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            if (!isInitialized)
            {
                int w = Width;
                int h = Height;
                position.set(w / 2, h / 2);
                isInitialized = true;
            }

            Paint paint = new Paint();

            transform.Reset();
            transform.PostTranslate(-width / 2.0f, -height / 2.0f);
            transform.PostRotate(getDegreesFromRadians(angle));
            transform.PostScale(scale, scale);
            transform.PostTranslate(position.getX(), position.getY());

            canvas.DrawBitmap(bitmap, transform, paint);

            try
            {
                /*paint.setColor(0xFF007F00);
                canvas.drawCircle(vca.getX(), vca.getY(), 64, paint);
                paint.setColor(0xFF7F0000);
                canvas.drawCircle(vcb.getX(), vcb.getY(), 64, paint);

                paint.setColor(0xFFFF0000);
                canvas.drawLine(vpa.getX(), vpa.getY(), vpb.getX(), vpb.getY(), paint);
                paint.setColor(0xFF00FF00);
                canvas.drawLine(vca.getX(), vca.getY(), vcb.getX(), vcb.getY(), paint);*/
            }
            catch (Exception e)
            {
                // Just being lazy here...
            }
        }


    }
}
