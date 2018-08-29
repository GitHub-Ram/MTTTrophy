using System;
using Android.Views;
using Java.Lang;

namespace MTTrophy
{
    public class TouchManager
    {
        private int maxNumberOfTouchPoints;

        private Vector2D[] points;

        private Vector2D[] previousPoints;

        public TouchManager(int maxNumberOfTouchPoints)
        {
            this.maxNumberOfTouchPoints = maxNumberOfTouchPoints;

            points = new Vector2D[maxNumberOfTouchPoints];
            previousPoints = new Vector2D[maxNumberOfTouchPoints];
        }

        public bool isPressed(int index)
        {
            return points[index] != null;
        }

        public int getPressCount()
        {
            int count = 0;
            foreach (Vector2D point in points)
            {
                if (point != null)
                    ++count;
            }
            return count;
        }

        public Vector2D moveDelta(int index)
        {
            if (isPressed(index))
            {
                Vector2D previous = previousPoints[index] != null ? previousPoints[index] : points[index];
                return Vector2D.subtract(points[index], previous);
            }
            else
            {
                return new Vector2D();
            }
        }

        private static Vector2D getVector(Vector2D a, Vector2D b)
        {
            if (a == null || b == null)
                throw new RuntimeException("can't do this on nulls");

            return Vector2D.subtract(b, a);
        }

        public Vector2D getPoint(int index)
        {
            return points[index] != null ? points[index] : new Vector2D();
        }

        public Vector2D getPreviousPoint(int index)
        {
            return previousPoints[index] != null ? previousPoints[index] : new Vector2D();
        }

        public Vector2D getVector(int indexA, int indexB)
        {
            return getVector(points[indexA], points[indexB]);
        }

        public Vector2D getPreviousVector(int indexA, int indexB)
        {
            if (previousPoints[indexA] == null || previousPoints[indexB] == null)
                return getVector(points[indexA], points[indexB]);
            else
                return getVector(previousPoints[indexA], previousPoints[indexB]);
        }

        public void update(MotionEvent events) {
            var actionCode = (events.Action & MotionEventActions.Mask);

            if (actionCode == MotionEventActions.PointerUp || actionCode == MotionEventActions.Up) {
                int index = (int)events.Action >> (int)MotionEventActions.PointerIdShift;
                previousPoints[index] = points[index] = null;
            }
            else {
                for(int i = 0; i<maxNumberOfTouchPoints; ++i) {
                    
                    if (i< events.PointerCount) 
                    {
                        int index = events.GetPointerId(i);
                        Vector2D newPoint = new Vector2D(events.GetX(i), events.GetY(i));
                        if (points[index] == null)
                            points[index] = newPoint;
                        else 
                        {
                            if (previousPoints[index] != null) 
                            {
                                previousPoints[index].set(points[index]);
                            }
                            else 
                            {
                                previousPoints[index] = new Vector2D(newPoint);
                            }
                            if (Vector2D.subtract(points[index], newPoint).getLength() < 150)
                                points[index].set(newPoint);
                        }
                    }
                    else 
                    { 
                        previousPoints[i] = points[i] = null;
                    }
                }
            }
        }
    }
}
