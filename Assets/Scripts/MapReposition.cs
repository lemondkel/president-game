using UnityEngine;

public class MapReposition : MonoBehaviour
{
    private Transform playerTransform;
    private float mapWidth;
    private float mapHeight;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        mapWidth = sr.bounds.size.x;
        mapHeight = sr.bounds.size.y;
    }

    void Update()
    {
        if (playerTransform == null) return;

        // 1. 현재 내 위치와 플레이어 위치의 거리 차이
        float diffX = playerTransform.position.x - transform.position.x;
        float diffY = playerTransform.position.y - transform.position.y;

        // 2. 방향 벡터 (플레이어가 어느 쪽에 있는지)
        float dirX = diffX > 0 ? 1 : -1;
        float dirY = diffY > 0 ? 1 : -1;

        // 3. 임계치 확인 (맵 크기만큼 벌어졌는가?)
        // 기존 1.5배 로직 대신, 절댓값 차이가 mapWidth를 넘어서면 즉시 이동
        if (Mathf.Abs(diffX) >= mapWidth)
        {
            // [핵심 변경] Translate(이동) 대신 position(좌표)을 재설정
            // "내 현재 위치" + "방향 * 맵너비 * 2" (반대편 끝이 아니라 바로 옆으로 붙이기 위해 로직 수정 가능하지만)
            // 가장 깔끔한 건 3x3 구조에서 "두 칸" 점프입니다.
            transform.Translate(Vector3.right * dirX * mapWidth * 3);
        }

        if (Mathf.Abs(diffY) >= mapHeight)
        {
            transform.Translate(Vector3.up * dirY * mapHeight * 3);
        }
    }
}