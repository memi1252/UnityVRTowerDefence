using UnityEngine;

public class CamRotate : MonoBehaviour
{
    // ���� ����
    Vector3 angle;
    // ���콺 ����
    public float sensitivity = 200;

    void Start()
    {
        // ������ �� ���� ī�޶��� ������ �����Ѵ�.
        angle = Camera.main.transform.eulerAngles;
        angle.x *= -1;
    }

    void Update()
    {
        // ���콺 �Է¿� ���� ī�޶� ȸ����Ű�� �ʹ�.
        // 1. ������� ���콺 �Է��� ���;� �Ѵ�.
        // ���콺�� �¿� �Է��� �޴´�.
        float x = Input.GetAxis("Mouse Y");
        float y = Input.GetAxis("Mouse X");
        // 2. ������ �ʿ��ϴ�.
        // �̵� ���Ŀ� ������ �� �Ӽ����� ȸ�� ���� ������Ų��.
        angle.x += x * sensitivity * Time.deltaTime;
        angle.y += y * sensitivity * Time.deltaTime;

        angle.x = Mathf.Clamp(angle.x, -90, 90);
        // 3. ȸ����Ű�� �ʹ�.
        // ī�޶��� ȸ�� ���� ���� ������� ȸ�� ���� �Ҵ��Ѵ�.
        transform.eulerAngles = new Vector3(-angle.x, angle.y, transform.eulerAngles.z);
    }
}
