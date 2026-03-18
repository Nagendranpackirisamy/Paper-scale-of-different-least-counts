using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [Header("Scene Settings")]
    public int sceneIndex;

    public void LoadSceneByIndex()
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // Optional: load by passing index from event
    public void LoadSceneByIndexDynamic(int index)
    {
        SceneManager.LoadScene(index);
    }
}