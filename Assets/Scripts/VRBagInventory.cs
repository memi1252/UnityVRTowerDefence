using System;
using System.Collections.Generic;
using UnityEngine;

public class VRBagInventory : MonoBehaviour
{
    [System.Serializable]
    public class BagSlot
    {
        public Transform slotPoint;
        [HideInInspector] public GameObject storedItem;
    }

    public GrabObject grabObject;
    public LayerMask storableLayer;
    public BagSlot[] slots;

    private readonly Dictionary<GameObject, int> _storedItemToSlot = new Dictionary<GameObject, int>();

    private void Reset()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }
    

    private void Update()
    {
        RefreshStoredState();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        GameObject itemObject = ResolveItemObject(other);
        if (itemObject == null)
        {
            return;
        }

        if (_storedItemToSlot.ContainsKey(itemObject))
        {
            return;
        }

        if (!IsLayerAllowed(itemObject.layer))
        {
            return;
        }

        if (grabObject != null && grabObject.IsObjectGrabbed(itemObject))
        {
            return;
        }

        TryStoreItem(itemObject);
    }

    private void RefreshStoredState()
    {
        if (_storedItemToSlot.Count == 0)
        {
            return;
        }

        List<GameObject> releaseList = null;
        foreach (KeyValuePair<GameObject, int> pair in _storedItemToSlot)
        {
            GameObject item = pair.Key;
            int slotIndex = pair.Value;

            bool isMissing = item == null;
            bool isGrabbed = !isMissing && grabObject != null && grabObject.IsObjectGrabbed(item);
            bool movedOut = !isMissing && item.transform.parent != slots[slotIndex].slotPoint;

            if (isMissing || isGrabbed || movedOut)
            {
                if (releaseList == null)
                {
                    releaseList = new List<GameObject>();
                }

                releaseList.Add(item);
            }
        }

        if (releaseList == null)
        {
            return;
        }

        for (int i = 0; i < releaseList.Count; i++)
        {
            RemoveStoredItem(releaseList[i]);
        }
    }

    private void TryStoreItem(GameObject itemObject)
    {
        int slotIndex = FindNearestEmptySlot(itemObject.transform.position);
        if (slotIndex < 0)
        {
            return;
        }

        BagSlot targetSlot = slots[slotIndex];
        targetSlot.storedItem = itemObject;
        _storedItemToSlot[itemObject] = slotIndex;

        itemObject.transform.SetParent(targetSlot.slotPoint, true);
        itemObject.transform.localPosition = Vector3.zero;
        itemObject.transform.localRotation = Quaternion.identity;

        Rigidbody rb = itemObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    private void RemoveStoredItem(GameObject itemObject)
    {
        if (!_storedItemToSlot.TryGetValue(itemObject, out int slotIndex))
        {
            return;
        }

        _storedItemToSlot.Remove(itemObject);
        if (slotIndex >= 0 && slotIndex < slots.Length)
        {
            if (slots[slotIndex].storedItem == itemObject)
            {
                slots[slotIndex].storedItem = null;
            }
        }
    }

    private int FindNearestEmptySlot(Vector3 itemPosition)
    {
        if (slots == null || slots.Length == 0)
        {
            return -1;
        }

        int nearestIndex = -1;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null || slots[i].slotPoint == null || slots[i].storedItem != null)
            {
                continue;
            }

            float distance = (slots[i].slotPoint.position - itemPosition).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    private GameObject ResolveItemObject(Collider other)
    {
        if (other == null)
        {
            return null;
        }

        Rigidbody attachedRigidbody = other.attachedRigidbody;
        GameObject candidate = attachedRigidbody != null ? attachedRigidbody.gameObject : other.gameObject;

        if (candidate == gameObject || candidate.transform.IsChildOf(transform))
        {
            return null;
        }

        if (candidate.GetComponent<Rigidbody>() == null)
        {
            return null;
        }

        return candidate;
    }

    private bool IsLayerAllowed(int layer)
    {
        if (storableLayer.value == 0)
        {
            return true;
        }

        return (storableLayer.value & (1 << layer)) != 0;
    }
}
