using UnityEngine;
using UnityEngine.UI;
using Tower.Player;
using Unity.Cinemachine;
using Tower.Game.Bless;

namespace Tower.Game.Bless
{
    public class ShopInteraction : MonoBehaviour
    {
        #region Variables
        public GameObject shopUI;
        public float interactDistance = 3f;

        private bool isShopOpen = false;

        private Character activeCharacter;
        [SerializeField] private GameObject inputManager;
        [SerializeField] private CinemachineCamera cameraController;
        [SerializeField] private StoreAreaCheck storeAreaCheck;
        [SerializeField] private GameObject mainCam;
        [SerializeField] private GameObject storeCam;
        [SerializeField] private BlessManager blessManager;

        #endregion

        #region Unity Event Method
        private void Start()
        {
            storeAreaCheck = GetComponentInChildren<StoreAreaCheck>();
        }

        void Update()
        {
            activeCharacter = TeamManager.Instance.characters[TeamManager.Instance.CurrentIndex];

            // float distance = Vector3.Distance(activeCharacter.transform.position, transform.position);

            // //플레이어와 God 사이의 거리
            // isPlayerInRange = distance <= interactDistance;

            if (storeAreaCheck.IsPlayerInRange && Input.GetKeyDown(KeyCode.F))
            {
                ShopOpen();
            }

            if (isShopOpen && (Input.GetKeyDown(KeyCode.T) /*   */))
            {
                ShopClose();
            }
        }
        #endregion

        #region Custom Method
        public void ShopOpen()
        {
            PlayerController pc = activeCharacter.gameObject.GetComponent<PlayerController>();
            Animator anim = activeCharacter.gameObject.GetComponent<Animator>();
            anim.SetBool(AnimHash.isMoving, false);
            pc.enabled = false;
            isShopOpen = true;
            shopUI.SetActive(true);
            inputManager.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            mainCam.SetActive(false);
            storeCam.SetActive(true);

            Vector3 targetPos = storeCam.transform.position;
            // 높이(Y) 값은 캐릭터의 현재 높이로 고정
            targetPos.y = activeCharacter.transform.position.y;
            // 바라보게 회전
            activeCharacter.transform.LookAt(targetPos);

            var axisController = cameraController.GetComponent<CinemachineInputAxisController>();
            axisController.enabled = false;
        }

        public void ShopClose()
        {
            if (blessManager.NowBlessing == false)
            {
                PlayerController pc = activeCharacter.gameObject.GetComponent<PlayerController>();
                pc.enabled = true;
                isShopOpen = false;
                shopUI.SetActive(false);
                inputManager.SetActive(true);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                mainCam.SetActive(true);
                storeCam.SetActive(false);

                var axisController = cameraController.GetComponent<CinemachineInputAxisController>();
                axisController.enabled = true;
            }
        }
        #endregion
    }
}