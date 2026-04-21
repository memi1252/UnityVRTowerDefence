using UnityEngine;

public class VRBagToggle : MonoBehaviour
{
    public GameObject bagRoot;
    public Transform followTarget;
    public Vector3 localOffset = new Vector3(0.2f, -0.1f, 0.25f);
    public Vector3 localEuler = new Vector3(0f, 180f, 0f);

    [Tooltip("Bag open state and teleport state can conflict on the same button.")]
    public MonoBehaviour[] disableWhenBagOpen;

    private bool _isBagOpen;
    private bool[] _cachedEnabledStates;

    private void Start()
    {
        if (bagRoot != null)
        {
            bagRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (ARAVRInput.GetDown(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch))
        {
            ToggleBag();
        }

        if (_isBagOpen)
        {
            UpdateBagPose();
        }
    }

    public void ToggleBag()
    {
        SetBagOpen(!_isBagOpen);
    }

    public void SetBagOpen(bool open)
    {
        _isBagOpen = open;
        SetDisableTargets(open);

        if (bagRoot == null)
        {
            return;
        }

        if (open)
        {
            UpdateBagPose();
        }

        bagRoot.SetActive(open);
    }

    private void UpdateBagPose()
    {
        if (bagRoot == null)
        {
            return;
        }

        Transform target = followTarget != null ? followTarget : ARAVRInput.LHand;
        if (target == null)
        {
            return;
        }

        bagRoot.transform.SetParent(target, false);
        bagRoot.transform.localPosition = localOffset;
        bagRoot.transform.localEulerAngles = localEuler;
    }

    private void SetDisableTargets(bool disable)
    {
        if (disableWhenBagOpen == null || disableWhenBagOpen.Length == 0)
        {
            return;
        }

        if (_cachedEnabledStates == null || _cachedEnabledStates.Length != disableWhenBagOpen.Length)
        {
            _cachedEnabledStates = new bool[disableWhenBagOpen.Length];
        }

        for (int i = 0; i < disableWhenBagOpen.Length; i++)
        {
            MonoBehaviour target = disableWhenBagOpen[i];
            if (target == null)
            {
                continue;
            }

            if (disable)
            {
                _cachedEnabledStates[i] = target.enabled;
                target.enabled = false;
            }
            else
            {
                target.enabled = _cachedEnabledStates[i];
            }
        }
    }
}

