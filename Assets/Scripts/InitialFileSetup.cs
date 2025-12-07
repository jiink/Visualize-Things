using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class InitialFileSetup : MonoBehaviour
{
    private string fileName = "Steering Assem Sample.glb";

    void Start()
    {
        StartCoroutine(CopyAssetToPersistentData());
    }

    IEnumerator CopyAssetToPersistentData()
    {
        string targetDir = Path.Combine(Application.persistentDataPath, Services.TransferDirName);
        string targetPath = Path.Combine(targetDir, fileName);
        if (File.Exists(targetPath))
        {
            Debug.Log("Sample file already exists. Skipping copy.");
            yield break;
        }
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }
        string sourcePath = Path.Combine(Application.streamingAssetsPath, fileName);
        Debug.Log("Copying sample file from: " + sourcePath);
        using (UnityWebRequest request = UnityWebRequest.Get(sourcePath))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error copying file: " + request.error);
            }
            else
            {
                File.WriteAllBytes(targetPath, request.downloadHandler.data);
                Debug.Log("Successfully copied sample file to: " + targetPath);
            }
        }
    }

}
