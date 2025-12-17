using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float delay = 2.0f; // 2초 뒤에 삭제

    void Start()
    {
        // 이 오브젝트를 delay 시간 뒤에 파괴하라
        Destroy(gameObject, delay);
    }
}