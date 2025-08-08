using System.Collections;
using Tower.Player;
using UnityEngine;

namespace JeaYoon.Test.ScriScript
{
    public class SafetyZone : MonoBehaviour
    {
        [Header("VFX")]
        [SerializeField] private float vfxDuration = 0f;  //vfx 실행 시간
        //[SerializeField] private GameObject vfx;

        private Character[] party;
        private void Awake()
        {
            party = TeamManager.Instance.characters;
        }
        private void OnTriggerEnter(Collider other)
        {
            StartCoroutine(RevibeAll());
        }

        private IEnumerator RevibeAll()
        {
            for (int i = 0; i < party.Length; i++)
            {
                party[i].Revibe();
            }
            //vfx 실행
            //vfx.SetActive(true);
            //...
            yield return new WaitForSeconds(vfxDuration);
            //vfx.SetActive(false);
            //트리거 해제
            gameObject.SetActive(false);
        }
    }
}

