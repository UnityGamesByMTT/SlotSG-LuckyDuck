using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using UnityEngine.Networking;

public class UIManager : MonoBehaviour
{

    [Header("Menu UI")]
    [SerializeField]
    private Button Menu_Button;
    [SerializeField]
    private GameObject Menu_Object;

    [Header("Settings UI")]
    [SerializeField]
    private Button Settings_Button;
    [SerializeField]
    private GameObject Settings_Object;
    [SerializeField]
    private RectTransform Settings_RT;
    [SerializeField]
    private Button Terms_Button;
    [SerializeField]
    private Button Privacy_Button;

    [SerializeField]
    private Button Exit_Button;
    [SerializeField]
    private GameObject Exit_Object;
    [SerializeField]
    private RectTransform Exit_RT;

    [SerializeField]
    private Button Paytable_Button;
    [SerializeField] private GameObject Paytable_Object;
    [SerializeField] private RectTransform Paytable_RT;

    [Header("Popus UI")]
    [SerializeField]
    private GameObject MainPopup_Object;

    [Header("About Popup")]
    [SerializeField]
    private GameObject AboutPopup_Object;
    [SerializeField]
    private Button AboutExit_Button;
    [SerializeField]
    private Image AboutLogo_Image;
    [SerializeField]
    private Button Support_Button;

    [Header("Paytable Popup")]
    [SerializeField]
    private GameObject PaytablePopup_Object;
    [SerializeField]
    private Button PaytableExit_Button;
    [SerializeField]
    private TMP_Text[] SymbolsText;
    [SerializeField]
    private TMP_Text FreeSpin_Text;

    [SerializeField]
    private TMP_Text Wild_Text;

    [Header("Settings Popup")]
    [SerializeField]
    private GameObject SettingsPopup_Object;
    [SerializeField]
    private Button SettingsExit_Button;
    [SerializeField]
    private Slider Sound_Button;
    [SerializeField]
    private Slider Music_Button;

    [SerializeField]
    private GameObject MusicOn_Object;
    [SerializeField]
    private GameObject MusicOff_Object;
    [SerializeField]
    private GameObject SoundOn_Object;
    [SerializeField]
    private GameObject SoundOff_Object;

    [Header("Win Popup")]
    [SerializeField]
    private Sprite BigWin_Sprites;
    [SerializeField]
    private Sprite HugeWin_Sprites;
    [SerializeField]
    private Sprite MegaWin_Sprites;

    [SerializeField]
    private Image Win_Image;
    [SerializeField]
    private GameObject WinPopup_Object;
    [SerializeField]
    private TMP_Text Win_Text;

    [Header("FreeSpins Popup")]
    [SerializeField]
    private GameObject FreeSpinPopup_Object;
    [SerializeField]
    private TMP_Text Free_Text;
    [SerializeField]
    private Button FreeSpin_Button;

    [Header("Splash Screen")]
    [SerializeField]
    private GameObject Loading_Object;
    [SerializeField]
    private Image Loading_Image;
    [SerializeField]
    private TMP_Text Loading_Text;
    [SerializeField]
    private TMP_Text LoadPercent_Text;
    [SerializeField]
    private Button QuitSplash_button;

    [Header("Disconnection Popup")]
    [SerializeField]
    private Button CloseDisconnect_Button;
    [SerializeField]
    private GameObject DisconnectPopup_Object;

    [Header("AnotherDevice Popup")]
    [SerializeField]
    private Button CloseAD_Button;
    [SerializeField]
    private GameObject ADPopup_Object;

    [Header("Reconnection Popup")]
    [SerializeField]
    private TMP_Text reconnect_Text;
    [SerializeField]
    private GameObject ReconnectPopup_Object;

    [Header("LowBalance Popup")]
    [SerializeField]
    private Button LBExit_Button;
    [SerializeField]
    private GameObject LBPopup_Object;

    [Header("Quit Popup")]
    [SerializeField]
    private GameObject QuitPopup_Object;
    [SerializeField]
    private Button YesQuit_Button;
    [SerializeField]
    private Button NoQuit_Button;
    [SerializeField]
    private Button CrossQuit_Button;

    [SerializeField]
    private AudioController audioController;
    [SerializeField]
    private Button m_AwakeGameButton;

    [SerializeField]
    private Button GameExit_Button;

    [SerializeField]
    private SlotBehaviour slotManager;

    [SerializeField]
    private SocketIOManager socketManager;
    [SerializeField] private Button SkipWinAnimation;

    private bool isMusic = true;
    private bool isSound = true;
    private bool isExit = false;

    internal int FreeSpins;

    private Tween WinPopupTextTween;
    private Tween ClosePopupTween;
    private void Awake()
    {
        //if (Loading_Object) Loading_Object.SetActive(true);
        //StartCoroutine(LoadingRoutine());
        SimulateClickByDefault();
    }

    private IEnumerator LoadingRoutine()
    {
        StartCoroutine(LoadingTextAnimate());
        float imageFill = 0f;
        DOTween.To(() => imageFill, (val) => imageFill = val, 0.7f, 2f).OnUpdate(() =>
        {
            if (Loading_Image) Loading_Image.fillAmount = imageFill;
            if (LoadPercent_Text) LoadPercent_Text.text = (100 * imageFill).ToString("f0") + "%";
        });
        yield return new WaitForSecondsRealtime(2);
        yield return new WaitUntil(() => socketManager.isLoaded);
        DOTween.To(() => imageFill, (val) => imageFill = val, 1, 1f).OnUpdate(() =>
        {
            if (Loading_Image) Loading_Image.fillAmount = imageFill;
            if (LoadPercent_Text) LoadPercent_Text.text = (100 * imageFill).ToString("f0") + "%";
        });
        yield return new WaitForSecondsRealtime(1f);
        if (Loading_Object) Loading_Object.SetActive(false);
        StopCoroutine(LoadingTextAnimate());
    }

    private IEnumerator LoadingTextAnimate()
    {
        while (true)
        {
            if (Loading_Text) Loading_Text.text = "Loading.";
            yield return new WaitForSeconds(1f);
            if (Loading_Text) Loading_Text.text = "Loading..";
            yield return new WaitForSeconds(1f);
            if (Loading_Text) Loading_Text.text = "Loading...";
            yield return new WaitForSeconds(1f);
        }
    }

    private void Start()
    {

        if (Menu_Button) Menu_Button.onClick.RemoveAllListeners();
        if (Menu_Button) Menu_Button.onClick.AddListener(OpenMenu);

        if (Exit_Button) Exit_Button.onClick.RemoveAllListeners();
        if (Exit_Button) Exit_Button.onClick.AddListener(CloseMenu);

        if (AboutExit_Button) AboutExit_Button.onClick.RemoveAllListeners();
        if (AboutExit_Button) AboutExit_Button.onClick.AddListener(delegate { ClosePopup(AboutPopup_Object); });

        if (Paytable_Button) Paytable_Button.onClick.RemoveAllListeners();
        if (Paytable_Button) Paytable_Button.onClick.AddListener(delegate { OpenPopup(PaytablePopup_Object); });

        if (PaytableExit_Button) PaytableExit_Button.onClick.RemoveAllListeners();
        if (PaytableExit_Button) PaytableExit_Button.onClick.AddListener(delegate { ClosePopup(PaytablePopup_Object); });

        if (Settings_Button) Settings_Button.onClick.RemoveAllListeners();
        if (Settings_Button) Settings_Button.onClick.AddListener(delegate { OpenPopup(SettingsPopup_Object); });

        if (SettingsExit_Button) SettingsExit_Button.onClick.RemoveAllListeners();
        if (SettingsExit_Button) SettingsExit_Button.onClick.AddListener(delegate { ClosePopup(SettingsPopup_Object); });

        if (MusicOn_Object) MusicOn_Object.SetActive(true);
        if (MusicOff_Object) MusicOff_Object.SetActive(false);

        if (SoundOn_Object) SoundOn_Object.SetActive(true);
        if (SoundOff_Object) SoundOff_Object.SetActive(false);

        if (GameExit_Button) GameExit_Button.onClick.RemoveAllListeners();
        if (GameExit_Button) GameExit_Button.onClick.AddListener(delegate
        {
            OpenPopup(QuitPopup_Object);
            Debug.Log("Quit event: pressed Big_X button");

        });

        if (NoQuit_Button) NoQuit_Button.onClick.RemoveAllListeners();
        if (NoQuit_Button) NoQuit_Button.onClick.AddListener(delegate
        {
            if (!isExit)
            {
                ClosePopup(QuitPopup_Object);
                Debug.Log("quit event: pressed NO Button ");
            }
        });

        if (CrossQuit_Button) CrossQuit_Button.onClick.RemoveAllListeners();
        if (CrossQuit_Button) CrossQuit_Button.onClick.AddListener(delegate
        {
            if (!isExit)
            {
                ClosePopup(QuitPopup_Object);
                Debug.Log("quit event: pressed Small_X Button ");

            }
        });

        if (LBExit_Button) LBExit_Button.onClick.RemoveAllListeners();
        if (LBExit_Button) LBExit_Button.onClick.AddListener(delegate { ClosePopup(LBPopup_Object); });

        if (YesQuit_Button) YesQuit_Button.onClick.RemoveAllListeners();
        if (YesQuit_Button) YesQuit_Button.onClick.AddListener(delegate
        {
            CallOnExitFunction();
            Debug.Log("quit event: pressed YES Button ");

        });

        if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.RemoveAllListeners();
        if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.AddListener(delegate { CallOnExitFunction(); socketManager.ReactNativeCallOnFailedToConnect(); }); //BackendChanges

        if (CloseAD_Button) CloseAD_Button.onClick.RemoveAllListeners();
        if (CloseAD_Button) CloseAD_Button.onClick.AddListener(CallOnExitFunction);

        // if (FreeSpin_Button) FreeSpin_Button.onClick.RemoveAllListeners();
        // if (FreeSpin_Button) FreeSpin_Button.onClick.AddListener(delegate { StartFreeSpins(FreeSpins); });

        if (QuitSplash_button) QuitSplash_button.onClick.RemoveAllListeners();
        if (QuitSplash_button) QuitSplash_button.onClick.AddListener(delegate { OpenPopup(QuitPopup_Object); });

        // if (audioController) audioController.ChangeVol(false);

        isMusic = true;
        isSound = true;

        if (Sound_Button) Sound_Button.onValueChanged.RemoveAllListeners();
        if (Sound_Button) Sound_Button.onValueChanged.AddListener(ToggleSound);

        if (Music_Button) Music_Button.onValueChanged.RemoveAllListeners();
        if (Music_Button) Music_Button.onValueChanged.AddListener(ToggleMusic);

        if (SkipWinAnimation) SkipWinAnimation.onClick.RemoveAllListeners();
        if (SkipWinAnimation) SkipWinAnimation.onClick.AddListener(SkipWin);

    }

    void SkipWin()
    {
        Debug.Log("Skip win called");
        if (ClosePopupTween != null)
        {
            ClosePopupTween.Kill();
            ClosePopupTween = null;
        }
        if (WinPopupTextTween != null)
        {
            WinPopupTextTween.Kill();
            WinPopupTextTween = null;
        }
        ClosePopup(WinPopup_Object);
        slotManager.CheckPopups = false;
    }

    private void SimulateClickByDefault()
    {
        Debug.Log("Awaken The Game...");
        m_AwakeGameButton.onClick.AddListener(() => { Debug.Log("Called The Game..."); });
        m_AwakeGameButton.onClick.Invoke();
    }

    internal void LowBalPopup()
    {
        OpenPopup(LBPopup_Object);
    }

    internal void DisconnectionPopup(bool isReconnection)
    {
        //if(isReconnection)
        //{
        //    OpenPopup(ReconnectPopup_Object);
        //}
        //else
        //{
        //    ClosePopup(ReconnectPopup_Object);
        //}

        if (!isExit)
        {
            OpenPopup(DisconnectPopup_Object);
        }
    }

    internal void PopulateWin(int value, double amount)
    {
        switch (value)
        {
            case 1:
                Win_Image.sprite = BigWin_Sprites;
                break;
            case 2:
                Win_Image.sprite = HugeWin_Sprites;
                break;
            case 3:
                Win_Image.sprite = MegaWin_Sprites;
                break;

        }

        StartPopupAnim(amount);
    }


    private void StartFreeSpins(int spins)
    {
        if (MainPopup_Object) MainPopup_Object.SetActive(false);
        if (FreeSpinPopup_Object) FreeSpinPopup_Object.SetActive(false);
        slotManager.FreeSpin(spins);
    }
    internal void FreeSpinProcess(int spins)
    {
        int ExtraSpins=spins-FreeSpins;
        FreeSpins=spins;
        slotManager.totalFreeSpins+=ExtraSpins;
        Debug.Log("ExtraSpins: " +ExtraSpins);
        Debug.Log("Total Spins: " +spins);
        if (FreeSpinPopup_Object) FreeSpinPopup_Object.SetActive(true);           
        if (Free_Text) Free_Text.text = ExtraSpins.ToString() + " Free spins awarded.";
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
        DOVirtual.DelayedCall(2f, ()=>{
            StartFreeSpins(spins);
        });
    }

    internal void CloseFreeSpinPopUP()
    {
        // if (MainPopup_Object) MainPopup_Object.SetActive(false);
        // if (FreeSpinPopup_Object) FreeSpinPopup_Object.SetActive(false);
        ClosePopup(FreeSpinPopup_Object);
    }
    private void StartPopupAnim(double amount)
    {
        double initAmount = 0;
        if (WinPopup_Object) WinPopup_Object.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
        Win_Image.transform.DOPunchScale(punch: new Vector3(0.5f, 0.5f, 0), duration: 2f, vibrato: 3, elasticity: 1);
        WinPopupTextTween = DOTween.To(() => initAmount, (val) => initAmount = val, amount, 3f).OnUpdate(() =>
        {
            if (Win_Text) Win_Text.text = initAmount.ToString("f3");
        });

        ClosePopupTween = DOVirtual.DelayedCall(4f, () =>
        {
            ClosePopup(WinPopup_Object);
            slotManager.CheckPopups = false;
        });
    }

    internal void ADfunction()
    {
        OpenPopup(ADPopup_Object);
    }

    internal void InitialiseUIData(string SupportUrl, string AbtImgUrl, string TermsUrl, string PrivacyUrl, Paylines symbolsText)
    {
        if (Support_Button) Support_Button.onClick.RemoveAllListeners();
        if (Support_Button) Support_Button.onClick.AddListener(delegate { UrlButtons(SupportUrl); });

        if (Terms_Button) Terms_Button.onClick.RemoveAllListeners();
        if (Terms_Button) Terms_Button.onClick.AddListener(delegate { UrlButtons(TermsUrl); });

        if (Privacy_Button) Privacy_Button.onClick.RemoveAllListeners();
        if (Privacy_Button) Privacy_Button.onClick.AddListener(delegate { UrlButtons(PrivacyUrl); });

        StartCoroutine(DownloadImage(AbtImgUrl));
        PopulateSymbolsPayout(symbolsText);
    }

    private void PopulateSymbolsPayout(Paylines paylines)
    {
        for (int i = 0; i < SymbolsText.Length; i++)
        {
            string text = null;
            if (paylines.symbols[i].Multiplier[0][0] != 0)
            {
                text += "all - " + (9 * paylines.symbols[i].Multiplier[0][0]) + "X";
                text += "\n5x - " + paylines.symbols[i].Multiplier[0][0] + "X";
            }
            if (paylines.symbols[i].Multiplier[1][0] != 0)
            {
                text += "\n4x - " + paylines.symbols[i].Multiplier[1][0] + "X";
            }
            if (paylines.symbols[i].Multiplier[2][0] != 0)
            {
                text += "\n3x - " + paylines.symbols[i].Multiplier[2][0] + "X";
            }
            if (SymbolsText[i]) SymbolsText[i].text = text;
        }

        for (int i = 0; i < paylines.symbols.Count; i++)
        {
            if (paylines.symbols[i].Name.ToUpper() == "FREESPIN")
            {
                if (FreeSpin_Text) FreeSpin_Text.text = paylines.symbols[i].description.ToString();
            }
            if (paylines.symbols[i].Name.ToUpper() == "WILD")
            {
                if (Wild_Text) Wild_Text.text = paylines.symbols[i].description.ToString();
            }
        }
    }

    private void CallOnExitFunction()
    {
        isExit = true;
        audioController.PlayButtonAudio();
        slotManager.CallCloseSocket();
    }

    private void OpenMenu()
    {
        audioController.PlayButtonAudio();
        if (Menu_Object) Menu_Object.SetActive(false);
        if (Exit_Object) Exit_Object.SetActive(true);
        //if (About_Object) About_Object.SetActive(true);
        if (Paytable_Object) Paytable_Object.SetActive(true);
        if (Settings_Object) Settings_Object.SetActive(true);

        //DOTween.To(() => About_RT.anchoredPosition, (val) => About_RT.anchoredPosition = val, new Vector2(About_RT.anchoredPosition.x, About_RT.anchoredPosition.y + 150), 0.1f).OnUpdate(() =>
        //{
        //    LayoutRebuilder.ForceRebuildLayoutImmediate(About_RT);
        //});

        DOTween.To(() => Paytable_RT.anchoredPosition, (val) => Paytable_RT.anchoredPosition = val, new Vector2(Paytable_RT.anchoredPosition.x, Paytable_RT.anchoredPosition.y + 125), 0.1f).OnUpdate(() =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Paytable_RT);
        });

        DOTween.To(() => Settings_RT.anchoredPosition, (val) => Settings_RT.anchoredPosition = val, new Vector2(Settings_RT.anchoredPosition.x, Settings_RT.anchoredPosition.y + 250), 0.1f).OnUpdate(() =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Settings_RT);
        });
    }

    private void CloseMenu()
    {

        if (audioController) audioController.PlayButtonAudio();
        //DOTween.To(() => About_RT.anchoredPosition, (val) => About_RT.anchoredPosition = val, new Vector2(About_RT.anchoredPosition.x, About_RT.anchoredPosition.y - 150), 0.1f).OnUpdate(() =>
        //{
        //    LayoutRebuilder.ForceRebuildLayoutImmediate(About_RT);
        //});

        DOTween.To(() => Paytable_RT.anchoredPosition, (val) => Paytable_RT.anchoredPosition = val, new Vector2(Paytable_RT.anchoredPosition.x, Paytable_RT.anchoredPosition.y - 125), 0.1f).OnUpdate(() =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Paytable_RT);
        });

        DOTween.To(() => Settings_RT.anchoredPosition, (val) => Settings_RT.anchoredPosition = val, new Vector2(Settings_RT.anchoredPosition.x, Settings_RT.anchoredPosition.y - 250), 0.1f).OnUpdate(() =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Settings_RT);
        });

        DOVirtual.DelayedCall(0.1f, () =>
         {
             if (Menu_Object) Menu_Object.SetActive(true);
             if (Exit_Object) Exit_Object.SetActive(false);
             //if (About_Object) About_Object.SetActive(false);
             if (Paytable_Object) Paytable_Object.SetActive(false);
             if (Settings_Object) Settings_Object.SetActive(false);
         });
    }

    private void OpenPopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
    }

    private void ClosePopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(false);
        if (!DisconnectPopup_Object.activeSelf)
        {
            if (MainPopup_Object) MainPopup_Object.SetActive(false);
        }
    }

    private void ToggleMusic(float value)
    {
        audioController.ChangeVol(value, "bg");

    }

    private void UrlButtons(string url)
    {
        Application.OpenURL(url);
    }

    private void ToggleSound(float value)
    {
        if (audioController) audioController.ChangeVol(value, "button");
        if (audioController) audioController.ChangeVol(value, "wl");

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
            AboutLogo_Image.sprite = sprite;
        }
        else
        {
            Debug.LogError("Error downloading image: " + request.error);
        }
    }
}
