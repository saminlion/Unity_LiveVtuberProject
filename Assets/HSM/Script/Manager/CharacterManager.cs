using System.Collections.Generic;
using UnityEngine;
using BaseController;
using System.Collections;
using System.Linq;
using UniVRM10;

public class CharacterManager : MonoBehaviour
{
    [Header("프리팹 및 위치")]
    public GameObject[] live2DPrefabs;
    public Transform spawnRoot;              // 생성된 캐릭터를 붙일 부모 (없으면 자기 자신)
    public float spawnOffsetX = -1.0f;        // 유저마다 좌우로 살짝 띄움

    public string defaultVrmPath = "";
    private HashSet<string> loadingUsers = new();

    private Dictionary<string, ICharacterController> characterMap = new();
    private int spawnCount = 0;

    async public void ApplyUserFaceAndInput(string userId, Dictionary<string, float> parameters, string vrmPath = null)
    {
        if (characterMap.TryGetValue(userId, out var controller))
        {
            controller.SetParameters(parameters);
            return;
        }

        if (loadingUsers.Contains(userId)) return;
        loadingUsers.Add(userId);

        GameObject go = null;
        ICharacterController newController = null;

        if (userId.StartsWith("vrm_"))
        {
            var instance = await Vrm10.LoadPathAsync(
                vrmPath ?? defaultVrmPath,
                canLoadVrm0X: true,
                controlRigGenerationOption: ControlRigGenerationOption.Generate,
                showMeshes: true
            );

            if (instance == null)
            {
                Debug.LogError("❌ VRM 로딩 실패");
                loadingUsers.Remove(userId);
                return;
            }

            instance.UpdateType = Vrm10Instance.UpdateTypes.Update;
            instance.LookAtTargetType = VRM10ObjectLookAt.LookAtTargetTypes.YawPitchValue;

            // ✅ 2. 외부 GameObject 생성 (드래그 및 위치 기준)
            go = new GameObject($"VRM_{userId}_Root");

            var boxCol = go.AddComponent<BoxCollider>();
            boxCol.center = new Vector3(0, 6, 0);
            boxCol.size = new Vector3(2, 3, 1);

            // ✅ 3. 캐릭터 이동을 위한 세팅
            var DCVRM = go.AddComponent<DraggableCharacterVRM>();
            DCVRM.target = go.transform;
            DCVRM.userId = userId;

            instance.transform.SetParent(go.transform, false);
            instance.transform.localPosition = new Vector3(0, -instance.transform.position.y, 0); // 중심 정렬 보정 (선택)

            // ✅ 4. 컨트롤러 및 기본 세팅
            var vrmCtrl = instance.gameObject.AddComponent<VRMCharacterController>();
            vrmCtrl.Initialize();
            newController = vrmCtrl;

            instance.transform.localRotation = Quaternion.Euler(0, 180, 0);
            instance.transform.localScale = new Vector3(5.5f, 5.5f, 5.5f);
        }
        else
        {
            int prefabIndex = spawnCount % live2DPrefabs.Length;
            var prefab = live2DPrefabs[prefabIndex];

            go = Instantiate(live2DPrefabs[prefabIndex]);
            go.AddComponent<BoxCollider>();

            var DC = go.AddComponent<DraggableCharacterLive2D>();
            DC.userId = userId;

            go.name = $"L2D_{userId}";

            var l2Controller = go.GetComponent<Live2DCustomController>();
            if (l2Controller == null)
            {
                l2Controller = go.AddComponent<Live2DCustomController>();
            }

            l2Controller.spawnCharacterName = prefab.name;
            newController = l2Controller;
            go.transform.localScale = new Vector3(5, 5, 5);
        }

        go.transform.SetParent(spawnRoot ?? this.transform);

        // 위치 계산
        float offsetX = spawnOffsetX * spawnCount;
        float posY = userId.StartsWith("vrm_") ? -5.9f : 0f;

        Vector3 defaultPos = new Vector3(spawnOffsetX * spawnCount, userId.StartsWith("vrm_") ? posY : 0f, 0f);
        go.transform.localPosition = CharacterPositionStorage.LoadPosition(userId, defaultPos);

        characterMap[userId] = newController;
        spawnCount++;
        loadingUsers.Remove(userId);

        newController.SetParameters(parameters);

    } // ApplyUserFaceAndInput End

} // Class End
