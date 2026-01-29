using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class AscensionUIConvertSpriteRendererToUIImage
{
    [MenuItem("Tools/Ascension/UI/Fix Weapon Sprites (SpriteRenderer -> UI Image)")]
    public static void ConvertInActiveScene()
    {
        var active = SceneManager.GetActiveScene();
        if (!active.IsValid())
        {
            EditorUtility.DisplayDialog("Fix Weapon Sprites", "No hay una escena activa válida.", "OK");
            return;
        }

        int converted = ConvertSpriteRenderersUnderCanvas();
        EditorSceneManager.MarkSceneDirty(active);

        EditorUtility.DisplayDialog("Fix Weapon Sprites", $"Convertidos: {converted}\n\nAhora esos sprites se dibujan como UI encima del fondo.", "OK");
    }

    public static int ConvertSpriteRenderersUnderCanvas()
    {
        var spriteRenderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        int converted = 0;

        foreach (var sr in spriteRenderers)
        {
            if (sr == null) continue;
            if (sr.GetComponentInParent<Canvas>() == null) continue; // solo cosas metidas dentro del Canvas

            var oldGo = sr.gameObject;
            var parent = oldGo.transform.parent;
            int siblingIndex = oldGo.transform.GetSiblingIndex();

            var sprite = sr.sprite;
            var oldLocal = oldGo.transform.localPosition;

            // Crear UI Image reemplazo
            var newGo = new GameObject(oldGo.name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            Undo.RegisterCreatedObjectUndo(newGo, "Convert SpriteRenderer to UI Image");

            newGo.layer = 5; // UI
            var rt = (RectTransform)newGo.transform;
            rt.SetParent(parent, false);
            rt.SetSiblingIndex(siblingIndex);

            // Intentar mantener la posición aproximada dentro del botón
            rt.anchoredPosition = new Vector2(oldLocal.x, oldLocal.y);
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;

            var img = newGo.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;

            if (sprite != null)
            {
                img.SetNativeSize();
            }
            else
            {
                // Tamaño razonable por defecto
                rt.sizeDelta = new Vector2(64, 64);
            }

            // Borrar el antiguo
            Undo.DestroyObjectImmediate(oldGo);
            converted++;
        }

        return converted;
    }
}
