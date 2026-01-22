using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [SerializeField] private int reserveAmount = 18;
    [SerializeField] private bool alsoTopOffMagazine = false;
    [SerializeField] private int magazineAmount = 0;

    public void Configure(int reserve, bool topOffMag = false, int mag = 0)
    {
        reserveAmount = Mathf.Max(0, reserve);
        alsoTopOffMagazine = topOffMag;
        magazineAmount = Mathf.Max(0, mag);
    }

    private void OnTriggerEnter(Collider other)
    {
        var weapon = other.GetComponentInChildren<WeaponRaycast>();
        if (weapon == null) return;

        weapon.AddReserveAmmo(reserveAmount);

        if (alsoTopOffMagazine && magazineAmount > 0)
            weapon.AddMagazineAmmo(magazineAmount);

        Destroy(gameObject);
    }
}
