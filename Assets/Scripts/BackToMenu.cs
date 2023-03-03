using UnityEngine;
using UnityEngine.SceneManagement;
public class BackToMenu : MonoBehaviour
{
    public static bool win;

    public void OnClick()
    {
        SceneManager.LoadScene("Game");
    }
}
