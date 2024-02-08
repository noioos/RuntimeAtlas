using System;
using System.Collections.Generic;
using RuntimeAtlas.PackagingStrategy;
using UnityEngine;

namespace RuntimeAtlas
{
    public class RuntimeAtlas
    {
        public int Width;
        public int Height;
        public int Padding;

        //区域打包策略
        private IPackStrategy _packStrategy;
        private Dictionary<string, AtlasRegion> _packRegions = new Dictionary<string, AtlasRegion>();
        private List<RuntimeAtlasPage> _pages = new List<RuntimeAtlasPage>();

        public RuntimeAtlas()
        {
        }

        public RuntimeAtlas(int width, int height, int padding, IPackStrategy packStrategy = null)
        {
            Width = width;
            Height = height;
            Padding = padding;
            _packStrategy = packStrategy ?? new BestAreaFitPackStrategy();

            _pages = new List<RuntimeAtlasPage> { new(Width, Height) };
        }


        /// <summary>
        /// 插入一个纹理，插入成功返回纹理与对应rect，失败则为null
        /// </summary>
        /// <param name="texture2D"></param>
        /// <returns>插入结果</returns>
        public (Texture2D tex, Rect region, bool succeed) InsertTexture(Texture2D texture2D)
        {
            if (texture2D.width > Width || texture2D.height > Height)
            {
                Debug.LogWarning($"{texture2D.name} size is too large");
                return (texture2D, region: Rect.zero, false);
            }

            //先从已有纹理寻找
            if (GetTexture(texture2D.name, out (Texture2D tex, Rect region) res))
            {
                return (res.tex, res.region, true);
            }

            var targetWithPadding = new IntegerRect(0, 0, texture2D.width + Padding, texture2D.height + Padding);
            var regionData = GetFreePageAndRegion(targetWithPadding);
            int pageIndex = regionData.pageIndex;
            IntegerRect freeRegion = regionData.freeRegion;
            targetWithPadding.X = freeRegion.X;
            targetWithPadding.Y = freeRegion.Y;

            GenerateNewFreeRegions(targetWithPadding, _pages[pageIndex].FreeRegions);
            _pages[pageIndex].FilterSubAreas();

            var resRegion = new AtlasRegion
            {
                Region = new Rect(freeRegion.X, freeRegion.Y, texture2D.width, texture2D.height),
                PageIndex = pageIndex,
                //默认引用计数为1
                RefCount = 1
            };
            try
            {
                _pages[pageIndex].AddTexture((int)resRegion.Region.x, (int)resRegion.Region.y, texture2D);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogError(texture2D.name);
                throw;
            }

            _packRegions.Add(texture2D.name, resRegion);
            return (_pages[pageIndex].Tex, resRegion.Region, true);
        }

        /// <summary>
        /// 获取对应纹理的tex与region
        /// </summary>
        /// <param name="name">tex名称</param>
        /// <param name="res">获取结果</param>
        /// <returns>是否成功</returns>
        public bool GetTexture(string name, out (Texture2D tex, Rect region) res)
        {
            res = new(null, Rect.zero);

            if (_packRegions.TryGetValue(name, out AtlasRegion region))
            {
                res.region = region.Region;
                res.tex = _pages[region.PageIndex].Tex;

                //增加对应引用计数
                region.RefCount++;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 对应纹理引用计数-1，为0时回收该区域
        /// </summary>
        /// <param name="name">tex名称</param>
        public void ReleaseTexture(string name)
        {
            if (_packRegions.TryGetValue(name, out AtlasRegion region))
            {
                region.RefCount--;
                if (region.RefCount <= 0)
                {
                    _pages[region.PageIndex].RemoveRegion(region);
                    _packRegions.Remove(name);
                }
            }
        }

        public void Clear()
        {
            _pages.Clear();
            _packRegions.Clear();
            _packStrategy = null;
        }

        /// <summary>
        /// 根据目标区域，分割生成新的空闲区域
        /// </summary>
        /// <param name="targetRegion">目标区域</param>
        /// <param name="freeRegions">已有全部空闲区域</param>
        private void GenerateNewFreeRegions(IntegerRect targetRegion, List<IntegerRect> freeRegions)
        {
            var newRegions = new List<IntegerRect>();
            for (var i = freeRegions.Count - 1; i >= 0; i--)
            {
                if (!CheckRegionCrossing(targetRegion, freeRegions[i]))
                    continue;

                DivideRegion(targetRegion, freeRegions[i], newRegions);

                var lastRegion = freeRegions[^1];

                freeRegions.RemoveAt(freeRegions.Count - 1);

                if (i < freeRegions.Count)
                {
                    freeRegions[i] = lastRegion;
                }

            }

            foreach (var integerRect in newRegions)
            {
                freeRegions.Add(integerRect);
            }
        }

        /// <summary>
        /// 分割重叠的区域
        /// </summary>
        /// <param name="divider"></param>
        /// <param name="area"></param>
        /// <param name="results"></param>
        private void DivideRegion(IntegerRect divider, IntegerRect area, List<IntegerRect> results)
        {

            int count = 0;

            //根据各方向边界的差值，生成分割后的区域
            int rightDelta = area.Right - divider.Right;
            if (rightDelta > 0)
            {
                results.Add(new IntegerRect(divider.Right, area.Y, rightDelta, area.Height));
                count++;
            }

            int leftDelta = divider.X - area.X;
            if (leftDelta > 0)
            {
                results.Add(new IntegerRect(area.X, area.Y, leftDelta, area.Height));
                count++;
            }

            int topDelta = area.Top - divider.Top;
            if (topDelta > 0)
            {
                results.Add(new IntegerRect(area.X, divider.Top, area.Width, topDelta));
            }

            int bottomDelta = divider.Y - area.Y;
            if (bottomDelta > 0)
            {
                results.Add(new IntegerRect(area.X, area.Y, area.Width, bottomDelta));
            }

        }

        /// <summary>
        /// 检查两区域是否相交
        /// </summary>
        /// <param name="regionA"></param>
        /// <param name="regionB"></param>
        /// <returns></returns>
        private bool CheckRegionCrossing(IntegerRect regionA, IntegerRect regionB)
        {
            return regionA.X < regionB.Right && regionA.Y < regionB.Top && regionA.Right > regionB.X &&
                   regionA.Top > regionB.Y;
        }

        
        private (int pageIndex, IntegerRect freeRegion)  GetFreePageAndRegion(IntegerRect targetWithPadding)
        {
            int freePageIndex = -1;
            IntegerRect freeRegion = null;

            for (var i = 0; i < _pages.Count; i++)
            {
                if (_packStrategy.GetFreeRegion(targetWithPadding, _pages[i], out freeRegion))
                {
                    freePageIndex = i;
                    break;
                }
            }

            if (freePageIndex == -1)
            {
                _pages.Add(new RuntimeAtlasPage(Width, Height));
                freePageIndex = _pages.Count - 1;
                freeRegion = _pages[freePageIndex].FreeRegions[0];
            }

            return (freePageIndex, freeRegion);
        }
    }

    public class RuntimeAtlasPage
    {
        public readonly int Width;
        public readonly int Height;
        public List<IntegerRect> FreeRegions;
        public readonly Texture2D Tex;

        public RuntimeAtlasPage(int width, int height)
        {
            Width = width;
            Height = height;
            FreeRegions = new List<IntegerRect> { new(0, 0, Width, Height) };
            Tex = new Texture2D(Width, Height, TextureFormat.ARGB32, false);
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    Tex.SetPixel(i, j, Color.clear);
                }
            }
        }

        public void AddTexture(int x, int y, Texture2D srcTex)
        {
            Graphics.CopyTexture(srcTex, 0, 0, 0, 0, srcTex.width, srcTex.height, Tex, 0, 0, x, y);
        }

        public void RemoveRegion(AtlasRegion region)
        {
            var rect = region.Region;

            //归还空闲区域
            FreeRegions.Add(new IntegerRect((int)region.Region.x, (int)region.Region.y, (int)region.Region.width,
                (int)region.Region.height));

            //回复纹理对应区域
            int width = (int)rect.width;
            int height = (int)rect.height;
            Color32[] colors = new Color32[width * height];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.clear;
            }

            Tex.SetPixels32((int)rect.x, (int)rect.y, width, height, colors);
            Tex.Apply();
        }


        /// <summary>
        /// 去除完全重叠的区域
        /// </summary>
        public void FilterSubAreas()
        {
            for (var i = FreeRegions.Count - 1; i >= 0; i--)
            {
                for (var j = FreeRegions.Count - 1; j >= 0; j--)
                {
                    //忽略自己
                    if (i == j)
                    {
                        continue;
                    }

                    //以边界判断是否完全重叠
                    if (FreeRegions[i].X < FreeRegions[j].X || FreeRegions[i].Right > FreeRegions[j].Right ||
                        FreeRegions[i].Y < FreeRegions[j].Y || FreeRegions[i].Top > FreeRegions[j].Top)
                    {
                        continue;
                    }


                    var lastRegion = FreeRegions[^1];
                    FreeRegions.RemoveAt(FreeRegions.Count - 1);
                    if (i < FreeRegions.Count)
                    {
                        FreeRegions[i] = lastRegion;
                    }

                    break;

                }
            }
        }
    }
}