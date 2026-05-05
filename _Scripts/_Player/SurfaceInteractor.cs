using UnityEngine;

public class SurfaceInteractor : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistance = 1.5f;
    [SerializeField] private Transform raycastOrigin;

    [Header("Gizmos")]
    [SerializeField] private Color rayColor = Color.blue;
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color hitPointColor = Color.red;

    public Vector3 GetNormalOfSurface()
    {
        if (raycastOrigin == null) raycastOrigin = transform;

        if (Physics.Raycast(
            raycastOrigin.position, 
            Vector3.down, 
            out RaycastHit hit, 
            raycastDistance, 
            groundLayer))
        {
            return hit.normal;
        }

        return Vector3.up;
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = raycastOrigin.position;
        Vector3 direction = Vector3.down * raycastDistance;

        Gizmos.color = rayColor;
        Gizmos.DrawLine(origin, origin + direction);

        if (Physics.Raycast(
            origin, 
            Vector3.down, 
            out RaycastHit hit, 
            raycastDistance, 
            groundLayer))
        {
            Gizmos.color = hitPointColor;
            Gizmos.DrawSphere(hit.point, 0.1f);

            Gizmos.color = normalColor;
            Gizmos.DrawLine(hit.point, hit.point + hit.normal * 0.5f);

            Gizmos.DrawSphere(hit.point + hit.normal * 0.5f, 0.05f);
        }
        else
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawSphere(origin + direction, 0.05f);
        }

        Gizmos.color = rayColor;
        Gizmos.DrawSphere(origin, 0.05f);
    }
}