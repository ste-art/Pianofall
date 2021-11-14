using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Interfaces;
using System;
using UnityEngine.Events;

namespace UI
{
    public class ModalWindow : Initable
    {
        public GameObject ModalPanel;
        public Text MessageText;
        public Button OkButton;
        public Button CancelButton;

        private static Lazy<ModalWindow> _instance = Singleton<ModalWindow>.Get();
        public static ModalWindow Instance => _instance.Value;

        public override void Init()
        {
            ModalPanel = transform.Find("Background Blocker").gameObject;
            MessageText = transform.Find("Background Blocker/Dialogue Panel/Text").GetComponent<Text>();
            OkButton = transform.Find("Background Blocker/Dialogue Panel/Buttons/OK Button").GetComponent<Button>();
            CancelButton = transform.Find("Background Blocker/Dialogue Panel/Buttons/Cancel Button").GetComponent<Button>();
        }

        public void ShowDialog(string message, UnityAction OnOk = null, UnityAction OnCancel = null)
        {
            ModalPanel.SetActive(true);
            MessageText.text = message;
            AppendListener(OkButton, OnOk);
            AppendListener(CancelButton, OnCancel);
        }

        public void AppendListener(Button button, UnityAction action)
        {
            button.onClick.RemoveAllListeners();
            if (action != null)
            {
                button.onClick.AddListener(action);
            }
            button.onClick.AddListener(ClosePanel);
        }

        void ClosePanel()
        {
            ModalPanel.SetActive(false);
        }

        public void Start()
        {
            ClosePanel();
        }
    }
}
