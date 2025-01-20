using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Networking;
using System.Text;


public class ForGiftCheck : MonoBehaviour
{

    DataBase dataBase = new DataBase();
    public InputField mainInputField;
    public string RFIDuser;
    private bool isDataRecieved = false;

    public Image image = null;
    public Text personName;
    public Text personRegion;
    public Text personID;
    public Text checkedIn;
    public Text entryCounterText;

    private int nameIndex = 0;
    private int idIndex = 1;
    private int regionIndex = 2;
    private int photoNameIndex = 3;

    private int entryCounter = 0;

    private int dataResetCounter = 0;
    private int dataSyncCounter = 0;


    private string checkedInWhen;

    List<string> allRFIDs = new List<string>();

    void Start()
    {
        //PlayerPrefs.DeleteAll();
        mainInputField.ActivateInputField();
        entryCounter = PlayerPrefs.GetInt("entryCounter", 0);
        entryCounterText.text = entryCounter.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDataRecieved)
        {
            allRFIDs.Add(RFIDuser);
            personName.text = "Rbve, " + dataBase.GetValue(RFIDuser, nameIndex);
            personRegion.text = dataBase.GetValue(RFIDuser, regionIndex);
            personID.text = dataBase.GetValue(RFIDuser, idIndex);
            image.sprite = Resources.Load<Sprite>("Photos/" + dataBase.GetValue(RFIDuser, photoNameIndex));
            //image.sprite = Resources.Load<Sprite>("Rangpur/" + dataBase.GetValue(RFIDuser, photoNameIndex));
            
            if (PlayerPrefs.HasKey(RFIDuser))
            {
                //already checked in
                checkedIn.color = Color.red;
                checkedIn.text = "Already Checked In!";
            }
            else
            {
                string time = System.DateTime.UtcNow.ToLocalTime().ToString();
                checkedInWhen = "Checked in at " + time;
                entryCounter++;
                entryCounterText.text = "Total New Entry: " + entryCounter.ToString();
                PlayerPrefs.SetString(RFIDuser, checkedInWhen);
                PlayerPrefs.SetInt("entryCounter", entryCounter);
                StartCoroutine(UploadUserInfo());
                checkedIn.color = Color.green;
                checkedIn.text = "New User - Entry Successful";

            }



            isDataRecieved = false;
            mainInputField.text = "";
        }
        DataReciever();

    }

    public void DataReciever()
    {
        if (mainInputField.text.Length == 10)
        {
            if(mainInputField.text == "0014053601")
            {
                mainInputField.text = "";
                dataResetCounter++;                
                Debug.Log("Counter "+dataResetCounter);
                if(dataResetCounter == 3)
                {
                    DeleteAllLocalData();
                }                
            }
            else if(mainInputField.text == "0014045429")
            {
                mainInputField.text = "";
                dataSyncCounter++;
                Debug.Log("Counter " + dataSyncCounter);
                if (dataSyncCounter == 3)
                {
                    StartCoroutine(ReUploadAllDataToSync());
                }
            }
            else
            {
                RFIDuser = mainInputField.text;
                isDataRecieved = true;
            }
        }
    }

    public class MyData
    {
        public string userId;
        public string name;
        public bool checkin;
        public string time;
    }

    IEnumerator UploadUserInfo()
    {
        MyData data = new MyData
        {
            userId = dataBase.GetValue(RFIDuser, idIndex).ToString(), /////// change the fucking ID
            name = dataBase.GetValue(RFIDuser, nameIndex).ToString(),
            checkin = true,
            time = checkedInWhen.ToString()
        };

        Debug.Log("ID:"+ data.userId + " \n" + "Name:"+data.name + "\n" + "Time:"+ data.time + "Bool:"+ data.checkin);


        string jsonData = JsonUtility.ToJson(data);

        UnityWebRequest request = UnityWebRequest.PostWwwForm("https://us-central1-nestle-activation-e65b3.cloudfunctions.net/users/checkin", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error sending request: " + request.result);
        }
        else
        {
            Debug.Log("Request sent successfully");
        }
        request.Dispose();
    }

    private void DeleteAllLocalData()
    {
        PlayerPrefs.DeleteAll();
        dataResetCounter = 0;
        entryCounter = 0;
        entryCounterText.text = entryCounter.ToString();
        personName.text = "";
        personRegion.text = "";
        checkedIn.text = "";
        personID.text = "Local Data Erased";
        entryCounterText.text = "";
        mainInputField.text = "";
        image.sprite = null;

    }

    IEnumerator ReUploadAllDataToSync()
    {
        dataSyncCounter = 0;
        foreach (string rfID in allRFIDs)
        {
            MyData data = new MyData
            {
                userId = dataBase.GetValue(rfID, idIndex).ToString(), /////// change the fucking ID
                name = dataBase.GetValue(rfID, nameIndex).ToString(),
                checkin = true,
                time = checkedInWhen.ToString()
            };

            Debug.Log("ID:" + data.userId + " \n" + "Name:" + data.name + "\n" + "Time:" + data.time + "Bool:" + data.checkin);


            string jsonData = JsonUtility.ToJson(data);

            UnityWebRequest request = UnityWebRequest.PostWwwForm("https://us-central1-nestle-activation-e65b3.cloudfunctions.net/users/checkin", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);

            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error sending request: " + request.result);
                checkedIn.text = "All Data Sync not Successfull!!!";
            }
            else
            {
                Debug.Log("Request sent successfully");
                checkedIn.text = "All Data Sync Successful";
            }
            request.Dispose();
        }
    }        
            
}