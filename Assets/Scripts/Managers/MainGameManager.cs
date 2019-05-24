using DllSky.Analytics;
using DllSky.Managers;
using DllSky.Patterns;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainGameManager : Singleton<MainGameManager>
{
    #region Variables
    [Header("Settings")]
    public bool usingEncryption = false;

    private string currentScene = null;

    private DateTime startSession;
    private DateTime stopPause;
    #endregion    

    #region Unity methods
    private void Start()
    {
        //Метрики
        startSession = DateTime.UtcNow;
        stopPause = DateTime.UtcNow;
        AnalyticsManager.Instance.SendEvent(EnumAnalyticsEventType.GameStart, null);

        StartCoroutine(StartGame());
    }

    private void OnApplicationPause(bool _pause)
    {        
        if (!_pause)
        {
            stopPause = DateTime.UtcNow;

            Debug.Log("<color=#FFD800>[MainGameManager]</color> Pause OFF: " + stopPause);            
        }
        else
        {
            //Метрики
            var session = (int)(DateTime.UtcNow - stopPause).TotalMinutes;
            if (session > 0)
            {
                var gamePauseData = new AnaliticsGameOverData(session);
                AnalyticsManager.Instance.SendEvent(EnumAnalyticsEventType.GamePause, gamePauseData);

                Debug.Log("<color=#FFD800>[MainGameManager]</color> Pause ON: " + DateTime.UtcNow);
            }

            //Сохранение
            Global.Instance.SaveSettings();
            Global.Instance.SaveProfile();
        }
    }

    private void OnApplicationQuit()
    {
        //Метрики
        var session = DateTime.UtcNow - startSession;
        var gameOverData = new AnaliticsGameOverData((int)session.TotalMinutes);
        AnalyticsManager.Instance.SendEvent(EnumAnalyticsEventType.GameOver, gameOverData);

        //Сохранение
        Global.Instance.SaveSettings();
        Global.Instance.SaveProfile();
    }
    #endregion

    #region Public methods
    public void LoadScene(string _scene, LoadSceneMode _mode = LoadSceneMode.Additive)
    {
        StartCoroutine( LoadSceneCoroutine(_scene, _mode) );
    }
    #endregion

    #region Private methods
    private void ApplySettings()
    {
        Application.targetFrameRate = 60;
        QualitySettings.antiAliasing = 4;

        Debug.Log("<color=#FFD800>[MainGameManager] Application.targetFrameRate: " + Application.targetFrameRate + "</color>");
    }
    #endregion

    #region Coroutine
    private IEnumerator StartGame()
    {
        //Стартовый прелоадер
        yield return SplashScreenManager.Instance.ShowStartingGame();        

        //Версия
        Debug.Log("<color=#FFD800>[VERSION] " + Application.version + "</color>");
        //Инициализация конфига
        Global.Instance.Initialize();
        //Ожидание окончания загрузки конфига и настроек
        while (!Global.Instance.isComplete)
            yield return null;

        ApplySettings();

        //TEST 1: Загрузка тестовой сцены
        yield return new WaitForSeconds(1.0f);

        ScreenManager.Instance.ShowScreen(ConstantsScreen.MENU); 
    }

    public IEnumerator LoadSceneCoroutine(string _scene, LoadSceneMode _mode = LoadSceneMode.Additive)
    {
        //Выгружаем предыдущую сцену
        if (SceneManager.sceneCount > 1)
        {
            var oldScene = SceneManager.GetSceneAt(SceneManager.sceneCount-1);
            var oldName = oldScene.name;

            var unloading = SceneManager.UnloadSceneAsync(oldScene);
            while (!unloading.isDone)
                yield return null;

            Debug.Log("<color=#FFD800>[MainGameManager] Scene unloaded: " + oldName + "</color>");
        }

        Resources.UnloadUnusedAssets();
        GC.Collect();

        //Загружаем новую сцену
        currentScene = _scene;

        var loading = SceneManager.LoadSceneAsync(currentScene, _mode);
        while (!loading.isDone)
            yield return null;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(currentScene));
        Debug.Log("<color=#FFD800>[MainGameManager] Scene loaded: " + currentScene + "</color>");
    }
    #endregion
}
