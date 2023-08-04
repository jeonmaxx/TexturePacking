using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using JetBrains.Annotations;

public enum UIStage {Choosing, DetailMask, Mask }
public enum Channel { Red, Green, Blue, Alpha }
public class TexturePacking : EditorWindow
{
    public Texture2D rTexture;
    public Texture2D gTexture;
    public Texture2D bTexture;
    public Texture2D aTexture;
    public Texture2D resTexture;

    public UIStage stage = UIStage.Choosing;

    public string textureName = "untitled";
    public int width = 2048;
    public int height = 2048;
    public bool inverseSmoothness;

    private string path
    {
        get 
        {
            string a = "";
            if(rTexture != null)
            {
                a = AssetDatabase.GetAssetPath((UnityEngine.Object)rTexture);
                a = a.Substring(0, a.IndexOf(((UnityEngine.Object)rTexture).name));
                return a;
            }
            if (gTexture != null)
            {
                a = AssetDatabase.GetAssetPath((UnityEngine.Object)gTexture);
                a = a.Substring(0, a.IndexOf(((UnityEngine.Object)gTexture).name));
                return a;
            }
            if (bTexture != null)
            {
                a = AssetDatabase.GetAssetPath((UnityEngine.Object)bTexture);
                a = a.Substring(0, a.IndexOf(((UnityEngine.Object)bTexture).name));
                return a;
            }
            if (aTexture != null)
            {
                a = AssetDatabase.GetAssetPath((UnityEngine.Object)aTexture);
                a = a.Substring(0, a.IndexOf(((UnityEngine.Object)aTexture).name));
                return a;
            }
            if (resTexture != null)
            {
                a = AssetDatabase.GetAssetPath((UnityEngine.Object)resTexture);
                a = a.Substring(0, a.IndexOf(((UnityEngine.Object)resTexture).name));
                return a;
            }

            return a;
        }
    }

    [MenuItem("Window/Texture Packing")]
    public static void ShowWindow()
    {
        GetWindow<TexturePacking>("Texture Channel Packing");
    }

    [Obsolete]
    private void OnGUI()
    {
        switch (stage)
        {
            case UIStage.Choosing:
                {
                    EditorGUILayout.BeginVertical();
                    if (GUILayout.Button("Pack Mask Texture"))
                    {
                        stage = UIStage.Mask;
                    }

                    if (GUILayout.Button("Pack Detail Mask Texture"))
                    {
                        stage = UIStage.DetailMask;
                    }

                    EditorGUILayout.EndVertical();
                    break;
                }
            case UIStage.Mask:
                {
                    PackMaskUI();
                    break;
                }
            case UIStage.DetailMask:
                {
                    PackDetailMaskUI();
                    break;
                }
        }
    }

    private void PackMaskUI()
    {
        EditorGUILayout.BeginVertical();
        GUILayout.Label("Füge die Texturen hinzu!", EditorStyles.boldLabel);

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        rTexture = TextureField("Metallic", rTexture);
        gTexture = TextureField("Occlusion", gTexture);
        bTexture = TextureField("Detail Mask", bTexture);
        aTexture = TextureField("Smoothness", aTexture);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);

        textureName = EditorGUILayout.TextField("Name", textureName);
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);
        inverseSmoothness = EditorGUILayout.Toggle("Inverse Smoothness", inverseSmoothness);

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        resTexture = TextureField("Mask Map", resTexture);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(30);


        if (GUILayout.Button("Pack Texture"))
        {
            Debug.Log("U wanna pack?");
            Pack(resTexture, ColorArrayMask(), "");
        }

        if (GUILayout.Button("Unpack Texture"))
        {
            Debug.Log("U wanna pack?");

            if (resTexture != null)
            {
                Pack(rTexture, ColorUnpackMask(Channel.Red), "_metallic");
                Pack(gTexture, ColorUnpackMask(Channel.Green), "_occlusion");
                Pack(bTexture, ColorUnpackMask(Channel.Blue), "_detailMask");
                Pack(aTexture, ColorUnpackMask(Channel.Alpha), "_smoothness");
            }
            else
            {
                Debug.LogError("No Mask Texture chosen!");
            }
        }

        if (GUILayout.Button("Go Back"))
        {
            stage = UIStage.Choosing;
        }

        GUILayout.EndVertical();
    }


    private void PackDetailMaskUI()
    {
        EditorGUILayout.BeginVertical();
        GUILayout.Label("Füge die Texturen hinzu!", EditorStyles.boldLabel);

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        rTexture = TextureField("Albedo", rTexture);
        gTexture = TextureField("Normal", gTexture);
        bTexture = TextureField("Smoothness", bTexture);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);

        textureName = EditorGUILayout.TextField("Name", textureName);
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);
        inverseSmoothness = EditorGUILayout.Toggle("Inverse Smoothness", inverseSmoothness);

        GUILayout.Space(50);

        EditorGUILayout.BeginHorizontal();
        resTexture = TextureField("Detail Mask Map", resTexture);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);

        if (GUILayout.Button("Pack Texture"))
        {
            Debug.Log("U wanna pack?");
            Pack(resTexture, ColorArrayDetailMask(), "") ;
        }

        if (GUILayout.Button("Unpack Texture"))
        {
            Debug.Log("U wanna pack?");

            if (resTexture != null)
            {
                Pack(rTexture, ColorUnpackDetailMask(Channel.Red), "_albedo");
                Pack(gTexture, ColorUnpackDetailMask(Channel.Green), "_normal");
                Pack(bTexture, ColorUnpackDetailMask(Channel.Blue), "_smoothness");
            }
            else
            {
                Debug.LogError("No Mask Texture chosen!");
            }
        }

        if (GUILayout.Button("Desaturate Albedo"))
        {
            Pack(resTexture, ColorDesaturateMask(), "");
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Go Back"))
        {
            stage = UIStage.Choosing;
        }

        GUILayout.EndVertical();
    }

    private static Texture2D TextureField(string name, Texture2D texture)
    {
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.UpperCenter;
        style.fixedWidth = 100;
        GUILayout.Label(name, style);
        var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(100), GUILayout.Height(100));
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        return result;
    }

    private void Pack(Texture2D texture, Color[] colorArray, string nameAdd)
    {
        texture = new Texture2D(width, height);
        texture.SetPixels(colorArray);

        byte[] tex = texture.EncodeToPNG();

        FileStream stream = new FileStream(path + textureName + nameAdd + ".png", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        BinaryWriter writer = new BinaryWriter(stream);

        for(int i  = 0; i < tex.Length; i++)
        {
            writer.Write(tex[i]);
        }

        stream.Close();
        writer.Close();

        AssetDatabase.ImportAsset(path + textureName + nameAdd + ".png", ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
    }

    private Color[] ColorUnpackMask(Channel channel)
    {
        Color[] colors = new Color[width * height];

        switch(channel)
        {
            case Channel.Red:
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Color();
                    if (resTexture != null)
                    {
                        colors[i].r = resTexture.GetPixel(i % width, i / width).r;
                        colors[i].g = resTexture.GetPixel(i % width, i / width).r;
                        colors[i].b = resTexture.GetPixel(i % width, i / width).r;
                        colors[i].a = 1;
                    }
                    else
                    {
                        colors[i].r = 1;
                        colors[i].g = 1;
                        colors[i].b = 1;
                        colors[i].a = 1;
                    }
                }
                return colors;

            case Channel.Green:
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Color();
                    if (resTexture != null)
                    {
                        colors[i].r = resTexture.GetPixel(i % width, i / width).g;
                        colors[i].g = resTexture.GetPixel(i % width, i / width).g;
                        colors[i].b = resTexture.GetPixel(i % width, i / width).g;
                        colors[i].a = resTexture.GetPixel(i % width, i / width).g;
                    }
                    else
                    {
                        colors[i].r = 1;
                        colors[i].g = 1;
                        colors[i].b = 1;
                        colors[i].a = 1;
                    }
                }
                return colors;

            case Channel.Blue:
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Color();
                    if (resTexture != null)
                    {
                        colors[i].r = resTexture.GetPixel(i % width, i / width).b;
                        colors[i].g = resTexture.GetPixel(i % width, i / width).b * 0.5f;
                        colors[i].b = 0;
                        colors[i].a = resTexture.GetPixel(i % width, i / width).b;
                    }
                    else
                    {
                        colors[i].r = 1;
                        colors[i].g = 1;
                        colors[i].b = 1;
                        colors[i].a = 1;
                    }
                }
                return colors;
            

            case Channel.Alpha:
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Color();

                    if (!inverseSmoothness)
                    {
                        if (resTexture != null)
                        {
                            colors[i].r = resTexture.GetPixel(i % width, i / width).a;
                            colors[i].g = resTexture.GetPixel(i % width, i / width).a;
                            colors[i].b = resTexture.GetPixel(i % width, i / width).a;
                            colors[i].a = resTexture.GetPixel(i % width, i / width).a;
                        }
                        else
                        {
                            colors[i].r = 0;
                            colors[i].g = 0;
                            colors[i].b = 0;
                            colors[i].a = 0;
                        }
                    }
                    else
                    {
                        if (resTexture != null)
                        {
                            colors[i].r = 1 - resTexture.GetPixel(i % width, i / width).a;
                            colors[i].g = 1 - resTexture.GetPixel(i % width, i / width).a;
                            colors[i].b = 1 - resTexture.GetPixel(i % width, i / width).a;
                            colors[i].a = 1;
                        }
                        else
                        {
                            colors[i].r = 1;
                            colors[i].g = 1;
                            colors[i].b = 1;
                            colors[i].a = 1;
                        }
                    }
                }        
                return colors;
        }
        return colors;
    }

    private Color[] ColorUnpackDetailMask(Channel channel)
    {
        Color[] colors = new Color[width * height];

        switch (channel)
        {
            case Channel.Red:
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Color();
                    colors[i].r = 1 - resTexture.GetPixel(i % width, i / width).r;
                    colors[i].g = 1 - resTexture.GetPixel(i % width, i / width).g;
                    colors[i].b = 1 - resTexture.GetPixel(i % width, i / width).b;
                    colors[i].a = 1;
                }
                return colors;

            case Channel.Green:
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Color();
                    colors[i].r = resTexture.GetPixel(i % width, i / width).a;
                    colors[i].g = resTexture.GetPixel(i % width, i / width).g;
                    colors[i].b = 1;
                    colors[i].a = 1;
                }
                return colors;

            case Channel.Blue:
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Color();
                    if (!inverseSmoothness)
                    {
                        colors[i].r = resTexture.GetPixel(i % width, i / width).b;
                        colors[i].g = resTexture.GetPixel(i % width, i / width).b;
                        colors[i].b = resTexture.GetPixel(i % width, i / width).b;
                        colors[i].a = 1;
                    }
                    else
                    {
                        colors[i].r = 1 - resTexture.GetPixel(i % width, i / width).b;
                        colors[i].g = 1 - resTexture.GetPixel(i % width, i / width).b;
                        colors[i].b = 1 - resTexture.GetPixel(i % width, i / width).b;
                        colors[i].a = 1;
                    }
                }
                return colors;
        }
        return colors;
    }

    private Color[] ColorArrayMask()
    {
        Color[] colors = new Color[width * height];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color();

            if (rTexture != null)
                colors[i].r = rTexture.GetPixel(i % width, i / width).r;
            else
                colors[i].r = 0;

            if (gTexture != null)
                colors[i].g = gTexture.GetPixel(i % width, i / width).g;
            else
                colors[i].g = 0;

            if (bTexture != null)
                colors[i].b = bTexture.GetPixel(i % width, i / width).b;
            else
                colors[i].b = 0;


            if (!inverseSmoothness)
            {
                if (aTexture != null)
                    colors[i].a = aTexture.GetPixel(i % width, i / width).a;
                else
                    colors[i].a = 1;
            }
            else
            {
                if (aTexture != null)
                    colors[i].a = 1 - aTexture.GetPixel(i % width, i / width).a;
                else
                    colors[i].a = 0;
            }
        }

        return colors;
    }

    private Color[] ColorArrayDetailMask()
    {
        Color[] colors = new Color[width * height];

        //Albedo -> desaturate -> .r
        //Normal.r -> .a
        //Normal.g = .g
        //Smoothness = .b

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color();

            if (rTexture != null)
            {
                Color tmp = DesaturateColor(rTexture.GetPixel(i % width, i / width));
                colors[i].r = tmp.r;
            }
            else
                colors[i].r = 0;

            if (gTexture != null)
                colors[i].g = gTexture.GetPixel(i % width, i / width).g;
            else
                colors[i].g = 0;

            if (!inverseSmoothness)
            {
                if (bTexture != null)
                    colors[i].b = bTexture.GetPixel(i % width, i / width).b;
                else
                    colors[i].b = 1;
            }
            else
            {
                if(bTexture != null)
                    colors[i].b = 1 - bTexture.GetPixel(i % width, i / width).b;
                else 
                    colors[i].b = 0;
            }

            if (gTexture != null)
                colors[i].a = gTexture.GetPixel(i % width, i / width).r;
            else
                colors[i].a = 0;
        }

        return colors;
    }

    private Color[] ColorDesaturateMask()
    {
        Color[] colors = new Color[width * height];

        for (int i = 0; i < colors.Length; i++)
        {
            //colors[i] = rTexture.GetPixel(i % width, i / width);
            //DesaturateColor(colors[i]);

            Color tmp = DesaturateColor(rTexture.GetPixel(i % width, i / width));
            colors[i] =tmp;
        }

        return colors;
    }

    private Color DesaturateColor(Color color)
    {
        double f = 0.5; // desaturate by 20%
        double L = 0.3 * color.r + 0.6 * color.g + 0.1 * color.b;
        color.r = (float)(color.r + f * (L - color.r));
        color.g = (float)(color.g + f * (L - color.g));
        color.b = (float)(color.b + f * (L - color.b));

        return color;
    }
}
