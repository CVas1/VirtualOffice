using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Assets.Scripts.Networking;
using UnityEngine.Serialization;

namespace Assets.Scripts.UI
{
    public class AuthUIManager : MonoBehaviour
    {
        [Header("Menus")] public GameObject signInMenu;
        public GameObject registerMenu;

        [Header("Sign In Fields")] public TMP_InputField signInMailInput;
        public TMP_InputField signInPasswordInput;
        public TMP_Text signInErrorText;

        [Header("Register Fields")] public TMP_InputField registerMailInput;
        public TMP_InputField registerPasswordInput;
        public TMP_InputField registerUsernameInput;
        public TMP_Text registerErrorText;

        [Header("Connection Status")] [SerializeField]
        private Image connectionStatusImage;

        private void Awake()
        {
            // Load last used email if available
            if (PlayerPrefs.HasKey(STDBAuthManager.LastUsedEmailKey))
            {
                string lastEmail = PlayerPrefs.GetString(STDBAuthManager.LastUsedEmailKey);
                signInMailInput.text = lastEmail;
            }

            // Subscribe to auth events
            STDBAuthManager.OnAuthenticationStateChanged += OnAuthStateChanged;
            STDBAuthManager.OnAuthenticationError += OnAuthError;

            STDBBackendManager.OnConnected += () => SetConnectionStatus(true);
            STDBBackendManager.OnDisconnected += () => SetConnectionStatus(false);
        }

        private void OnDestroy()
        {
            STDBAuthManager.OnAuthenticationStateChanged -= OnAuthStateChanged;
            STDBAuthManager.OnAuthenticationError -= OnAuthError;
        }

        public void OnClickShowRegister()
        {
            signInMenu.SetActive(false);
            registerMenu.SetActive(true);
            registerErrorText.gameObject.SetActive(false);
        }

        public void OnClickShowSignIn()
        {
            registerMenu.SetActive(false);
            signInMenu.SetActive(true);
            signInErrorText.gameObject.SetActive(false);
        }

        public void OnClickLogin()
        {
            string mail = signInMailInput.text.Trim();
            string password = signInPasswordInput.text;

            if (string.IsNullOrEmpty(mail) || !mail.Contains("@"))
            {
                signInErrorText.text = "Invalid email address.";
                signInErrorText.gameObject.SetActive(true);
                return;
            }

            if (password.Length < 6)
            {
                signInErrorText.text = "Password must be at least 6 characters.";
                signInErrorText.gameObject.SetActive(true);
                return;
            }

            signInErrorText.gameObject.SetActive(false);
            STDBBackendManager.Instance.authManager.Login(mail, password);
        }

        public void OnClickRegister()
        {
            string mail = registerMailInput.text.Trim();
            string username = registerUsernameInput.text.Trim();
            string password = registerPasswordInput.text.Trim();

            if (string.IsNullOrEmpty(mail) || !mail.Contains("@"))
            {
                registerErrorText.text = "Invalid email address.";
                registerErrorText.gameObject.SetActive(true);
                return;
            }

            if (username.Length < 3)
            {
                registerErrorText.text = "Username must be at least 3 characters.";
                registerErrorText.gameObject.SetActive(true);
                return;
            }

            if (password.Length < 6)
            {
                registerErrorText.text = "Password must be at least 6 characters.";
                registerErrorText.gameObject.SetActive(true);
                return;
            }

            registerErrorText.gameObject.SetActive(false);
            STDBBackendManager.Instance.authManager.Register(mail, username, password);
        }

        private void OnAuthStateChanged(bool loggedIn)
        {
            if (loggedIn)
            {
                // Successful login, load main menu
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                // Successful logout, show sign in
                OnClickShowSignIn();
            }
        }

        private void OnAuthError(string error)
        {
            // Show error on the active menu
            if (registerMenu.activeSelf)
            {
                registerErrorText.text = error;
                registerErrorText.gameObject.SetActive(true);
            }
            else
            {
                signInErrorText.text = error;
                signInErrorText.gameObject.SetActive(true);
            }
        }

        public void SetConnectionStatus(bool isConnected)
        {
            if (isConnected)
            {
                connectionStatusImage.color = Color.green;
            }
            else
            {
                connectionStatusImage.color = Color.red;
            }
        }
    }
}