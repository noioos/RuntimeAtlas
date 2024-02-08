using UnityEngine;

namespace RuntimeAtlas.PackagingStrategy
{
    public class AtlasRegion
    {
        public int PageIndex;
        public int RefCount;
        public Rect Region;
    }

    public class IntegerRect
    {
        public int X;
        public int Y;

        public int Width;
        public int Height;

        public int Top => Y + Height;
        public int Right => X + Width;
        
        
        public IntegerRect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        
    }
}