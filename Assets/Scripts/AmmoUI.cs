using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    [Header("Rifle")]
    [SerializeField] private GameObject rifleActive;
    [SerializeField] private TMP_Text rifleActiveText;
    [SerializeField] private TMP_Text rifleInactiveText;
    [Header("Shotgun")]
    [SerializeField] private GameObject shotgunAcitve;
    [SerializeField] private TMP_Text shotgunActiveText;
    [SerializeField] private TMP_Text shotgunInactiveText;

    [SerializeField] private TMP_Text magazinesText;

    [SerializeField] private Image reloadCircle;
    private void Start()
    {
        SetActiveWeapon(WeaponType.Rifle);
    }
    public void SetActiveWeapon(WeaponType weapon)
    {
        if (weapon == WeaponType.Rifle)
        {
            rifleActive.SetActive(true);

            shotgunAcitve.SetActive(false);

            rifleActiveText.text = rifleInactiveText.text;
            shotgunInactiveText.text = shotgunActiveText.text;
        }
        else if (weapon == WeaponType.Shotgun)
        {
            shotgunAcitve.SetActive(true);

            rifleActive.SetActive(false);

            shotgunActiveText.text = shotgunInactiveText.text;
            rifleInactiveText.text = rifleActiveText.text;
        }
    }
    public void UpdateRifleActiveText(int ammo, int maxAmmo)
    {
        UpdateUI(rifleActiveText, ammo + " / " + maxAmmo);
    }
    public void UpdateShotgunActiveText(int ammo, int maxAmmo)
    {
        UpdateUI(shotgunActiveText, ammo + " / " + maxAmmo);
    }

    public void UpdateMagazines(int magazines)
    {
        UpdateUI(magazinesText, magazines.ToString());
    }
    public void UpdateUI(TMP_Text text, string newText)
    {
        text.text = newText;
    }
    
    public void VisualReload(float duration, WeaponType currentWeapon)
    {
        StartCoroutine(StartVisualReload(duration, currentWeapon));
    }
    private IEnumerator StartVisualReload(float duration, WeaponType currentWeaponType) 
    {
        float t = 0;
        while (t < 1)
        {
            if (Inventory.Instance.activeWeapon != currentWeaponType)
            {
                reloadCircle.fillAmount = 0;
                break;
            }
            t += Time.deltaTime / duration;
            reloadCircle.fillAmount = t;
            yield return null;
        }
        reloadCircle.fillAmount = 0;
    }
}
