using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CaptureScreenshot : MonoBehaviour
{

    [MenuItem("CaptureScreenshot/CaptureScreen", false, 1)]
    private static void CaptureScreenMenu()
    {
        CaptureScreen();
    }

    [MenuItem("CaptureScreenshot/CaptureScreenshotRect", false, 2)]
    private static void CaptureScreenshotRectMenu()
    {
        Debug.Log(Screen.width );
        CaptureScreenshotRect(new Rect(0, 0, Screen.width, Screen.height));
    }

    [MenuItem("CaptureScreenshot/CaptureCamera", false, 3)]
    private static void CaptureCameraMenu()
    {
        CaptureCamera(Camera.main, new Rect(0, 0, Screen.width, Screen.height));
    }

    //这个方法，截取的是某一帧时整个游戏的画面，或者说是全屏截图吧。
    //a、不能针对某一个相机（camera）的画面，进行截图。
    //b、对局部画面截图，实现起来不方便，效率也低，不建议在项目中使用：
    //虽然CaptureScreenshot这个方法呢，本身是不要做到这一点的。但是我们可以走曲线救国的路线来实现它。
    //思路是这样的：你可以先用这个方法截图一个全屏，然后通过路径获取到这个截图；
    //接下来就通过相关的图形类来，取得这个截图的局部区域并保存下来，这样就能得到一个局部截图了。

    private static void CaptureScreen()
    {
        Application.CaptureScreenshot("Screenshot.png", 0);
    }

    //2、这第二个截图的方法是，使用Texture2d类下的相关方法，也可实现截图功能。
    //截全屏：
    //CaptureScreenshotRect( new Rect(Screen.width*0f, Screen.height*0f, Screen.width*1f, Screen.height*1f));
    //截中间4分之1:
    //CaptureScreenshotRect( new Rect(Screen.width*0.25f, Screen.height*0.25f, Screen.width*0.5f, Screen.height*0.5f));

    //注意：读取的是整个Unity Game视图的截屏，没什么意义

    /// <summary>  
    /// CaptureScreenshotRect  
    /// </summary>  
    /// <param name="rect">Rect.截图的区域，左下角为o点</param>  
    private static Texture2D CaptureScreenshotRect(Rect rect)
    {
        // 先创建一个的空纹理，大小可根据实现需要来设置  
        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);

        // 读取屏幕像素信息并存储为纹理数据，  
        screenShot.ReadPixels(rect, 0, 0, false);
        screenShot.Apply();

        // 然后将这些纹理数据，成一个png图片文件  
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = Application.dataPath + "/Screenshot.png";
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("截屏了一张图片: {0}", filename));

        AssetDatabase.Refresh();

        return screenShot;
    }

    //3、这第三个方法，可以针对某个相机进行截图。

    /// <summary>  
    /// 对相机截图。   
    /// </summary>  
    /// <returns>The screenshot2.</returns>  
    /// <param name="camera">Camera.要被截屏的相机</param>  
    /// <param name="rect">Rect.截屏的区域</param>  
    private static Texture2D CaptureCamera(Camera camera, Rect rect)
    {
        // 创建一个RenderTexture对象  
        RenderTexture rt = new RenderTexture((int)rect.width, (int)rect.height, 0);
        
        // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机  
        camera.targetTexture = rt;
        camera.Render();
        
        //ps: --- 如果这样加上第二个相机，可以实现只截图某几个指定的相机一起看到的图像。  
        //ps: camera2.targetTexture = rt;  
        //ps: camera2.Render();  
        //ps: -------------------------------------------------------------------  

        // 激活这个rt, 并从中中读取像素。  
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(rect, 0, 0);// 注：这个时候，它是从RenderTexture.active中读取像素  
        screenShot.Apply();

        // 重置相关参数，以使用camera继续在屏幕上显示  
        camera.targetTexture = null;

        //ps: camera2.targetTexture = null;  

        RenderTexture.active = null; // JC: added to avoid errors  
        GameObject.Destroy(rt);
        
        // 最后将这些纹理数据，成一个png图片文件  
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = Application.dataPath + "/Screenshot1.png";
        System.IO.File.WriteAllBytes(filename, bytes);

        Debug.Log(string.Format("截屏了一张照片: {0}", filename));

        AssetDatabase.Refresh();

        return screenShot;
    }
}
