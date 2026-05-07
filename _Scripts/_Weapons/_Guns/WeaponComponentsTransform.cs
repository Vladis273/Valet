using UnityEngine;

public class WeaponComponentsTransform : MonoBehaviour
{
    [Header("Core References")]
    public WeaponController controller;
    public ArmsController armController;
    
    [Header("Fire Point")]
    [Tooltip("Точка вылета пуль")]
    public Transform firePoint;
    
    [Header("Shell Ejection")]
    [Tooltip("Префаб стреляной гильзы")]
    public GameObject shellPrefab;
    [Tooltip("Точка выброса гильз")]
    public Transform ejectPoint;
    [Tooltip("Сила выброса гильзы")]
    public float shellForce = 5f;
    [Tooltip("Вертикальная составляющая выброса")]
    public float shellUpward = 2f;
    
    [Header("Tracer")]
    [Tooltip("Префаб трассера")]
    public GameObject tracerPrefab;
    [Tooltip("Цвет трассера")]
    public Color tracerColor = Color.yellow;
    [Tooltip("Ширина трассера")]
    public float tracerWidth = 0.05f;
    [Tooltip("Время жизни трассера")]
    public float tracerLifetime = 0.2f;
    [Tooltip("Скорость затухания трассера")]
    public float tracerFadeSpeed = 0.5f;
    
    private void Start()
    {
        if (!controller) controller = GetComponent<WeaponController>();
        if (!armController) armController = GetComponentInParent<ArmsController>();
        if (!firePoint) firePoint = transform.Find("FirePoint");
        if (!ejectPoint) ejectPoint = transform.Find("EjectPoint");
    }
}
