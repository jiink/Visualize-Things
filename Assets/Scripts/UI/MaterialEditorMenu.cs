using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MaterialEditorMenu : MonoBehaviour
{
    private GameObject _inspectedOb;
    public GameObject InspectedObject
    {
        get
        {
            return _inspectedOb;
        }
        set
        {
            _inspectedOb = value;
            if (_inspectedOb != null)
            {
                gameObjectNameLabel.text = _inspectedOb.name;
                AddMaterialEntries(_inspectedOb);
            }
        }
    }
    public TextMeshProUGUI gameObjectNameLabel;
    public GameObject materialListingPrefab;
    public Transform materialListingsParent;

    // Add all materials from the object to the menu
    void AddMaterialEntries(GameObject model)
    {
        // Clean existing entries
        foreach (Transform child in materialListingsParent)
        {
            Destroy(child.gameObject);
        }
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Count() > 0)
        {
            foreach (Renderer renderer in renderers)
            {
                Material[] materialsArray = renderer.materials;
                List<Material> materials = new(materialsArray);
                // Process the materials list as needed
                for (int i = 0; i < materialsArray.Length; i++)
                {
                    MaterialListing materialListing = Instantiate(materialListingPrefab, materialListingsParent).GetComponent<MaterialListing>();
                    materialListing.MaterialNum = i;
                    materialListing.Material = materialsArray[i];
                }
            }
        }
    }
}
