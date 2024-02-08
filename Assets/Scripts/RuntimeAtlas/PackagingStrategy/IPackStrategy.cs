using UnityEngine;

namespace RuntimeAtlas.PackagingStrategy
{
    public interface IPackStrategy
    {
        bool GetFreeRegion(IntegerRect targetRect, RuntimeAtlasPage page, out IntegerRect freeRegion);
    }
}