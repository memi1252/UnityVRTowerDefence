using UnityEngine;

public class VRBagTestBootstrap : MonoBehaviour
{
    public GrabObject grabObject;
    public VRBagToggle bagToggle;
    public MonoBehaviour[] disableWhenBagOpen;

    [Header("Bag Shape")]
    public Vector3 bagBodyScale = new Vector3(0.24f, 0.24f, 0.24f);
    public Vector3 triggerSize = new Vector3(0.2f, 0.2f, 0.2f);
    public int slotCount = 4;

    [Header("Test Items")]
    public int itemCount = 3;
    public Vector3 itemSpawnOffset = new Vector3(0.35f, 0f, 0.5f);
    public float itemSpacing = 0.15f;

    private const string RuntimeBagName = "RuntimeVRBag";

    private void Start()
    {
        if (grabObject == null)
        {
            grabObject = GetComponent<GrabObject>();
        }

        if (grabObject == null)
        {
            grabObject = FindObjectOfType<GrabObject>();
        }

        if (grabObject == null)
        {
            Debug.LogWarning("VRBagTestBootstrap: GrabObject를 찾지 못했습니다.");
            return;
        }

        GameObject bagRoot = BuildRuntimeBag();
        ConfigureToggle(bagRoot);
        SpawnTestItems(grabObject.grabbedLayer);
    }

    private GameObject BuildRuntimeBag()
    {
        Transform existing = transform.Find(RuntimeBagName);
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        GameObject bagRoot = new GameObject(RuntimeBagName);
        bagRoot.transform.SetParent(transform, false);

        GameObject bagBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bagBody.name = "BagBody";
        bagBody.transform.SetParent(bagRoot.transform, false);
        bagBody.transform.localScale = bagBodyScale;

        GameObject triggerObject = new GameObject("BagTrigger");
        triggerObject.transform.SetParent(bagRoot.transform, false);
        BoxCollider triggerCollider = triggerObject.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = triggerSize;

        Rigidbody triggerBody = triggerObject.AddComponent<Rigidbody>();
        triggerBody.isKinematic = true;
        triggerBody.useGravity = false;

        VRBagInventory inventory = triggerObject.AddComponent<VRBagInventory>();
        inventory.grabObject = grabObject;
        inventory.storableLayer = grabObject.grabbedLayer;
        inventory.slots = CreateSlots(bagRoot.transform);

        bagRoot.SetActive(false);
        return bagRoot;
    }

    private VRBagInventory.BagSlot[] CreateSlots(Transform bagRoot)
    {
        int count = Mathf.Max(1, slotCount);
        VRBagInventory.BagSlot[] bagSlots = new VRBagInventory.BagSlot[count];

        GameObject slotRoot = new GameObject("Slots");
        slotRoot.transform.SetParent(bagRoot, false);

        int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
        float spacing = 0.08f;

        for (int i = 0; i < count; i++)
        {
            GameObject slot = new GameObject("Slot_" + i);
            slot.transform.SetParent(slotRoot.transform, false);

            int x = i % columns;
            int y = i / columns;
            float offsetX = (x - (columns - 1) * 0.5f) * spacing;
            float offsetY = (y - (columns - 1) * 0.5f) * spacing;

            slot.transform.localPosition = new Vector3(offsetX, offsetY, 0f);

            bagSlots[i] = new VRBagInventory.BagSlot
            {
                slotPoint = slot.transform
            };
        }

        return bagSlots;
    }

    private void ConfigureToggle(GameObject bagRoot)
    {
        if (bagToggle == null)
        {
            bagToggle = GetComponent<VRBagToggle>();
        }

        if (bagToggle == null)
        {
            bagToggle = gameObject.AddComponent<VRBagToggle>();
        }

        bagToggle.bagRoot = bagRoot;
        bagToggle.followTarget = ARAVRInput.LHand;
        bagToggle.disableWhenBagOpen = disableWhenBagOpen;
    }

    private void SpawnTestItems(LayerMask grabbedLayerMask)
    {
        int layer = ResolveFirstLayer(grabbedLayerMask);
        Vector3 basePosition = transform.position + itemSpawnOffset;

        for (int i = 0; i < Mathf.Max(1, itemCount); i++)
        {
            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            item.name = "BagTestItem_" + i;
            item.transform.position = basePosition + Vector3.right * (i * itemSpacing);
            item.transform.localScale = new Vector3(0.1f, 0.12f, 0.1f);

            Rigidbody rb = item.AddComponent<Rigidbody>();
            rb.mass = 0.6f;

            if (layer >= 0)
            {
                item.layer = layer;
            }
        }
    }

    private int ResolveFirstLayer(LayerMask mask)
    {
        int value = mask.value;
        if (value == 0)
        {
            return -1;
        }

        for (int i = 0; i < 32; i++)
        {
            if ((value & (1 << i)) != 0)
            {
                return i;
            }
        }

        return -1;
    }
}

