using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private Button btn_Start;
    [SerializeField] private Button btn_Exit;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        btn_Start.onClick.AddListener(() => SceneManager.LoadScene("02_LobbyScene"));
        btn_Exit.onClick.AddListener(() => Application.Quit());
    }
}
