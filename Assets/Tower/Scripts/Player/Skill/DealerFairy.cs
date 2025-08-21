using System.Collections;
using Tower.Player.Data;
using UnityEngine;
using EnemyClass = Tower.Enemy.Enemy;

namespace Tower.Player
{
    public class DealerFairy : MonoBehaviour
    {
        #region Variables
        [SerializeField] private float interval = 1f;
        [SerializeField] private CharacterBaseSO dealerBase;    //데이터 참조
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private float hitPointOffset = 1f;
        [SerializeField] private Projectile[] projectiles;
        [SerializeField] private float damageMultiplier = 0.25f;

        private EnemyDetector detector;
        #endregion

        #region Property
        private float Damage => dealerBase.atk * damageMultiplier;
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            //참조
            detector = GetComponent<EnemyDetector>();

            //투사체 초기셋팅 : 비활성화
            foreach  (Projectile projectile in projectiles)
            {
                projectile.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            InvokeRepeating("SpitFire", 1f, interval);
        }
        #endregion

        #region Custom Method
        private void SpitFire()
        {
            //Debug.Log("히히 발싸 1");
            if (detector.detectedEnemies.Count == 0) return;

            //Debug.Log("히히 발싸 2");

            foreach (Projectile projectile in projectiles)
            { 
                if (!projectile.gameObject.activeSelf)
                {
                    StartCoroutine(LaunchProjectile(projectile, GetClosestEnemy()));
                    break;
                }
            }

        }

        private IEnumerator LaunchProjectile(Projectile projectile, EnemyClass target)
        {
            //Debug.Log("호잇짜!");
            Vector3 startPos = transform.position;
            if (startPos.y < 0.2f) // 너무 낮으면 살짝 올려줌
                startPos.y = 0.2f;

            projectile.transform.position = startPos;
            projectile.gameObject.SetActive(true);

            //투사체 셋팅
            projectile.SetTarget(target, Damage, 0);

            //투사체 이동
            while (projectile.gameObject.activeSelf)
            {
                //날아가는 도중에 타겟이 죽으면 즉시 오브젝트 비활성화 후 반복문 종료
                if (target == null || target.IsDead)
                {
                    projectile.gameObject.SetActive(false);
                    yield break;
                }

                //타격 부위 보정
                //Vector3 targetPosition = new Vector3(target.transform.position.x, 1f, target.transform.position.z);
                Vector3 targetPosition = target.transform.position + Vector3.up * hitPointOffset;
                Vector3 dir = (targetPosition - projectile.transform.position).normalized;
                projectile.transform.Translate(dir * projectileSpeed * Time.deltaTime);

                yield return null;
            }
        }

        private EnemyClass GetClosestEnemy()
        {
            EnemyClass closest = null;
            float closestDist = Mathf.Infinity;

            foreach (var enemy in detector.detectedEnemies)
            {
                if (enemy == null) continue;
                float dist = (transform.position - enemy.transform.position).sqrMagnitude;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = enemy;
                }
            }
            return closest;
        }
        #endregion
    }

}
