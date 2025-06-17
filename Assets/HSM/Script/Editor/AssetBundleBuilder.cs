using UnityEditor;

public class AssetBundleBuilder
{
    [MenuItem("Assets/Build Live2D AssetBundle")]
    public static void Build()
    {
        string path = "Assets/AssetBundles";

        if (!System.IO.Directory.Exists(path))
            System.IO.Directory.CreateDirectory(path);

        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }
}