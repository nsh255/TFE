using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BakeScoresButton
{
    private const string DefaultNewName = "ScoresButton";

    [MenuItem("Tools/Ascension/Bake Scores Button")]
    public static void Bake()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            EditorUtility.DisplayDialog("Bake Scores Button", "No hay escena activa.", "OK");
            return;
        }

        if (GameObject.Find(DefaultNewName) != null)
        {
            EditorUtility.DisplayDialog("Bake Scores Button", $"Ya existe '{DefaultNewName}' en la escena.", "OK");
            return;
        }

        var template = GetTemplateButton();
        if (template == null)
        {
            EditorUtility.DisplayDialog(
                "Bake Scores Button",
                "No se encontró el botón plantilla (selecciona el botón 'Jugar/Play' y vuelve a ejecutar, o asegúrate de que exista en la escena).",
                "OK"
            );
            return;
        }

        var menu = Object.FindFirstObjectByType<MainMenuManager>();
        if (menu == null)
        {
            EditorUtility.DisplayDialog(
                "Bake Scores Button",
                "No se encontró MainMenuManager en la escena. Añádelo al MainMenu para poder enlazar ToggleScores.",
                "OK"
            );
            return;
        }

        var cloneGo = Object.Instantiate(template.gameObject, template.transform.parent);
        Undo.RegisterCreatedObjectUndo(cloneGo, "Bake Scores Button");
        cloneGo.name = DefaultNewName;

        var cloneBtn = cloneGo.GetComponent<Button>();
        if (cloneBtn == null)
        {
            EditorUtility.DisplayDialog("Bake Scores Button", "El template no tiene componente Button.", "OK");
            Undo.DestroyObjectImmediate(cloneGo);
            return;
        }

        // Etiqueta
        if (!TrySetLabel(cloneGo, "Scores"))
        {
            Debug.LogWarning("[BakeScoresButton] No se encontró componente Text/TMP_Text para cambiar la etiqueta.");
        }

        // Posición: mismo que template pero un poco abajo.
        var templateRt = template.GetComponent<RectTransform>();
        var cloneRt = cloneGo.GetComponent<RectTransform>();
        if (templateRt != null && cloneRt != null)
        {
            cloneRt.anchorMin = templateRt.anchorMin;
            cloneRt.anchorMax = templateRt.anchorMax;
            cloneRt.pivot = templateRt.pivot;
            cloneRt.sizeDelta = templateRt.sizeDelta;
            cloneRt.anchoredPosition = templateRt.anchoredPosition + new Vector2(0f, -60f);
            cloneRt.localRotation = templateRt.localRotation;
            cloneRt.localScale = templateRt.localScale;
        }

        // OnClick -> MainMenuManager.ToggleScores (listener persistente)
        cloneBtn.onClick = new Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(cloneBtn.onClick, menu.ToggleScores);

        EditorUtility.SetDirty(cloneBtn);
        EditorUtility.SetDirty(cloneGo);
        EditorUtility.SetDirty(menu);

        EditorSceneManager.MarkSceneDirty(scene);

        EditorUtility.DisplayDialog(
            "Bake Scores Button",
            "Botón 'Scores' creado y guardable en la escena. Guarda la escena para hacerlo permanente.",
            "OK"
        );
    }

    private static Button GetTemplateButton()
    {
        // 1) Si el usuario seleccionó el botón
        var selected = Selection.activeGameObject;
        if (selected != null)
        {
            var selBtn = selected.GetComponent<Button>();
            if (selBtn != null) return selBtn;
        }

        // 2) Buscar por etiqueta
        var b = FindButtonByLabel("Jugar");
        if (b != null) return b;
        b = FindButtonByLabel("Play");
        if (b != null) return b;
        b = FindButtonByLabel("Start");
        if (b != null) return b;

        // 3) Fallback por nombre
        var buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var btn in buttons)
        {
            if (btn == null) continue;
            var n = btn.name ?? string.Empty;
            if (n.IndexOf("jugar", System.StringComparison.OrdinalIgnoreCase) >= 0) return btn;
            if (n.IndexOf("play", System.StringComparison.OrdinalIgnoreCase) >= 0) return btn;
            if (n.IndexOf("start", System.StringComparison.OrdinalIgnoreCase) >= 0) return btn;
        }

        return null;
    }

    private static Button FindButtonByLabel(string label)
    {
        var buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var b in buttons)
        {
            if (b == null) continue;

            var t = b.GetComponentInChildren<Text>(true);
            if (t != null && string.Equals(t.text?.Trim(), label, System.StringComparison.OrdinalIgnoreCase))
                return b;

            var tmp = b.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null && string.Equals(tmp.text?.Trim(), label, System.StringComparison.OrdinalIgnoreCase))
                return b;
        }
        return null;
    }

    private static bool TrySetLabel(GameObject root, string label)
    {
        var t = root.GetComponentInChildren<Text>(true);
        if (t != null)
        {
            t.text = label;
            EditorUtility.SetDirty(t);
            return true;
        }

        var tmp = root.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            tmp.text = label;
            EditorUtility.SetDirty(tmp);
            return true;
        }

        return false;
    }
}
