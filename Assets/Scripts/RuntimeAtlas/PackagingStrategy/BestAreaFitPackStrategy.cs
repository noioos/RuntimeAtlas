using System;

namespace RuntimeAtlas.PackagingStrategy
{
    public class BestAreaFitPackStrategy : IPackStrategy
    {
        public bool GetFreeRegion(IntegerRect targetRect, RuntimeAtlasPage page, out IntegerRect freeRegion)
        {
            
            var best = new IntegerRect(page.Width + 1, page.Height + 1, page.Width+1,page.Height+1);
            bool rFlag = false;

            //所有空闲区域中，面积与目标最接近的，靠左下角的区域作为最佳区域
            for (var i = 0; i < page.FreeRegions.Count; i++)
            {
                var region = page.FreeRegions[i];
                
                if (region.Height >= targetRect.Height && region.Width >= targetRect.Width)
                {
                    if (region.Height * region.Width < best.Height * best.Width)
                    {
                        best = region;
                        rFlag = true;
                    }
                }
            }

            freeRegion = rFlag ? best : null;

            if (rFlag)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }
    }
} 