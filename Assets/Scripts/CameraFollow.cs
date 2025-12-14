using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("카메라가 따라다닐 플레이어 GameObject를 여기에 연결하세요.")]
    public Transform target; // 따라다닐 대상 (플레이어)

    [Header("Speed Settings")]
    [Tooltip("카메라가 대상을 따라가는 속도입니다. 값이 높을수록 즉각적으로 따라갑니다.")]
    public float smoothSpeed = 0.125f; // 부드럽게 따라가는 속도

    // FixedUpdate 대신 LateUpdate를 사용하는 것이 좋습니다. 
    // 모든 Update() 호출(플레이어 이동) 후 카메라를 이동시켜 떨림을 방지합니다.
    // CameraFollow.cs 의 LateUpdate 함수
    void LateUpdate()
    {
        if (target == null) return;

        // 핵심: 플레이어의 X, Y 위치를 목표로 설정하되,
        // 카메라의 Z 위치는 스크립트가 붙어있는 카메라 자신의 현재 Z값을 유지합니다.
        Vector3 desiredPosition = new Vector3(target.position.x,
                                              target.position.y,
                                              transform.position.z); // Z값 유지!

        // 즉시 위치 적용
        transform.position = desiredPosition;
    }
}