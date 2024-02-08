using System;
using UnityEngine;
using UnityEngine.UI;

namespace RuntimeAtlas
{
    public class NImage : Image
    {
        private string _texName = null;

        protected override void Awake()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                TryRePackTex();
#else
            TryRePackTex();
#endif
            base.Awake();
        }

        void TryRePackTex()
        {

            if(sprite==null)
                return;
            
            if (sprite.texture == null)
                return;

            //此处应改为自己控制的Atlas
            NImagePacker.Atlas ??= new RuntimeAtlas(1024, 1024, 1);

            _texName = sprite.texture.name;

            var res = NImagePacker.Atlas.InsertTexture(sprite.texture);
            if (res.succeed)
            {
                sprite = Sprite.Create(res.tex, res.region, Vector2.zero, sprite.pixelsPerUnit, 1, SpriteMeshType.Tight,
                    sprite.border);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
#if UNITY_EDITOR
            if (Application.isPlaying)
                Release();
#else
            Release();
#endif
        }

        protected void Release()
        {
            if (_texName != String.Empty)
            {
                NImagePacker.Atlas.ReleaseTexture(_texName);
            }
        }
    }
}