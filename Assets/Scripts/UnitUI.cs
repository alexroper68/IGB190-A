using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    public RectTransform container;
    public Image healthBar;
    public Vector3 offset = new Vector3(0, 2, 0);

    private IDamageable trackedDamageable;
    private Transform trackedTransform;

    private void Awake()
    {
        trackedDamageable = GetComponentInParent<IDamageable>();
        trackedTransform = transform.parent;
    }

    private void LateUpdate()
    {
        Vector3 world = trackedTransform.position + offset;
        container.anchoredPosition = Camera.main.WorldToScreenPoint(world);
        healthBar.fillAmount = trackedDamageable.GetCurrentHealthPercent();
    }
}
