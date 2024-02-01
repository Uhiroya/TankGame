using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.SceneManagement;

public class SceneUIManager : MonoBehaviour 
{
    [SerializeField] Image _fadeImage ;
    [SerializeField] GameObject _nextStageUI;
    [SerializeField] Text _nextStageText;
    [SerializeField] Text _playerCountText;
    [SerializeField] Image _startImage;
    [SerializeField] Image _clearImage;
    [SerializeField] Image _gameOverImage;
    [SerializeField] Image _pauseImage;
    [SerializeField] GameObject _resultUI;
    [SerializeField] Text _resultCountText;
    [SerializeField] float _fadeTime = 0.5f;
    [SerializeField] float _startStageFade = 1.5f;
    [SerializeField] float _fadeUpTime = 0.7f;
    [SerializeField] float _fadeUpSize = 1.2f;
    
    [SerializeField] private GameObject _waitingUI;
    [SerializeField] private Text _waitingText;
    [SerializeField] private Text _remainWaitingTimeText;
    [SerializeField] private GameObject _playerLeftText;
    
    
    CanvasGroup _nextStageUIgroup;

    Animator _pauseAnim;
    public static SceneUIManager Instance;
    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        _pauseAnim = _pauseImage.GetComponent<Animator>();
        _nextStageUIgroup = _nextStageUI.gameObject.GetComponent<CanvasGroup>();
    }

    private int _callWaitCount;
    private float _remainWaitingTime;
    public void UpdateWaitingUI(int waitTime)
    {
        if (!_waitingUI.activeSelf)
        {
            if (_callWaitCount > 5)
            {
                _remainWaitingTime = 9f;
                _waitingUI.SetActive(true);
            }
        }

        _remainWaitingTime -= (float)waitTime / 1000;
        _callWaitCount++;
        string dot = "";
        for (int i = 0; i < _callWaitCount % 4; i++)
        {
            dot += ".";
        }
        _waitingText.text = dot;
        _remainWaitingTimeText.text = _remainWaitingTime.ToString("0");
    }
    public void StopWaitingUI()
    {
        _waitingUI.SetActive(false);
        _callWaitCount = 0;
    }

    public async UniTaskVoid ShowPlayerLeftText(int milliSecond)
    {
        _playerLeftText.SetActive(true);
        await UniTask.Delay(milliSecond);
        _playerLeftText.SetActive(false);
    }
    public async UniTask FadeIn()
    {
        _fadeImage.gameObject.SetActive(true);
        await _fadeImage.DOFade(1, _fadeTime);
    }

    public async UniTask FadeOut()
    {
        await  _fadeImage.DOFade(0, _fadeTime);
        _fadeImage.gameObject.SetActive(false);
    }

    public async UniTask FadeInStageUI(string nextStage , int lifeCount)
    {
        _nextStageUI.gameObject.SetActive(true);
        _nextStageText.text = nextStage + "\n" ;
        _playerCountText.text = "×" + lifeCount;
        await _nextStageUIgroup.DOFade(1, _startStageFade);
    }

    public async UniTask FadeOutStageUI()
    {
        await _nextStageUIgroup.DOFade(0, _startStageFade);
        _nextStageUI.gameObject.SetActive(false);
    }
    public async UniTask ShowUpResult(int enemyCount)
    {
        _resultUI.gameObject.SetActive(true);
        await _resultCountText.DOCounter(0 , enemyCount ,1f);
        await UniTask.WaitForSeconds(2f);
        _resultUI.GetComponent<Animator>().Play("PauseEnd");
        await UniTask.WaitForSeconds(0.2f);
        _resultUI.gameObject.SetActive(false);
    }
    public async UniTask ShowStartText()
    {
         await FadeUpImage(_startImage);
    }
    public async UniTask ShowClearText()
    {
        await FadeUpImage(_clearImage);
    }
    public async UniTask ShowGameOverText()
    {
        await FadeUpImage(_gameOverImage);
    }
    async UniTask FadeUpImage(Image image)
    {
        image.gameObject.SetActive(true);
        _ = image.DOFade(1, _fadeTime);
        await image.rectTransform.DOScale(Vector3.one * _fadeUpSize, _fadeUpTime);
        await image.DOFade(0, _fadeTime);
        image.gameObject.SetActive(false);
    }
    public void Pause()
    {
        _pauseImage.gameObject.SetActive(true);
    }
    public void Resume()
    {
        _pauseAnim.Play("PauseEnd");
        _pauseImage.gameObject.SetActive(false);
    }
}
