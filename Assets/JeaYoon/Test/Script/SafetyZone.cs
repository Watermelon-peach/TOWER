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
            //party = TeamManager.Instance.characters;
        }
        private void OnTriggerEnter(Collider other)
        {
            StartCoroutine(RevibeAll());
        }

        private IEnumerator RevibeAll()
        {
            // 비활성화된 오브젝트도 포함해서 찾기
            Tower.Player.Character[] allCharacters = FindObjectsOfType<Tower.Player.Character>(true);
            foreach (Tower.Player.Character character in allCharacters)
            {
                if (character != null)
                {
                    character.Revibe();
                    Debug.Log($"Revived: {character.gameObject.name}");
                }
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

