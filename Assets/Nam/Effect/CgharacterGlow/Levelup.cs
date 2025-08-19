using UnityEngine;
using System.Collections;

//������ ȿ��
//������ �ִϸ��̼� ����, ������ �۷ο� ���͸��� ��ü
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
        //�ʱ�ȭ
        isLevelup = false;
        originalMaterial = bodyRenderer.material;
    }

    private void Update()
    {
        //���콺 ��Ŭ���� ������ ȿ�� ����
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
