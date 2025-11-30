using System;
using UnityEngine;

public class ReflectionService : MonoBehaviour
{
    [SerializeField] private ReflectionProbe _reflectionProbe;
    [SerializeField] private Material _defaultSky;
    private void OnValidate()
    {
        if (_reflectionProbe != null)
        {
            _reflectionProbe = FindFirstObjectByType<ReflectionProbe>();
        }
    }

    private void Start()
    {
        ApplyDefaultEnvMap();
    }

    private void UpdateEnvironmentLighting()
    {
        if (_reflectionProbe != null)
        {
            Debug.Log($"Found reflection probe on {_reflectionProbe.gameObject.name}.");
            // Exclude all objects from being baked into the reflection probe
            _reflectionProbe.cullingMask = 0;
            _reflectionProbe.RenderProbe();
        }
        else
        {
            Debug.LogError("Reflection probe not found.");
        }
        DynamicGI.UpdateEnvironment();
        Debug.Log("Updated environment lighting.");
    }

    private void ApplyDefaultEnvMap()
    {
        Debug.Log("Applying default skybox...");
        RenderSettings.skybox = _defaultSky;
        UpdateEnvironmentLighting();
        Debug.Log("Applied default environment map.");
    }

    public void ApplyEnvironmentMap(string fullFilePath)
    {
        if (string.IsNullOrEmpty(fullFilePath) || !System.IO.File.Exists(fullFilePath))
        {
            Debug.LogError("Invalid file path.");
            return;
        }
        try
        {
            // Load the texture
            byte[] fileData = System.IO.File.ReadAllBytes(fullFilePath);
            Texture2D _texture = new(2, 2, TextureFormat.RGBAHalf, false);
            //if (fullFilePath.EndsWith(".hdr") || fullFilePath.EndsWith(".exr")) // exr not working tho :(
            //{
            //    try
            //    {
            //        // Use RadianceHDRTexture to load HDR texture
            //        RadianceHDRTexture hdrTexture = new RadianceHDRTexture(fileData);
            //        _texture = hdrTexture.texture;
            //    }
            //    catch (Exception ex)
            //    {
            //        Debug.LogError($"Failed to load HDR texture from file: {ex.Message}");
            //        return;
            //    }
            //}
            //else
            //{
            //    if (!_texture.LoadImage(fileData))
            //    {
            //        Debug.LogError("Failed to load texture from file.");
            //        return;
            //    }
            //}
            if (!_texture.LoadImage(fileData))
            {
                Debug.LogError("Failed to load texture from file.");
                return;
            }
            _texture.Apply();
            ApplyEnvironmentMap(_texture);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading texture: {ex.Message}");
        }
    }

    public void ApplyEnvironmentMap(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogError("null texture.");
            return;
        }
        try
        {
            // make a new "Skybox/Panoramic" material and set its texture to the loaded texture
            Material newSkyMat = new(Shader.Find("Skybox/Panoramic"));
            newSkyMat.SetTexture("_MainTex", texture);
            // I should probably do this on the CaptureSphere and not here but uhhh....
            newSkyMat.SetTextureScale("_MainTex", new Vector2(-1, 1));
            newSkyMat.SetFloat("_Rotation", 90f);
            // ---
            RenderSettings.skybox = newSkyMat;
            UpdateEnvironmentLighting();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error applying environment map: {ex.Message}");
        }
    }
}
