using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private WeaponRaycast weapon;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("UI - Ammo")]
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private GameObject reloadHint;

    [Header("UI - Health")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthText;

    void Awake()
    {
        if (weapon == null) weapon = FindFirstObjectByType<WeaponRaycast>();
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (ammoText == null) Debug.LogError("HUDController: ammoText atanmadý.");
        if (healthBar == null) Debug.LogError("HUDController: healthBar atanmadý.");
        if (healthText == null) Debug.LogError("HUDController: healthText atanmadý.");
    }

    void Start()
    {
        if (playerHealth != null && healthBar != null)
        {
            healthBar.minValue = 0;
            healthBar.maxValue = playerHealth.MaxHP;
            healthBar.value = playerHealth.HP;
        }
    }

    void Update()
    {
        UpdateAmmoUI();
        UpdateHealthUI();
    }

    private void UpdateAmmoUI()
    {
        if (weapon == null || ammoText == null) return;

        ammoText.text = $"{weapon.MagAmmo} / {weapon.MagSize}   |   {weapon.ReserveAmmo}";


        if (reloadHint != null)
            reloadHint.SetActive(weapon.MagAmmo <= 0);
        if (weapon.IsReloading) ammoText.text += " (RELOADING)";

    }

    private void UpdateHealthUI()
    {
        if (playerHealth == null) return;

        if (healthBar != null)
            healthBar.value = playerHealth.HP;

        if (healthText != null)
            healthText.text = $"{playerHealth.HP} / {playerHealth.MaxHP}";
    }
}
