using System.Collections;
using UnityEngine;
using EnemyClass = Tower.Enemy.Enemy;

namespace Tower.Player
{
    public class TankerFairy : MonoBehaviour
    {
        #region Variables
        [SerializeField] private Transform gravityPoint;

        [SerializeField] private float duration = 1.5f;
        [SerializeField] private float interval = 2f;
        [SerializeField] private Vector3 offset = new Vector3(0f,0.5f,0f);

        private EnemyDetector detector;
        private ParticleSystem gravityVfx;
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            //참조
            detector = GetComponent<EnemyDetector>();
            gravityVfx = gravityPoint.gameObject.GetComponent<ParticleSystem>();
        }
        private void Start()
        {
            InvokeRepeating("Gravitation", 1f, interval);
        }
        #endregion

        #region Custom Method
        private void Gravitation()
        {
            gravityVfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            gravityVfx.Play();
            // detectedEnemies 리스트 순회
            foreach (EnemyClass enemy in detector.detectedEnemies)
            {
                //Enemy 클래스의 KnockBack() 활용하기 -> 어색해서 Drag 새로 만듬
                //Vector3 dir = (gravityPoint.position - enemy.transform.position).normalized;
                //StartCoroutine(enemy.Knockback(dir, duration));
                if (enemy == null || enemy.IsDead)
                    continue;

                StartCoroutine(enemy.Drag(duration, gravityPoint.position + offset));
            }
        }
        #endregion
    }

}
