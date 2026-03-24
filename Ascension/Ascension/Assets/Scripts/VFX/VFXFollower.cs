using UnityEngine;

/// <summary>
/// Hace que un VFX siga a una entidad (transform target).
/// Útil para que efectos visuales acompañen al jugador o enemigos mientras se mueven.
/// </summary>
public class VFXFollower : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    [SerializeField] private bool maintainBaseLocalPos = false;

    private Vector3 baseLocalPos;

    public void SetTarget(Transform newTarget, Vector3 offset = default)
    {
        target = newTarget;
        positionOffset = offset;
        
        if (target != null)
        {
            baseLocalPos = transform.localPosition;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // Si mantainBaseLocalPos es true, preservamos la offset local (para PulseVFX que usa localPosition)
        if (maintainBaseLocalPos)
        {
            // La posición local se mantiene igual (PulseVFX maneja el bob/animación)
            // Solo actualizamos la posición world del padre para estar cerca del target
            transform.position = target.position + positionOffset;
        }
        else
        {
            // Posición world directa del target + offset
            transform.position = target.position + positionOffset;
        }
    }
}
