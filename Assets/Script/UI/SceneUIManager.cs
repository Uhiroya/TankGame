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

    private static SceneUIManager instance = null;
    public static SceneUIManager Instance => instance;
    [SerializeField] Image _fadeImage ;
    [SerializeField] GameObject _nextStageUI;
    [SerializeField] Text _nextStageText;
    //[SerializeField] Image _nextStageImage;
    [SerializeField] Text _playerCountText;
    //[SerializeField] Image _playerImage;
    [SerializeField] Image _startImage;
    [SerializeField] Image _clearImage;
    [SerializeField] Image _gameOverImage;
    [SerializeField] Image _pauseImage;
    [SerializeField] GameObject _resultUI;
    [SerializeField] Image _resultImage;
    [SerializeField] Text _resultCountText;
    [SerializeField] float _fadeTime = 0.5f;
    [SerializeField] float _startStageFade = 1.5f;
    [SerializeField] float _fadeUpTime = 0.7f;
    [SerializeField] float _fadeUpSize = 1.2f;

    CanvasGroup _nextStageUIgroup;

    Animator _pauseAnim;
    void Awake()
    {
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            FadeAndNextScene("Title");
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
    public async void FadeAndNextScene(string nextScene)
    {
        _fadeImage.gameObject.SetActive(true);
        await _fadeImage.DOFade(1, _fadeTime);
        await SceneManager.LoadSceneAsync(nextScene);
        _ = _fadeImage.DOFade(0, _fadeTime);
        if (nextScene.Contains("Title"))
        {
           await GameManager.Instance.TitleInitialize();
        }
        else
        {
            await GameManager.Instance.GameInitialize();
        }
        _fadeImage.gameObject.SetActive(false);
    }
    public async void FadeAndNextStage(string nextStage)
    {
        _fadeImage.gameObject.SetActive(true);
        await _fadeImage.DOFade(1, _fadeTime);
        await SceneManager.LoadSceneAsync(nextStage);
        //UI‚É•¶Žš‚ð“ü‚ê‚éB
        _nextStageUI.gameObject.SetActive(true);
        _nextStageText.text = nextStage +"\n" ;
        _playerCountText.text = "~" + GameManager.NowPlayerCount.ToString() ;
        await _nextStageUIgroup.DOFade(1, _startStageFade);
        _fadeImage.gameObject.SetActive(false);
        var task = GameManager.Instance.GameInitialize();
        await _nextStageUIgroup.DOFade(0, _startStageFade);
        await UniTask.WhenAll(task);
        _nextStageUI.gameObject.SetActive(false);
    }
    public async UniTask ShowUpResult(int enemyCount)
    {
        _resultUI.gameObject.SetActive(true);
        await _resultCountText.DOCounter(0 , enemyCount ,1f);
        //await UniTask.WaitUntil(() => Input.anyKey);
        await UniTask.WaitForSeconds(2f);
        _resultUI.GetComponent<Animator>().Play("PauseEnd");
        await UniTask.WaitForSeconds(0.2f);
        _resultUI.gameObject.SetActive(false);
    }
    public async UniTask StartUI()
    {
         await FadeUpImage(_startImage);
    }
    public async UniTask ClearUI()
    {
        await FadeUpImage(_clearImage);
    }
    public async UniTask GameOverUI()
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
