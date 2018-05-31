using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class ParticleExporter : MonoBehaviour
{
    public string folder = "PNG_Animations";

    public int frameRate = 25;                  // export frame rate 导出帧率，设置Time.captureFramerate会忽略真实时间，直接使用此帧率
    public float frameCount = 5;              // export frame count 导出帧的数目，100帧则相当于导出5秒钟的光效时间。由于导出每一帧的时间很长，所以导出时间会远远长于直观的光效播放时间
    public int screenWidth = 1136;               // not use 暂时没用，希望可以直接设置屏幕的大小（即画布的大小）
    public int screenHeight = 640;
    public Vector3 cameraPosition = Vector3.zero;
    public Vector3 cameraRotation = Vector3.zero;

    private string realFolder = "";
    private float originaltimescaleTime;
    private float currentTime = 0;
    private bool over = false;
    private int currentIndex = 0;
    private Camera exportCamera;    // camera for export 导出的摄像机，使用RenderTexture

    public void Start()
    {
        Time.captureFramerate = frameRate;

        realFolder = Path.Combine(folder, name);

        if( !Directory.Exists(realFolder) )
        {
            Directory.CreateDirectory(realFolder);
        }

        originaltimescaleTime = Time.timeScale;

        GameObject goCamera = Camera.main.gameObject;
        if( cameraPosition != Vector3.zero )
        {
            goCamera.transform.position = cameraPosition;
        }

        if( cameraRotation != Vector3.zero )
        {
            goCamera.transform.rotation = Quaternion.Euler(cameraRotation);
        }

        GameObject go = Instantiate(goCamera) as GameObject;
        exportCamera = go.GetComponent<Camera>();

        currentTime = 0;


    }

    void Update()
    {
        currentTime += Time.deltaTime;
        if( !over && currentIndex >= frameCount )
        {
            over = true;
            Cleanup();
            Debug.Log("Finish");
            return;
        }

        // 每帧截屏
        StartCoroutine(CaptureFrame());
    }

    void Cleanup()
    {
        //DestroyImmediate(exportCamera);
        DestroyImmediate(gameObject);
    }

    IEnumerator CaptureFrame()
    {
        Time.timeScale = 0;
       
        yield return new WaitForEndOfFrame();

        string filename = String.Format("{0}/{1:D04}.png", realFolder, ++currentIndex);
        Debug.Log(filename);

        int width = Screen.width;
        int height = Screen.height;

        RenderTexture blackCamRenderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        RenderTexture whiteCamRenderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);

        exportCamera.targetTexture = blackCamRenderTexture;
        exportCamera.backgroundColor = Color.black;
        exportCamera.Render();
        RenderTexture.active = blackCamRenderTexture;
        Texture2D texb = GetTex2D();

        //Now do it for Alpha Camera
        exportCamera.targetTexture = whiteCamRenderTexture;
        exportCamera.backgroundColor = Color.white;
        exportCamera.Render();
        RenderTexture.active = whiteCamRenderTexture;
        Texture2D texw = GetTex2D();

        // If we have both textures then create final output texture
        if( texw && texb )
        {
            Texture2D outputtex = new Texture2D(width, height, TextureFormat.ARGB32, false);

            for( int y = 0; y < outputtex.height; ++y )
            {
                for( int x = 0; x < outputtex.width; ++x )
                {
                    float alpha;
                    alpha = texw.GetPixel(x, y).r - texb.GetPixel(x, y).r;
                    alpha = 1.0f - alpha;
                    Color color;
                    if( alpha == 0 )
                    {
                        color = Color.clear;
                    }
                    else
                    {
                        color = texb.GetPixel(x, y);
                    }
                    color.a = alpha;
                    outputtex.SetPixel(x, y, color);
                }
            }


            byte[] pngShot = outputtex.EncodeToPNG();
            File.WriteAllBytes(filename, pngShot);

            pngShot = null;
            RenderTexture.active = null;
            DestroyImmediate(outputtex);
            outputtex = null;
            DestroyImmediate(blackCamRenderTexture);
            blackCamRenderTexture = null;
            DestroyImmediate(whiteCamRenderTexture);
            whiteCamRenderTexture = null;
            DestroyImmediate(texb);
            texb = null;
            DestroyImmediate(texw);
            texb = null;

            System.GC.Collect();

            Time.timeScale = originaltimescaleTime;
        }
    }

    private Texture2D GetTex2D()
    {
        int width = Screen.width;
        int height = Screen.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        return tex;
    }
}