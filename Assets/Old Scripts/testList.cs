using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class testList : MonoBehaviour
{

    DataBase dataBase = new DataBase();
    public InputField mainInputField;
    public string RFIDuser;
    private bool isDataRecieved = false;

    public Image image = null;
    public GameObject screenSaverPanel;
    public Text personName;
    public Text personRegion;
    //public Text personID;
    //public bool checkedIn;

    private float transitionTime = 0.5f; 
    public CanvasGroup screenSaverPanelCG;

    private int nameIndex = 0;
    //private int idIndex = 1;
    private int regionIndex = 2;
    private int photoNameIndex = 3;
void Start()
    {
        mainInputField.ActivateInputField();
        screenSaverPanel.SetActive(true);
    }

    void Update()
    {
        if(isDataRecieved)
        {
            ScreenSaverOff();
            CancelInvoke("ScreenSaverOn");
            Invoke("ScreenSaverOn", 10);
            personName.text = "Rbve, " + dataBase.GetValue(RFIDuser, nameIndex);
            personRegion.text = dataBase.GetValue(RFIDuser, regionIndex);
            //personID.text = dataBase.GetValue(RFIDuser, regionIndex);
            image.sprite = Resources.Load<Sprite>("Photos/" + dataBase.GetValue(RFIDuser, photoNameIndex));         
            isDataRecieved = false;
            mainInputField.text = "";
        }        
        DataReciever();

    }
    
    public void DataReciever()
    {
        if (mainInputField.text.Length == 10)
        {
            RFIDuser = mainInputField.text;
            isDataRecieved = true;
        }     
    }

    private void ScreenSaverOn()
    {
        screenSaverPanelCG.DOFade(1, transitionTime);
    }

    private void ScreenSaverOff()
    {
        screenSaverPanelCG.DOFade(0, transitionTime);
    }

 


}
