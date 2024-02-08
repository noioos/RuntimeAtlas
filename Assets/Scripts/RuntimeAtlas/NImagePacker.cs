using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

 

namespace RuntimeAtlas
{
    public class NImagePacker : MonoBehaviour
    {
        //临时公共RuntimeAtlas
        public static RuntimeAtlas Atlas;

        //所有子对象imgae纹理的名称
        private List<string> _texNames = new List<string>();

        private void Awake()
        {
            //此处应改为自己控制的Atlas
            Atlas ??= new RuntimeAtlas(1024, 1024, 1);

            RepackAllImage();
        }


        public void RepackAllImage()
        {
            TraversePackImage(this.transform);
        }


        private void TraversePackImage(Transform parent)
        {
            _texNames.Clear();

            //递归将所有子物体的image组件纹理使用动态图集处理
            foreach (Transform child in parent)
            {
                var comImg = child.GetComponent<Image>();
                if (comImg != null)
                {
                    var res = Atlas.InsertTexture(comImg.sprite.texture);
                    if (res.succeed)
                    {
                        //记录对应纹理名称
                        _texNames.Add(comImg.sprite.texture.name);
                        comImg.sprite = Sprite.Create(res.tex, res.region, Vector2.zero);
                    }
                }

                TraversePackImage(child);
            }
        }

        private void ReleaseImage()
        {
            foreach (var texName in _texNames)
            {
                Atlas.ReleaseTexture(texName);
            }
        }

        private void OnDestroy()
        {
            ReleaseImage();
        }
    }
}