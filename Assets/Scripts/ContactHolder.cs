using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ContactHolder : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI contectName;

    public void SetContactName(string name)
    {
        contectName.text = name;
    }
}
