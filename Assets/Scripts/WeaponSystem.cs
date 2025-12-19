using UnityEngine;

// 이 컴포넌트가 붙으면 AudioSource도 같이 붙도록 강제 (실수 방지)
[RequireComponent(typeof(AudioSource))]
public class WeaponSystem : MonoBehaviour
{
    [Header("References")]
    public Transform projectileParent;
    public GameObject projectilePrefab;

    [Header("Stats")]
    public float damage = 10f;
    public float speed = 15f;
    public float attackRange = 10f;
    public float fireRate = 0.5f;

    [Header("Audio")]
    public AudioClip fireClip; // 인스펙터에서 쓩~ 소리 파일 연결
    [Range(0f, 1f)] public float soundVolume = 0.5f; // 볼륨 조절
    public bool usePitchRandom = true; // 기계적인 소리 방지용

    private float timer;
    private int enemyLayerMask;
    private AudioSource audioSource; // 소리 재생기

    void Start()
    {
        enemyLayerMask = 1 << LayerMask.NameToLayer("Enemy");

        // 오디오 소스 가져오기
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= fireRate)
        {
            if (FireAtNearestEnemy())
            {
                timer = 0f;
            }
        }
    }

    bool FireAtNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayerMask);

        if (hits.Length == 0) return false;

        Transform nearestTarget = null;
        float minDistanceSqr = float.MaxValue;
        Vector3 currentPos = transform.position;

        foreach (Collider2D hit in hits)
        {
            if (!hit.gameObject.activeInHierarchy) continue;

            Vector3 diff = hit.transform.position - currentPos;
            float distSqr = diff.sqrMagnitude;

            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                nearestTarget = hit.transform;
            }
        }

        if (nearestTarget != null)
        {
            Vector3 targetDir = nearestTarget.position - currentPos;
            SpawnProjectile(targetDir);

            // ★ 발사 성공 시 사운드 재생
            PlayFireSound();

            return true;
        }

        return false;
    }

    void SpawnProjectile(Vector3 direction)
    {
        if (projectilePrefab == null) return;

        GameObject bullet = Instantiate(projectilePrefab, transform.position, Quaternion.identity, projectileParent);

        Projectile projScript = bullet.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.Initialize(direction, speed);
        }
    }

    // ★ 사운드 재생 함수 추가
    void PlayFireSound()
    {
        if (fireClip == null || audioSource == null) return;

        // [타격감 팁] 피치(음정)를 살짝 랜덤하게 주면 기관총 소리가 덜 기계적으로 들림
        if (usePitchRandom)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
        }
        else
        {
            audioSource.pitch = 1f;
        }

        // PlayOneShot: 소리가 겹쳐도 끊기지 않고 위에 덮어씌워 재생됨 (기관총에 필수)
        audioSource.PlayOneShot(fireClip, soundVolume);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}