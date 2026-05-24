using UnityEngine;
using UnityEngine.SceneManagement;

public class LogicMainScene : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
