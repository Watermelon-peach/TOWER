using UnityEngine;

public class InputBlocker : MonoBehaviour 
{
    private static bool isBlocked = false;

    public static void BlockInputs() 
    {
        isBlocked = true;
    }

    public static void AllowInputs() 
    {
        isBlocked = false;
    }

    //���� �ݱ�
    public static bool IsInputAllowed(KeyCode key) 
    {
        // ESC�� �׻� ���
        return !isBlocked || key == KeyCode.Escape;
    }

    // ����: ������ �ٸ� ��ũ��Ʈ���� �̰ɷ� Ű �Է��� �˻��ؾ� ��
    // if (InputBlocker.IsInputAllowed(KeyCode.Space) && Input.GetKeyDown(KeyCode.Space))
}
