#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;

/// Automatic item icon generator from 3D FBX models.
/// Creates high-quality 1024x1024 sprites with transparent background for inventory systems.
/// Supports automatic scaling for long items to fit properly in square inventory slots.
public class ItemIconGenerator : EditorWindow
{
    [Header("Model and Output")]
    [SerializeField] private GameObject modelPrefab;
    [SerializeField] private string outputPath = "Assets/Icons/";
    [SerializeField] private string iconName = "NewIcon";
    
    [Header("Camera Settings")]
    [SerializeField] private Vector3 cameraPosition = new Vector3(-2, 2, -2);
    [SerializeField] private Vector3 cameraRotation = new Vector3(20, 45, 0);
    [SerializeField] private float orthographicSize = 1.5f;
    [SerializeField] private bool autoFitToModel = true;
    [SerializeField] private float paddingPercent = 15f;
    [SerializeField] private float minCameraDistance = 1f;
    
    [Header("Quality Settings")]
    [SerializeField] private int textureSize = 1024;
    [SerializeField] private bool transparentBackground = true;
    
    // Collapsible sections
    private bool showCameraSettings = true;
    private bool showQualitySettings = true;
    private bool showPresets = true;
    
    // Preview components
    private RenderTexture previewTexture;
    private Camera renderCamera;
    private GameObject previewInstance;
    private Light primaryLight;
    private Light secondaryLight;
    
    [MenuItem("Tools/Item Icon Generator")]
    public static void ShowWindow()
    {
        ItemIconGenerator window = GetWindow<ItemIconGenerator>("Item Icon Generator");
        window.minSize = new Vector2(400, 600);
    }
    
    private void OnEnable()
    {
        SetupPreview();
    }
    
    private void OnDisable()
    {
        CleanupPreview();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(5);
        
        // === MODEL AND OUTPUT SECTION (Always visible) ===
        EditorGUILayout.BeginVertical("box");
        {
            GUILayout.Label("Model and Output", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);
            
            modelPrefab = (GameObject)EditorGUILayout.ObjectField("3D Model (FBX)", modelPrefab, typeof(GameObject), false);
            
            EditorGUILayout.BeginHorizontal();
            outputPath = EditorGUILayout.TextField("Output Folder", outputPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    outputPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            iconName = EditorGUILayout.TextField("Icon Name", iconName);
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        // === CAMERA SETTINGS SECTION (Collapsible) ===
        EditorGUILayout.BeginVertical("box");
        {
            EditorGUILayout.BeginHorizontal();
            showCameraSettings = EditorGUILayout.Foldout(showCameraSettings, "Camera Settings", true, EditorStyles.foldoutHeader);
            EditorGUILayout.EndHorizontal();
            
            if (showCameraSettings)
            {
                EditorGUILayout.Space(5);
                
                // Rotation sliders
                EditorGUILayout.LabelField("Rotation", EditorStyles.miniBoldLabel);
                EditorGUI.BeginChangeCheck();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("X (Pitch)", GUILayout.Width(60));
                float newRotX = EditorGUILayout.Slider(cameraRotation.x, -180f, 180f);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Y (Yaw)", GUILayout.Width(60));
                float newRotY = EditorGUILayout.Slider(cameraRotation.y, -180f, 180f);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Z (Roll)", GUILayout.Width(60));
                float newRotZ = EditorGUILayout.Slider(cameraRotation.z, -180f, 180f);
                EditorGUILayout.EndHorizontal();
                
                if (EditorGUI.EndChangeCheck())
                {
                    cameraRotation = new Vector3(newRotX, newRotY, newRotZ);
                    if (modelPrefab != null) UpdatePreview();
                }
                
                EditorGUILayout.Space(5);
                
                // Auto fit settings
                EditorGUI.BeginChangeCheck();
                autoFitToModel = EditorGUILayout.Toggle("Auto Fit to Model", autoFitToModel);
                if (autoFitToModel)
                {
                    paddingPercent = EditorGUILayout.Slider("Padding (%)", paddingPercent, 0f, 50f);
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                    if (modelPrefab != null) UpdatePreview();
                }
            }
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        // === QUALITY SETTINGS SECTION (Collapsible) ===
        EditorGUILayout.BeginVertical("box");
        {
            EditorGUILayout.BeginHorizontal();
            showQualitySettings = EditorGUILayout.Foldout(showQualitySettings, "Quality Settings", true, EditorStyles.foldoutHeader);
            EditorGUILayout.EndHorizontal();
            
            if (showQualitySettings)
            {
                EditorGUILayout.Space(5);
                textureSize = EditorGUILayout.IntSlider("Texture Size", textureSize, 512, 2048);
                transparentBackground = EditorGUILayout.Toggle("Transparent Background", transparentBackground);
            }
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        // === QUICK PRESETS SECTION (Collapsible) ===
        EditorGUILayout.BeginVertical("box");
        {
            EditorGUILayout.BeginHorizontal();
            showPresets = EditorGUILayout.Foldout(showPresets, "Quick Presets", true, EditorStyles.foldoutHeader);
            EditorGUILayout.EndHorizontal();
            
            if (showPresets)
            {
                EditorGUILayout.Space(5);
                
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Weapon"))
                {
                    cameraPosition = new Vector3(-1.5f, 0.5f, -2.5f);
                    cameraRotation = new Vector3(10, 25, 0);
                    orthographicSize = 1.5f;
                    if (modelPrefab != null) UpdatePreview();
                }
                
                if (GUILayout.Button("Armor"))
                {
                    cameraPosition = new Vector3(0, 0, -2.5f);
                    cameraRotation = new Vector3(0, 0, 0);
                    orthographicSize = 1.8f;
                    if (modelPrefab != null) UpdatePreview();
                }
                
                if (GUILayout.Button("Shield"))
                {
                    cameraPosition = new Vector3(-0.8f, 0.3f, -2f);
                    cameraRotation = new Vector3(5, 20, 0);
                    orthographicSize = 1.6f;
                    if (modelPrefab != null) UpdatePreview();
                }
                
                if (GUILayout.Button("Isometric"))
                {
                    cameraPosition = new Vector3(-2, 2, -2);
                    cameraRotation = new Vector3(20, 45, 0);
                    orthographicSize = 1.5f;
                    if (modelPrefab != null) UpdatePreview();
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        // === PREVIEW SECTION ===
        EditorGUILayout.BeginVertical("box");
        {
            if (modelPrefab != null)
            {
                UpdatePreview();
                
                if (previewTexture != null)
                {
                    // Center the preview
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    Rect previewRect = GUILayoutUtility.GetRect(256, 256, GUILayout.ExpandWidth(false));
                    EditorGUI.DrawPreviewTexture(previewRect, previewTexture);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Select a 3D model to see preview", MessageType.Info);
            }
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // === GENERATE BUTTON ===
        GUI.enabled = modelPrefab != null;
        if (GUILayout.Button("Generate Icon", GUILayout.Height(40)))
        {
            GenerateIcon();
        }
        GUI.enabled = true;
        
        EditorGUILayout.Space(5);
    }
    
    private void SetupPreview()
    {
        // Preview camera - renders only specified layers
        GameObject cameraObj = new GameObject("IconPreviewCamera");
        cameraObj.hideFlags = HideFlags.HideAndDontSave;
        renderCamera = cameraObj.AddComponent<Camera>();
        
        // Configure camera to render only specific layer
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = transparentBackground ? new Color(0, 0, 0, 0) : Color.black;
        renderCamera.orthographic = true;
        renderCamera.nearClipPlane = 0.1f;
        renderCamera.farClipPlane = 10f;
        renderCamera.cullingMask = 1 << 31; // Render only layer 31 (isolated)
        
        // Lighting setup
        GameObject lightObj = new GameObject("IconPrimaryLight");
        lightObj.hideFlags = HideFlags.HideAndDontSave;
        lightObj.layer = 31; // Place light on isolated layer
        primaryLight = lightObj.AddComponent<Light>();
        primaryLight.type = LightType.Directional;
        primaryLight.color = Color.white;
        primaryLight.intensity = 1.2f;
        primaryLight.transform.rotation = Quaternion.Euler(-30, 30, -30);
        primaryLight.cullingMask = 1 << 31; // Illuminate only layer 31
        
        GameObject secondaryLightObj = new GameObject("IconSecondaryLight");
        secondaryLightObj.hideFlags = HideFlags.HideAndDontSave;
        secondaryLightObj.layer = 31;
        secondaryLight = secondaryLightObj.AddComponent<Light>();
        secondaryLight.type = LightType.Directional;
        secondaryLight.color = Color.white;
        secondaryLight.intensity = 0.6f;
        secondaryLight.transform.rotation = Quaternion.Euler(30, -20, 30);
        secondaryLight.cullingMask = 1 << 31;
        
        // RenderTexture for preview
        previewTexture = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32);
        previewTexture.antiAliasing = 4;
        renderCamera.targetTexture = previewTexture;
    }
    
    private void CleanupPreview()
    {
        if (renderCamera != null) DestroyImmediate(renderCamera.gameObject);
        if (primaryLight != null) DestroyImmediate(primaryLight.gameObject);
        if (secondaryLight != null) DestroyImmediate(secondaryLight.gameObject);
        if (previewInstance != null) DestroyImmediate(previewInstance);
        if (previewTexture != null)
        {
            previewTexture.Release();
            DestroyImmediate(previewTexture);
        }
    }
    
    private void UpdatePreview()
    {
        if (renderCamera == null || modelPrefab == null) return;
        
        // Remove old instance
        if (previewInstance != null) DestroyImmediate(previewInstance);
        
        // Create new model instance
        previewInstance = Instantiate(modelPrefab);
        previewInstance.hideFlags = HideFlags.HideAndDontSave;
        
        // Place model on isolated layer
        SetLayerRecursively(previewInstance, 31);
        
        // Position camera with automatic distance adjustment
        PositionCameraWithDistance();
        
        // Update camera settings
        renderCamera.orthographicSize = orthographicSize;
        renderCamera.backgroundColor = transparentBackground ? new Color(0, 0, 0, 0) : Color.black;
        
        // Auto-fit to model if enabled
        if (autoFitToModel)
        {
            AutoFitCamera();
        }
        
        // Render
        renderCamera.Render();
    }
    
    private void PositionCameraWithDistance()
    {
        if (previewInstance == null) return;
        
        // Get model bounds for distance calculation
        Renderer[] renderers = previewInstance.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;
        
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        
        // Calculate safe distance based on model size
        float modelSize = bounds.size.magnitude;
        float safeDistance = Mathf.Max(minCameraDistance, modelSize * 1.5f);
        
        // Apply rotation
        renderCamera.transform.rotation = Quaternion.Euler(cameraRotation);
        
        // Position camera at safe distance from model center, using default isometric position
        Vector3 modelCenter = bounds.center;
        Vector3 defaultDirection = new Vector3(1, -1, 1).normalized; // Default isometric direction
        Vector3 cameraDirection = renderCamera.transform.forward.magnitude > 0.1f ? renderCamera.transform.forward : defaultDirection;
        renderCamera.transform.position = modelCenter - cameraDirection * safeDistance;
    }
    
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
    
    private void AutoFitCamera()
    {
        if (previewInstance == null) return;
        
        // Get model bounds
        Renderer[] renderers = previewInstance.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;
        
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        
        if (bounds.size.magnitude == 0) return;
        
        // Calculate maximum size considering camera angle
        Vector3 cameraForward = renderCamera.transform.forward;
        Vector3 cameraRight = renderCamera.transform.right;
        Vector3 cameraUp = renderCamera.transform.up;
        
        // Project bounds onto camera plane
        float projectedWidth = Mathf.Abs(Vector3.Dot(bounds.size, cameraRight));
        float projectedHeight = Mathf.Abs(Vector3.Dot(bounds.size, cameraUp));
        
        float maxProjectedSize = Mathf.Max(projectedWidth, projectedHeight);
        
        // Add padding
        float padding = maxProjectedSize * (paddingPercent / 100f);
        orthographicSize = (maxProjectedSize + padding) * 0.5f;
        renderCamera.orthographicSize = orthographicSize;
        
        // Center model in frame
        Vector3 center = bounds.center;
        Vector3 offset = renderCamera.transform.position - center;
        float distance = Vector3.Dot(offset, renderCamera.transform.forward);
        
        renderCamera.transform.position = center + renderCamera.transform.forward * distance;
    }
    
    private void GenerateIcon()
    {
        if (modelPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Select a 3D model to generate icon", "OK");
            return;
        }
        
        // Create output folder if it doesn't exist
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        
        // Temporary objects for high-quality rendering
        GameObject tempCameraObj = new GameObject("TempIconCamera");
        Camera tempCamera = tempCameraObj.AddComponent<Camera>();
        
        GameObject tempLightObj = new GameObject("TempIconLight");
        Light tempPrimaryLight = tempLightObj.AddComponent<Light>();
        
        GameObject tempSecondaryLightObj = new GameObject("TempIconSecondaryLight");
        Light tempSecondaryLight = tempSecondaryLightObj.AddComponent<Light>();
        
        GameObject tempModel = Instantiate(modelPrefab);
        
        try
        {
            // Place everything on isolated layer
            tempCameraObj.layer = 31;
            tempLightObj.layer = 31;
            tempSecondaryLightObj.layer = 31;
            SetLayerRecursively(tempModel, 31);
            
            // Camera setup - render only layer 31
            tempCamera.clearFlags = CameraClearFlags.SolidColor;
            tempCamera.backgroundColor = transparentBackground ? new Color(0, 0, 0, 0) : Color.black;
            tempCamera.orthographic = true;
            tempCamera.orthographicSize = orthographicSize;
            tempCamera.nearClipPlane = 0.1f;
            tempCamera.farClipPlane = 10f;
            tempCamera.cullingMask = 1 << 31; // Render only layer 31
            
            // Position camera with safe distance
            PositionTempCameraWithDistance(tempModel, tempCamera);
            
            // Lighting setup
            tempPrimaryLight.type = LightType.Directional;
            tempPrimaryLight.color = Color.white;
            tempPrimaryLight.intensity = 1.2f;
            tempPrimaryLight.transform.rotation = Quaternion.Euler(-30, 30, -30);
            tempPrimaryLight.cullingMask = 1 << 31; // Illuminate only layer 31
            
            tempSecondaryLight.type = LightType.Directional;
            tempSecondaryLight.color = Color.white;
            tempSecondaryLight.intensity = 0.6f;
            tempSecondaryLight.transform.rotation = Quaternion.Euler(30, -20, 30);
            tempSecondaryLight.cullingMask = 1 << 31;
            
            // Auto-fit if enabled
            if (autoFitToModel)
            {
                AutoFitCameraForModel(tempModel, tempCamera);
            }
            
            // Create high-quality RenderTexture
            RenderTexture renderTexture = new RenderTexture(textureSize, textureSize, 24, 
                transparentBackground ? RenderTextureFormat.ARGB32 : RenderTextureFormat.Default);
            renderTexture.antiAliasing = 8; // Maximum anti-aliasing
            
            tempCamera.targetTexture = renderTexture;
            tempCamera.Render();
            
            // Save as PNG
            RenderTexture.active = renderTexture;
            Texture2D texture2D = new Texture2D(textureSize, textureSize, 
                transparentBackground ? TextureFormat.ARGB32 : TextureFormat.RGB24, false);
            texture2D.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
            texture2D.Apply();
            
            byte[] bytes = texture2D.EncodeToPNG();
            string filePath = GetUniqueFilePath(outputPath, iconName);
            File.WriteAllBytes(filePath, bytes);
            
            // Free memory - clear camera target first to avoid error
            tempCamera.targetTexture = null;
            RenderTexture.active = null;
            renderTexture.Release();
            DestroyImmediate(texture2D);
            DestroyImmediate(renderTexture);
            
            // Update AssetDatabase
            AssetDatabase.Refresh();
            
            // Configure import as Sprite
            TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = transparentBackground;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = textureSize;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                
                AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
            }
            
            // Select created icon
            Sprite createdSprite = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
            if (createdSprite != null)
            {
                EditorGUIUtility.PingObject(createdSprite);
                Selection.activeObject = createdSprite;
            }
            
            EditorUtility.DisplayDialog("Complete!", 
                $"Icon created: {filePath}\n\nSize: {textureSize}x{textureSize}\n" +
                $"Transparent background: {(transparentBackground ? "Yes" : "No")}", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to create icon: {e.Message}", "OK");
        }
        finally
        {
            // Cleanup
            DestroyImmediate(tempModel);
            DestroyImmediate(tempCameraObj);
            DestroyImmediate(tempLightObj);
            DestroyImmediate(tempSecondaryLightObj);
        }
    }
    
    private void PositionTempCameraWithDistance(GameObject model, Camera camera)
    {
        // Get model bounds for distance calculation
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;
        
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        
        // Calculate safe distance based on model size
        float modelSize = bounds.size.magnitude;
        float safeDistance = Mathf.Max(minCameraDistance, modelSize * 1.5f);
        
        // Apply rotation
        camera.transform.rotation = Quaternion.Euler(cameraRotation);
        
        // Position camera at safe distance from model center
        Vector3 modelCenter = bounds.center;
        Vector3 defaultDirection = new Vector3(1, -1, 1).normalized; // Default isometric direction
        Vector3 cameraDirection = camera.transform.forward.magnitude > 0.1f ? camera.transform.forward : defaultDirection;
        camera.transform.position = modelCenter - cameraDirection * safeDistance;
    }
    
    private void AutoFitCameraForModel(GameObject model, Camera camera)
    {
        // Ensure model is on correct layer
        SetLayerRecursively(model, 31);
        
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;
        
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        
        if (bounds.size.magnitude == 0) return;
        
        Vector3 cameraForward = camera.transform.forward;
        Vector3 cameraRight = camera.transform.right;
        Vector3 cameraUp = camera.transform.up;
        
        float projectedWidth = Mathf.Abs(Vector3.Dot(bounds.size, cameraRight));
        float projectedHeight = Mathf.Abs(Vector3.Dot(bounds.size, cameraUp));
        
        float maxProjectedSize = Mathf.Max(projectedWidth, projectedHeight);
        float padding = maxProjectedSize * (paddingPercent / 100f);
        
        camera.orthographicSize = (maxProjectedSize + padding) * 0.5f;
        
        Vector3 center = bounds.center;
        Vector3 offset = camera.transform.position - center;
        float distance = Vector3.Dot(offset, camera.transform.forward);
        
        camera.transform.position = center + camera.transform.forward * distance;
    }
    
    private string GetUniqueFilePath(string outputPath, string baseName)
    {
        string originalPath = Path.Combine(outputPath, baseName + ".png");
        
        // If file doesn't exist, use original name
        if (!File.Exists(originalPath))
        {
            return originalPath;
        }
        
        // Find unique name by adding numbers
        int counter = 2;
        string uniquePath;
        
        do
        {
            string uniqueName = baseName + counter.ToString();
            uniquePath = Path.Combine(outputPath, uniqueName + ".png");
            counter++;
        }
        while (File.Exists(uniquePath));
        
        return uniquePath;
    }
}
#endif