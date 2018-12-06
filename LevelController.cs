using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    //array of array of images for the stims we'll show in the levels
    [Serializable]
    public class StimuliObject
    {
        public string conditionName;
        public Material[] stimuliArray;
    }
    public StimuliObject[] stimuli;

    [Serializable]
    public class LevelObject
    {
        public string levelName;
        public AudioClip[] narration;
        public float levelTimeLimit;
    }

    //array if level objects
    public LevelObject[] levels;
    public string endURL = "";
    public AudioClip audioEnd;
    public GameObject togglePrefab;
    public GameObject toggleParent;

    private int levelProgress = 0;
    private List<Toggle> toggles = new List<Toggle>();
    private int condition;
    private float levelTimeLimit = 0;
    private float audioTimer = 1.5f;
    private bool levelEnding = false;
    private int gameEnding = 0;
    private Texture2D loadScreen;
    private GUIStyle loadScreenStyle;
    private PlayerGUI playerGUI;
    private AudioSource audioSource;
    void OnGUI()
    {
        //if we're ending this level, show a blank screen with loading to indicate we're loading the next level
        if (levelEnding)
        {
            if (loadScreen == null)
            {
                loadScreen = new Texture2D(1, 1);
                loadScreen.SetPixel(0, 0, Color.black);
                loadScreen.Apply();
            }
            if (loadScreenStyle == null)
            {
                loadScreenStyle = new GUIStyle();
            }
            loadScreenStyle.normal.background = loadScreen;
            loadScreenStyle.normal.textColor = Color.white;
            loadScreenStyle.fontSize = 30;
            loadScreenStyle.alignment = TextAnchor.MiddleCenter;

            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "Loading...", loadScreenStyle);
        }
    }
    void Start()
    {
        //keep the gameobject this script is a part of in the scene
        DontDestroyOnLoad(this.gameObject);
        //get the audiosource for this gameobject
        audioSource = GetComponent<AudioSource>();
        //run through the conditions we're supposed to have and create a toggle box on the menu for each one
        int counter = 1;
        foreach (StimuliObject so in stimuli)
        {
            GameObject tp = Instantiate(togglePrefab);
            tp.transform.SetParent(toggleParent.transform);
            tp.transform.localPosition = Vector3.zero;
            tp.transform.Translate(Vector3.up * (-30f * counter));
            counter++;
            tp.GetComponentInChildren<Text>().text = so.conditionName;
            Toggle tog = tp.GetComponent<Toggle>();
            tog.group = toggleParent.GetComponent<ToggleGroup>();
            toggles.Add(tog);
        }
    }
    void Update()
    {
        if (audioTimer >= 0)
        {
            audioTimer -= Time.deltaTime;
        }
        if (audioTimer < 0 && audioTimer != -99)
        {
            audioSource.Play();
            audioTimer = -99;
        }
        levelTimeLimit -= Time.deltaTime;
        if ((levelTimeLimit <= 0 && levelProgress != 0) || (Input.GetButtonDown("Skip") && !levelEnding))
        {
            EndLevel();
        }
        if (gameEnding > 0)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = audioEnd;
                audioSource.Play();
                gameEnding = 2;
            }
            else if(!audioSource.isPlaying && gameEnding == 2)
            {
                gameEnding = 0;
                Application.OpenURL(endURL + "?ID=" + PlayerPrefs.GetString("PlayerID") + "&Condition=" + PlayerPrefs.GetInt("Posters"));
            }
        }
    }
    public void EndLevel()
    {
        //create a flag and then, if we're on the start menu, check to make sure a toggle was selected before we start the game
        bool flag = false;
        if (levelProgress == 0) {
            //when we look through the toggles, if one is selected we set the condition of the experiment from it
            int counter = 0;
            foreach (Toggle tog in toggles)
            {
                if (tog.isOn)
                {
                    flag = true;
                    condition = counter;
                }
                counter++;
            }
        }
        else
        {
            flag = true;
        }
        //only run the function to end the game if we aren't currently ending the level and if the toggle select flag is set
        if (!levelEnding && flag) {
            levelEnding = true;
            if (playerGUI != null) {
                playerGUI.paused = true;
            }
            Time.timeScale = 0;
            if (levelProgress < levels.Length)
            {
                StartCoroutine(LoadYourAsyncScene());
            }
            else
            {
                gameEnding = 1;
            }
        }
    }
    IEnumerator LoadYourAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levels[levelProgress].levelName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        levelEnding = false;
        Time.timeScale = 1;
        //setup timer for this next scene
        levelTimeLimit = levels[levelProgress].levelTimeLimit;
        //find the player GUI, which we'll need to pause when the next scene after this one is loading in
        playerGUI = GameObject.FindObjectOfType<PlayerGUI>();
        //reset audio timer
        audioTimer = 1.5f;
        //set audio clip to play, if any
        if (levels[levelProgress].narration.Length > 0)
        {
            if (condition >= levels[levelProgress].narration.Length)
            {
                audioSource.clip = levels[levelProgress].narration[0];
            }
            else
            {
                audioSource.clip = levels[levelProgress].narration[condition];
            }
        }
        else
        {
            audioSource.clip = null;
        }
        //grab the posters in the level and set their materials to the stims set in this script
        DataTrackingObject[] dataTrackingObjects = GameObject.FindObjectsOfType<DataTrackingObject>();
        int counter = 0;
        foreach (DataTrackingObject dto in dataTrackingObjects)
        {
            dto.GetComponent<Renderer>().material = stimuli[condition].stimuliArray[counter];
            counter++;
            if(counter >= stimuli[condition].stimuliArray.Length)
            {
                counter = 0;
            }
        }
        //iterate level counter so we progress to the next level when EndLevel is run next time
        levelProgress++;
    }
}
