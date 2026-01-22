using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int hp;
    [SerializeField] private DamageFlashUI damageFlash;


    public int HP => hp;
    public int MaxHP => maxHP;

    private void Awake()
    {
        hp = maxHP;
    }

    public void TakeDamage(int amount)
    {
        if (damageFlash != null) damageFlash.Trigger();

        if (hp <= 0) return;

        hp = Mathf.Max(hp - amount, 0);

        if (hp == 0)
            Die();


    }

    private void Die()
    {
        Debug.Log("PLAYER DEAD");
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
