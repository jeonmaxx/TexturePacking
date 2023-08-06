using UnityEngine;
using UnityEditor;
using System.IO;
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
    public bool isRoughness;

    private string normalPath;

    /// <summary>
    /// Hier wird ein verfügbarer Path gesucht, wo die Textur(en) später gespeichert werden können
    /// </summary>
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

    /// <summary>
    /// Hier ist das UI vom Editor
    /// </summary>
    private void OnGUI()
    {
        switch (stage)
        {
            case UIStage.Choosing:
                {
                    GUILayout.Space(20);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Pack Detail Mask Texture", GUILayout.Width(200), GUILayout.Height(100)))
                    {
                        stage = UIStage.DetailMask;
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Pack Mask Texture", GUILayout.Width(200), GUILayout.Height(100)))
                    {
                        stage = UIStage.Mask;
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
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

    /// <summary>
    /// UI fürs Packen und Unpacken
    /// </summary>
    private void PackMaskUI()
    {
        EditorGUILayout.BeginVertical();
        GUILayout.Label("Add the texture maps", EditorStyles.boldLabel);

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
        isRoughness = EditorGUILayout.Toggle("Roughness", isRoughness);

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        resTexture = TextureField("Mask Map", resTexture);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(30);


        if (GUILayout.Button("Pack Texture"))
        {
            Pack(resTexture, ColorArrayMask(), "", false);
        }

        if (GUILayout.Button("Unpack Texture"))
        {
            if (resTexture != null)
            {
                Pack(rTexture, ColorUnpackMask(Channel.Red), "_metallic", false);
                Pack(gTexture, ColorUnpackMask(Channel.Green), "_occlusion", false);
                Pack(bTexture, ColorUnpackMask(Channel.Blue), "_detailMask", false);
                Pack(aTexture, ColorUnpackMask(Channel.Alpha), "_smoothness", false);
            }
            else
            {
                Debug.LogError("No Mask Texture chosen!");
            }
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Go Back"))
        {
            stage = UIStage.Choosing;
        }

        GUILayout.EndVertical();
    }
    private void PackDetailMaskUI()
    {
        EditorGUILayout.BeginVertical();
        GUILayout.Label("Add the texture maps", EditorStyles.boldLabel);

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
        isRoughness = EditorGUILayout.Toggle("Roughness", isRoughness);

        GUILayout.Space(50);

        EditorGUILayout.BeginHorizontal();
        resTexture = TextureField("Detail Mask Map", resTexture);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);

        if (GUILayout.Button("Pack Texture"))
        {
            Pack(resTexture, ColorArrayDetailMask(), "", false) ;
        }

        if (GUILayout.Button("Unpack Texture"))
        {
            if (resTexture != null)
            {
                Pack(rTexture, ColorUnpackDetailMask(Channel.Red), "_albedo", false);
                Pack(gTexture, ColorUnpackDetailMask(Channel.Green), "_normal", true);
                Pack(bTexture, ColorUnpackDetailMask(Channel.Blue), "_smoothness", false);
            }
            else
            {
                Debug.LogError("No Mask Texture chosen!");
            }
        }

        if (GUILayout.Button("Desaturate Albedo"))
        {
            Pack(resTexture, ColorDesaturateMask(), "", false);
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Go Back"))
        {
            stage = UIStage.Choosing;
        }

        GUILayout.EndVertical();
    }

    /// <summary>
    /// Macht die neue Textur
    /// </summary>
    private void Pack(Texture2D texture, Color[] colorArray, string nameAdd, bool normal)
    {
        texture = new Texture2D(width, height);
        texture.SetPixels(colorArray);

        byte[] tex = texture.EncodeToPNG();        

        FileStream stream = new FileStream(path + textureName + nameAdd + ".png", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        BinaryWriter writer = new BinaryWriter(stream);

        for (int i  = 0; i < tex.Length; i++)
        {
            writer.Write(tex[i]);
        }

        stream.Close();
        writer.Close();

        AssetDatabase.ImportAsset(path + textureName + nameAdd + ".png", ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        if (normal)
        {
            normalPath = path + textureName + nameAdd + ".png";
            OnPostprocessTexture();
        }
    }

    /// <summary>
    /// Color[] fürs erstellen der Mask Texturen
    /// </summary>
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

            if (!isRoughness)
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

            if (!isRoughness)
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
                colors[i].a = 1;
        }

        return colors;
    }

    /// <summary>
    /// Color[] fürs erstellen der Texturen aus der Maske
    /// Bei Channel muss der Channel angegeben werden, in welchem die Textur in der Maske gespeichert ist.
    /// </summary>
    private Color[] ColorUnpackMask(Channel channel)
    {
        Color[] colors = new Color[width * height];

        switch (channel)
        {
            //Metallic
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
            //Occlusion
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
            //DetailMask -> gibt fürs erste nur graue Textur aus 
            case Channel.Blue:
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Color();
                    if (resTexture != null)
                    {
                        colors[i].r = resTexture.GetPixel(i % width, i / width).r;
                        colors[i].g = resTexture.GetPixel(i % width, i / width).r;
                        colors[i].b = resTexture.GetPixel(i % width, i / width).r;
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
            //Smoothness
            case Channel.Alpha:
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Color();

                    if (!isRoughness)
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
        // .r -> saturate -> albedo
        // .a -> Normal.r
        // .g -> Normal.g
        // .b -> Smoothness
        Color[] colors = new Color[width * height];

        switch (channel)
        {
            //Albedo -> gibt fürs erste nur graue Textur aus 
            case Channel.Red:
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Color();
                    colors[i].r = resTexture.GetPixel(i % width, i / width).r;
                    colors[i].g = resTexture.GetPixel(i % width, i / width).r;
                    colors[i].b = resTexture.GetPixel(i % width, i / width).r;
                    colors[i].a = 1;
                }
                return colors;
            //Normal
            case Channel.Green:
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Color();
                    colors[i].r = resTexture.GetPixel(i % width, i / width).g;
                    colors[i].g = resTexture.GetPixel(i % width, i / width).g;
                    colors[i].b = 1;
                    colors[i].a = 1;
                }
                return colors;
            //Smoothness
            case Channel.Blue:
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Color();
                    if (!isRoughness)
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

    /// <summary>
    /// Texture2D, damit sie im UI angezeigt wird
    /// </summary>
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

    /// <summary>
    /// Hier wird die Textur entsättigt und Color[] damit man die entsättigte Textur auch einzelnd ausgeben kann.
    /// </summary>
    private Color DesaturateColor(Color color)
    {
        double f = 0.5;
        double L = 0.3 * color.r + 0.6 * color.g + 0.1 * color.b;
        color.r = (float)(color.r + f * (L - color.r));
        color.g = (float)(color.g + f * (L - color.g));
        color.b = (float)(color.b + f * (L - color.b));

        return color;
    }
    private Color[] ColorDesaturateMask()
    {
        Color[] colors = new Color[width * height];

        for (int i = 0; i < colors.Length; i++)
        {
            Color tmp = DesaturateColor(rTexture.GetPixel(i % width, i / width));
            colors[i] = tmp;
        }
        return colors;
    }

    /// <summary>
    /// Hier wird einer Textur der TextureType NormalMap gegeben
    /// </summary>
    void OnPostprocessTexture()
    {
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(normalPath);
        importer.textureType = TextureImporterType.NormalMap;

        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(importer.assetPath, typeof(Texture2D));
        if (asset)
            EditorUtility.SetDirty(asset);
        else
            importer.textureType = TextureImporterType.NormalMap;
        AssetDatabase.Refresh();
    }
}
