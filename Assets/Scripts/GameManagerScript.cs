using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour
{
    public float levelHeight;
    public GameObject cubePrefab;
    private GameObject player;
    private Transform envT;
    private Camera cam;
    private Vector3 camOffset;

    public Text ScoreText;
    public GameObject GameOverObj;

    private void Start()
    {
        // Adjust UI and camera view to fit screen size.
        float screenRatio = (float)Screen.height / Screen.width; 
        if (screenRatio < 1.7f)
            Camera.main.GetComponent<CameraConstantWidth>().WidthOrHeight = 1f;
        else if (screenRatio > 1.95f)
            GameObject.Find("Canvas").GetComponent<CanvasScaler>().matchWidthOrHeight = .6f;

        player = GameObject.Find("Worm");
        envT = GameObject.Find("Environment").transform;
        cam = Camera.main;
        camOffset = cam.transform.position - player.transform.position;

        LevelGenerator();
    }

    private void LateUpdate()
    {
        Vector3 camTargetPos = player.transform.position + camOffset;
        cam.transform.position = camTargetPos;
    }

    private void LevelGenerator()
    {
        float length = 0;
        for (float height = 0; height <= levelHeight;)
        {
            Vector3 rotation = Vector3.zero;
            if (Random.value < .5f)
                rotation += new Vector3(0, 0, Random.Range(10, 90));
            GameObject prefab = cubePrefab;
            prefab.transform.localScale = new Vector3(Random.Range(.1f, 1), .1f, 3);
            GameObject step = Instantiate(prefab, new Vector3(length, height, -1.5f) + new Vector3(prefab.transform.localScale.x / 2, 
                prefab.transform.localScale.y / 2, 0), Quaternion.Euler(rotation), envT);
            step.GetComponent<MeshRenderer>().material.color = new Color(rotation.z / 90, 1 - rotation.z / 90, 0);
            length = step.GetComponent<MeshRenderer>().bounds.max.x;
            height = step.GetComponent<MeshRenderer>().bounds.max.y;
        }
    }

    public void GameOver()
    {
        GameOverObj.SetActive(true);
    }

    public void RestartButton()
    {
        SceneManager.LoadScene(0);
    }
}
