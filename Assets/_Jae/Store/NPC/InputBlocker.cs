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

    //상점 닫기
    public static bool IsInputAllowed(KeyCode key) 
    {
        // ESC는 항상 허용
        return !isBlocked || key == KeyCode.Escape;
    }

    // 예시: 게임의 다른 스크립트에서 이걸로 키 입력을 검사해야 함
    // if (InputBlocker.IsInputAllowed(KeyCode.Space) && Input.GetKeyDown(KeyCode.Space))
}
