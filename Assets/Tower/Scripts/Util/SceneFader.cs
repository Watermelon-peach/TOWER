using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Tower.Util
{
    //씬 시작시 페이드인, 씬 종료시 페이드 아웃 효과 구현
    public class SceneFader : MonoBehaviour
    {
        #region Field
        //페이더 이미지
        public Image img;

        //애니메이션 커브
        public AnimationCurve curve;

        //페이드인 딜레이 타임
        //[SerializeField] private float fadeInDelay = 0f;
        #endregion

        private void Start()
        {
            //초기화 - 페이드 이미지
            img.color = new Color(0f, 0f, 0f, 1f);
            //페이드 인
            //StartCoroutine(FadeIn(fadeInDelay));
        }

        //코루틴으로 구현
        //delayTime : 매개변수로 딜레이 타임
        //FadeIn : 1초동안 : 검정에서 완전 투명으로 (이미지 알파값 a:1 -> a:0)
        IEnumerator FadeIn(float delayTime = 0f)
        {
            //delayTime 지연
            if (delayTime > 0)
            {
                yield return new WaitForSeconds(delayTime);
            }

            float t = 1f;

            while (t > 0)
            {
                t -= Time.deltaTime;
                float a = curve.Evaluate(t);
                img.color = new Color(0f, 0f, 0f, a);

                yield return 0f;    //한 프레임 지연
            }
        }

        //페이드인 외부에서 호출
        public void FadeStart(float delayTime = 0f)
        {
            StartCoroutine(FadeIn(delayTime));
        }
        //FadeOut : 1초동안 : 투명에서 완전 검정으로 (이미지 알파값 a:0 -> a:1)
        //FadeOut 효과 후 매개변수로 받은 씬 이름으로 LoadScene 이동
        IEnumerator FadeOut(string sceneName)
        {
            //FadeOut 효과 후
            float t = 0f;

            while (t <= 1)
            {
                t += Time.deltaTime;
                float a = curve.Evaluate(t);
                img.color = new Color(0f, 0f, 0f, a);

                yield return 0f;
            }

            //씬 이동
            if (sceneName != "")
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        //FadeOut 효과 후 매개변수로 받은 씬 빌드번호로 LoadScene 이동
        IEnumerator FadeOut(int sceneNumber)
        {
            //FadeOut 효과 후
            float t = 0f;

            while (t <= 1)
            {
                t += Time.deltaTime;
                float a = curve.Evaluate(t);
                img.color = new Color(0f, 0f, 0f, a);

                yield return 0f;
            }

            //씬 이동
            if (sceneNumber >= 0)
            {
                SceneManager.LoadScene(sceneNumber);
            }
        }

        //다른 씬으로 이동 시 FadeOut 효과 후 LoadScene으로 이동
        public void FadeTo(string sceneName = "")
        {
            StartCoroutine(FadeOut(sceneName));
        }

        //씬 번호를 매개변수로 받음
        public void FadeTo(int sceneNumber = -1)
        {
            if (true)
            {
                StartCoroutine(FadeOut(sceneNumber));
            }
        }
    }

}
