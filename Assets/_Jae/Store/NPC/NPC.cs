using UnityEngine;
using UnityEngine.UI;

namespace Tower.Gmae.Bless
{
    public class ShopInteraction : MonoBehaviour 
    {
        public GameObject shopUI;           // ���� UI ������Ʈ
        public Transform player;            // �÷��̾� Ʈ������
        public float interactDistance = 3f; // ��ȣ�ۿ� �Ÿ�

        private bool isShopOpen = false;
        private bool isPlayerInRange = false;

        void Update() 
        {
            float distance = Vector3.Distance(player.position, transform.position);
        
            //�÷��̾�� NPC ��ȣ�ۿ� ���� �˻�
            isPlayerInRange = distance <= interactDistance;

            if (isPlayerInRange && Input.GetKeyDown(KeyCode.F)) 
            {
                ShopOpen();
            }

            if (isShopOpen && (Input.GetKeyDown(KeyCode.Escape) /* ������ ���� �ٸ� ���� ��ư Ŭ�� */)) 
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