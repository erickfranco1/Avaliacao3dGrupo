
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    public void SelectErick()
    {
        SceneManager.LoadScene("CenaErick");
    }

    public void SelectDavid()
    {
        SceneManager.LoadScene("David_scene");
    }

    public void SelectFeldhaus()
    {
        SceneManager.LoadScene("Feldhaus");
    }
}
