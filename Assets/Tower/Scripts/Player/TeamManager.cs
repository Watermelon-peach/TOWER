using UnityEngine;
using Tower.Util;
using Unity.Cinemachine;

namespace Tower.Player
{
    /// <summary>
    /// 파티 관리 (스위칭, 버프, 마나회복 등)
    /// </summary>
    public class TeamManager : Singleton<TeamManager>
    {
        #region Variables
        public Character[] characters = new Character[3];

        private CameraController cam;

        private int currentIndex;
        #endregion

        #region Property
        public int CurrentIndex => currentIndex;
        #endregion

        #region Unity Event Method
        protected override void Awake()
        {
            base.Awake();
            cam = GetComponent<CameraController>();

        
            //Debug.Log("Awake");
        }

        private void Start()
        {
            currentIndex = 0; // 첫 캐릭터 선택
            SelectCharacter(currentIndex);
        }

        private void Update()
        {
            //스위칭
            if (InputManager.Instance.SwapPressed)
            {
                Debug.Log("SPACE");
                //등장스킬 발동 여부 전달
                /*if(IsParrying)
                {

                }*/
                currentIndex++;
                if (currentIndex > 2)
                {
                    currentIndex = 0;
                }
                
                SelectCharacter(currentIndex);
            }
        }
        #endregion

        #region Custom Method
        public void SelectCharacter(int index)
        {
            //선택한 캐릭터만 활성화, 나머지 비활성화
            for (int i = 0; i < characters.Length; i++)
            {
                characters[i].gameObject.SetActive(i==index);
            }

            cam.LookAtCharacter(index);
            Debug.Log($"{index}번 캐릭터 등장");
        }
        #endregion
    }

}
