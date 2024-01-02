using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{
    public void loadLevel(string level)
    {
        StartCoroutine(StartLevelAfterDelay(level));
    }

    IEnumerator StartLevelAfterDelay(string level)
    {
        GameObject.FindGameObjectWithTag("LevelLoader").GetComponentInChildren<Animator>().SetTrigger("End");

        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(level);
    }
}
