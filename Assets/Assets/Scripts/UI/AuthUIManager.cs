using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Assets.Scripts.Networking;

namespace Assets.Scripts.UI
{
    public class AuthUIManager : MonoBehaviour
    {
        [Header("Menus")] public GameObject signInMenu;
        public GameObject registerMenu;

        [Header("Sign In Fields")] public TMP_InputField signInUsernameInput;
        public TMP_InputField signInPasswordInput;
        public TMP_Text signInErrorText;

        [Header("Register Fields")] public TMP_InputField registerUsernameInput;
        public TMP_InputField registerPasswordInput;
        public TMP_Text registerErrorText;

        private void Awake()
        {
            // Subscribe to auth events
            STDBAuthManager.OnAuthenticationStateChanged += OnAuthStateChanged;
            STDBAuthManager.OnAuthenticationError += OnAuthError;
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
            string username = signInUsernameInput.text.Trim();
            string password = signInPasswordInput.text;
            if (username.Length < 3)
            {
                signInErrorText.text = "Username must be at least 3 characters.";
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
            STDBBackendManager.Instance.authManager.Login(username, password);
        }

        public void OnClickRegister()
        {
            string username = registerUsernameInput.text.Trim();
            string password = registerPasswordInput.text;
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
            STDBBackendManager.Instance.authManager.Register(username, password);
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
    }
}