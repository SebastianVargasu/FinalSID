using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogErrorPanel : MonoBehaviour
{
    public static LogErrorPanel instance;
    [SerializeField] TextMeshProUGUI logText;
    [SerializeField] GameObject logPanel;


    private void Awake() {
        if(instance != null){
            Destroy(this);
        }else{
            instance = this;
        }
    }

    public void LogError(string error){
        logText.text = error;
        logPanel.SetActive(true);
    }

    public void ClosePanel(){
        logPanel.SetActive(false);
    }
}
