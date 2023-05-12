using UnityEngine;
using System.Collections;
using System.IO;

public class SceneStarter : Singleton<SceneStarter>
{

    public GameObject playerCharacter;
    public GameObject mainCamera;

    private void Awake()
    {
        string autosavePath = SaveUtil.GetAutosavePath();
        Directory.CreateDirectory(autosavePath);
    }

    // Use this for initialization
    void Start()
    {
        playerCharacter.SetActive(false);
        //camera.SetActive(false);
        TerrainManager.Main.OnInitializeTerrain += SetScene;
    }

    public void SetScene()
    {
        //camera.SetActive(true);
        playerCharacter.SetActive(true);
    }
}
