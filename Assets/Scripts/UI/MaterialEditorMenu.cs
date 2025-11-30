using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MaterialEditorMenu : MonoBehaviour
{
    private Dictionary<Material, Color> _originalColors = new();
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
                _gameObjectNameLabel.text = _inspectedOb.name;
                AddMaterialEntries(_inspectedOb);
            }
        }
    }
    public TextMeshProUGUI _gameObjectNameLabel;
    public GameObject _materialButtonPrefab;
    public Transform _materialButtonsParent;

    // Add all materials from the object to the menu
    void AddMaterialEntries(GameObject model)
    {
        // Clean existing entries
        foreach (Transform child in _materialButtonsParent)
        {
            Destroy(child.gameObject);
        }
        _originalColors.Clear();
        int materialCounter = 0;
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Count() > 0)
        {
            foreach (Renderer renderer in renderers)
            {
                Material[] materialsArray = renderer.materials; // do .sharedMaterials instead if you dont want to make copies...
                List<Material> materials = new(materialsArray);
                // Process the materials list as needed
                for (int i = 0; i < materialsArray.Length; i++)
                {
                    Material currentMat = materialsArray[i];
                    if (!_originalColors.ContainsKey(currentMat))
                    {
                        if (currentMat.HasProperty("_Color"))
                        {
                            _originalColors.Add(currentMat, currentMat.color);
                        }
                        else
                        {
                            Debug.LogWarning("this doesnt have a color property...");
                        }
                    }
                    MaterialButton matBtn = Instantiate(_materialButtonPrefab, _materialButtonsParent).GetComponent<MaterialButton>();
                    matBtn.Setup(materialCounter, currentMat);
                    materialCounter++;
                    matBtn.HoverEvent += (o, _) => {
                        MaterialButton mb = o as MaterialButton;
                        if (mb != null)
                        {
                            OnMaterialBtnHover(mb.Material);
                        }
                        else
                        {
                            Debug.LogError("Couldn't convert sender into MaterialButton");
                        }
                    };
                    matBtn.UnhoverEvent += (o, _) => {
                        MaterialButton mb = o as MaterialButton;
                        if (mb != null)
                        {
                            OnMaterialBtnUnhover(mb.Material);
                        }
                        else
                        {
                            Debug.LogError("Couldn't convert sender into MaterialButton");
                        }
                    };
                    matBtn.PressEvent += (_, _) => { Debug.Log(">>>> matBtn.PressEvent happened"); };
                }
            }
        }
    }

    void OnMaterialBtnHover(Material mat)
    {
        if (mat == null)
        {
            Debug.LogError("Null material");
            return;
        }
        if (!_originalColors.ContainsKey(mat) && mat.HasProperty("_Color"))
        {
            _originalColors[mat] = mat.color;
        }
        mat.color = Color.magenta;
    }

    void OnMaterialBtnUnhover(Material mat)
    {
        if (mat == null)
        {
            Debug.LogError("Null material");
            return;
        }
        if (_originalColors.TryGetValue(mat, out Color originalColor))
        {
            mat.color = originalColor;
        }
        else
        {
            Debug.LogWarning("didnt find original color for material: " + mat.name);
        }

    }
}
