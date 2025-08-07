using UnityEngine;
using UnityEngine.UI;

namespace Tower.Game.Bless
{
    public class ShopInteraction : MonoBehaviour 
    {
        public GameObject shopUI;           // 상점 UI 오브젝트
        public Transform player;            // 플레이어 트랜스폼
        public float interactDistance = 3f; // 상호작용 거리

        private bool isShopOpen = false;
        private bool isPlayerInRange = false;

        void Update() 
        {
            float distance = Vector3.Distance(player.position, transform.position);
        
            //플레이어와 NPC 상호작용 여부 검사
            isPlayerInRange = distance <= interactDistance;

            if (isPlayerInRange && Input.GetKeyDown(KeyCode.F)) 
            {
                ShopOpen();
            }

            if (isShopOpen && (Input.GetKeyDown(KeyCode.Escape) /* 상점을 닫을 다른 조건 버튼 클릭 */)) 
            {
                ShopClose();
            }
        }

        public void ShopOpen()
        {
                isShopOpen = true;
                shopUI.SetActive(true);
        }

        public void ShopClose() 
        {
            isShopOpen = false;
            shopUI.SetActive(false);
        }
    }
}