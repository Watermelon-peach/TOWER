using UnityEngine;

namespace Wilson.Game.Puzzle
{
    public class FirePick : MonoBehaviour
    {
        #region Variables
        public GameObject torchObject;             // FireTourch 전체 오브젝트
        
        public GameObject playerSettingsObject;    // PlayerSettings 오브젝트 (플레이어 꺼주기용)
        #endregion

        #region IInteractable Implementation
        public void Interact()
        {
            if (torchObject != null)
                torchObject.SetActive(true);            // FireTourch 오브젝트 활성화

            if (playerSettingsObject != null)
                playerSettingsObject.SetActive(false);  // 플레이어 비활성화

           

            gameObject.SetActive(false); // 자신 비활성화 (이벤트 진행 중에는 비활성)
        }

        public string GetPrompt() => "E - 횃불 점화"; // UI에 표시될 안내 텍스트
        #endregion

        #region Callback
        private void OnTorchComplete()
        {
            // 이벤트 종료 후 실행됨 (FireTourch에서 Invoke함)
        
            // FirePick은 FireTourch 내부에서 다시 SetActive(true) 처리됨
        }
        #endregion
    }
}
