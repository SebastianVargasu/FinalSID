using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationHolder : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameField;

    public void SetName(string name)
    {
        nameField.text = name;
        Destroy(gameObject, 4f);
    }
}
