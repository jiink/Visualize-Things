using Oculus.Interaction.Surfaces;
using System.IO;
using System.Threading.Tasks;
using TriLibCore;
using UnityEngine;

public class ModelLoadingService : MonoBehaviour
{

    [SerializeField] private GameObject _modelTemplatePrefab;
    [SerializeField] private GameObject _cubeVisualizerPrefab;
    [SerializeField] private UnityEngine.Material _occlusionFriendlyLit;
    [SerializeField] private GameObject _modelLoadingIndicator;

    private GameObject _loadingIndicator;

    // Returns the loaded model object once its done loading.
    public async Task<GameObject> ImportModelAsync(string filePath, Vector3 position)
    {
        Debug.Log($"Importing file: {filePath}");

        // Spawn a model loading indicator that will be destroyed when the model is done loading
        _loadingIndicator = Instantiate(_modelLoadingIndicator);
        _loadingIndicator.transform.position = position;
        //_loadingIndicator.transform.position = UnityEngine.Camera.main.transform.position +
        //                                 (UnityEngine.Camera.main.transform.forward * 0.3f);
        var _loadingIndicatorC = _loadingIndicator.GetComponent<LoadingIndicator>();
        _loadingIndicatorC.Text = $"Loading {Path.GetFileName(filePath)}...";

        var tcs = new TaskCompletionSource<GameObject>();
        AssetLoader.LoadModelFromFile(filePath,
            (assetLoaderContext) => // when model is done loading
            {
                Debug.Log("Model imported successfully!");
                GameObject loadedModel = assetLoaderContext.RootGameObject;
                if (loadedModel != null)
                {
                    var loadedRoot = ProcessLoadedModel(loadedModel, filePath, position);
                    _loadingIndicatorC.Text = "Model loaded!";
                    Destroy(_loadingIndicator, 2.0f);
                    tcs.SetResult(loadedRoot);
                }
                else
                {
                    _loadingIndicatorC.Text = "Failed to retrieve the loaded model.";
                    Debug.LogError("Failed to retrieve the loaded model.");
                    Destroy(_loadingIndicator, 2.0f);
                    tcs.SetResult(null);
                }
            },
            (assetLoaderContext) => // when model's materials are done loading
            {
                ReplaceMaterialsRecursively(assetLoaderContext.RootGameObject.transform, _occlusionFriendlyLit);
            },
            (_, _) => { },
            (error) =>
            {
                Debug.LogError($"Failed to load model: {error}");
                _loadingIndicatorC.Text = "Failed to load model: " + error;
                Destroy(_loadingIndicator, 8.0f);
                tcs.SetResult(null);
            });
        return await tcs.Task;
    }


    private GameObject ProcessLoadedModel(GameObject loadedModel, string filePath, Vector3 position)
    {
        if (loadedModel == null)
        {
            Debug.LogError("Loaded model is null!");
            return null;
        }

        GameObject template = Instantiate(_modelTemplatePrefab);
        loadedModel.transform.SetParent(template.transform);

        Bounds bounds = new(loadedModel.transform.position, Vector3.zero);
        // Calculate bounding box for collider of whole object including all children
        AddBoundsRecursively(loadedModel.transform, ref bounds);

        // Need collider for the whole object
        BoxCollider boxCollider = template.AddComponent<BoxCollider>();
        boxCollider.center = bounds.center - template.transform.position;
        boxCollider.size = bounds.size;

        // fill in collider field
        ColliderSurface colliderSurface = template.GetComponent<ColliderSurface>();
        colliderSurface.InjectCollider(boxCollider);

        // Have Core listen to wselection events
        SelectableModel selectableModel = template.GetComponent<SelectableModel>();
        //selectableModel.Selected += StrodeloCore.Instance.OnModelSelected;
        selectableModel.modelFileSourcePath = filePath; // just so it can be saved later

        //template.transform.position = UnityEngine.Camera.main.transform.position +
        //                                 (UnityEngine.Camera.main.transform.forward * 0.3f);
        template.transform.position = position;

        GameObject colliderVisualizer = Instantiate(_cubeVisualizerPrefab);
        colliderVisualizer.tag = "SelectionVisualizer";
        colliderVisualizer.transform.SetParent(template.transform);
        colliderVisualizer.transform.localPosition = boxCollider.center;
        colliderVisualizer.transform.localScale = boxCollider.size;
        colliderVisualizer.SetActive(false);

        return template;
    }

    private void ReplaceMaterialsRecursively(Transform transform, UnityEngine.Material newMaterial)
    {
        MeshRenderer meshRenderer = transform.gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            // copy the materials array into a new array
            int numMats = meshRenderer.materials.Length;
            if (numMats == 0)
            {
                Debug.LogError("No materials found on mesh renderer!");
                return;
            }
            UnityEngine.Material[] newMats = new UnityEngine.Material[numMats];
            Debug.Log($"Number of materials: {numMats}");
            for (int i = 0; i < numMats; i++)
            {
                UnityEngine.Material oldMat = meshRenderer.materials[i];
                UnityEngine.Material newMat = new(newMaterial);
                // transfer properties
                newMat.CopyPropertiesFromMaterial(oldMat);
                newMats[i] = newMat;
            }
            meshRenderer.materials = newMats;
        }
        // Recursively call this method for each child
        foreach (Transform child in transform)
        {
            ReplaceMaterialsRecursively(child, newMaterial);
        }
    }

    private void AddBoundsRecursively(Transform transform, ref Bounds bounds)
    {
        foreach (Transform child in transform)
        {
            MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                Debug.Log("Hit!");
                bounds.Encapsulate(meshRenderer.bounds);
            }
            // Recursively call this method for each child
            AddBoundsRecursively(child, ref bounds);
        }
    }
}
