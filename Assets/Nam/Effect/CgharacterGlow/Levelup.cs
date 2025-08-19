using UnityEngine;
using System.Collections;

//레벨업 효과
//레벨업 애니메이션 포즈, 레벨업 글로우 메터리얼 교체
public class Levelup : MonoBehaviour
{
    #region Variables
    private Animator animator;

    private bool isLevelup = false;

    public Renderer bodyRenderer;
    public Material levelupGlow;
    private Material originalMaterial;

    //
    #endregion

    #region Unity Event Method
    private void Awake()
    {
        animator = this.GetComponent<Animator>();
    }

    private void Start()
    {
        //초기화
        isLevelup = false;
        originalMaterial = bodyRenderer.material;
    }

    private void Update()
    {
        //마우스 우클릭시 레벨업 효과 실행
        if(Input.GetMouseButtonDown(1) && isLevelup == false)
        {
            StartCoroutine(LevelupEffect());
        }
    }
    #endregion

    #region Custom Method
    IEnumerator LevelupEffect()
    {
        isLevelup = true;
        bodyRenderer.material = levelupGlow;

        yield return new WaitForSeconds(2f);

        bodyRenderer.material = originalMaterial;
        isLevelup = false;
    }
    #endregion


}
