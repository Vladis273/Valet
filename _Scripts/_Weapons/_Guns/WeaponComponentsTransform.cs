using UnityEngine;

public class WeaponComponentsTransform : MonoBehaviour
{
    public WeaponController controller;
    public ArmsController armController;

    private void Start()
    {
        if (!controller) controller = GetComponent<WeaponController>();
        if (!armController) armController = GetComponentInParent<ArmsController>();
    }
}
