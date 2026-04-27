using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GrabObject : MonoBehaviour
{
    // 필요 속성 : 물체를 잡고 있는 여부, 잡고 있는 물체, 잡을 물체의 종류, 잡을 수 있는 거리
    // 물체를 잡고 있는 여부
    public bool isGrabbing = false;
    // 잡고 있는 물체
    public GameObject grabbedObject;
    // 잡을 물체의 종류
    public LayerMask grabbedLayer;
    // 잡을수 있는 거리
    public float grabRange = 0.2f;
    
    // 던질 힘
    [SerializeField]
    private float throwPower = 10;
    [SerializeField]
    private float remoteThrowMultiplier = 0.6f;
    [SerializeField]
    private float maxThrowSpeed = 10f;
    // 회전력
    public float rotPower = 5f;
    
    // 원거리에서 물체를 잡는 기능 활성화 여부
    public bool isRemoteGrab = true;
    // 원거리에서 물체를 잡을 수 있는 거리
    public float remoteGrabDistance = 20f;
    // 리모트 해제 시 보장할 최소 투척 속도
    public float minRemoteThrowSpeed = 4f;

    private class HandGrabState
    {
        public ARAVRInput.Controller Controller;
        public bool IsGrabbing;
        public bool IsRemoteGrab;
        public GameObject GrabbedObject;
        public Vector3 PrevPos;
        public Quaternion PrevRot;
        public Coroutine GrabbingAnimationRoutine;
        public Transform HandTransform;
    }

    private readonly HashSet<GameObject> _grabbedObjects = new HashSet<GameObject>();
    private HandGrabState _rightHandState;
    private HandGrabState _leftHandState;
    
    
    private void Awake()
    {
        _rightHandState = new HandGrabState { Controller = ARAVRInput.Controller.RTouch };
        _leftHandState = new HandGrabState { Controller = ARAVRInput.Controller.LTouch };
    }
    
    void Update()
    {
        ProcessHand(_rightHandState);
        ProcessHand(_leftHandState);
        SyncLegacyFields();
    }

    private void ProcessHand(HandGrabState handState)
    {
        if (handState.IsGrabbing)
        {
            TryUngrab(handState);
            return;
        }

        if (ARAVRInput.GetDown(ARAVRInput.Button.HandTrigger, handState.Controller))
        {
            TryGrab(handState);
        }
    }

    private void TryGrab(HandGrabState handState)
    {
        Transform handTransform = GetHandTransform(handState.Controller);
        if (handTransform == null)
        {
            return;
        }

        handState.HandTransform = handTransform;
        Vector3 handPosition = handTransform.position;
        Vector3 handDirection = handTransform.forward;

        if (isRemoteGrab)
        {
            Ray ray = new Ray(handPosition, handDirection);
            RaycastHit hitInfo;
            if (Physics.SphereCast(ray, 0.5f, out hitInfo, remoteGrabDistance, grabbedLayer))
            {
                GameObject hitObject = hitInfo.transform.gameObject;
                if (_grabbedObjects.Contains(hitObject))
                {
                    return;
                }

                BeginGrab(handState, hitObject, true);
                return;
            }
        }

        Collider[] hitObjects = Physics.OverlapSphere(handPosition, grabRange, grabbedLayer);
        if (hitObjects.Length == 0)
        {
            return;
        }

        int closest = -1;
        float closestDistance = float.MaxValue;
        for (int i = 0; i < hitObjects.Length; i++)
        {
            GameObject candidate = hitObjects[i].gameObject;
            if (_grabbedObjects.Contains(candidate))
            {
                continue;
            }

            Vector3 nextPos = hitObjects[i].transform.position;
            float nextDistance = Vector3.Distance(nextPos, handPosition);
            if (nextDistance < closestDistance)
            {
                closestDistance = nextDistance;
                closest = i;
            }
        }

        if (closest < 0)
        {
            return;
        }

        BeginGrab(handState, hitObjects[closest].gameObject, false);
    }

    private void BeginGrab(HandGrabState handState, GameObject targetObject, bool remote)
    {
        handState.IsGrabbing = true;
        handState.IsRemoteGrab = remote;
        handState.GrabbedObject = targetObject;
        _grabbedObjects.Add(targetObject);

        if (handState.GrabbingAnimationRoutine != null)
        {
            StopCoroutine(handState.GrabbingAnimationRoutine);
            handState.GrabbingAnimationRoutine = null;
        }

        if (remote)
        {
            handState.GrabbingAnimationRoutine = StartCoroutine(GrabbingAnimation(handState, targetObject));
            return;
        }

        AttachObjectToHand(handState, targetObject);
        handState.PrevPos = handState.HandTransform.position;
        handState.PrevRot = handState.HandTransform.rotation;
    }

    private void TryUngrab(HandGrabState handState)
    {
        if (handState.HandTransform == null)
        {
            ForceReleaseState(handState);
            return;
        }

        Vector3 throwDirection = handState.HandTransform.position - handState.PrevPos;
        handState.PrevPos = handState.HandTransform.position;
        
        // 쿼터니온 공식
        // angle1 = Q1, angle2 = Q2
        // angle1 + angle2 = Q1 * Q2
        // -angle2 = Quaternion.Inverse(Q2)
        // angle2 - angle1 = Quaternion.FromToRotation(Q1, Q2) = Q2 * Quaternion.Inverse(Q1)
        Quaternion deltaRotation = handState.HandTransform.rotation * Quaternion.Inverse(handState.PrevRot);
        handState.PrevRot = handState.HandTransform.rotation;
        
        
        
        if (ARAVRInput.GetUp(ARAVRInput.Button.HandTrigger, handState.Controller))
        {
            if (handState.GrabbingAnimationRoutine != null)
            {
                StopCoroutine(handState.GrabbingAnimationRoutine);
                handState.GrabbingAnimationRoutine = null;
            }

            GameObject releasedObject = handState.GrabbedObject;
            if (releasedObject == null)
            {
                handState.IsGrabbing = false;
                return;
            }

            handState.IsGrabbing = false;
            Rigidbody rb = releasedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            releasedObject.transform.parent = null;
            float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
            Vector3 throwVelocity = (throwDirection / deltaTime) * throwPower;
            if (handState.IsRemoteGrab)
            {
                throwVelocity *= remoteThrowMultiplier;
            }
            if (handState.IsRemoteGrab && throwVelocity.sqrMagnitude < minRemoteThrowSpeed * minRemoteThrowSpeed)
            {
                throwVelocity = handState.HandTransform.forward.normalized * minRemoteThrowSpeed;
            }
            throwVelocity = Vector3.ClampMagnitude(throwVelocity, maxThrowSpeed);
            if (rb != null)
            {
                rb.velocity = throwVelocity;
            }
            
            float angle;
            Vector3 axis;
            deltaRotation.ToAngleAxis(out angle, out axis);
            Vector3 angularVelocity = (1.0f / deltaTime) * angle * axis;
            if (rb != null)
            {
                rb.angularVelocity = angularVelocity;
            }

            if (releasedObject.layer == LayerMask.NameToLayer("Weapon"))
            {
                Weapon weapon = releasedObject.GetComponentInChildren<Weapon>();
                if (weapon != null)
                {
                    weapon.itemEffect.SetActive(true);
                    weapon.HideAmmoUIImmediate();
                }
            }

            if (releasedObject.layer == LayerMask.NameToLayer("Drone"))
            {
                StartCoroutine(ThrowDrone(releasedObject));
            }

            _grabbedObjects.Remove(releasedObject);
            handState.GrabbedObject = null;
            handState.IsRemoteGrab = false;
        }
    }

    IEnumerator ThrowDrone(GameObject releasedObject)
    {
        yield return new WaitForSeconds(0.3f);
        releasedObject.GetComponent<bomb>().isbomb = true;
    }

    IEnumerator GrabbingAnimation(HandGrabState handState, GameObject targetObject)
    {
        if (targetObject == null)
        {
            handState.GrabbingAnimationRoutine = null;
            yield break;
        }

        Rigidbody targetRb = targetObject.GetComponent<Rigidbody>();
        if (targetRb == null)
        {
            ForceReleaseState(handState);
            yield break;
        }

        targetRb.isKinematic = true;
        if (handState.HandTransform == null)
        {
            ForceReleaseState(handState);
            yield break;
        }

        handState.PrevPos = handState.HandTransform.position;
        handState.PrevRot = handState.HandTransform.rotation;
        Vector3 startLocation = targetObject.transform.position;
        Vector3 targetLocation = handState.HandTransform.position + handState.HandTransform.forward * 0.1f;

        float currentTime = 0f;
        float finishTime = 0.2f;
        float elapsedRate = currentTime / finishTime;
        targetObject.transform.SetParent(handState.HandTransform);
        while (elapsedRate < 1)
        {
            if (!handState.IsGrabbing || handState.GrabbedObject != targetObject)
            {
                handState.GrabbingAnimationRoutine = null;
                yield break;
            }

            currentTime += Time.deltaTime;
            elapsedRate = currentTime / finishTime;
            targetObject.transform.localPosition = Vector3.Lerp(targetObject.transform.localPosition, Vector3.zero, elapsedRate);
            targetObject.transform.localEulerAngles = Vector3.Lerp(targetObject.transform.localEulerAngles, new Vector3(0f, 90f, 0f), elapsedRate);
            yield return null;
        }

        if (!handState.IsGrabbing || handState.GrabbedObject != targetObject)
        {
            handState.GrabbingAnimationRoutine = null;
            yield break;
        }
        if (targetObject.layer == LayerMask.NameToLayer("Weapon"))
        {
            targetObject.transform.localPosition = Vector3.zero;
            targetObject.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
            Weapon weapon = targetObject.GetComponentInChildren<Weapon>();
            if (weapon != null)
            {
                Gun gun = GetComponent<Gun>();
                if (handState.Controller == ARAVRInput.Controller.RTouch)
                {
                    gun.rightDamge = weapon.damage;
                }
                else
                {
                    gun.leftDamge = weapon.damage;
                }
                weapon.itemEffect.SetActive(false);
                weapon.ShowAmmoUI();
            }
        }
        
        if (targetObject.layer == LayerMask.NameToLayer("Drone"))
        {
            targetObject.transform.localPosition = Vector3.zero;
            targetObject.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
            targetObject.GetComponent<Rigidbody>().isKinematic = true;
            targetObject.GetComponent<NavMeshAgent>().enabled = false;
        }
        
        
        else
        {
            while (elapsedRate < 1)
            {
                if (!handState.IsGrabbing || handState.GrabbedObject != targetObject)
                {
                    handState.GrabbingAnimationRoutine = null;
                    yield break;
                }

                if (handState.HandTransform == null)
                {
                    ForceReleaseState(handState);
                    yield break;
                }

                currentTime += Time.deltaTime;
                elapsedRate = currentTime / finishTime;
                targetLocation = handState.HandTransform.position + handState.HandTransform.forward * 0.1f;
                targetObject.transform.position = Vector3.Lerp(startLocation, targetLocation, elapsedRate);
                yield return null;
            }

            if (!handState.IsGrabbing || handState.GrabbedObject != targetObject)
            {
                handState.GrabbingAnimationRoutine = null;
                yield break;
            }

            targetObject.transform.position = targetLocation;
            targetObject.transform.parent = handState.HandTransform;
        }

        handState.GrabbingAnimationRoutine = null;
    }

    private void AttachObjectToHand(HandGrabState handState, GameObject targetObject)
    {
        targetObject.transform.parent = handState.HandTransform;

        Rigidbody rb = targetObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        if (targetObject.layer == LayerMask.NameToLayer("Weapon"))
        {
            targetObject.transform.localPosition = Vector3.zero;
            targetObject.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
            Weapon weapon = targetObject.GetComponentInChildren<Weapon>();
            if (weapon != null)
            {
                Gun gun = GetComponent<Gun>();
                if (handState.Controller == ARAVRInput.Controller.RTouch)
                {
                    gun.rightDamge = weapon.damage;
                }
                else
                {
                    gun.leftDamge = weapon.damage;
                }
                weapon.itemEffect.SetActive(false);
                weapon.ShowAmmoUI();
            }
        }
        
        if (targetObject.layer == LayerMask.NameToLayer("Drone"))
        {
            targetObject.transform.localPosition = Vector3.zero;
            targetObject.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
            targetObject.GetComponent<Rigidbody>().isKinematic = false;
            targetObject.GetComponent<NavMeshAgent>().enabled = false;
        }
    }

    private void ForceReleaseState(HandGrabState handState)
    {
        if (handState.GrabbedObject != null)
        {
            _grabbedObjects.Remove(handState.GrabbedObject);
        }

        handState.IsGrabbing = false;
        handState.IsRemoteGrab = false;
        handState.GrabbedObject = null;
        handState.GrabbingAnimationRoutine = null;
    }

    private void SyncLegacyFields()
    {
        isGrabbing = _rightHandState.IsGrabbing || _leftHandState.IsGrabbing;

        // 기존 코드 호환을 위해 오른손 우선으로 공개 참조를 유지한다.
        grabbedObject = _rightHandState.GrabbedObject != null
            ? _rightHandState.GrabbedObject
            : _leftHandState.GrabbedObject;
    }

    public GameObject GetGrabbedObject(ARAVRInput.Controller controller)
    {
        HandGrabState handState = GetHandState(controller);
        return handState != null ? handState.GrabbedObject : null;
    }

    public bool IsHandGrabbing(ARAVRInput.Controller controller)
    {
        return GetGrabbedObject(controller) != null;
    }

    public bool IsObjectGrabbed(GameObject targetObject)
    {
        return targetObject != null && _grabbedObjects.Contains(targetObject);
    }

    private HandGrabState GetHandState(ARAVRInput.Controller controller)
    {
        if (controller == ARAVRInput.Controller.RTouch)
        {
            return _rightHandState;
        }

        return _leftHandState;
    }

    private Transform GetHandTransform(ARAVRInput.Controller controller)
    {
        if (controller == ARAVRInput.Controller.RTouch)
        {
            return ARAVRInput.RHand;
        }

        return ARAVRInput.LHand;
    }
}
