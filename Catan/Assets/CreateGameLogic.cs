using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateGameLogic : MonoBehaviour
{
    [SerializeField] private Button Startbutton, SmallButton, MediumButton, LargeButton;
    private static int gameSize = 0;
    void Start()
    {
        Startbutton.onClick.AddListener(()=> StartGame());
        SmallButton.onClick.AddListener(()=> gameSize=2);
        MediumButton.onClick.AddListener(()=> gameSize=3);
        LargeButton.onClick.AddListener(()=> gameSize=4);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void StartGame()
    {
        if(gameSize == 0) return;
        UnityRelay.CreateRelay();
        SceneManager.LoadScene("Loby");
    }

    public static int GetGameSize()
    {
        return gameSize;
    }
}
