using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using TriLibCore;
using UnityEngine;

public class ModelSpawnedEventArgs : EventArgs
{
    public GameObject SpawnedModel { get; }
    public ModelSpawnedEventArgs(GameObject spawnedModel)
    {
        SpawnedModel = spawnedModel;
    }
}

public class ModelLoadingService : MonoBehaviour
{

    [SerializeField] private GameObject _modelTemplatePrefab;
    [SerializeField] private GameObject _cubeVisualizerPrefab;
    [SerializeField] private UnityEngine.Material _occlusionFriendlyLit;
    [SerializeField] private GameObject _modelLoadingIndicator;
    // for organization in the hierarchy
    [SerializeField] private GameObject _wrapperObject; 

    private GameObject _loadingIndicator;
    private AssetLoaderOptions _trilibAssetLoaderOptions;

    public event EventHandler<ModelSpawnedEventArgs> ModelSpawnedEvent;

    void Start()
    {
        _trilibAssetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        _trilibAssetLoaderOptions.GenerateColliders = false;
        _trilibAssetLoaderOptions.ConvexColliders = false;
    }

    // Returns the loaded model object once its done loading.
    public async Task<(GameObject go, float longestDimension)> ImportModelAsync(string filePath, Vector3 position)
    {
        Debug.Log($"Importing file: {filePath}");

        // Spawn a model loading indicator that will be destroyed when the model is done loading
        _loadingIndicator = Instantiate(_modelLoadingIndicator);
        _loadingIndicator.transform.position = position;
        var _loadingIndicatorC = _loadingIndicator.GetComponent<LoadingIndicator>();
        _loadingIndicatorC.Text = $"Loading {Path.GetFileName(filePath)}...";

        var tcs = new TaskCompletionSource<(GameObject, float)>();
        AssetLoader.LoadModelFromFile(filePath,
            (assetLoaderContext) => // when model is done loading
            {
                Debug.Log("Model imported successfully!");
                GameObject loadedModel = assetLoaderContext.RootGameObject;
                if (loadedModel != null)
                {
                    (var loadedRoot, float longest) = ProcessLoadedModel(loadedModel, filePath, position);
                    _loadingIndicatorC.Text = "Model loaded!";
                    Destroy(_loadingIndicator, 2.0f);
                    tcs.SetResult((loadedRoot, longest));
                    ModelSpawnedEvent?.Invoke(this, new ModelSpawnedEventArgs(loadedRoot));
                }
                else
                {
                    _loadingIndicatorC.Text = "Failed to retrieve the loaded model.";
                    Debug.LogError("Failed to retrieve the loaded model.");
                    Destroy(_loadingIndicator, 2.0f);
                    tcs.SetResult((null, 0));
                }
            },
            (assetLoaderContext) => // when model's materials are done loading (final stage of loading process)
            {
                ReplaceMaterialsRecursively(assetLoaderContext.RootGameObject.transform, _occlusionFriendlyLit);
            },
            (_, _) => { },
            (error) =>
            {
                Debug.LogError($"Failed to load model: {error}");
                _loadingIndicatorC.Text = "Failed to load model: " + error;
                Destroy(_loadingIndicator, 8.0f);
                tcs.SetResult((null, 0));
            },
            null,
            _trilibAssetLoaderOptions);
        return await tcs.Task;
    }

    private float FindBoundsLongestDimension(Bounds bounds)
    {
        Vector3 s = bounds.size;
        float big = Mathf.Max(s.x, s.y, s.z);
        return big;
    }


    private (GameObject go, float longestDimension) ProcessLoadedModel(GameObject loadedModel, string filePath, Vector3 position)
    {
        if (loadedModel == null)
        {
            Debug.LogError("Loaded model is null!");
            return (null, 0);
        }

        GameObject template = Instantiate(_modelTemplatePrefab, _wrapperObject.transform);
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
        //selectableModel.modelFileSourcePath = filePath; // just so it can be saved later

        //template.transform.position = UnityEngine.Camera.main.transform.position +
        //                                 (UnityEngine.Camera.main.transform.forward * 0.3f);
        template.transform.position = position;

        GameObject colliderVisualizer = Instantiate(_cubeVisualizerPrefab);
        colliderVisualizer.tag = "SelectionVisualizer";
        colliderVisualizer.transform.SetParent(template.transform);
        colliderVisualizer.transform.localPosition = boxCollider.center;
        colliderVisualizer.transform.localScale = boxCollider.size;
        if (!template.TryGetComponent<PointableUnityEventWrapper>(out var evWrap))
        {
            Debug.LogError("no event wrapper on new model");
            return (null, 0);
        }
        if (!colliderVisualizer.TryGetComponent<ColliderVisualizer>(out var cvcmp))
        {
            Debug.LogError("no collider visualizer component in new model");
            return (null, 0);
        }
        evWrap.WhenHover.AddListener((_) => { cvcmp.OnHover(); });
        evWrap.WhenUnhover.AddListener((_) => { cvcmp.OnUnhover(); });
        evWrap.WhenSelect.AddListener((_) => { cvcmp.OnSelect(); });
        evWrap.WhenUnselect.AddListener((_) => { cvcmp.OnUnselect(); });
        return (template, FindBoundsLongestDimension(bounds));
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
                Texture oldTexture = oldMat.mainTexture;
                Color oldColor = oldMat.color;
                Debug.Log($"[MaterialDebug] material [{i}]: Old shader '{oldMat.shader.name}' | Has texture: {(oldTexture != null ? oldTexture.name : "NULL")} | Color: {oldColor}");
                UnityEngine.Material newMat = new(newMaterial);
                newMat.CopyPropertiesFromMaterial(oldMat);
                if (newMat.mainTexture == null && oldTexture != null)
                {
                    Debug.LogError($"[MaterialDebug] CopyPropertiesFromMaterial failed to transfer. should look into manual shader property setting...");
                }
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
