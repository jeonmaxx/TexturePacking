using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class TexturePacking : EditorWindow
{
    public Texture2D rTexture;
    public Texture2D gTexture;
    public Texture2D bTexture;
    public Texture2D aTexture;
    public Texture2D resTexture;

    public string textureName = "untitled";
    public int width;
    public int height;
    public bool invereseSmoothness;

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
        EditorGUILayout.BeginVertical();
        GUILayout.Label("Füge die Texturen hinzu!", EditorStyles.boldLabel);

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        rTexture = TextureField("Metallic", rTexture);
        gTexture = TextureField("Occlusion", gTexture);
        bTexture = TextureField("Detail Mask", bTexture);
        aTexture = TextureField("Smoothness", aTexture);
        EditorGUILayout.EndHorizontal();

        textureName = EditorGUILayout.TextField("Name", textureName);
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);
        invereseSmoothness = EditorGUILayout.Toggle("Inverse Smoothness", invereseSmoothness);

        GUILayout.Space(50);

        if (GUILayout.Button("Pack Texture"))
        {
            Debug.Log("U wanna pack?");
            Pack();
        }

        GUILayout.Space(50);

        EditorGUILayout.BeginHorizontal();
        resTexture = TextureField("Result", resTexture);
        EditorGUILayout.EndHorizontal();

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

    private void Pack()
    {
        resTexture = new Texture2D(width, height);
        resTexture.SetPixels(ColorArray());

        byte[] tex = resTexture.EncodeToPNG();

        FileStream stream = new FileStream(path + textureName + ".png", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        BinaryWriter writer = new BinaryWriter(stream);

        for(int i  = 0; i < tex.Length; i++)
        {
            writer.Write(tex[i]);
        }

        stream.Close();
        writer.Close();

        AssetDatabase.ImportAsset(path + textureName + ".png", ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
    }

    private Color[] ColorArray()
    {
        Color[] colors = new Color[width * height];

        for(int i = 0; i < colors.Length;  i++)
        {
            colors[i] = new Color();

            if (aTexture != null)
                colors[i].r = aTexture.GetPixel(i % width, i / width).r;
            else 
                colors[i].r = 1;

            if (gTexture != null)
                colors[i].g = gTexture.GetPixel(i % width, i / width).r;
            else
                colors[i].g = 1;

            if (bTexture != null)
                colors[i].b = bTexture.GetPixel(i % width, i / width).r;
            else
                colors[i].b = 1;
            if(aTexture != null)
            {
                if(!invereseSmoothness)
                    colors[i].a = aTexture.GetPixel(i % width, i / width).r;
                else
                    colors[i].a = 1 - aTexture.GetPixel(i % width, i / width).r;
            }
            else
                colors[i].a = 1;
        }

        return colors;
    }

}
