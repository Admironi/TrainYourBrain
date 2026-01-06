using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class Bootstrapper : MonoBehaviour
{
    [field: SerializeField] public string GameSceneName { get; private set; } = "Game";

    private static bool s_bootstrapped;

    private void Awake()
    {
        if (s_bootstrapped)
        {
            Destroy(gameObject);
            return;
        }

        s_bootstrapped = true;
        DontDestroyOnLoad(gameObject);

        if (string.IsNullOrWhiteSpace(GameSceneName))
        {
            Debug.LogError("GameSceneName is not set.", this);
            return;
        }

        if (SceneManager.GetActiveScene().name != GameSceneName)
        {
            SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
        }
    }
}

