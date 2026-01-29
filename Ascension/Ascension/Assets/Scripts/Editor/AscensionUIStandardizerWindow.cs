using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AscensionUIStandardizerWindow : EditorWindow
{
    private const string DefaultFontTtfPath = "Assets/Font/VCR_OSD_MONO_1.001.ttf";
    private const string DefaultBackgroundSpritePath = "Assets/Sprites/UI/FondoPantallaAscension.png";
    private const string DefaultFontAssetOutPath = "Assets/Font/VCR_OSD_MONO.asset";

    private Font sourceTtf;
    private TMP_FontAsset tmpFontAsset;
    private Sprite backgroundSprite;

    [MenuItem("Tools/Ascension/UI Standardizer")]
    public static void ShowWindow()
    {
        var w = GetWindow<AscensionUIStandardizerWindow>();
        w.titleContent = new GUIContent("Ascension UI");
        w.minSize = new Vector2(420, 360);
        w.Show();
    }

    private void OnEnable()
    {
        if (sourceTtf == null)
            sourceTtf = AssetDatabase.LoadAssetAtPath<Font>(DefaultFontTtfPath);

        if (backgroundSprite == null)
            backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(DefaultBackgroundSpritePath);

        if (tmpFontAsset == null)
            tmpFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DefaultFontAssetOutPath);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Assets", EditorStyles.boldLabel);
        sourceTtf = (Font)EditorGUILayout.ObjectField("TTF (Font)", sourceTtf, typeof(Font), false);
        tmpFontAsset = (TMP_FontAsset)EditorGUILayout.ObjectField("TMP Font Asset", tmpFontAsset, typeof(TMP_FontAsset), false);
        backgroundSprite = (Sprite)EditorGUILayout.ObjectField("MainMenu Background", backgroundSprite, typeof(Sprite), false);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("TMP Defaults", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Create TMP Font Asset"))
            {
                CreateTmpFontAsset();
            }

            if (GUILayout.Button("Set TMP Default Font"))
            {
                SetTmpDefaultFont();
            }
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Apply", EditorStyles.boldLabel);

        if (GUILayout.Button("Apply TMP Font To Active Scene"))
        {
            ApplyTmpFontToActiveScene();
        }

        if (GUILayout.Button("Apply TMP Font To All Prefabs"))
        {
            ApplyTmpFontToAllPrefabs();
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("MainMenu", EditorStyles.boldLabel);

        if (GUILayout.Button("Setup MainMenu Background + Title (does not auto-save)"))
        {
            SetupMainMenuBackgroundAndTitle(openSceneIfNeeded: true, saveScene: false);
        }

        if (GUILayout.Button("Setup MainMenu Background + Title AND Save Scene"))
        {
            SetupMainMenuBackgroundAndTitle(openSceneIfNeeded: true, saveScene: true);
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("ClassSelection", EditorStyles.boldLabel);

        if (GUILayout.Button("Setup ClassSelection Background (does not auto-save)"))
        {
            SetupClassSelectionBackground(openSceneIfNeeded: true, saveScene: false);
        }

        if (GUILayout.Button("Setup ClassSelection Background AND Save Scene"))
        {
            SetupClassSelectionBackground(openSceneIfNeeded: true, saveScene: true);
        }

        if (GUILayout.Button("Fix Weapon Sprites In Scene (SpriteRenderer -> UI Image)"))
        {
            EditorApplication.ExecuteMenuItem("Tools/Ascension/UI/Fix Weapon Sprites (SpriteRenderer -> UI Image)");
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "Recomendado: 1) Create TMP Font Asset  2) Set TMP Default Font  3) Setup MainMenu...  4) Apply TMP Font...\n" +
            "Nota: El listado de Scores ya usa TMP; con default font se estandariza automáticamente.",
            MessageType.Info);
    }

    private void CreateTmpFontAsset()
    {
        if (sourceTtf == null)
        {
            EditorUtility.DisplayDialog("Create TMP Font Asset", "No hay TTF asignada.", "OK");
            return;
        }

        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DefaultFontAssetOutPath);
        if (existing != null)
        {
            tmpFontAsset = existing;
            // Reparar si quedó a medias (sin atlas/material)
            RepairTmpFontAsset(tmpFontAsset);
            EditorUtility.DisplayDialog("Create TMP Font Asset", "Ya existe el TMP Font Asset en Assets/Font. Se reutiliza (y se repara si hacía falta).", "OK");
            return;
        }

        // Crear un TMP_FontAsset dinámico desde la TTF
        TMP_FontAsset created = null;
        try
        {
            created = TMP_FontAsset.CreateFontAsset(
                sourceTtf,
                90,
                9,
                UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                1024,
                1024,
                AtlasPopulationMode.Dynamic,
                true
            );
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AscensionUIStandardizer] No se pudo crear TMP_FontAsset: {ex}");
        }

        if (created == null)
        {
            EditorUtility.DisplayDialog("Create TMP Font Asset", "No se pudo crear el TMP Font Asset. Revisa consola.", "OK");
            return;
        }

        created.name = Path.GetFileNameWithoutExtension(DefaultFontAssetOutPath);
        AssetDatabase.CreateAsset(created, DefaultFontAssetOutPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // En algunos setups TMP no guarda atlas/material al crear, así que lo forzamos.
        RepairTmpFontAsset(created);

        tmpFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DefaultFontAssetOutPath);
        EditorUtility.DisplayDialog("Create TMP Font Asset", "TMP Font Asset creado en Assets/Font/VCR_OSD_MONO.asset", "OK");
    }

    private void SetTmpDefaultFont()
    {
        if (tmpFontAsset == null)
        {
            EditorUtility.DisplayDialog("Set TMP Default Font", "No hay TMP Font Asset asignado.", "OK");
            return;
        }

        RepairTmpFontAsset(tmpFontAsset);

        TMP_Settings.defaultFontAsset = tmpFontAsset;
        EditorUtility.SetDirty(TMP_Settings.instance);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Set TMP Default Font", "TMP default font actualizado.", "OK");
    }

    private void ApplyTmpFontToActiveScene()
    {
        if (tmpFontAsset == null)
        {
            EditorUtility.DisplayDialog("Apply TMP Font", "No hay TMP Font Asset asignado.", "OK");
            return;
        }

        int changed = 0;
        var texts = GameObject.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        foreach (var t in texts)
        {
            if (t == null) continue;
            if (t.font == tmpFontAsset) continue;
            Undo.RecordObject(t, "Apply TMP Font");
            t.font = tmpFontAsset;
            EditorUtility.SetDirty(t);
            changed++;
        }

        EditorUtility.DisplayDialog("Apply TMP Font", $"Actualizado {changed} textos TMP en la escena activa.", "OK");
    }

    private void ApplyTmpFontToAllPrefabs()
    {
        if (tmpFontAsset == null)
        {
            EditorUtility.DisplayDialog("Apply TMP Font", "No hay TMP Font Asset asignado.", "OK");
            return;
        }

        var guids = AssetDatabase.FindAssets("t:Prefab");
        int changedPrefabs = 0;
        int changedTexts = 0;

        try
        {
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                bool prefabDirty = false;
                var texts = prefab.GetComponentsInChildren<TMP_Text>(true);
                foreach (var t in texts)
                {
                    if (t == null) continue;
                    if (t.font == tmpFontAsset) continue;
                    t.font = tmpFontAsset;
                    EditorUtility.SetDirty(t);
                    prefabDirty = true;
                    changedTexts++;
                }

                if (prefabDirty)
                {
                    PrefabUtility.SavePrefabAsset(prefab);
                    changedPrefabs++;
                }
            }
        }
        finally
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        EditorUtility.DisplayDialog("Apply TMP Font", $"Prefabs actualizados: {changedPrefabs}\nTextos cambiados: {changedTexts}", "OK");
    }

    private void SetupMainMenuBackgroundAndTitle(bool openSceneIfNeeded, bool saveScene)
    {
        if (backgroundSprite == null)
        {
            EditorUtility.DisplayDialog("Setup MainMenu", "No hay Sprite de fondo asignado.", "OK");
            return;
        }

        var active = SceneManager.GetActiveScene();
        if (!active.IsValid() || (!string.Equals(active.name, "MainMenu", StringComparison.OrdinalIgnoreCase) && openSceneIfNeeded))
        {
            var scenePath = "Assets/Scenes/MainMenu.unity";
            if (File.Exists(Path.Combine(Application.dataPath, "Scenes/MainMenu.unity")))
            {
                active = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
        }

        var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Setup MainMenu", "No se encontró Canvas en la escena.", "OK");
            return;
        }

        // Background
        var existingBg = GameObject.Find("MainMenuBackground");
        GameObject bgGo = existingBg;
        if (bgGo == null)
        {
            bgGo = new GameObject("MainMenuBackground");
            Undo.RegisterCreatedObjectUndo(bgGo, "Create MainMenu Background");
            bgGo.transform.SetParent(canvas.transform, false);
            bgGo.transform.SetAsFirstSibling();
            bgGo.AddComponent<CanvasRenderer>();
            bgGo.AddComponent<Image>();
        }

        var bgImg = bgGo.GetComponent<Image>();
        bgImg.sprite = backgroundSprite;
        bgImg.preserveAspect = true;
        bgImg.color = Color.white;
        bgImg.raycastTarget = false;

        var bgRt = bgGo.GetComponent<RectTransform>();
        if (bgRt == null) bgRt = bgGo.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // Title
        var titleGo = GameObject.Find("Title_ASCENSION");
        if (titleGo == null)
        {
            titleGo = new GameObject("Title_ASCENSION");
            Undo.RegisterCreatedObjectUndo(titleGo, "Create MainMenu Title");
            titleGo.transform.SetParent(canvas.transform, false);
            titleGo.transform.SetAsLastSibling();
        }

        var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
        if (titleTmp == null) titleTmp = titleGo.AddComponent<TextMeshProUGUI>();

        titleTmp.text = "ASCENSION";
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = Color.white;
        titleTmp.fontSize = 72;
        titleTmp.textWrappingMode = TextWrappingModes.NoWrap;
        titleTmp.raycastTarget = false;

        if (tmpFontAsset != null)
        {
            titleTmp.font = tmpFontAsset;
        }

        var titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -40f);
        titleRt.sizeDelta = new Vector2(1200f, 120f);

        EditorSceneManager.MarkSceneDirty(active);
        if (saveScene)
        {
            EditorSceneManager.SaveScene(active);
        }

        EditorUtility.DisplayDialog("Setup MainMenu", saveScene ? "MainMenu actualizado y guardado." : "MainMenu actualizado (sin guardar).", "OK");
    }

    private void SetupClassSelectionBackground(bool openSceneIfNeeded, bool saveScene)
    {
        if (backgroundSprite == null)
        {
            EditorUtility.DisplayDialog("Setup ClassSelection", "No hay Sprite de fondo asignado.", "OK");
            return;
        }

        var active = SceneManager.GetActiveScene();
        if (!active.IsValid() || (!string.Equals(active.name, "ClassSelection", StringComparison.OrdinalIgnoreCase) && openSceneIfNeeded))
        {
            var scenePath = "Assets/Scenes/ClassSelection.unity";
            if (File.Exists(Path.Combine(Application.dataPath, "Scenes/ClassSelection.unity")))
            {
                active = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
        }

        var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Setup ClassSelection", "No se encontró Canvas en la escena.", "OK");
            return;
        }

        var existingBg = GameObject.Find("ClassSelectionBackground");
        GameObject bgGo = existingBg;
        if (bgGo == null)
        {
            bgGo = new GameObject("ClassSelectionBackground");
            Undo.RegisterCreatedObjectUndo(bgGo, "Create ClassSelection Background");
            bgGo.transform.SetParent(canvas.transform, false);
            bgGo.transform.SetAsFirstSibling();
            bgGo.AddComponent<CanvasRenderer>();
            bgGo.AddComponent<Image>();
        }

        var bgImg = bgGo.GetComponent<Image>();
        bgImg.sprite = backgroundSprite;
        bgImg.preserveAspect = true;
        bgImg.color = Color.white;
        bgImg.raycastTarget = false;

        var bgRt = bgGo.GetComponent<RectTransform>();
        if (bgRt == null) bgRt = bgGo.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        EditorSceneManager.MarkSceneDirty(active);
        if (saveScene)
        {
            EditorSceneManager.SaveScene(active);
        }

        EditorUtility.DisplayDialog("Setup ClassSelection", saveScene ? "ClassSelection actualizado y guardado." : "ClassSelection actualizado (sin guardar).", "OK");
    }

    private static bool IsTmpFontAssetBroken(TMP_FontAsset fontAsset)
    {
        if (fontAsset == null) return false;
        var atlas = fontAsset.atlasTextures;
        if (atlas == null || atlas.Length == 0 || atlas[0] == null) return true;
        if (fontAsset.material == null) return true;
        return false;
    }

    private static void RepairTmpFontAsset(TMP_FontAsset fontAsset)
    {
        if (fontAsset == null) return;
        if (!IsTmpFontAssetBroken(fontAsset)) return;

        var path = AssetDatabase.GetAssetPath(fontAsset);
        if (string.IsNullOrEmpty(path)) return;

        int w = fontAsset.atlasWidth > 0 ? fontAsset.atlasWidth : 1024;
        int h = fontAsset.atlasHeight > 0 ? fontAsset.atlasHeight : 1024;

        var atlasTex = new Texture2D(w, h, TextureFormat.Alpha8, false, true)
        {
            name = fontAsset.name + " Atlas",
            hideFlags = HideFlags.HideInHierarchy
        };

        var shader = Shader.Find("TextMeshPro/Distance Field");
        if (shader == null)
            shader = Shader.Find("TextMeshPro/Mobile/Distance Field");

        var mat = new Material(shader)
        {
            name = fontAsset.name + " Material",
            hideFlags = HideFlags.HideInHierarchy
        };
        if (shader != null)
        {
            mat.SetTexture(ShaderUtilities.ID_MainTex, atlasTex);
        }

        AssetDatabase.AddObjectToAsset(atlasTex, path);
        AssetDatabase.AddObjectToAsset(mat, path);

        var so = new SerializedObject(fontAsset);
        var atlasProp = so.FindProperty("m_AtlasTextures");
        if (atlasProp != null)
        {
            atlasProp.arraySize = 1;
            atlasProp.GetArrayElementAtIndex(0).objectReferenceValue = atlasTex;
        }

        var matProp = so.FindProperty("m_Material");
        if (matProp != null)
        {
            matProp.objectReferenceValue = mat;
        }

        var legacyAtlasProp = so.FindProperty("atlas");
        if (legacyAtlasProp != null)
        {
            legacyAtlasProp.objectReferenceValue = atlasTex;
        }

        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }
}
