using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailinMailBox : MonoBehaviour
{
    public TMP_Text sender;
    public TMP_Text titleText;
    public GameObject seenIcon;
    public Button mailOpenButton;
    public Button deleteButton;
    
    

    private void Start()
    {
        mailOpenButton.onClick.AddListener(MailOpen);
        deleteButton.onClick.AddListener(DeleteMail);
    }

    private void DeleteMail()
    {
        print("Mail deleted: " + titleText.text);
        Destroy(gameObject);
    }

    private void MailOpen()
    {
        UIMailPanel.Instance.OpenMailReadPanel(sender.text, titleText.text, "This is a sample message for the mail titled lorem ipsumi zortumi forsumi");
    }
}