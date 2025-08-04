using UnityEngine;
using Tower.Game;
using Tower.Player.Data;

namespace Tower.Player
{
    public class Character : MonoBehaviour, IDamageable
    {
        #region Variables
        public CharacterBaseSO characterBase;
        public GameObject fairyForm;
        private GameObject fgo;

        private float currentHP;
        private float maxHP;
        private float currentMP;
        private float maxMP;
        #endregion

        #region Property
        public bool IsDead => currentHP <= 0;
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            UpdateStats();
        }

        private void OnEnable()
        {
            if (fgo != null)
            {
                Destroy(fgo);
            }
        }

        private void OnDisable()
        {
            fgo = Instantiate(fairyForm, new Vector3(transform.position.x, 2f, transform.position.z), Quaternion.identity);
        }
        #endregion

        #region Custom Method
        public void TakeDamage(float damage)
        {
            if (IsDead) return;

            currentHP = Mathf.Max(currentHP - damage, 0);
            if (IsDead)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("사망");
            //Destroy(gameObject); 사망처리
        }

        public int GetHPForUI()
        {
            return Mathf.CeilToInt(currentHP);
        }

        public void UpdateStats()
        {
            maxHP = characterBase.maxHp;
            currentHP = maxHP;

            maxMP = characterBase.maxMp;
            currentMP = maxMP;
        }
        #endregion


    }

}
