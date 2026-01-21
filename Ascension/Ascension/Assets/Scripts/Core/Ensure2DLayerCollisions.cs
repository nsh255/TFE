using UnityEngine;

/// <summary>
/// En algunos proyectos la matriz de colisión por Layers puede quedar desactivada
/// y el Player atraviesa muros aunque existan colliders.
/// Este helper fuerza en runtime las colisiones críticas para el gameplay.
/// </summary>
public static class Ensure2DLayerCollisions
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Apply()
    {
        EnableCollision("Player", "Wall");
        EnableCollision("Enemy", "Wall");
        EnableCollision("Entities", "Wall");
        EnableCollision("Player", "Enemy");
    }

    private static void EnableCollision(string layerAName, string layerBName)
    {
        int layerA = LayerMask.NameToLayer(layerAName);
        int layerB = LayerMask.NameToLayer(layerBName);
        if (layerA < 0 || layerB < 0) return;

        int maskA = Physics2D.GetLayerCollisionMask(layerA);
        maskA |= 1 << layerB;
        Physics2D.SetLayerCollisionMask(layerA, maskA);

        int maskB = Physics2D.GetLayerCollisionMask(layerB);
        maskB |= 1 << layerA;
        Physics2D.SetLayerCollisionMask(layerB, maskB);
    }
}
