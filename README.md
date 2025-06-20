# Unity_LiveVtuberProject
LiveVtuberProject

🎬 **Demo Video**  
https://www.youtube.com/watch?v=-pb2VaZuflg

---

# Unity Code-Only Repository

🎯 This repository contains only the **C# script files** from a Unity project. All non-code assets have been excluded to comply with copyright and distribution policies.


Only the `.cs` files are included. All asset-related files have been ignored via `.gitignore`.
---

## 🚫 Excluded Items

To ensure clean version control and avoid legal/license issues, the following items are excluded:

- Unity asset files: `.prefab`, `.mat`, `.fbx`, `.png`, `.wav`, etc.
- External plugins and SDKs: VRM, Live2D, lilToon, etc.
- Build outputs and cache folders: `Library/`, `Build/`, `Temp/`, `.apk`, `.unitypackage`, etc.

---

## 🔗 External Dependencies (Required Separately)

The original project uses the following external plugins and SDKs. These are **not included** in this repository and must be downloaded manually:

| Plugin/SDK | Description |
|------------|-------------|
| **VRM10 / UniVRM** | VRM model loader for Unity |
| **Live2D Cubism SDK** | For Live2D animated characters |
| **lilToon Shader** | Advanced Unity shader |
| **Extenject (Zenject)** | Dependency injection framework       |
| **UniRx**           | Reactive Extensions for Unity (Rx)        |
| **UniTask**         | Lightweight async/await for Unity         |

> ⚠️ Each plugin must be installed according to its license and usage terms.

---

## 📦 AssetBundle Support
This project uses Unity AssetBundle workflow to separate and dynamically load character prefabs and other assets.

All in-game resources (such as Live2D/VRM prefabs) are intended to be distributed separately as AssetBundles.

You must build the required AssetBundles yourself using Unity's AssetBundle pipeline and assign them at runtime.

See the AssetBundleBuilder.cs script in the repository for a sample editor utility.

No asset files are included in this code-only repository; only script files are versioned.

---


## ✅ License

This repository is published under the **MIT License**.  
However, **external libraries referenced above are subject to their own licenses**.

---

## 📌 Notes

- This repository is intended for reference, review, or submission of **code only**.
- All intellectual property related to characters, models, or other media assets is retained by their respective owners.
