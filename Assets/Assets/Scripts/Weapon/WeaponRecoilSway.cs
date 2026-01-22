using UnityEngine;

public class WeaponRecoilSway : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform cameraPivot; 
    [SerializeField] private Transform weaponRoot;  

    [Header("Recoil (per shot)")]
    [SerializeField] private float recoilKickPitch = 1.2f;   
    [SerializeField] private float recoilKickBack = 0.06f;   
    [SerializeField] private float recoilKickUp = 0.02f;     
    [SerializeField] private float recoilRecoverSpeed = 16f; 

    [Header("Sway (mouse)")]
    [SerializeField] private float swayAmount = 0.02f;
    [SerializeField] private float swayMax = 0.04f;
    [SerializeField] private float swaySmooth = 12f;

    private Vector3 weaponDefaultLocalPos;
    private Quaternion weaponDefaultLocalRot;

    private Vector3 recoilOffset; 
    private float recoilPitch;    

    private Vector3 swayOffset;

    void Awake()
    {
        if (weaponRoot == null) weaponRoot = transform;

        weaponDefaultLocalPos = weaponRoot.localPosition;
        weaponDefaultLocalRot = weaponRoot.localRotation;

        if (cameraPivot == null)
            Debug.LogError("WeaponRecoilSway: cameraPivot atanmadý (Player/CameraPivot).");
    }

    void Update()
    {
        UpdateSway();
        RecoverRecoil();
        ApplyTransforms();
    }

    private void UpdateSway()
    {
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        Vector3 target = new Vector3(
            Mathf.Clamp(-mx * swayAmount, -swayMax, swayMax),
            Mathf.Clamp(-my * swayAmount, -swayMax, swayMax),
            0f
        );

        swayOffset = Vector3.Lerp(swayOffset, target, Time.deltaTime * swaySmooth);
    }

    private void RecoverRecoil()
    {
        recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, Time.deltaTime * recoilRecoverSpeed);
        recoilPitch = Mathf.Lerp(recoilPitch, 0f, Time.deltaTime * recoilRecoverSpeed);
    }

    private void ApplyTransforms()
    {
        if (weaponRoot != null)
            weaponRoot.localPosition = weaponDefaultLocalPos + recoilOffset + swayOffset;

        if (cameraPivot != null)
        {
            cameraPivot.localRotation *= Quaternion.Euler(-recoilPitch, 0f, 0f);
        }
    }

    public void AddRecoil()
    {
        recoilOffset += new Vector3(0f, recoilKickUp, -recoilKickBack);
        recoilPitch += recoilKickPitch;
    }
}
