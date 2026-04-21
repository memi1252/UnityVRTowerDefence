using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    // 이동 속도
    public float speed = 5;
    // 회전 속도
    public float turnSpeed = 90f;
    // 썸스틱 데드존
    public float stickDeadzone = 0.2f;
    // CharacterController 컴포넌트
    private CharacterController _cc;
    // 중력 가속도의 크기
    public float gravity = -20;
    // 수직 속도
    private float _yVelocity;
    // 점프 크기
    public float jumpPower = 5f;

    private void Start()
    {
        _cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 사용자의 입력에 따라 전후좌우로 이동하고 싶다.
        // 1. 사용자의 입력을 받는다.
        Vector2 leftStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
        float h = leftStick.x;
        float v = leftStick.y;
        float x = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).x;

        // 1.5 오른쪽 썸스틱으로 캐릭터를 회전시킨다.
        if (Mathf.Abs(x) > stickDeadzone)
        {
            transform.Rotate(0f, x * turnSpeed * Time.deltaTime, 0f);
        }
        
        
        // 2. 방향을 만든다.
        Vector3 dir = new Vector3(h, 0, v);
        dir.Normalize();
        
        // 2.0 사용자가 바라보는 방향으로 입력 값 변화시키기
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }
        dir = mainCamera.transform.TransformDirection(dir);
        
        // 2.1 중력을 적용한 수직 방향 추가 V=v0+at
        _yVelocity += gravity * Time.deltaTime;
        
        // 2.2 바닥에 있을 경우, 수직 향력을 처리하기 위해 속도를 0으로 한다.
        if (_cc.isGrounded)
        {
            _yVelocity = 0;
        }
        
        // 2.3 사용자가 점프 버튼을 누르면 속도에 점프 크기를 할당한다.
        if (ARAVRInput.GetDown(ARAVRInput.Button.Two, ARAVRInput.Controller.RTouch))
        {
            _yVelocity = jumpPower;
        }
        dir.y = _yVelocity;
        // 3. 이동한다.
        _cc.Move(dir * (speed * Time.deltaTime));
        
    }
}
