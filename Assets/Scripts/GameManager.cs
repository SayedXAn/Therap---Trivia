using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using UnityEngine.XR;

public class GameManager : MonoBehaviour
{
    MainDB db = new MainDB();
    //db indexes >> question number - Key, list[0] - question, list[1] - option1, list[2] - option2, list[3] - option3, list[4] - option4, list[5] - right answer index(string, do a int.parse)

    [Header("Panels")]
    public GameObject MenuPanel;
    public GameObject InstructionPanel;
    public GameObject QuizPanel;
    public GameObject GOPanel;

    [Header("GameObjects")]
    public GameObject[] optionsBG;
    public GameObject nextButton;
    public GameObject timerBG;
    public GameObject questionBG;
    public Image[] stageLines;
    

    [Header("TextFields")]
    public TMP_Text question;
    public TMP_Text[] optionsText;
    public TMP_Text generateButtonText;
    public TMP_Text timerText;
    public TMP_Text notificationText;
    public TMP_Text scoreText;
    public TMP_Text finishText;
    public TMP_Text instructionText;
    public TMP_InputField nameIF;
    public string insText = "Place your hand over the leap motion device and Navigate your hand to the correct option. \r\n\r\nPinch with your thumb and index fingers to choose the right answer.\r\n\r\nThe faster you answer,\r\nthe more points you score!�\r\n\r\nTrivia starting in: ";

    [Header("Audio")]
    public AudioSource bgmAS;
    public AudioSource sfxAS;
    public AudioClip bgm;
    public AudioClip[] sfx;

    [Header("LBManager")]
    public LBManager lbMan;


    [Header("Variables")]
    [SerializeField] int timer = 20;
    [SerializeField] int questionPerPlayer = 6;
    [SerializeField] float finishPanelTime = 5f;
    [SerializeField] float instructionTime = 5f;
    [SerializeField] float goToNextQuesTime = 3f;
    
    private bool hasAnswered = false;
    private int currentCorrect = -1;
    private int playerPos = -2;
    private bool quizOn = false;
    private bool menuOn = true;
    private bool timeUp = false;
    private bool TestingOn = false;
    public int timeBonus = 0;
    
    private int score = 0;
    private int currentQuestion = 0;

    private string[] bhaloKotha = { "That's Correct! Keep it up!", "That�s right! Well done!", "Correct! Great job!" };
    private string[] kharapKotha = { "Incorrect. Try the next one!", "Wrong answer. Don�t give up!"};

    private List<int> selectedQuestions = new List<int>();

    [Header("Colors")]
    Color32 bluee = new Color32(15, 0, 128, 255);
    Color32 offWhitee = new Color32(255, 236, 212, 255);
    Color32 orangeColor = new Color32(244, 111, 33, 255);

    private void Start()
    {
        MenuPanel.SetActive(true);
        QuizPanel.SetActive(false);
        GOPanel.SetActive(false);


        bgmAS.clip = bgm;
        bgmAS.loop = true;
        bgmAS.Play();
        menuOn = true;
        nameIF.ActivateInputField();
    }

    private void Update()
    {
        if(!menuOn && Input.GetKeyDown(KeyCode.R))
        {
            HomeButton();
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        /*if (Input.GetKeyDown(KeyCode.T))
        {
            if(!TestingOn)
            {
                TestingOn = true;
                StartCoroutine(TestingQuestions());
            }
        }*/
    }

    public void GenerateQuestion()
    {
        
        QuestionOptionAnimationReset();
        hasAnswered = false;
        timeUp = false;
        StopAllCoroutines();
        if(currentQuestion < questionPerPlayer)
        {
            currentQuestion++;            
            stageLines[currentQuestion - 1].color = orangeColor;
        }
        else if(currentQuestion == questionPerPlayer)
        {
            StartCoroutine(GameFinish());
            StartCoroutine(ShowPlayerPositon());
            return;
        }
        ResetOptions();
        nextButton.SetActive(false);
        
        int randInt = Random.Range(0, db.table.Count);
        while(selectedQuestions.Contains(randInt))
        {
            randInt = Random.Range(0, db.table.Count);
        }
        selectedQuestions.Add(randInt);
        //generateButtonText.text = randInt.ToString();
        question.text = db.GetValueFromDB(randInt, 0);
        for(int i = 0; i < optionsText.Length; i++)
        {
            optionsText[i].text = db.GetValueFromDB(randInt, i+1);
        }
        currentCorrect = int.Parse(db.GetValueFromDB(randInt, 5));
        notificationText.text = "";
        ResetTimer();
        QuestionOptionAnimation();

    }


    public void ShowInstructionPanel()
    {
        StartCoroutine(ShowInstruction());
    }
    IEnumerator ShowInstruction()
    {
        MenuPanel.SetActive(false);
        InstructionPanel.SetActive(true);
        instructionText.text = insText + instructionTime.ToString() + " Second(s)";
        yield return new WaitForSeconds(1f);
        instructionTime--;
        if(instructionTime > 0)
        {
            StartCoroutine(ShowInstruction());
        }
        else
        {

            InstructionPanel.SetActive(false);
            QuizPanel.SetActive(true);
            GOPanel.SetActive(false);
            GenerateQuestion();
            quizOn = true;
            StopCoroutine(ShowInstruction());
        }
    }
    public void OnButtonPress(int buttonId)
    {        
        if(!hasAnswered)
        {
            timeBonus = timer;
            StopCoroutine(Timer());
            quizOn = false;
            optionsBG[buttonId].transform.GetChild(5).gameObject.SetActive(true);
            if (currentCorrect == buttonId)
            {
                score = score + 10 + timeBonus;
                notificationText.text = (10 + timeBonus).ToString()+" points\n" + bhaloKotha[Random.Range(0, bhaloKotha.Length)];                
                scoreText.text = score.ToString();
                optionsBG[buttonId].transform.GetChild(0).gameObject.SetActive(true);
                PlaySFX(0);
            }
            else
            {
                notificationText.text = kharapKotha[Random.Range(0, kharapKotha.Length)];
                //score += 5;
                //scoreText.text = score.ToString();
                optionsBG[buttonId].transform.GetChild(1).gameObject.SetActive(true);
                //optionsText[buttonId].color = offWhitee;
                PaintOptions(currentCorrect);
                PlaySFX(1);
            }
            //nextButton.SetActive(true);
            StartCoroutine(GoToNextQuestion());
            timerBG.SetActive(false);
            hasAnswered = true;
        }        
    }

    IEnumerator GoToNextQuestion()
    {
        yield return new WaitForSeconds(goToNextQuesTime);
        GenerateQuestion();
        StopCoroutine(GoToNextQuestion());
    }

    public void PaintOptions(int correctOption)
    {
        for (int i = 0; i < optionsBG.Length; i++)
        {
            if(i == correctOption)
            {
                if(!timeUp)
                {
                    optionsBG[i].transform.GetChild(0).gameObject.SetActive(true);
                }
                else
                {
                    optionsBG[i].transform.GetChild(2).gameObject.SetActive(true);
                }
                return;
            }
            /*else
            {
                optionsBG[i].transform.GetChild(1).gameObject.SetActive(true);
            }*/
        }
    }
    public void ResetOptions()
    {
        for (int i = 0; i < optionsBG.Length; i++)
        {
            optionsBG[i].transform.GetChild(0).gameObject.SetActive(false);
            optionsBG[i].transform.GetChild(1).gameObject.SetActive(false);
            optionsBG[i].transform.GetChild(2).gameObject.SetActive(false);
            optionsBG[i].transform.GetChild(5).gameObject.SetActive(false);
            //optionsText[i].color = bluee;
        }
    }

    public void PlayGameButton()
    {
        menuOn = false;
        if(nameIF.text == "")
        {
            nameIF.placeholder.GetComponent<TMP_Text>().text = "Enter your name before proceeding";
        }
        else
        {
            StartCoroutine(ShowInstruction());
        }        
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(1f);
        timer--;
        timerText.text = "Time Left\n" + timer.ToString();
        if(quizOn && timer > 0)
        {
            StartCoroutine(Timer());
        }
        else if(quizOn && timer == 0 && !hasAnswered)
        {
            StopCoroutine(Timer());
            timeUp = true;
            quizOn = false;
            hasAnswered = true;
            //nextButton.SetActive(true);            
            PaintOptions(currentCorrect);
            timer = 20;
            timerBG.SetActive(false);
            notificationText.text = "Time's Up!";
            StartCoroutine(GoToNextQuestion());
        }
    }

    public void ResetTimer()
    {
        quizOn = true;
        hasAnswered = false;
        timer = 20;
        timerBG.SetActive(true);
        timerText.text = "Time Left\n" + timer.ToString();
        StartCoroutine(Timer());
    }

    IEnumerator GameFinish()
    {
        lbMan.SetEntry(nameIF.text, score);
        MenuPanel.SetActive(false);
        QuizPanel.SetActive(false);
        GOPanel.SetActive(true);
        finishText.text = "Thanks for playing!\nYour final score is " + score.ToString() +"\n ";
        yield return new WaitForSeconds(finishPanelTime);
        StartCoroutine(GoToLeaderboard());
    }
    public void ShowLeaderboardManually()
    {
        MenuPanel.SetActive(false);
        QuizPanel.SetActive(false);
        GOPanel.SetActive(false);
        lbMan.GenerateLeaderboard();
    }

    IEnumerator GoToLeaderboard()
    {
        MenuPanel.SetActive(false);
        QuizPanel.SetActive(false);
        GOPanel.SetActive(false);
        lbMan.GenerateLeaderboard();
        yield return new WaitForSeconds(10f);
        SceneManager.LoadScene("Trivia");
    }

    public void HomeButton()
    {
        SceneManager.LoadScene("Trivia");
    }

    public void QuestionOptionAnimation()
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(questionBG.transform.DOMoveY(850, 0.3f));
        mySequence.Append(optionsBG[0].transform.DOMoveX(570, 0.2f));
        mySequence.Append(optionsBG[1].transform.DOMoveX(1350, 0.2f));
        mySequence.Append(optionsBG[2].transform.DOMoveX(570, 0.2f));
        mySequence.Append(optionsBG[3].transform.DOMoveX(1350, 0.2f));
    }

    public void QuestionOptionAnimationReset()
    {
        questionBG.transform.position = new Vector3(questionBG.transform.position.x, 1500f, questionBG.transform.position.z);
        optionsBG[0].transform.position = new Vector3(-450f, optionsBG[0].transform.position.y, optionsBG[0].transform.position.z);
        optionsBG[1].transform.position = new Vector3(2350, optionsBG[1].transform.position.y, optionsBG[1].transform.position.z);
        optionsBG[2].transform.position = new Vector3(-450f, optionsBG[2].transform.position.y, optionsBG[2].transform.position.z);
        optionsBG[3].transform.position = new Vector3(2350, optionsBG[3].transform.position.y, optionsBG[3].transform.position.z);
    }

    public void PlaySFX(int index)
    {
        sfxAS.clip = sfx[index];
        sfxAS.Play();
    }
    public void SetPlayerPos(int pos)
    {
        playerPos = pos;
    }
    IEnumerator ShowPlayerPositon()
    {
        yield return new WaitForSeconds(0.1f);
        if(playerPos == -2)
        {
            StartCoroutine(ShowPlayerPositon());
        }
        else
        {
            finishText.text = finishText.text + "your position: " + playerPos.ToString();
            StopCoroutine(ShowPlayerPositon());
        }
    }

    //this is only for testing
    /*IEnumerator TestingQuestions()
    {

        QuestionOptionAnimationReset();
        hasAnswered = false;
        ResetOptions();
        int randInt = Random.Range(0, db.table.Count);
        while (selectedQuestions.Contains(randInt))
        {
            randInt = Random.Range(0, db.table.Count);
        }

        selectedQuestions.Add(randInt);
        Debug.Log("rand int: " +randInt + "  list size: " +selectedQuestions.Count);
        //generateButtonText.text = randInt.ToString();
        question.text = db.GetValueFromDB(randInt, 0);
        for (int i = 0; i < optionsText.Length; i++)
        {
            optionsText[i].text = db.GetValueFromDB(randInt, i + 1);
        }
        currentCorrect = int.Parse(db.GetValueFromDB(randInt, 5));
        QuestionOptionAnimation();
        
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(TestingQuestions());
    }*/
}
