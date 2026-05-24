using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManagment : MonoBehaviour
{
    void Start()
    {

    }
    void Update()
    {

    }
    public void OnDropdownValueChanged(int index)
    {
        switch (index)
        {
            case 0:
                
                break;
            case 1:
                SceneManager.LoadScene("Main_scene");
                break;
        }
    }
}
