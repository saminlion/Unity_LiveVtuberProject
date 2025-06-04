using UnityEngine;
using UnityEditor;
using UniVRM10;
using System.Threading.Tasks;
using System.IO;

public class VrmPrefabBaker : MonoBehaviour
{
    [MenuItem("Tools/VRM/Generate Prefab with ControlRig")]
    public static async void BakeVRMToPrefab()
    {
        string path = EditorUtility.OpenFilePanel("Load VRM", "Assets", "vrm");
        if (string.IsNullOrEmpty(path)) return;

        var instance = await Vrm10.LoadPathAsync(
            path,
            canLoadVrm0X: true,
            controlRigGenerationOption: ControlRigGenerationOption.Generate,
            showMeshes: true
        );

        if (instance == null)
        {
            Debug.LogError("❌ VRM 로딩 실패");
            return;
        }

        var prefabFolder = "Assets/Prefabs/";
        if (!Directory.Exists(prefabFolder))
        {
            Directory.CreateDirectory(prefabFolder);
        }

        string fileName = Path.GetFileNameWithoutExtension(path);
        string savePath = $"{prefabFolder}{fileName}_ControlRig.prefab";

        var prefab = PrefabUtility.SaveAsPrefabAsset(instance.gameObject, savePath);
        Debug.Log($"✅ Prefab 저장 완료: {savePath}");

        // 로드된 인스턴스 제거
        DestroyImmediate(instance.gameObject);
    }
}