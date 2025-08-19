using UnityEngine;
using System.Collections;

namespace Tower.Game.Main
{
    public class StartButton : MonoBehaviour
    {
        #region Variables
        [SerializeField] private GameObject startButton;
        [SerializeField] private float minScale = 3f;
        [SerializeField] private float maxScale = 4f;
        [SerializeField] private float halfDuration = 1.5f;
        [SerializeField] private Animator DealerAnim;
        [SerializeField] private Animator HealerAnim;
        [SerializeField] private Animator TankerAnim;
        [SerializeField] private GameObject PartyPos;

        [SerializeField] private float moveDistance = 25f;
        [SerializeField] private float moveDuration = 3f;

        #endregion

        #region Unity Event Method
        private void Update()
        {
            if (!startButton) return;

            // 0→1→0을 halfDuration*2(=3초) 주기로 반복
            float t = Mathf.PingPong(Time.unscaledTime, halfDuration) / halfDuration;

            float s = Mathf.Lerp(minScale, maxScale, t);
            startButton.transform.localScale = Vector3.one * s;
        }

        #endregion

        #region Custom Method
        public void OnStart()
        {
            DealerAnim.SetTrigger("Walk");
            HealerAnim.SetTrigger("Walk");
            TankerAnim.SetTrigger("Walk");

            if (PartyPos) StartCoroutine(MovePartyForward());
        }

        private IEnumerator MovePartyForward()
        {
            Vector3 start = PartyPos.transform.position;
            // PartyPos의 “로컬 전방” 기준으로 이동 (월드 +Z로 가고 싶으면 Vector3.forward 사용)
            Vector3 end = start + PartyPos.transform.forward * moveDistance;

            float t = 0f;
            while (t < moveDuration)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / moveDuration);
                float eased = Mathf.SmoothStep(0f, 1f, u); // 자연스러운 가감속
                PartyPos.transform.position = Vector3.LerpUnclamped(start, end, eased);
                yield return null;
            }
            PartyPos.transform.position = end;
        }

        #endregion
    }
}