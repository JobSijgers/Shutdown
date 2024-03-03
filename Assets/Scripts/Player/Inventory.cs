using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum WeaponType
{
    none,
    Rifle,
    Shotgun
}


public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }
    public WeaponType activeWeapon;
    [SerializeField] private KeycardUI keycardUI;
    [SerializeField] private AmmoUI ammoUI;
    [SerializeField] private PlayerShooting player;
    [SerializeField] private WeaponType[] weapons = new WeaponType[1];
    private List<Keycard> keycards = new();
    private CustomInput input;

    private void Awake()
    {
        Instance = this;
        input = new CustomInput();
        input.Controls.ChangeWeapons.performed += ChangeWeapons;
    }

    private void Start()
    {
        activeWeapon = weapons[0] = WeaponType.Rifle;
        weapons[1] = WeaponType.Shotgun; //- temporary until weapon pickups are done
    }
    private void OnEnable()
    {
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }

    public void SetWeapon(WeaponType weapon)
    {
        if (weapons[0] == WeaponType.Rifle)
        {
            weapons[1] = weapon;
        }
        else
        {
            weapons[0] = weapon;
            activeWeapon = weapons[0];
            player.UpdateUI(); //- tweak this if more weapons are added
        }
    }

    public void AddKeycard(Keycard keycard)
    {
        keycards.Add(keycard);
        keycardUI.ChangeKeycardUIState(keycard.type);
    }
    public void RemoveKeycard(KeycardType type)
    {
        for (int i = 0; i < keycards.Count; i++)
        {
            if (keycards[i].type == type)
            {
                keycards.Remove(keycards[i]);
                keycardUI.ChangeKeycardUIState(type);
            }
        }
    }

    public bool CheckForKeycard(KeycardType type)
    {
        for (int i = 0; i < keycards.Count; i++)
        {
            if (keycards[i].type == type) return true;
        }
        return false;
    }
    private void ChangeWeapons(InputAction.CallbackContext context)
    {
        if (weapons[1] == WeaponType.none) return;

        // swap weapons
        WeaponType lastSelected = weapons[0];
        weapons[0] = weapons[1];
        weapons[1] = lastSelected;
        activeWeapon = weapons[0];
        if (activeWeapon == WeaponType.Rifle)
        {
            player.UpdateUI();
            player.SetNewWeapon(activeWeapon);
        }
        else //- tweak this a bit if adding more weapons
        {
            player.UpdateUI();
            player.SetNewWeapon(activeWeapon);
        }
    }
}
