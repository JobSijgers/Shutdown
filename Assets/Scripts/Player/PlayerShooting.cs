using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.InputSystem;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private Quaternion rotationOffset;

    [SerializeField] private GameObject bullet;
    [SerializeField] private AmmoUI ammoUI;
    [SerializeField] private int magazines;
    [SerializeField] private LayerMask lineObstruction;
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Base")]
    [SerializeField] private int maxBaseAmmo = 30;
    [SerializeField] private float baseweaponReloadTime;
    [SerializeField] private Transform rifleRotationHandle;
    [SerializeField] private Transform baseSpawnpoint;
    [SerializeField] private float baseShootInterval;

    [Header("Shotgun")]
    [SerializeField] private float shotgunReloadTime;
    [SerializeField] private int maxShotgunAmmo = 5;
    [SerializeField] float spreadAngle = 20;
    [SerializeField] float projCount = 8;
    [SerializeField] float projOffset = 20;
    [SerializeField] float minSpeed = 3;
    [SerializeField] float maxSpeed = 9;
    [SerializeField] float despawnMultiplier = 0.1f;
    [SerializeField] float despawnVariation = 1.5f;
    [SerializeField] private Transform shotgunRotationHandle;
    [SerializeField] private Transform shotgunSpawnpoint;
    [SerializeField] private float shotgunInterval;

    private float shootInterval;
    private float shootTimer;
    private Inventory inventory;
    private int baseAmmo;
    private int shotgunAmmo;
    private bool reloading = false;
    private CustomInput input;
    private bool firePressed;
    private Transform activeSpawnpoint;
    private Transform activeRotationHandle;


    private void Awake()
    {
        input = new CustomInput();
        input.Controls.Reload.performed += ReloadActiveWeapon;
    }
    private void Start()
    {
        SetNewWeapon(WeaponType.Rifle);
        GameManager.Instance.PauseDuringCutsceneEvent += OnPause;
        inventory = Inventory.Instance;
        baseAmmo = maxBaseAmmo;
        shotgunAmmo = maxShotgunAmmo;
        UpdateUI();
    }
    void Update()
    {
        if (Time.timeScale == 0)
            return;
        firePressed = IsFirePressed();
        if (firePressed && shootTimer >= shootInterval)
        {
            if (GetActiveWeapon() == WeaponType.Rifle && baseAmmo > 0 && !reloading)
            {
                DefaultShoot();
            }
            else if (GetActiveWeapon() == WeaponType.Shotgun && shotgunAmmo > 0 && !reloading)
            {
                ShotgunShoot();
            }
            shootTimer = 0;
        }
        else if (shootTimer < shootInterval)
        {
            shootTimer += Time.deltaTime;
        }

        if (baseAmmo == 0 && magazines > 0 && !reloading && GetActiveWeapon() == WeaponType.Rifle)
        {
            StartCoroutine(ReloadBaseWeapon());
        }
        if (shotgunAmmo == 0 && magazines > 0 && !reloading && GetActiveWeapon() == WeaponType.Shotgun)
        {
            StartCoroutine(ReloadShotgun());
        }


    }
    private void FixedUpdate()
    {
        RenderAttackLine();
    }
    private void OnEnable()
    {
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
    private WeaponType GetActiveWeapon()
    {
        switch (inventory.activeWeapon)
        {
            case WeaponType.Rifle:
                return WeaponType.Rifle;
            case WeaponType.Shotgun:
                return WeaponType.Shotgun;
            default:
                return WeaponType.none;
        }
    }
    private void DefaultShoot()
    {
        Instantiate(bullet, activeSpawnpoint.position, activeSpawnpoint.transform.rotation * rotationOffset);
        baseAmmo -= 1;
        UpdateUI();
        AudioManager.Instance.Play("Shoot");
    }
    private void ShotgunShoot()
    {
        float angleDiff = spreadAngle / projCount;
        for (float curAngle = -spreadAngle / 2; curAngle <= spreadAngle / 2; curAngle += angleDiff)
        {
            float offset = Random.Range(0, projOffset);
            Bullet spawnedBullet =
                Instantiate(bullet, activeSpawnpoint.position, activeSpawnpoint.transform.rotation * Quaternion.Euler(Vector3.up * (curAngle - projOffset / 2 + offset)) * rotationOffset)
                .GetComponent<Bullet>();
            float bulletSpeed = Random.Range(minSpeed, maxSpeed);
            spawnedBullet.movementSpeed = bulletSpeed;
            spawnedBullet.despawnTime = despawnMultiplier * (maxSpeed * despawnVariation - bulletSpeed);
        }
        shotgunAmmo -= 1;
        UpdateUI();
        AudioManager.Instance.Play("Shoot");
    }
    public void UpdateUI()
    {
        if (inventory.activeWeapon == WeaponType.Rifle)
        {
            ammoUI.UpdateRifleActiveText(baseAmmo, maxBaseAmmo);
        }
        if (inventory.activeWeapon == WeaponType.Shotgun)
        {
            ammoUI.UpdateShotgunActiveText(shotgunAmmo, maxShotgunAmmo);
        }
        ammoUI.UpdateMagazines(magazines);
    }
    private IEnumerator ReloadBaseWeapon()
    {
        ammoUI.VisualReload(baseweaponReloadTime, GetActiveWeapon());
        reloading = true;
        yield return new WaitForSeconds(baseweaponReloadTime);
        if (inventory.activeWeapon == WeaponType.Rifle)
        {
            baseAmmo = maxBaseAmmo;
            magazines--;
            UpdateUI();
        }
        reloading = false;
    }
    private IEnumerator ReloadShotgun()
    {
        ammoUI.VisualReload(shotgunReloadTime, GetActiveWeapon());
        reloading = true;
        yield return new WaitForSeconds(shotgunReloadTime);
        if (inventory.activeWeapon == WeaponType.Shotgun)
        {
            shotgunAmmo = maxShotgunAmmo;
            magazines--;
            UpdateUI();
        }
        reloading = false;
    }
    public void AddMagazines(int magazines)
    {
        this.magazines += magazines;
        UpdateUI();
    }
    private bool IsFirePressed()
    {
        if (input.Controls.Shoot.ReadValue<float>() > 0.1f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void ReloadActiveWeapon(InputAction.CallbackContext context)
    {
        if (GetActiveWeapon() == WeaponType.Rifle && !reloading && baseAmmo != maxBaseAmmo)
        {
            if (magazines == 0)
                return;
            StartCoroutine(ReloadBaseWeapon());
        }
        else if (GetActiveWeapon() == WeaponType.Shotgun && !reloading && shotgunAmmo != maxShotgunAmmo )
        {
            if (magazines == 0)
                return;
            StartCoroutine(ReloadShotgun());
        }
    }
    public void SetNewWeapon(WeaponType type)
    {
        ammoUI.SetActiveWeapon(type);
        if (type == WeaponType.Rifle)
        {
            activeSpawnpoint = baseSpawnpoint;
            activeRotationHandle = rifleRotationHandle;
            shootInterval = baseShootInterval;
        }
        else
        {
            activeSpawnpoint = shotgunSpawnpoint;
            activeRotationHandle = shotgunRotationHandle;
            shootInterval = shotgunInterval;
        }
            shootTimer = 10;
    }
    private void OnPause(bool pause)
    {
        enabled = pause;
    }
    private void OnDestroy()
    {
        GameManager.Instance.PauseDuringCutsceneEvent -= OnPause;
    }
    private void RenderAttackLine()
    {
        lineRenderer.SetPosition(0, activeSpawnpoint.position);
        Ray ray = new Ray(activeSpawnpoint.position, new Vector3(-activeSpawnpoint.right.x, 0, -activeSpawnpoint.right.z));
        if (Physics.Raycast(ray, out RaycastHit attackRayHit, Mathf.Infinity, lineObstruction))
        {
            lineRenderer.SetPosition(1, attackRayHit.point);
        }
        else
        {
            lineRenderer.SetPosition(1, ray.GetPoint(100));
        }
    }
    public Transform GetActiveRotationHandle()
    {
        return activeRotationHandle;
    }
}
