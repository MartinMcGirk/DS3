using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHolderSlot : MonoBehaviour
{
    public Transform parentOverride;
    public bool isLeftHandSlot;
    public bool isRightHandSlot;

    public GameObject currentWeaponModel;

    public void UnloadWeapon()
    {
        if (currentWeaponModel != null)
        {
            currentWeaponModel.gameObject.SetActive(false);
        }
    }

    public void UnloadWeaponAndDestroy()
    {
        if (currentWeaponModel != null)
        {
            Destroy(currentWeaponModel.gameObject);
        }
    }

    public void LoadWeaponModel(WeaponItem weapon)
    {
        UnloadWeaponAndDestroy();

        if (weapon == null)
        {
            UnloadWeapon();
            return;
        }

        GameObject model = Instantiate(weapon.modelPrefab);
        if (model != null)
        {
            if (parentOverride != null)
            {
                model.transform.parent = parentOverride; //.transform?
            }
            else
            {
                model.transform.parent = transform;
            }

            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
        }

        currentWeaponModel = model;
    }
}
