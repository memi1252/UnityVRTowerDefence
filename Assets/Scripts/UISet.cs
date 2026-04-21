using UnityEngine;

public class UISet : MonoBehaviour
{
    // 왼손 자식으로 붙일 때의 로컬 오프셋
    public Vector3 localOffset = new Vector3(0.02f, 0.03f, 0.12f);

    // 보기 편하게 카메라를 향하게 할지 여부
    public bool faceCamera = true;

    private Transform _cameraTransform;

    void Start()
    {
        Transform leftHandTransform = ARAVRInput.LHand;

        if (leftHandTransform == null)
        {
            Debug.LogWarning("왼손 컨트롤러를 찾을 수 없습니다!");
            return;
        }

        // 왼손의 자식으로 붙여 손과 함께 움직이도록 한다.
        transform.SetParent(leftHandTransform, false);
        transform.localPosition = localOffset;

        _cameraTransform = Camera.main != null ? Camera.main.transform : null;
        if (_cameraTransform == null)
        {
            Debug.LogWarning("메인 카메라를 찾을 수 없습니다!");
        }
        else if (faceCamera)
        {
            Vector3 directionToCamera = _cameraTransform.position - transform.position;
            if (directionToCamera.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
            }
        }
    }
}


