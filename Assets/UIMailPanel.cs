using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMailPanel : MonoBehaviourSingleton<UIMailPanel>
{
    [Header("Panels")]
    public Transform mailBoxPanel;
    public Transform mailReadPanel;
    public Transform mailWritePanel;
    
    
    [Header("Mail Box")]
    public GameObject mailBoxPrefab;
    public Transform mailBoxContent;
    public Button mailBoxWriteButton;
    
    [Header("Mail Read")]
    public TMP_Text mailReadSender;
    public TMP_Text mailReadTitle;
    public TMP_Text mailReadMessage;
    public Button mailReadReplyButton;
    
    [Header("Mail Write")]
    public TMP_InputField mailWriteTitleInput;  
    public TMP_InputField mailWriteMessageInput;
    public TMP_InputField mailWriteToInput;
    public Button mailWriteSendButton;


    private void Start()
    {
        mailBoxPanel.gameObject.SetActive(false);
        mailReadPanel.gameObject.SetActive(false);
        mailWritePanel.gameObject.SetActive(false);

        mailBoxWriteButton.onClick.AddListener(OpenMailWritePanel);
        mailReadReplyButton.onClick.AddListener(OpenMailWritePanel);
        //mailWriteSendButton.onClick.AddListener(==============> SendMail);
    }

    public void OpenMailBoxPanel()
    {
        mailBoxPanel.gameObject.SetActive(true);
        mailReadPanel.gameObject.SetActive(false);
        mailWritePanel.gameObject.SetActive(false);
    }
    public void CloseMailBoxPanel()
    {
        mailBoxPanel.gameObject.SetActive(false);
        mailReadPanel.gameObject.SetActive(false);
        mailWritePanel.gameObject.SetActive(false);
    }
    
    public void OpenMailReadPanel(string sender, string title, string message)
    {
        mailBoxPanel.gameObject.SetActive(false);
        mailReadPanel.gameObject.SetActive(true);
        mailWritePanel.gameObject.SetActive(false);
        
        mailReadSender.text = sender;
        mailReadTitle.text = title;
        mailReadMessage.text = message;
    }
    public void OpenMailWritePanel()
    {
        mailBoxPanel.gameObject.SetActive(false);
        mailReadPanel.gameObject.SetActive(false);
        mailWritePanel.gameObject.SetActive(true);
        
        mailWriteTitleInput.text = "";
        mailWriteMessageInput.text = "";
        mailWriteToInput.text = "";
    }
    
    public void AddMailToBox(string sender, string title, bool isRead = false)
    {
        GameObject mailBox = Instantiate(mailBoxPrefab, mailBoxContent);
        MailinMailBox mailinMailBox = mailBox.GetComponent<MailinMailBox>();
        mailinMailBox.sender.text = sender;
        mailinMailBox.titleText.text = title;
        mailinMailBox.seenIcon.SetActive(isRead);
        
        // Optionally, you can add a listener to open the mail when clicked
        mailinMailBox.mailOpenButton.onClick.AddListener(() => OpenMailReadPanel(sender, title, "This is a sample message."));
    }
    
    
}
