using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    public GameObject itemEffect;
    public float damage;

    [Header("Ammo")]
    public int maxAmmo = 12;
    public float emptyHideDelay = 2f;

    [Header("Ammo UI")]
    public GameObject ammoUIRoot;
    public Text ammoText;

    private int _currentAmmo;
    private Coroutine _hideAmmoRoutine;

    public int CurrentAmmo
    {
        get { return _currentAmmo; }
    }

    private void Awake()
    {
        _currentAmmo = Mathf.Max(0, maxAmmo);
        RefreshAmmoUI();
        HideAmmoUIImmediate();
    }

    public bool CanShoot()
    {
        return _currentAmmo > 0;
    }

    public bool ConsumeAmmo()
    {
        if (_currentAmmo <= 0)
        {
            StartHideAmmoRoutine();
            return false;
        }

        _currentAmmo--;
        ShowAmmoUI();
        RefreshAmmoUI();

        if (_currentAmmo == 0)
        {
            StartHideAmmoRoutine();
        }

        return true;
    }

    public void ShowAmmoUI()
    {
        if (_hideAmmoRoutine != null)
        {
            StopCoroutine(_hideAmmoRoutine);
            _hideAmmoRoutine = null;
        }

        if (ammoUIRoot != null)
        {
            ammoUIRoot.SetActive(true);
        }

        RefreshAmmoUI();
    }

    public void HideAmmoUIImmediate()
    {
        if (_hideAmmoRoutine != null)
        {
            StopCoroutine(_hideAmmoRoutine);
            _hideAmmoRoutine = null;
        }

        if (ammoUIRoot != null)
        {
            ammoUIRoot.SetActive(false);
        }
    }

    private void StartHideAmmoRoutine()
    {
        if (_hideAmmoRoutine != null)
        {
            StopCoroutine(_hideAmmoRoutine);
        }

        _hideAmmoRoutine = StartCoroutine(HideAmmoAfterDelay());
    }

    private IEnumerator HideAmmoAfterDelay()
    {
        yield return new WaitForSeconds(emptyHideDelay);

        if (_currentAmmo <= 0 && ammoUIRoot != null)
        {
            ammoUIRoot.SetActive(false);
        }

        _hideAmmoRoutine = null;
    }

    private void RefreshAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = _currentAmmo.ToString();
        }
    }
}
