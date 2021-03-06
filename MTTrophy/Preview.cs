﻿using System;
using System.Collections.Generic;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace MTTrophy
{
    public class Preview : ViewGroup, ISurfaceHolderCallback, Android.Hardware.Camera.IPictureCallback
    {
        string TAG = "Preview";
        TrophyCameraActivity _Context;
        SurfaceView mSurfaceView;
        internal ISurfaceHolder mHolder;
        internal Android.Hardware.Camera.Size mPreviewSize;
        internal IList<Android.Hardware.Camera.Size> mSupportedPreviewSizes;
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
                    foreach (Android.Hardware.Camera.Size size in mSupportedPreviewSizes)
                    {
                        Console.WriteLine("FIXED HEIGHT:" + size.Height + " WIDTH:" + size.Width);
                    }
                    RequestLayout();
                }
            }
        }

        public Preview(System.IntPtr intPtr, Android.Runtime.JniHandleOwnership ownership): base(intPtr, ownership)
        {

        }

        public Preview(TrophyCameraActivity context) : base(context.ApplicationContext)
        {
            _Context = context;
            mSurfaceView = new SurfaceView(context);

            AddView(mSurfaceView);
            SetBackgroundColor(Color.Red);
            // Install a SurfaceHolder.Callback so we get notified when the
            // underlying surface is created and destroyed.
            mHolder = mSurfaceView.Holder;
            mHolder.AddCallback(this);
            if (Build.VERSION.SdkInt < BuildVersionCodes.Honeycomb)
            {
                mHolder.SetType(SurfaceType.PushBuffers);
            }
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
            Console.WriteLine("Param mPreviewSize.Width:"+ mPreviewSize.Width+ " mPreviewSize.height:"+ mPreviewSize.Height);
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
                child.Layout(0, 0, width, height);

                //if (mPreviewSize != null)
                //{
                //    previewWidth = mPreviewSize.Width;
                //    previewHeight = mPreviewSize.Height;
                //}

                //// Center the child SurfaceView within the parent.
                //if (width * previewHeight > height * previewWidth)
                //{
                //    int scaledChildWidth = previewWidth * height / previewHeight;
                //    child.Layout((width - scaledChildWidth) / 2, 0,
                //                 (width + scaledChildWidth) / 2, height);
                //}
                //else
                //{
                //    int scaledChildHeight = previewHeight * width / previewWidth;
                //    child.Layout(0, (height - scaledChildHeight) / 2,
                //                 width, (height + scaledChildHeight) / 2);
                //}
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
                _Context.previewing = false;
            }
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
            //if (mSupportedPreviewSizes != null)
            //{
            //    mPreviewSize = _Context.GetOptimalPreviewSize(mSupportedPreviewSizes);
            //}
            if (_camera == null)
                return;
            if (_Context.previewing)
            {
                //_camera.StopPreview();
                //_Context.previewing = false;
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
                return;
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
                Console.WriteLine("Param mPreviewSize.Width:" + mPreviewSize.Width + " mPreviewSize.height:" + mPreviewSize.Height);
                RequestLayout();
                parameters.SetPictureSize(mPreviewSize.Width, mPreviewSize.Height);
                parameters.JpegQuality = (100);
                parameters.PictureFormat = (ImageFormat.Jpeg);



                PreviewCamera.SetParameters(parameters);
                PreviewCamera.GetParameters().FocusMode = Android.Hardware.Camera.Parameters.FocusModeAuto;
                if (PreviewCamera.GetParameters().IsZoomSupported)
                    PreviewCamera.GetParameters().Zoom = (0);
                PreviewCamera.StartPreview();
                _Context.previewing = true;
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("SurfaceChanged:" + e.ToString());
            }
        }

        public void OnPictureTaken(byte[] data, Android.Hardware.Camera camera)
        {
            Bitmap newBitmap = _Context.ProcessImage(data, camera);
            //linearButton.Enabled = true;
            System.GC.Collect();
            _Context.trophyFragment.CapturedBitmap(newBitmap);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            int width = ResolveSize(SuggestedMinimumWidth, widthMeasureSpec);
            int height = ResolveSize(SuggestedMinimumHeight, heightMeasureSpec);
            if (mSupportedPreviewSizes != null)
            {
                mPreviewSize = _Context.GetOptimalPreviewSize(mSupportedPreviewSizes);
            }

            float ratio;
            if (mPreviewSize.Height >= mPreviewSize.Width)
                ratio = (float)mPreviewSize.Height / (float)mPreviewSize.Width;
            else
                ratio = (float)mPreviewSize.Width / (float)mPreviewSize.Height;

            // One of these methods should be used, second method squishes preview slightly
            SetMeasuredDimension(width, (int)(width * ratio));

            return;
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

            //int width = ResolveSize(SuggestedMinimumWidth, widthMeasureSpec);
            //int height = ResolveSize(SuggestedMinimumHeight, heightMeasureSpec);
            //SetMeasuredDimension(width, height);

            //if (mSupportedPreviewSizes != null)
            //{
            //    mPreviewSize = GetOptimalPreviewSize(mSupportedPreviewSizes, width, height);
            //}
            mPreviewSize = _Context.GetOptimalPreviewSize(mSupportedPreviewSizes);
            if (mPreviewSize != null)
            {
                SetMeasuredDimension(width, height);
                //float ratio;
                //if (mPreviewSize.Height >= mPreviewSize.Width)
                //    ratio = (float)mPreviewSize.Height / (float)mPreviewSize.Width;
                //else
                //    ratio = (float)mPreviewSize.Width / (float)mPreviewSize.Height;

                //// One of these methods should be used, second method squishes preview slightly
                ////SetMeasuredDimension(width, (int)(width * ratio));
                ////        setMeasuredDimension((int) (width * ratio), height);
                //float camHeight = (int)(width * ratio);
                //float newCamHeight;
                //float newHeightRatio;

                //if (camHeight < height)
                //{
                //    newHeightRatio = (float)height / (float)mPreviewSize.Height;
                //    newCamHeight = (newHeightRatio * camHeight);
                //    System.Console.WriteLine(camHeight + " " + height + " " + mPreviewSize.Height + " " + newHeightRatio + " " + newCamHeight);
                //    SetMeasuredDimension((int)(width * newHeightRatio), (int)newCamHeight);
                //    //System.Console.WriteLine(mPreviewSize.Width + " | " + mPreviewSize.Height + " | ratio - " + ratio + " | H_ratio - " + newHeightRatio + " | A_width - " + (width * newHeightRatio) + " | A_height - " + newCamHeight);
                //    //mPreviewSize.Width = (int)(width * newHeightRatio);
                //    //mPreviewSize.Height = (int)newCamHeight;
                //}
                //else
                //{
                //    newCamHeight = camHeight;
                //    SetMeasuredDimension(width, (int)newCamHeight);
                //    //System.Console.WriteLine(mPreviewSize.Width + " | " + mPreviewSize.Height + " | ratio - " + ratio + " | A_width - " + (width) + " | A_height - " + newCamHeight);
                //    //mPreviewSize.Width = width;
                //    //mPreviewSize.Height = (int)newCamHeight;
                //}

            }
        }
    }
}