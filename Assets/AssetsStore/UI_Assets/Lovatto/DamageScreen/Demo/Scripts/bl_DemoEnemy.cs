using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Lovatto.DamageScreen.Demo
{
    public class bl_DemoEnemy : MonoBehaviour
    {
        [SerializeField] private Transform target = null;
        [SerializeField] private Animator animator = null;
        [SerializeField] private GameObject fireballPrefab = null;
        [SerializeField] private Transform firePoint = null;
        [SerializeField] private float turnDuration = 0.2f;
        [SerializeField] private string attackStateName = "attack";
        [SerializeField] private float attackTransitionDuration = 0.2f;
        [SerializeField] private int attackLayer = 0;
        [SerializeField] private int burstFireballCount = 3;
        [SerializeField] private float burstAnimationSpeedMultiplier = 3f;

        private bool isAttacking;
        private int attackStateHash;
        private int pendingFireballCount;
        private float cachedAnimatorSpeed = 1f;
        private Coroutine attackCoroutine;
        private Coroutine rotateCoroutine;

        private void Awake()
        {
            attackStateHash = Animator.StringToHash(attackStateName);
        }

        void Update()
        {
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                Attack();
            }

            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                BurstAttack();
            }
        }

        private void Attack()
        {
            Attack(1, 1f);
        }

        private void BurstAttack()
        {
            Attack(burstFireballCount, burstAnimationSpeedMultiplier);
        }

        private void Attack(int fireballCount, float animationSpeedMultiplier)
        {
            if (isAttacking || animator == null)
            {
                return;
            }

            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
            }

            pendingFireballCount = Mathf.Max(1, fireballCount);
            cachedAnimatorSpeed = animator.speed;
            animator.speed = cachedAnimatorSpeed * Mathf.Max(0.01f, animationSpeedMultiplier);

            attackCoroutine = StartCoroutine(AttackRoutine());
            //fireball spawn is called by animation event
        }

        public void SpawnFireball()
        {
            if (fireballPrefab != null && firePoint != null)
            {
                Quaternion spawnRotation = firePoint.rotation;

                if (target != null)
                {
                    Vector3 toTarget = target.position - firePoint.position;
                    if (toTarget.sqrMagnitude > 0.0001f)
                    {
                        spawnRotation = Quaternion.LookRotation(toTarget.normalized);
                    }
                }

                Instantiate(fireballPrefab, firePoint.position, spawnRotation);

                if (isAttacking)
                {
                    pendingFireballCount = Mathf.Max(0, pendingFireballCount - 1);

                    if (pendingFireballCount > 0 && animator != null)
                    {
                        // Restart attack animation for the next projectile in the burst.
                        animator.Play(attackStateHash, attackLayer, 0f);
                    }
                }
            }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            //head look at target
            if (target != null && animator != null)
            {
                animator.SetLookAtWeight(1, 0.7f, 0.9f, 1, 0.5f);
                animator.SetLookAtPosition(target.position);
            }
        }

        private IEnumerator AttackRoutine()
        {
            isAttacking = true;
            animator.CrossFade(attackStateName, attackTransitionDuration, attackLayer);

            if (rotateCoroutine != null)
            {
                StopCoroutine(rotateCoroutine);
            }

            rotateCoroutine = StartCoroutine(SmoothRotateToTarget(turnDuration));

            float enterTimeout = 1f;
            while (enterTimeout > 0f)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(attackLayer);
                if (stateInfo.shortNameHash == attackStateHash)
                {
                    break;
                }

                enterTimeout -= Time.deltaTime;
                yield return null;
            }

            float attackTimeout = Mathf.Max(2f, pendingFireballCount * 2f);
            while (true)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(attackLayer);
                bool inAttackState = stateInfo.shortNameHash == attackStateHash;

                if (!inAttackState && !animator.IsInTransition(attackLayer))
                {
                    if (pendingFireballCount <= 0)
                    {
                        break;
                    }

                    animator.CrossFade(attackStateName, attackTransitionDuration, attackLayer);
                    yield return null;
                    continue;
                }

                if (inAttackState && pendingFireballCount <= 0 && stateInfo.normalizedTime >= 1f && !animator.IsInTransition(attackLayer))
                {
                    break;
                }

                attackTimeout -= Time.deltaTime;
                if (attackTimeout <= 0f)
                {
                    break;
                }

                yield return null;
            }

            isAttacking = false;
            pendingFireballCount = 0;
            animator.speed = cachedAnimatorSpeed;
            attackCoroutine = null;
        }

        private IEnumerator SmoothRotateToTarget(float duration)
        {
            if (target == null)
            {
                rotateCoroutine = null;
                yield break;
            }

            Vector3 direction = target.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                rotateCoroutine = null;
                yield break;
            }

            Quaternion startRotation = transform.rotation;
            Quaternion desiredRotation = Quaternion.LookRotation(direction.normalized);

            if (duration <= 0f)
            {
                transform.rotation = desiredRotation;
                rotateCoroutine = null;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.rotation = Quaternion.Slerp(startRotation, desiredRotation, t);
                yield return null;
            }

            transform.rotation = desiredRotation;

            rotateCoroutine = null;
        }
    }
}