using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class UIManager : MonoBehaviour
{
    [Header("Popus UI")]
    [SerializeField] private GameObject MainPopup_Object;
    [Header("Disconnection Popup")]
    [SerializeField] private Button CloseDisconnect_Button;
    [SerializeField] private GameObject DisconnectPopup_Object;
    [Header("AnotherDevice Popup")]
    [SerializeField] private GameObject ADPopup_Object;
    [Header("LowBalance Popup")]
    [SerializeField] private Button LBExit_Button;
    [SerializeField] private GameObject LBPopup_Object;
    [Header("Audio Objects")]
    [SerializeField] private GameObject Settings_Object;
    [SerializeField] private Button SettingsQuit_Button;
    [SerializeField] private Button Sound_Button;
    [SerializeField] private Button Music_Button;
    [SerializeField] private RectTransform SoundToggle_RT;
    [SerializeField] private RectTransform MusicToggle_RT;
    [Header("Paytable Objects")]
    [SerializeField] private GameObject PaytableMenuObject;
    [SerializeField] private Button Paytable_Button;
    [SerializeField] private Button PaytableClose_Button;
    [SerializeField] private Button PaytableLeft_Button;
    [SerializeField] private Button PaytableRight_Button;
    [SerializeField] private List<GameObject> GameRulesPages = new();
    [SerializeField] private TMP_Text BlueFS_Text;
    [SerializeField] private TMP_Text PurpleFS_Text;

    [Header("Game Quit Objects")]
    [SerializeField] private Button Quit_Button;
    [SerializeField] private Button QuitYes_Button;
    [SerializeField] private Button QuitNo_Button;
    [SerializeField] private GameObject QuitMenuObject;

    [Header("Menu Objects")]
    [SerializeField] private Button Menu_Button;
    [SerializeField] private Sprite Menu_Image, MenuHover_Image, MenuPressed_Image, CloseMenu_image, CloseMenuHover_Image, CloseMenuPressed_Image;
    [SerializeField] private Button Info_Button;
    [SerializeField] private Button Settings_Button;
    [SerializeField] private Button RaycastLayerButton;
    [SerializeField] private RectTransform Info_BttnTransform;
    [SerializeField] private RectTransform Settings_BttnTransform;

    [Header("Paytable Slot Text")]
    [SerializeField] private List<TMP_Text> SymbolsText = new();

    [Header("Major Payout UI")]
    [SerializeField] private TMP_Text Minor_Text;
    [SerializeField] private TMP_Text Major_Text;
    [SerializeField] private TMP_Text Grand_Text;
    [SerializeField] private TMP_Text JokerMinor_Text;
    [SerializeField] private TMP_Text JokerMajor_Text;
    [SerializeField] private TMP_Text JokerGrand_Text;

    [Header("Controllers")]
    [SerializeField] private AudioController audioController;
    [SerializeField] private SlotBehaviour slotManager;
    [SerializeField] private SocketIOManager socketManager;
    private int PageIndex;
    private bool isMusic = true;
    private bool isSound = true;
    private bool isExit = false;
    private bool isMenu = false;


    private void Start()
    {
        if (RaycastLayerButton) RaycastLayerButton.onClick.RemoveAllListeners();
        if (RaycastLayerButton) RaycastLayerButton.onClick.AddListener(() => CanCloseMenu());

        if (LBExit_Button) LBExit_Button.onClick.RemoveAllListeners();
        if (LBExit_Button) LBExit_Button.onClick.AddListener(delegate { ClosePopup(LBPopup_Object); });

        if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.RemoveAllListeners();
        if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.AddListener(delegate { CallOnExitFunction(); socketManager.closeSocketReactnativeCall(); });

        if (Sound_Button) Sound_Button.onClick.RemoveAllListeners();
        if (Sound_Button) Sound_Button.onClick.AddListener(delegate
        {
            Debug.Log("Here");
            if (isSound)
            {
                SoundOnOFF(false);
            }
            else
            {
                SoundOnOFF(true);
            }
        });

        if (Music_Button) Music_Button.onClick.RemoveAllListeners();
        if (Music_Button) Music_Button.onClick.AddListener(delegate
        {

            if (isMusic)
            {
                MusicONOFF(false);
            }
            else
            {
                MusicONOFF(true);
            }
        });

        if (Quit_Button) Quit_Button.onClick.RemoveAllListeners();
        if (Quit_Button) Quit_Button.onClick.AddListener(OpenQuitPanel);

        if (QuitNo_Button) QuitNo_Button.onClick.RemoveAllListeners();
        if (QuitNo_Button) QuitNo_Button.onClick.AddListener(delegate { ClosePopup(QuitMenuObject); });

        if (QuitYes_Button) QuitYes_Button.onClick.RemoveAllListeners();
        if (QuitYes_Button) QuitYes_Button.onClick.AddListener(CallOnExitFunction);

        if (Paytable_Button) Paytable_Button.onClick.RemoveAllListeners();
        if (Paytable_Button) Paytable_Button.onClick.AddListener(OpenPaytablePanel);

        if (PaytableClose_Button) PaytableClose_Button.onClick.RemoveAllListeners();
        if (PaytableClose_Button) PaytableClose_Button.onClick.AddListener(delegate { ClosePopup(PaytableMenuObject); });

        if (Menu_Button) Menu_Button.onClick.RemoveAllListeners();
        if (Menu_Button) Menu_Button.onClick.AddListener(delegate
        {
            if (!isMenu)
            {
                OpenCloseMenu(true);
            }
            else
            {
                OpenCloseMenu(false);
            }
        });



        if (Settings_Button) Settings_Button.onClick.RemoveAllListeners();
        if (Settings_Button) Settings_Button.onClick.AddListener(OpenSettingsPanel);

        if (SettingsQuit_Button) SettingsQuit_Button.onClick.RemoveAllListeners();
        if (SettingsQuit_Button) SettingsQuit_Button.onClick.AddListener(delegate { ClosePopup(Settings_Object); });

        if (PaytableLeft_Button) PaytableLeft_Button.onClick.RemoveAllListeners();
        if (PaytableLeft_Button) PaytableLeft_Button.onClick.AddListener(() => ChangePage(false));

        if (PaytableRight_Button) PaytableRight_Button.onClick.RemoveAllListeners();
        if (PaytableRight_Button) PaytableRight_Button.onClick.AddListener(() => ChangePage(true));
    }

    internal void CanCloseMenu()
    {
        if (isMenu)
        {
            OpenCloseMenu(false);
        }
    }

    private void ChangePage(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();

        if (IncDec)
        {
            PageIndex++;
            if (PageIndex == GameRulesPages.Count)
            {
                PageIndex = 0;
            }
        }
        else
        {
            PageIndex--;
            if (PageIndex < 0)
            {
                PageIndex = GameRulesPages.Count - 1;
            }
        }
        foreach (GameObject g in GameRulesPages)
        {
            g.SetActive(false);
        }
        if (GameRulesPages[PageIndex]) GameRulesPages[PageIndex].SetActive(true);
    }

    private void OpenCloseMenu(bool toggle)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (toggle)
        {
            isMenu = true;

            SpriteState state = new SpriteState
            {
                highlightedSprite = CloseMenuHover_Image,
                pressedSprite = CloseMenuPressed_Image
            };
            Menu_Button.image.sprite = CloseMenu_image;
            Menu_Button.spriteState = state;

            if (Info_Button) Info_Button.gameObject.SetActive(true);
            if (Settings_Button) Settings_Button.gameObject.SetActive(true);

            DOTween.To(() => Info_BttnTransform.anchoredPosition, (val) => Info_BttnTransform.anchoredPosition = val, new Vector2(Info_BttnTransform.anchoredPosition.x, Info_BttnTransform.anchoredPosition.y + 150), 0.1f).OnUpdate(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(Info_BttnTransform);
            });

            DOTween.To(() => Settings_BttnTransform.anchoredPosition, (val) => Settings_BttnTransform.anchoredPosition = val, new Vector2(Settings_BttnTransform.anchoredPosition.x, Settings_BttnTransform.anchoredPosition.y + 300), 0.1f).OnUpdate(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(Settings_BttnTransform);
            });
        }
        else
        {
            isMenu = false;

            SpriteState state = new SpriteState
            {
                highlightedSprite = MenuHover_Image,
                pressedSprite = MenuPressed_Image
            };
            Menu_Button.image.sprite = Menu_Image;
            Menu_Button.spriteState = state;

            DOTween.To(() => Info_BttnTransform.anchoredPosition, (val) => Info_BttnTransform.anchoredPosition = val, new Vector2(Info_BttnTransform.anchoredPosition.x, Info_BttnTransform.anchoredPosition.y - 150), 0.1f).OnUpdate(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(Info_BttnTransform);
            });

            DOTween.To(() => Settings_BttnTransform.anchoredPosition, (val) => Settings_BttnTransform.anchoredPosition = val, new Vector2(Settings_BttnTransform.anchoredPosition.x, Settings_BttnTransform.anchoredPosition.y - 300), 0.1f).OnUpdate(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(Settings_BttnTransform);
            });

            DOVirtual.DelayedCall(0.1f, () =>
            {
                if (Info_Button) Info_Button.gameObject.SetActive(false);
                if (Settings_Button) Settings_Button.gameObject.SetActive(false);
            });
        }
    }

    private void SoundOnOFF(bool state)
    {
        if (state)
        {
            isSound = true;
            audioController.ToggleMute(!state, "sound");
            DOTween.To(() => SoundToggle_RT.anchoredPosition, (val) => SoundToggle_RT.anchoredPosition = val, new Vector2(SoundToggle_RT.anchoredPosition.x + 95, SoundToggle_RT.anchoredPosition.y), 0.1f).OnUpdate(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(Info_BttnTransform);
            });
        }
        else
        {
            isSound = false;
            audioController.ToggleMute(!state, "sound");
            DOTween.To(() => SoundToggle_RT.anchoredPosition, (val) => SoundToggle_RT.anchoredPosition = val, new Vector2(SoundToggle_RT.anchoredPosition.x - 95, SoundToggle_RT.anchoredPosition.y), 0.1f).OnUpdate(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(Info_BttnTransform);
            });
        }
    }

    private void MusicONOFF(bool state)
    {
        if (state)
        {
            isMusic = true;
            audioController.ToggleMute(!state, "music");
            DOTween.To(() => MusicToggle_RT.anchoredPosition, (val) => MusicToggle_RT.anchoredPosition = val, new Vector2(MusicToggle_RT.anchoredPosition.x + 95, MusicToggle_RT.anchoredPosition.y), 0.1f).OnUpdate(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(Info_BttnTransform);
            });
        }
        else
        {
            isMusic = false;
            audioController.ToggleMute(!state, "music");
            DOTween.To(() => MusicToggle_RT.anchoredPosition, (val) => MusicToggle_RT.anchoredPosition = val, new Vector2(MusicToggle_RT.anchoredPosition.x - 95, MusicToggle_RT.anchoredPosition.y), 0.1f).OnUpdate(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(Info_BttnTransform);
            });
        }
    }

    private void OpenSettingsPanel()
    {
        if (audioController) audioController.PlayButtonAudio();
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
        if (Settings_Object) Settings_Object.SetActive(true);
        CanCloseMenu();
    }

    private void OpenQuitPanel()
    {
        if (audioController) audioController.PlayButtonAudio();
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
        if (QuitMenuObject) QuitMenuObject.SetActive(true);
        CanCloseMenu();
    }

    private void OpenPaytablePanel()
    {
        if (audioController) audioController.PlayButtonAudio();

        if (MainPopup_Object) MainPopup_Object.SetActive(true);

        PageIndex = 0;

        foreach (GameObject g in GameRulesPages)
        {
            g.SetActive(false);
        }

        GameRulesPages[0].SetActive(true);
        if (PaytableLeft_Button) PaytableLeft_Button.interactable = true;
        if (PaytableRight_Button) PaytableRight_Button.interactable = true;

        if (PaytableMenuObject) PaytableMenuObject.SetActive(true);

        CanCloseMenu();
    }

    internal void LowBalPopup()
    {
        CanCloseMenu();
        OpenPopup(LBPopup_Object);
    }

    internal void DisconnectionPopup(bool isReconnection)
    {
        if (!isExit)
        {
            CanCloseMenu();
            OpenPopup(DisconnectPopup_Object);
        }
    }

    internal void ADfunction()
    {
        OpenPopup(ADPopup_Object);
    }

    internal void InitialiseUIData(string SupportUrl, string AbtImgUrl, string TermsUrl, string PrivacyUrl, Paylines symbolsText)
    {
        StartCoroutine(DownloadImage(AbtImgUrl));
        PopulateSymbolsPayout(symbolsText);
        SetLargePayoutUI();
    }

    internal void SetLargePayoutUI()
    {
        Minor_Text.text = socketManager.initialData.Joker[0].ToString() + "x";
        Major_Text.text = socketManager.initialData.Joker[1].ToString() + "x";
        Grand_Text.text = socketManager.initialData.Joker[2].ToString() + "x";
        JokerMinor_Text.text = socketManager.initialData.Joker[0].ToString() + "x";
        JokerMajor_Text.text = socketManager.initialData.Joker[1].ToString() + "x";
        JokerGrand_Text.text = socketManager.initialData.Joker[2].ToString() + "x";
    }

    internal void PopulateSymbolsPayout(Paylines paylines)
    {
        for (int i = 0; i < SymbolsText.Count; i++)
        {
            string text = null;
            if (paylines.symbols[i + 1].payout != 0)
            {
                // Debug.Log("symbol name " + paylines.symbols[i+1].Name + " amt : " + paylines.symbols[i+1].payout.ToString());
                text = paylines.symbols[i + 1].payout.ToString() + "x";
            }
            SymbolsText[i].text = text;
        }

        foreach (Symbol symbol in paylines.symbols)
        {
            if (symbol.Name == "ScatterBlue")
            {
                BlueFS_Text.text = symbol.description.ToString();
            }
            else if (symbol.Name == "ScatterPurple")
            {
                PurpleFS_Text.text = symbol.description.ToString();
            }
        }
    }

    private void CallOnExitFunction()
    {
        isExit = true;
        audioController.PlayButtonAudio();
        slotManager.CallCloseSocket();
    }

    internal void OpenPopup(GameObject Popup)
    {
        if (audioController && Popup != MainPopup_Object.transform.GetChild(1).gameObject) audioController.PlayButtonAudio();

        if (Popup) Popup.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
    }

    internal void ClosePopup(GameObject Popup)
    {
        if (audioController && Popup != MainPopup_Object.transform.GetChild(1).gameObject) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(false);
        if (!DisconnectPopup_Object.activeSelf)
        {
            if (MainPopup_Object) MainPopup_Object.SetActive(false);
        }
    }

    private void UrlButtons(string url)
    {
        Application.OpenURL(url);
    }

    private IEnumerator DownloadImage(string url)
    {
        // Create a UnityWebRequest object to download the image
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

        // Wait for the download to complete
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            // Apply the sprite to the target image
            //AboutLogo_Image.sprite = sprite;
        }
        else
        {
            Debug.LogError("Error downloading image: " + request.error);
        }
    }
}
