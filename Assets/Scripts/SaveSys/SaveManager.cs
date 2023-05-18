using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public delegate void AutosaveHandler();

public class SaveManager : Singleton<SaveManager>
{
    [SerializeField]
    string gameScene = "VoxelWorld";

    public static AutosaveHandler OnAutosave;

    private void Awake()
    {
        string autosavePath = SaveUtil.GetAutosavePath();
        Directory.CreateDirectory(autosavePath);
    }

    public void NewGame()
    {
        CleanAutosave();
        SceneManager.LoadScene(gameScene);
    }

    public void Save(string saveName)
    {
        string savePath = SaveUtil.GetSavePath(saveName);
        Directory.CreateDirectory(savePath);

        StartCoroutine(ProcessSave(savePath));
    }

    public IEnumerator ProcessSave(string savePath)
    {
        yield return new WaitForEndOfFrame();

        string autosavePath = SaveUtil.GetAutosavePath();

        // Save player data (pos, stats, inv)
        PlayerManager.Main.Save();

        // Save all voxels and world items (including chests)
        TerrainManager.Main.SaveAll();


        // Clean TargetSave
        CleanSave(savePath);

        // AutoSave -> TargetSave
        foreach (string file in Directory.GetFiles(autosavePath))
        {
            File.Copy(file, Path.Combine(savePath, Path.GetFileName(file)), true);
        }
    }

    public void Load(string saveName)
    {
        SceneManager.LoadScene(gameScene);
        string savePath = SaveUtil.GetSavePath(saveName);
        string autosavePath = SaveUtil.GetAutosavePath();

        CleanAutosave();

        // TargetSave -> AutoSave
        foreach (string file in Directory.GetFiles(savePath))
        {
            File.Copy(file, Path.Combine(autosavePath, Path.GetFileName(file)), true);
        }

        // Player data gets loaded by PlayerManager
        // Chunks get loaded by TerrainManager
    }

    public void CleanAutosave()
    {
        string autosavePath = SaveUtil.GetAutosavePath();
        System.IO.DirectoryInfo di = new DirectoryInfo(autosavePath);
        foreach (FileInfo file in di.EnumerateFiles())
        {
            file.Delete();
        }
    }

    public void CleanSave(string savePath)
    {
        System.IO.DirectoryInfo di = new DirectoryInfo(savePath);
        foreach (FileInfo file in di.EnumerateFiles())
        {
            file.Delete();
        }
    }

    public void DeleteSave(string saveName)
    {
        string savePath = SaveUtil.GetSavePath(saveName);
        Directory.Delete(savePath, true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
