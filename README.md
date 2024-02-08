# RuntimeAtlas
Unity运行时图集
## 使用方法
### UGUI
1. 使用NImage替换Image<br>
2. 在需要合并对象的父节点添加NImagePacker，NImagePacker会在Awake时收集所有子节点上的Image组件并打入公共图集中<br>
注意将NImagePacker中的Atlas改为自己的管理的对象
### spriternder等
创建一个RuntimeAtlas，并将对应纹理插入<br>
``` c#
var Atlas ??= new RuntimeAtlas(1024, 1024, 1);
var res = Atlas.InsertTexture(sprite.texture);
//插入成功后，会返回对应纹理与插入的区域
if (res.succeed)
 {
      var tex = res.tex;
      var region = res.region;
      //做点什么
 }
```
[矩形区域划分参考](https://villekoskela.org/2012/08/12/rectangle-packing/)
