using UnityEngine;
using Tower.Util;
using Tower.UI;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace Tower.Player
{
    /// <summary>
    /// 파티 관리 (스위칭, 버프, 마나회복 등)
    /// </summary>
    public class TeamManager : Singleton<TeamManager>
    {
        #region Variables
        public Character[] characters = new Character[3];

        private CharacterController[] controllers = new CharacterController[3];

        private CameraController cam;

        private int currentIndex;

        private Vector3[] positionOffsets;
        private Quaternion[] rotationOffsets;

        [Header("교체 쿨")]
        [SerializeField] private float coolTime = 3f;
        [SerializeField] private Image switchIcon;
        [SerializeField] private TextMeshProUGUI timeText;
        private float currentCool;
        private bool isCoolingDown = false;
        
        #endregion

        #region Property
        public int CurrentIndex => currentIndex;
        public Character NextCharacter
        {
            get
            {
                Character next = characters[(currentIndex + 1) % 3];
                if(next.IsDead)
                {
                    next = characters[(currentIndex + 2) % 3];
                    if(next.IsDead)
                    {
                        return null;
                    }
                }
                return next;
            }
        }

        public bool SwitchComboSignal { get; set; }

        public bool IsSomeoneParrying
        {
            get
            {
                bool sp = false;
                foreach  (Character character in characters)
                {
                    sp |= character.GetComponent<Parrying>().IsParrying;
                }
                return sp;
            }
        }

        public bool CanSwitch { get; set; }
        #endregion

        #region Unity Event Method
        protected override void Awake()
        {
            base.Awake();
            cam = GetComponent<CameraController>();
            for (int i = 0; i < controllers.Length; i++)
            {
                controllers[i] = characters[i].gameObject.GetComponent<CharacterController>();
            }
        }

        private void Start()
        {
            CanSwitch = true;
            timeText.text = "";
            SaveFormation();
            currentIndex = 0; // 첫 캐릭터 선택
            SelectCharacter(currentIndex);
        }

        private void Update()
        {
            if (InputManager.Instance.SwapPressed && CanSwitch)
            {
                //Debug.Log("SPACE");

                //for (int i = 0; i < characters.Length; i++)
                //{
                //    Debug.Log(i.ToString() + characters[i].IsDead);
                //}
                SwitchToNextCharacter();
            }
        }

        #endregion

        #region Custom Method

        public void SwitchToNextCharacter()
        {
            if (isCoolingDown) return;
            StartCoroutine(SwitchingCoolDown());
            // 모든 캐릭터가 죽었는지 먼저 체크
            bool allDead = true;
            for (int i = 0; i < characters.Length; i++)
            {
                if (!characters[i].IsDead)
                {
                    allDead = false;
                    break;
                }
            }

            if (allDead)
            {
                GameOver();
                return;
            }

            // 다음 살아있는 캐릭터로 이동
            int startIndex = currentIndex;

            do
            {
                currentIndex++;
                if (currentIndex >= characters.Length)
                    currentIndex = 0;

                //Debug.Log("ㅂㅂㅁ");
            } while (characters[currentIndex].IsDead);

            SelectCharacter(currentIndex);
        }

        private IEnumerator SwitchingCoolDown()
        {
            isCoolingDown = true;
            while(currentCool <= coolTime)
            {
                currentCool += Time.deltaTime;
                switchIcon.fillAmount = currentCool / coolTime;
                timeText.text = $"{(coolTime - currentCool): 0.0}";
                yield return null;
            }
            timeText.text = "";
            currentCool = 0f;
            isCoolingDown = false;
        }
        public void SelectCharacter(int index)
        {
            //선택한 캐릭터만 활성화, 나머지 정령화
            for (int i = 0; i < characters.Length; i++)
            {
                //한번씩 껐다 켜줘서 정령 위치 업데이트 (살아있는 애들만)
                if (!characters[i].IsDead)
                    characters[i].gameObject.SetActive(true);
                characters[i].gameObject.SetActive(i==index);
                characters[i].gameObject.tag = (characters[i].gameObject.activeSelf) ? "Player" : "Fairy";
            }

            cam.LookAtCharacter(index);
            PlayerStatsInfo.Instance.SwitchCharatersInfo();

            //교체공격
            if (SwitchComboSignal)
            {
                characters[currentIndex].SwitchCombo();
            }
            //Debug.Log($"{index}번 캐릭터 등장");
        }

        public void GameOver()
        {
            Debug.Log("Game Over!");
            //여기에 게임오버 UI 표시 등
            //...
        }

        private void SaveFormation()
        {
            int leaderIndex = currentIndex; // 기준 캐릭터(리더)
            positionOffsets = new Vector3[characters.Length];
            rotationOffsets = new Quaternion[characters.Length];

            Vector3 leaderPos = characters[leaderIndex].transform.position;
            Quaternion leaderRot = characters[leaderIndex].transform.rotation;

            for (int i = 0; i < characters.Length; i++)
            {
                positionOffsets[i] = Quaternion.Inverse(leaderRot) * (characters[i].transform.position - leaderPos);
                rotationOffsets[i] = Quaternion.Inverse(leaderRot) * characters[i].transform.rotation;
            }

            //Debug.Log("포메이션 저장");
        }

        // 지정한 위치로 포메이션 이동
        public void MoveFormation(Vector3 newLeaderPos, Quaternion newLeaderRot)
        {
            if (positionOffsets == null || rotationOffsets == null)
            {
                //Debug.LogWarning("저장 안돼있당께롱");
                return;
            }

            int leaderIndex = currentIndex;

            for (int i = 0; i < characters.Length; i++)
            {
                Vector3 worldPos = newLeaderPos + (newLeaderRot * positionOffsets[i]);
                Quaternion worldRot = newLeaderRot * rotationOffsets[i];

                controllers[i].enabled = false;
                characters[i].transform.position = worldPos;
                characters[i].transform.rotation = worldRot;
                controllers[i].enabled = true;
            }

            SelectCharacter(currentIndex);

            //Debug.Log("포메이션 이동");
        }
        #endregion
    }

}
