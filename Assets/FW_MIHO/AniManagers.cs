using UnityEngine;

namespace FreewrokGame
{
    public enum targetDirectType
    {
        forward = 0,
        backward = 1
    }
    public enum AniType
    {
        idle = 0,
        walk = 1,
        run = 2,
        win = 3,
        lose = 4
    }

    public class PlayerMovementAndAnimation : MonoBehaviour
    {
        public static PlayerMovementAndAnimation Instance;

        // === [ Joystick 추가 ] ===
        [Header("Joystick Settings")]
        public Joystick joystick;

        // === [ Animation Settings ] ===
        [Header("Animation Settings")]
        public Animator[] targetAnimators; // 0- forward, 1- backward

        private float speedWalk = 2.5f;
        private float speedRun = 7f;

        // 360도 이동을 위해 고정 Step 변수(moveXstep, moveYstep)는 제거했습니다.

        private targetDirectType currentDirectType = targetDirectType.forward;
        private bool isRun = false;
        private bool isPose = false;

        // === [ Scale 저장용 ] ===
        private Vector3 initialScale;

        // === [ Movement Settings ] ===
        [Header("Movement Settings")]
        public Rigidbody2D rb;

        private Vector2 currentMovementInput = Vector2.zero;
        private float currentMoveSpeed;

        void Start()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    enabled = false;
                    return;
                }
            }

            rb.gravityScale = 0f;

            initialScale = transform.localScale;
            if (initialScale.x == 0) initialScale = new Vector3(1, 1, 1);

            Restart();
        }

        public float getSpeedWalk()
        {
            return speedWalk;
        }

        public void setSpeedWalk(float val)
        {
            this.speedWalk += val;
        }

        void Restart()
        {
            SetAni(AniType.idle);

            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                currentDirectType = targetDirectType.forward;
                SetAni(AniType.idle);

                isPose = false;
                isRun = false;

                rb.position = new Vector2(11, -8);
                rb.velocity = Vector2.zero;

                ApplyScale(true);
            }
        }

        private void Update()
        {
            Restart();
            HandleInputAndAnimation();
            Pose();
        }

        private void FixedUpdate()
        {
            MoveCharacter();
        }

        // [핵심 수정 부분] 360도 입력 처리 및 애니메이션 분기
        private void HandleInputAndAnimation()
        {
            bool isMove = false;

            // 1. 입력 받기 (키보드 + 조이스틱 통합)
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            if (joystick != null)
            {
                // 키보드 입력이 없을 때만 조이스틱 사용 (중복 입력 방지)
                if (h == 0 && v == 0)
                {
                    h = joystick.Horizontal;
                    v = joystick.Vertical;
                }
            }

            // 2. 입력 벡터 생성 (360도 방향)
            Vector2 inputVector = new Vector2(h, v);

            // 애니메이션 초기화
            if (!isPose) SetAni(AniType.idle);

            // 달리기 모드 체크
            if (Input.GetKey(KeyCode.Alpha1)) isRun = false;
            if (Input.GetKey(KeyCode.Alpha2)) isRun = true;

            currentMovementInput = Vector2.zero;

            // 3. 이동 중인지 확인 (입력값이 조금이라도 있으면)
            if (inputVector.sqrMagnitude > 0.01f)
            {
                if (isPose) targetAnimators[(int)currentDirectType].gameObject.SetActive(false);
                isPose = false;
                isMove = true;

                // [방향 전환 로직]
                // Y축 입력이 위쪽(+)이면 Backward(뒷모습), 아래쪽(-)이면 Forward(앞모습)
                // 입력이 미미하면 기존 방향 유지
                if (v > 0.1f) currentDirectType = targetDirectType.backward;
                else if (v < -0.1f) currentDirectType = targetDirectType.forward;

                // X축 입력에 따라 좌우 반전 (Scale)
                // 왼쪽(< 0)이면 정방향(Scale 1), 오른쪽(> 0)이면 반전(Scale -1)
                if (h < -0.1f) ApplyScale(true);
                else if (h > 0.1f) ApplyScale(false);

                // [속도 벡터 설정] 정규화(Normalize)하여 모든 방향 속도 일정하게 유지
                currentMovementInput = inputVector.normalized;
            }

            // 4. 속도 적용
            if (isMove)
            {
                if (isRun)
                {
                    currentMoveSpeed = speedRun;
                    SetAni(AniType.run);
                }
                else
                {
                    currentMoveSpeed = speedWalk;
                    SetAni(AniType.walk);
                }
            }
            else
            {
                currentMoveSpeed = 0;
            }
        }

        private void MoveCharacter()
        {
            if (currentMovementInput != Vector2.zero && !isPose)
            {
                rb.velocity = currentMovementInput * currentMoveSpeed;
            }
            else
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        void Pose()
        {
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                isPose = true;
                currentDirectType = targetDirectType.forward;
                SetAni(AniType.win);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                isPose = true;
                currentDirectType = targetDirectType.forward;
                SetAni(AniType.lose);
            }
        }

        void SetAni(AniType aType)
        {
            if (targetAnimators.Length < 2) return;

            int currentTypeIndex = (int)currentDirectType;
            int otherTypeIndex = 1 - currentTypeIndex;

            if (!targetAnimators[currentTypeIndex].gameObject.activeSelf)
            {
                targetAnimators[currentTypeIndex].gameObject.SetActive(true);
                targetAnimators[otherTypeIndex].gameObject.SetActive(false);
            }

            targetAnimators[currentTypeIndex].SetInteger("aniInt", (int)aType);
        }

        void ApplyScale(bool isNormal)
        {
            float absX = Mathf.Abs(initialScale.x);

            if (isNormal) // 왼쪽 이동 (정방향)
            {
                this.gameObject.transform.localScale = new Vector3(absX, initialScale.y, initialScale.z);
            }
            else // 오른쪽 이동 (반전)
            {
                this.gameObject.transform.localScale = new Vector3(-absX, initialScale.y, initialScale.z);
            }
        }
    }
}