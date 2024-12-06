using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using Newtonsoft.Json;

public class SlotBehaviour : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;  //images taken initially

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;     //class to store total images
    [SerializeField]
    private List<SlotImage> slotMatrix;     //class to store the result matrix
    [SerializeField] private Image[] extraIcons;

    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;

    [Header("Buttons")]
    [SerializeField]
    private CustomBtn SlotStart_Button;
    // [SerializeField]
    // private Button AutoSpin_Button;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField]
    private Button MaxBet_Button;
    [SerializeField]
    private Button BetPlus_Button;
    [SerializeField]
    private Button BetMinus_Button;

    [Header("Animated Sprites")]

    [SerializeField] private Sprite[] ID_0;
    [SerializeField] private Sprite[] ID_1;
    [SerializeField] private Sprite[] ID_2;
    [SerializeField] private Sprite[] ID_3;
    [SerializeField] private Sprite[] ID_4;
    [SerializeField] private Sprite[] ID_5;
    [SerializeField] private Sprite[] ID_6;
    [SerializeField] private Sprite[] ID_7;
    [SerializeField] private Sprite[] ID_8;
    [SerializeField] private Sprite[] ID_9;

    [Header("Miscellaneous UI")]
    [SerializeField]
    private TMP_Text Balance_text;
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private TMP_Text LineBet_text;
    [SerializeField]
    private TMP_Text TotalWin_text;


    [Header("Audio Management")]
    [SerializeField]
    private AudioController audioController;

    [SerializeField]
    private UIManager uiManager;



    [Header("Free Spins Board")]
    [SerializeField]
    private GameObject FSBoard_Object;
    [SerializeField]
    private TMP_Text FSnum_text;

    [SerializeField] private Sprite freeSpinReel;
    [SerializeField] private Sprite normalReel;
    [SerializeField] private Image reelBG;

    [SerializeField] private ImageAnimation[] ringAnim;

    int tweenHeight = 0;  //calculate the height at which tweening is done

    [SerializeField]
    private PayoutCalculation PayCalculator;

    private List<Tweener> alltweens = new List<Tweener>();

    private Tweener WinTween = null;

//stores the sprites whose animation is running at present 

    [SerializeField]
    private SocketIOManager SocketManager;

    private Coroutine AutoSpinRoutine = null;
    private Coroutine FreeSpinRoutine = null;
    private Coroutine tweenroutine;

    private Coroutine winAnimRoutine;
    private bool IsAutoSpin = false;
    private bool IsFreeSpin = false;
    private bool IsSpinning = false;
    private bool CheckSpinAudio = false;
    internal bool CheckPopups = false;

    private int BetCounter = 0;
    private double currentBalance = 0;
    private double currentTotalBet = 0;
    protected int Lines = 9;
    [SerializeField]
    private int IconSizeFactor = 100;
    private int numberOfSlots = 5;

    [SerializeField] private GameObject[] normalWinBlinkObj;
    [SerializeField] private GameObject[] freeSpinBlinkObj;

    private void Start()
    {
        IsAutoSpin = false;

        if (SlotStart_Button) SlotStart_Button.SpinAction = () => StartSlots();
        if (SlotStart_Button) SlotStart_Button.AutoSpinACtion = AutoSpin;
        // if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

        if (BetPlus_Button) BetPlus_Button.onClick.RemoveAllListeners();
        if (BetPlus_Button) BetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); });
        if (BetMinus_Button) BetMinus_Button.onClick.RemoveAllListeners();
        if (BetMinus_Button) BetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); });

        if (MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
        if (MaxBet_Button) MaxBet_Button.onClick.AddListener(MaxBet);

        // if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        // if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(AutoSpin);


        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(StopAutoSpin);

        if (FSBoard_Object) FSBoard_Object.SetActive(false);

        tweenHeight = (15 * IconSizeFactor) - 280;
        shuffleInitialMatrix();
    }

    #region Autospin
    private void AutoSpin()
    {
        if (!IsAutoSpin)
        {

            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (SlotStart_Button) SlotStart_Button.gameObject.SetActive(false);

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());

        }
    }

    private void StopAutoSpin()
    {
        if (IsAutoSpin)
        {
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (SlotStart_Button) SlotStart_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }

    private IEnumerator AutoSpinCoroutine()
    {
        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
        }
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            AutoSpinRoutine = null;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }
    #endregion

    #region FreeSpin
    internal void FreeSpin(int spins)
    {
        if (!IsFreeSpin)
        {
            if (FSnum_text) FSnum_text.text = $"{spins}/{spins}";
            if (FSBoard_Object) FSBoard_Object.SetActive(true);
            IsFreeSpin = true;
            ToggleButtonGrp(false);

            if (FreeSpinRoutine != null)
            {
                StopCoroutine(FreeSpinRoutine);
                FreeSpinRoutine = null;
            }
            FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));
        }
    }

    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        int i = 0;
        InvokeRepeating("FreeSpinBlinkAnim", 0.2f, 0.15f);
        reelBG.sprite = freeSpinReel;
        while (i < spinchances)
        {
            i++;
            if (FSnum_text) FSnum_text.text = $"{spinchances - i}/{spinchances}";
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            yield return new WaitForSeconds(2);
        }
        reelBG.sprite = normalReel;

        CancelInvoke("FreeSpinBlinkAnim");
        if (FSBoard_Object) FSBoard_Object.SetActive(false);
        ToggleButtonGrp(true);
        IsFreeSpin = false;
    }
    #endregion

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
        }
    }



    private void MaxBet()
    {
        if (audioController) audioController.PlayButtonAudio();
        BetCounter = SocketManager.initialData.Bets.Count - 1;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
    }

    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            if (BetCounter < SocketManager.initialData.Bets.Count - 1)
            {
                BetCounter++;
            }
        }
        else
        {
            if (BetCounter > 0)
            {
                BetCounter--;
            }
        }
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
    }

    #region InitialFunctions
    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < slotMatrix.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, myImages.Length);
                slotMatrix[i].slotImages[j].rendererDelegate.sprite = myImages[randomIndex];
            }
        }
        shuffleExtraIcons();
    }

    internal void shuffleExtraIcons()
    {

        for (int i = 0; i < extraIcons.Length; i++)
        {
            extraIcons[i].sprite = myImages[UnityEngine.Random.Range(0, myImages.Length)];
        }
    }
    internal void SetInitialUI()
    {
        BetCounter = 0;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        if (TotalWin_text) TotalWin_text.text = "0.00";
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");
        currentBalance = SocketManager.playerdata.Balance;
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        PayCalculator.paylines.AddRange(SocketManager.initialData.Lines);
        // _bonusManager.PopulateWheel(SocketManager.bonusdata);
        CompareBalance();
        Debug.Log(JsonConvert.SerializeObject(SocketManager.initialData.Lines));
        uiManager.InitialiseUIData(SocketManager.initUIData.AbtLogo.link, SocketManager.initUIData.AbtLogo.logoSprite, SocketManager.initUIData.ToULink, SocketManager.initUIData.PopLink, SocketManager.initUIData.paylines);
    }
    #endregion

    private void OnApplicationFocus(bool focus)
    {
        audioController.CheckFocusFunction(focus, CheckSpinAudio);
    }

    //function to populate animation sprites accordingly
    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        switch (val)
        {


            case 0:
                animScript.textureArray.AddRange(ID_0);
                animScript.AnimationSpeed = ID_0.Length;
                break;
            case 1:
                animScript.textureArray.AddRange(ID_1);
                animScript.AnimationSpeed = ID_1.Length;
                break;
            case 2:
                animScript.textureArray.AddRange(ID_2);
                animScript.AnimationSpeed = ID_2.Length;
                break;
            case 3:
                animScript.textureArray.AddRange(ID_3);
                animScript.AnimationSpeed = ID_3.Length;
                break;
            case 4:
                animScript.textureArray.AddRange(ID_4);
                animScript.AnimationSpeed = ID_4.Length;
                break;
            case 5:
                animScript.textureArray.AddRange(ID_5);
                animScript.AnimationSpeed = ID_5.Length;
                break;
            case 6:
                animScript.textureArray.AddRange(ID_6);
                animScript.AnimationSpeed = ID_6.Length;
                break;
            case 7:
                animScript.textureArray.AddRange(ID_7);
                animScript.AnimationSpeed = ID_7.Length;
                break;
            case 8:
                animScript.textureArray.AddRange(ID_8);
                animScript.AnimationSpeed = ID_8.Length;
                break;
            case 9:
                animScript.textureArray.AddRange(ID_9);
                animScript.AnimationSpeed = ID_9.Length;
                break;

        }
        if (animScript.AnimationSpeed <= 16)
            animScript.AnimationSpeed = 7.5f;
    }

    #region SlotSpin
    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        if (audioController) audioController.PlaySpinButtonAudio();

        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }
        WinningsAnim(false);
        if (winAnimRoutine != null)
            StopCoroutine(winAnimRoutine);



        if (SlotStart_Button) SlotStart_Button.btn.interactable = false;
        // if (TempList.Count > 0)
        // {
        StopGameAnimation();
        // }
        PayCalculator.ResetLines(true);
        tweenroutine = StartCoroutine(TweenRoutine());
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine()
    {
        if (currentBalance < currentTotalBet && !IsFreeSpin)
        {
            CompareBalance();
            StopAutoSpin();
            yield return new WaitForSeconds(1);
            ToggleButtonGrp(true);
            yield break;
        }
        if (audioController) audioController.PlayWLAudio("spin");
        CheckSpinAudio = true;

        IsSpinning = true;

        ToggleButtonGrp(false);

        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0.1f);
        }

        if (!IsFreeSpin)
        {
            BalanceDeduction();
        }
        SocketManager.AccumulateResult(BetCounter);

        yield return new WaitUntil(() => SocketManager.isResultdone);

        List<int> ringIndex = new List<int>();
        for (int j = 0; j < SocketManager.resultData.ResultReel.Count; j++)
        {
            for (int i = 0; i < SocketManager.resultData.ResultReel[j].Count; i++)
            {

                int id = Int32.Parse(SocketManager.resultData.ResultReel[j][i]);
                if (id == 8 && i<SocketManager.resultData.ResultReel[j].Count)
                {

                    if (!ringIndex.Contains(i+1))
                        ringIndex.Add(i+1);
                }

                slotMatrix[i].slotImages[j].rendererDelegate.sprite = myImages[id];
                PopulateAnimationSprites(slotMatrix[i].slotImages[j], id);
            }
        }

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < numberOfSlots; i++)
        {
            if (i + 1 < numberOfSlots && ringIndex.Contains(i + 1))
            {
                ActivateRing(i + 1);
                yield return StopTweening(5, Slot_Transform[i], i, true);

            }
            else
                yield return StopTweening(5, Slot_Transform[i], i, false);
            DeactivateRing();
        }

        yield return new WaitForSeconds(0.3f);
        if (audioController) audioController.StopWLAaudio();
        if (!IsAutoSpin && !IsFreeSpin)
            winAnimRoutine = StartCoroutine(ShowIconByPayline(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit));
        else
            CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit);
        KillAllTweens();

        CheckPopups = true;

        if (SocketManager.playerdata.currentWining > 0)
            TotalWin_text.text = $" Win\n{SocketManager.playerdata.currentWining.ToString("f3")}";
        else if (SocketManager.resultData.freeSpins.isNewAdded)
            TotalWin_text.text = $"Win\n{SocketManager.resultData.freeSpins.count} Free Spins";
        else
            TotalWin_text.text = $"Better Luck Next Time";

        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f3");

        currentBalance = SocketManager.playerdata.Balance;

        CheckWinPopups();

        yield return new WaitUntil(() => !CheckPopups);
        if (audioController) audioController.StopWLAaudio();
        if (!IsAutoSpin && !IsFreeSpin)
        {
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            yield return new WaitForSeconds(2f);
            IsSpinning = false;
        }
        if (SocketManager.resultData.freeSpins.isNewAdded)
        {
            if (IsFreeSpin)
            {
                IsFreeSpin = false;
                if (FreeSpinRoutine != null)
                {
                    StopCoroutine(FreeSpinRoutine);
                    FreeSpinRoutine = null;
                }
            }
            uiManager.FreeSpinProcess((int)SocketManager.resultData.freeSpins.count);
            if (IsAutoSpin)
            {
                StopAutoSpin();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private void BalanceDeduction()
    {
        double bet = 0;
        double balance = 0;
        try
        {
            bet = double.Parse(TotalBet_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            balance = double.Parse(Balance_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }
        double initAmount = balance;

        balance = balance - bet;

        DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = initAmount.ToString("f2");
        });
    }

    void ActivateRing(int index)
    {
        Debug.Log("index"+index);
        ringAnim[index].gameObject.SetActive(true);

    }

    void DeactivateRing()
    {
        for (int i = 1; i < ringAnim.Length; i++)
        {
            ringAnim[i].gameObject.SetActive(false);
        }
    }
    internal void CheckWinPopups()
    {
        if (SocketManager.resultData.WinAmout >= currentTotalBet * 10 && SocketManager.resultData.WinAmout < currentTotalBet * 15)
        {
            uiManager.PopulateWin(1, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 15 && SocketManager.resultData.WinAmout < currentTotalBet * 20)
        {
            uiManager.PopulateWin(2, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 20)
        {
            uiManager.PopulateWin(3, SocketManager.resultData.WinAmout);
        }
        else
        {
            CheckPopups = false;
        }
    }

    IEnumerator ShowIconByPayline(List<int> LineId, List<string> points_AnimString)
    {
        if (LineId.Count == 0 && points_AnimString.Count == 0)
            yield break;

        if (audioController) audioController.PlayWLAudio("win");
        List<List<string>> coordToAnimate = new List<List<string>>();

        for (int i = 0; i < LineId.Count; i++)
        {
            List<string> iconPerPayline = new List<string>();
            for (int j = 0; j < PayCalculator.paylines[LineId[i]].Count; j++)
            {
                if (points_AnimString.Contains($"{j},{PayCalculator.paylines[LineId[i]][j]}"))
                {
                    iconPerPayline.Add($"{j},{PayCalculator.paylines[LineId[i]][j]}");
                }

            }

            coordToAnimate.Add(iconPerPayline); ;
        }
        List<int> points = new List<int>();
        List<ImageAnimation> animatingIcon = new List<ImageAnimation>();
        WinningsAnim(true);
        while (true)
        {
            
            for (int i = 0; i < coordToAnimate.Count; i++)
            {

                PayCalculator.GeneratePayoutLines(LineId[i], true);
                for (int j = 0; j < coordToAnimate[i].Count; j++)
                {
                    points = coordToAnimate[i][j]?.Split(',')?.Select(Int32.Parse)?.ToList();
                    slotMatrix[points[0]].slotImages[points[1]].StartAnimation();
                    animatingIcon.Add(slotMatrix[points[0]].slotImages[points[1]]);

                }
                yield return new WaitForSeconds(2f);
                PayCalculator.ResetLines(true);
                for (int j = 0; j < animatingIcon.Count; j++)
                {


                    animatingIcon[j].StopAnimation();

                }
                animatingIcon.Clear();
                points.Clear();
                // yield return new WaitForSeconds(0.5f);

            }

            yield return null;
        }

    }
    private void CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString)
    {
        List<int> points_anim = null;
        if (LineId.Count > 0 || points_AnimString.Count > 0)
        {

            if (audioController) audioController.PlayWLAudio("win");


            for (int i = 0; i < LineId.Count; i++)
            {
                PayCalculator.GeneratePayoutLines(LineId[i], true);
            }


            for (int i = 0; i < points_AnimString.Count; i++)
            {
                points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();
                Debug.Log(JsonConvert.SerializeObject(points_anim));
                slotMatrix[points_anim[0]].slotImages[points_anim[1]].StartAnimation();
            }

            WinningsAnim(true);
        }

        CheckSpinAudio = false;
    }

    private void WinningsAnim(bool IsStart)
    {
        if (IsStart)
        {
            WinTween = TotalWin_text.transform.DOScale(new Vector2(1.5f, 1.5f), 1f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
            InvokeRepeating("BlinkAnim", 0.2f, 0.25f);
        }
        else
        {
            CancelInvoke("BlinkAnim");
            WinTween.Kill();
            TotalWin_text.transform.localScale = Vector3.one;
        }
    }

    #endregion

    internal void CallCloseSocket()
    {
        SocketManager.CloseSocket();
    }

    void BlinkAnim()
    {

        if (normalWinBlinkObj[0].activeSelf)
        {
            normalWinBlinkObj[0].SetActive(false);
            normalWinBlinkObj[1].SetActive(true);
        }
        else
        {
            normalWinBlinkObj[1].SetActive(false);
            normalWinBlinkObj[0].SetActive(true);
        }
    }
    void FreeSpinBlinkAnim()
    {

        if (freeSpinBlinkObj[0].activeSelf)
        {
            freeSpinBlinkObj[0].SetActive(false);
            freeSpinBlinkObj[1].SetActive(true);
        }
        else
        {
            freeSpinBlinkObj[1].SetActive(false);
            freeSpinBlinkObj[0].SetActive(true);
        }
    }

    void ToggleButtonGrp(bool toggle)
    {

        if (SlotStart_Button) SlotStart_Button.btn.interactable = toggle;
        if (MaxBet_Button) MaxBet_Button.interactable = toggle;
        // if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if (BetMinus_Button) BetMinus_Button.interactable = toggle;
        if (BetPlus_Button) BetPlus_Button.interactable = toggle;

    }

    //start the icons animation


    //stop the icons animation
    private void StopGameAnimation()
    {
        for (int i = 0; i < slotMatrix.Count; i++)
        {
            for (int j = 0; j < slotMatrix[i].slotImages.Count; j++)
            {
                slotMatrix[i].slotImages[j].StopAnimation();
                slotMatrix[i].slotImages[j].textureArray.Clear();
            }
        }
    }


    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.2f).SetLoops(-1, LoopType.Restart).SetDelay(0);
        tweener.Play();
        alltweens.Add(tweener);
    }



    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index, bool delay = false)
    {
        alltweens[index].Pause();

        alltweens[index] = slotTransform.DOLocalMoveY(-815, 0.3f).SetEase(Ease.OutElastic);
        if (delay)
            yield return new WaitForSeconds(1f);
        else
            yield return new WaitForSeconds(0.3f);
    }


    private void KillAllTweens()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<ImageAnimation> slotImages = new List<ImageAnimation>(10);
}

