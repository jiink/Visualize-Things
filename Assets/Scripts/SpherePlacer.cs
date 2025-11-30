using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpherePlacer : MonoBehaviour
{
    [Header("Settings")]
    public GameObject prefabToSpawn;
    [Range(1, 1000)] public int numberOfObjects = 50;
    public float radius = 5f;

    // A list to keep track of spawned objects so we can clear them easily
    [HideInInspector]
    public List<GameObject> spawnedObjects = new List<GameObject>();

    [ContextMenu("Generate Sphere")]
    public void Generate()
    {
        // 1. Cleanup old objects before spawning new ones
        Clear();

        if (prefabToSpawn == null)
        {
            Debug.LogError("Please assign a Prefab to spawn.");
            return;
        }

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;

        for (int i = 0; i < numberOfObjects; i++)
        {
            float t = (float)i / numberOfObjects;
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);

            // 1. Calculate Local Position (relative to center 0,0,0)
            Vector3 localPos = new Vector3(x, y, z) * radius;

            // 2. Convert to World Space (Applies parent's Position, Rotation, and Scale)
            Vector3 worldPos = transform.TransformPoint(localPos);

            GameObject obj = null;

#if UNITY_EDITOR
            obj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
            obj.transform.position = worldPos;
            obj.transform.rotation = Quaternion.identity;
            Undo.RegisterCreatedObjectUndo(obj, "Spawn Sphere Object");
#else
    obj = Instantiate(prefabToSpawn, worldPos, Quaternion.identity);
#endif

            obj.transform.parent = transform;

            // 3. Rotate feet towards the center
            // We use (worldPos - transform.position) instead of localPos to handle 
            // cases where the parent is non-uniformly scaled (ellipsoid)
            obj.transform.up = (worldPos - transform.position).normalized;

            spawnedObjects.Add(obj);

        }
    }

    [ContextMenu("Clear Objects")]
    public void Clear()
    {
        // Iterate backwards to safely remove items
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
            {
#if UNITY_EDITOR
                // Use DestroyImmediate in the editor, but wrap it in Undo
                Undo.DestroyObjectImmediate(spawnedObjects[i]);
#else
                Destroy(spawnedObjects[i]);
#endif
            }
        }
        spawnedObjects.Clear();
    }
}