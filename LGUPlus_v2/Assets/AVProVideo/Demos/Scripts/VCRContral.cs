#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_5 || UNITY_5_4_OR_NEWER
#define UNITY_FEATURE_UGUI
#endif

using UnityEngine;
#if UNITY_FEATURE_UGUI
using UnityEngine.UI;
using System.Collections;
using RenderHeads.Media.AVProVideo;
using UnityEngine.SceneManagement;


//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Demos
{
    public class VCRContral : MonoBehaviour
    {
        public MediaPlayer _mediaPlayer;
        public MediaPlayer _mediaPlayerB;
        public DisplayUGUI _mediaDisplay;

        public RectTransform _bufferedSliderRect;

        public Slider _videoSeekSlider;
        public Image _videoSeekImageSlider;

        public GameObject _radial;

        public Slider _audioVolumeSlider;

        public Toggle _AutoStartToggle;
        public Toggle _MuteToggle;

        public MediaPlayer.FileLocation _location = MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder;
        public string _folder = "AVProVideoDemos/";
        public string[] _videoFiles = { "4320 1280 3M LOOP.mp4", "4320 1280 3M VR.mp4" };

        public Slider _sliderHander;
        public GameObject startBtn;

        public CanvasGroup sliderControal;
        public float fadeInDelay;

        public GameObject buleLiteScreen;

        public GameObject btnCont;
        public GameObject gameBtnCont;

        private bool isBuleLite = true;
        private float _setAudioVolumeSliderValue;
        private float _setVideoSeekSliderValue;
        private bool _wasPlayingOnScrub;
        private float _currentTime;
        private bool _isAlpha = false;

        private int _VideoIndex = 0;
        private int currentCount;
        private Image _bufferedSliderImage;

        [Header("VARIABLES (IN-GAME)")]
        public bool isOn;
        public bool restart;
        [Range(0, 100)] public float currentPercent;
        [Range(0, 100)] public int speed;

        [Header("SPECIFIED PERCENT")]
        public bool enableSpecified;
        public bool enableLoop;
        [Range(0, 100)] public float specifiedValue;

        public float[] _vidoeTimeSection;
        public TMPro.TextMeshProUGUI debugTxt;
        public GameObject _radialImage;

        private MediaPlayer _loadingPlayer;
        private bool _isTimer = false;

        private bool _isNext = true;

        private float _NTimer = 0;

        public MediaPlayer PlayingPlayer
        {
            get
            {
                if (_mediaPlayer.Control == null)
                    return _mediaPlayerB;
                else if (_mediaPlayerB.Control == null)
                    return _mediaPlayer;

                if (LoadingPlayer == _mediaPlayer)
                {
                    return _mediaPlayerB;
                }
                return _mediaPlayer;
            }
        }

        public MediaPlayer LoadingPlayer
        {
            get
            {
                return _loadingPlayer;
            }
        }

        private void SwapPlayers()
        {
            // Pause the previously playing video
            if (PlayingPlayer != null && PlayingPlayer.Control != null)
                PlayingPlayer.Control.Pause();
            // Swap the videos
            if (LoadingPlayer == _mediaPlayer)
            {
                _loadingPlayer = _mediaPlayerB;
            }
            else
            {
                _loadingPlayer = _mediaPlayer;
            }
            // Change the displaying video
            _mediaDisplay.CurrentMediaPlayer = PlayingPlayer;
        }

        public void OnOpenVideoInx(int idx)
        {
            _VideoIndex = idx;
            if (idx == 0)
            {
                btnCont.SetActive(true);
                gameBtnCont.SetActive(false);
            }
            else
            {
                btnCont.SetActive(false);
                gameBtnCont.SetActive(false);
            }

            LoadingPlayer.m_VideoPath = System.IO.Path.Combine(_folder, _videoFiles[_VideoIndex]);
            if (_VideoIndex == 1)
            {
                _isAlpha = true;
                GameObject.Find("ThumPanel").GetComponent<ThumbnailManager>().OnThumbnail(true);

            }
            else if (_VideoIndex == 2)
            {
                _isAlpha = true;
                GameObject.Find("ThumPanel").GetComponent<ThumbnailManager>().OnThumbnail(false);
            }
            else
            {
                _isAlpha = false;
                GameObject.Find("ThumPanel").GetComponent<ThumbnailManager>().init();
                GameObject.Find("ThumPanel").GetComponent<ThumbnailManager>().OnThumbnail(false);
            }

            if (_VideoIndex == 10)
            {
                gameBtnCont.SetActive(true);
            }


            if (string.IsNullOrEmpty(LoadingPlayer.m_VideoPath))
            {
                LoadingPlayer.CloseVideo();
                _VideoIndex = 0;
            }
            else
            {
                LoadingPlayer.OpenVideoFromFile(_location, LoadingPlayer.m_VideoPath, _AutoStartToggle.isOn);
            }

            if (_bufferedSliderRect != null)
            {
                _bufferedSliderImage = _bufferedSliderRect.GetComponent<Image>();
            }

        }

        public void OnOpenVideoFile()
        {
            LoadingPlayer.m_VideoPath = System.IO.Path.Combine(_folder, _videoFiles[_VideoIndex]);
            _VideoIndex = (_VideoIndex + 1) % (_videoFiles.Length);

            if (_VideoIndex == 0)
            {
                SaveVideoData();
            }


            if (string.IsNullOrEmpty(LoadingPlayer.m_VideoPath))
            {
                LoadingPlayer.CloseVideo();
                _VideoIndex = 0;
            }
            else
            {
                LoadingPlayer.OpenVideoFromFile(_location, LoadingPlayer.m_VideoPath, _AutoStartToggle.isOn);
            }

            if (_bufferedSliderRect != null)
            {
                _bufferedSliderImage = _bufferedSliderRect.GetComponent<Image>();
            }
        }

        IEnumerator delaytTime(float delayTime)
        {

            yield return new WaitForSeconds(delayTime);
            _radial.SetActive(true);
        }




        public void OnAutoStartChange()
        {
            if (PlayingPlayer &&
                _AutoStartToggle && _AutoStartToggle.enabled &&
                PlayingPlayer.m_AutoStart != _AutoStartToggle.isOn)
            {
                PlayingPlayer.m_AutoStart = _AutoStartToggle.isOn;
            }
            if (LoadingPlayer &&
                _AutoStartToggle && _AutoStartToggle.enabled &&
                LoadingPlayer.m_AutoStart != _AutoStartToggle.isOn)
            {
                LoadingPlayer.m_AutoStart = _AutoStartToggle.isOn;
            }
        }

        public void OnMuteChange()
        {
            if (PlayingPlayer)
            {
                PlayingPlayer.Control.MuteAudio(_MuteToggle.isOn);
            }
            if (LoadingPlayer)
            {
                LoadingPlayer.Control.MuteAudio(_MuteToggle.isOn);
            }
        }

        public void OnPlayButton()
        {
            if (PlayingPlayer)
            {
                PlayingPlayer.Control.Play();
            }
        }
        public void OnPauseButton()
        {
            if (PlayingPlayer)
            {
                PlayingPlayer.Control.Pause();
            }
        }

        public void OnVideoSeekSlider()
        {

            if (PlayingPlayer && _videoSeekImageSlider && _videoSeekImageSlider.fillAmount != _setVideoSeekSliderValue)
            {
                //Debug.Log("OnVideoSeekSlider >>  "  +  _videoSeekSlider.value * PlayingPlayer.Info.GetDurationMs() );
                // _videoSeekSlider.value = _videoSeekImageSlider.fillAmount;
                //PlayingPlayer.Control.Seek( _videoSeekSlider.value * PlayingPlayer.Info.GetDurationMs() );
                //PlayingPlayer.Control.Seek(_videoSeekImageSlider.fillAmount * PlayingPlayer.Info.GetDurationMs());
            }
        }


        public void OnVidepSeekValue(float value)
        {
            //_videoSeekSlider.value = valuet;

            PlayingPlayer.Control.Seek(value);
        }

        public void OnVideoSeekRadialSlider(float sliderValue)
        {
            if (PlayingPlayer && _videoSeekSlider && sliderValue != _setVideoSeekSliderValue)
            {
                Debug.Log(sliderValue);

                PlayingPlayer.Control.Seek(sliderValue * PlayingPlayer.Info.GetDurationMs());
            }
        }



        public void OnBuleLite()
        {
            buleLiteScreen.SetActive(isBuleLite);
            isBuleLite = !isBuleLite;
        }

        public void OnVideoSliderDown()
        {
            if (PlayingPlayer)
            {
                _wasPlayingOnScrub = PlayingPlayer.Control.IsPlaying();
                debugTxt.text = "MouseDown";
                if (_wasPlayingOnScrub)
                {
                    PlayingPlayer.Control.Pause();

                }

                OnVideoSeekSlider();
            }
        }

        public void OnVideoSliderUp()
        {
            if (PlayingPlayer && _wasPlayingOnScrub)
            {
                PlayingPlayer.Control.Play();
                _wasPlayingOnScrub = false;
                debugTxt.text = "Mouse UP ";
                //				SetButtonEnabled( "PlayButton", false );
                //				SetButtonEnabled( "PauseButton", true );
            }

        }

        public void OnAudioVolumeSlider()
        {
            if (PlayingPlayer && _audioVolumeSlider && _audioVolumeSlider.value != _setAudioVolumeSliderValue)
            {
                PlayingPlayer.Control.SetVolume(_audioVolumeSlider.value);
            }
            if (LoadingPlayer && _audioVolumeSlider && _audioVolumeSlider.value != _setAudioVolumeSliderValue)
            {
                LoadingPlayer.Control.SetVolume(_audioVolumeSlider.value);
            }
        }


        public void OnRewindButton()
        {
            if (PlayingPlayer)
            {
                PlayingPlayer.Control.Rewind();
            }
        }

        private void Awake()
        {
            _loadingPlayer = _mediaPlayerB;
            //   _sliderHander.GetComponent<Slider>().value = 1;
        }

        void Start()
        {
            if (PlayingPlayer)
            {
                //PlayingPlayer.Events.AddListener(OnVideoEvent);

                if (LoadingPlayer)
                {
                    //LoadingPlayer.Events.AddListener(OnVideoEvent);
                }

                if (_audioVolumeSlider)
                {
                    // Volume
                    if (PlayingPlayer.Control != null)
                    {
                        float volume = PlayingPlayer.Control.GetVolume();
                        _setAudioVolumeSliderValue = volume;
                        _audioVolumeSlider.value = volume;
                    }
                }

                // Auto start toggle
                _AutoStartToggle.isOn = PlayingPlayer.m_AutoStart;

                if (PlayingPlayer.m_AutoOpen)
                {
                    //	RemoveOpenVideoButton();

                    //SetButtonEnabled( "PlayButton", !_mediaPlayer.m_AutoStart );
                    //SetButtonEnabled( "PauseButton", _mediaPlayer.m_AutoStart );
                }
                else
                {
                    //SetButtonEnabled( "PlayButton", false );
                    //SetButtonEnabled( "PauseButton", false );
                }

                // SetButtonEnabled( "MuteButton", !_mediaPlayer.m_Muted );
                //SetButtonEnabled( "UnmuteButton", _mediaPlayer.m_Muted );

                //  OnOpenVideoFile();

                if (DataSender.Instance.OpenGameBtns == true)
                {
                    DataSender.Instance.OpenGameBtns = false;
                    OnOpenVideoInx(10);
                }
                else
                    OnOpenVideoInx(0);
            }

        }

        private void OnDestroy()
        {
            if (LoadingPlayer)
            {
                //LoadingPlayer.Events.RemoveListener(OnVideoEvent);
            }
            if (PlayingPlayer)
            {
                //PlayingPlayer.Events.RemoveListener(OnVideoEvent);
            }
        }


        void Update()
        {
            // 컨트롤러 보이게 하기.
            if (_isAlpha == true)
            {
                sliderControal.alpha += Time.deltaTime / fadeInDelay;
            }
            else
            {
                sliderControal.alpha -= Time.deltaTime / fadeInDelay * 2;
            }
            if (PlayingPlayer && PlayingPlayer.Info != null && PlayingPlayer.Info.GetDurationMs() > 0f)
            {
                float time = PlayingPlayer.Control.GetCurrentTimeMs();
                float duration = PlayingPlayer.Info.GetDurationMs();
                float tValue = time / duration;
                float d = Mathf.Clamp(tValue, 0.0f, 1.0f);

                //float _nt = 0f;

                float _ntValue = 0;  // 새로운 시간.
                float _d1 = 0f;  // 컨트롤러 value 
                float _curtime;
                float _curduration;
                bool _isOnclick = false;


                //float _nTimer = 0;
                // 다시 체크 하자 ..
                if (_VideoIndex == 1)
                {
                    _currentTime += Time.deltaTime;
                }


                if (_VideoIndex == 1)
                {

                    if (time < _vidoeTimeSection[0])
                    {

                        currentCount = 0;
                        _ntValue = time / _vidoeTimeSection[currentCount];
                        _d1 = Mathf.Clamp(_ntValue, 0.0f, 1.0f);
                        // Debug.Log(" currentCount   : " + currentCount  + "    _isNext   : " + _isNext  ) ;


                        if (_isNext == false)
                        {
                            Debug.Log("NEXT  :  " + currentCount);
                            GameObject.Find("ThumPanel").GetComponent<ThumbnailManager>().RightThum(false);
                            _isNext = true;
                        }


                        if (_d1 > 0.999f)
                        {
                            if (_isOnclick == false)
                            {
                                _isNext = false;
                                _isOnclick = true;
                                //    Debug.Log("============================ ON CLICK 0============================");
                            }
                        }
                    }

                    else if (time > _vidoeTimeSection[0] && time < _vidoeTimeSection[1])
                    {
                        //우주 
                        currentCount = 1;

                        if (_isNext == false)
                        {
                            Debug.Log("NEXT  :  " + currentCount);
                            GameObject.Find("ThumPanel").GetComponent<ThumbnailManager>().RightThum(false);
                            _isNext = true;
                        }

                        _curtime = time - _vidoeTimeSection[currentCount - 1];
                        _curduration = _vidoeTimeSection[currentCount] - _vidoeTimeSection[currentCount - 1];
                        _ntValue = _curtime / _curduration;
                        _d1 = Mathf.Clamp(_ntValue, 0.0f, 1.0f);

                        //Debug.Log(" currentCount    " + currentCount + "  /  " + _isNext);

                        if (_d1 > 0.999f)
                        {
                            if (_isOnclick == false)
                            {
                                _isNext = false;
                                _isOnclick = true;
                                Debug.Log("============================ ON CLICK 1============================");
                            }
                        }
                    }

                    else if (time > _vidoeTimeSection[1] && time < _vidoeTimeSection[2])
                    {
                        //노르웨이 오로라
                        currentCount = 2;

                        if (_isNext == false)
                        {
                            _isOnclick = false;
                            Debug.Log("NEXT  :  " + currentCount);
                            GameObject.Find("ThumPanel").GetComponent<ThumbnailManager>().RightThum(false);
                            _isNext = true;

                        }
                        _curtime = time - _vidoeTimeSection[currentCount - 1];
                        _curduration = _vidoeTimeSection[currentCount] - _vidoeTimeSection[currentCount - 1];
                        _ntValue = _curtime / _curduration;
                        _d1 = Mathf.Clamp(_ntValue, 0.0f, 1.0f);
                        //Debug.Log("_d2   : " + _d1 + "    _isOnclick    " + _isOnclick);

                        if (_isNext == false)
                        {
                            _isOnclick = false;
                            Debug.Log("NEXT  :  " + currentCount);


                        if (_d1 > 0.999f)
                        {
                            if (_isOnclick == false)
                            {
                                _isNext = false;

                                _isOnclick = true;

                                Debug.Log("============================ ON CLICK 2============================");
                            }
                        }


                        //  Debug.Log("_isNext : " + time + "    /     " + _vidoeTimeSection[1] + "    d :  " + _d1);
                    }

                    else if (time > _vidoeTimeSection[2] && time < _vidoeTimeSection[3])
                    {
                        //에베레스트
                        currentCount = 3;

                            GameObject.Find("ThumPanel").GetComponent<ThumbnailManager>().RightThum(false);
                            _isNext = true;

                        }


                        _curtime = time - _vidoeTimeSection[currentCount - 1];
                        _curduration = _vidoeTimeSection[currentCount] - _vidoeTimeSection[currentCount - 1];
                        _ntValue = _curtime / _curduration;
                        _d1 = Mathf.Clamp(_ntValue, 0.0f, 1.0f);
                        if (_d1 > 0.999f)
                        {
                            if (_isOnclick == false)
                            {
                                _isNext = false;

                                _isOnclick = true;

                                Debug.Log("============================ ON CLICK 3============================");
                            }
                        }

                    }

                    else if (time > _vidoeTimeSection[3] && time < _vidoeTimeSection[4])
                    {
                        // 파리 에펠탑
                        currentCount = 4;
                        if (_isNext == false)
                        {
                            Debug.Log("NEXT  :  " + currentCount);
                            GameObject.Find("ThumPanel").GetComponent<ThumbnailManager>().RightThum(false);
                            _isNext = true;

                        }
                        _curtime = time - _vidoeTimeSection[currentCount - 1];
                        _curduration = _vidoeTimeSection[currentCount] - _vidoeTimeSection[currentCount - 1];
                        _ntValue = _curtime / _curduration;
                        _d1 = Mathf.Clamp(_ntValue, 0.0f, 1.0f);
                        if (_d1 > 0.999f)
                        {
                            if (_isOnclick == false)
                            {
                                _isNext = false;

                                _isOnclick = true;

                                Debug.Log("============================ ON CLICK 4============================");
                            }
                        }
                        //_slidebg.GetComponent<Image>().sprite = _slideImg[currentCount ];
                    }

                    else if (time > _vidoeTimeSection[4] && time < _vidoeTimeSection[5])
                    {
                        //아프리카
                        currentCount = 5;
                        if (_isNext == false)
                        {
                            Debug.Log("NEXT  :  " + currentCount);
                            GameObject.Find("ThumPanel").GetComponent<ThumbnailManager>().RightThum(false);
                            _isNext = true;

                        }
                        _curtime = time - _vidoeTimeSection[currentCount - 1];
                        _curduration = _vidoeTimeSection[currentCount] - _vidoeTimeSection[currentCount - 1];
                        _ntValue = _curtime / _curduration;
                        _d1 = Mathf.Clamp(_ntValue, 0.0f, 1.0f);



                        if (_d1 > 0.999f)
                        {
                            if (_isOnclick == false)
                            {
                                _isNext = false;

                                _isOnclick = true;

                                Debug.Log("============================ ON CLICK 5 ============================");
                            }
                        }



                    }
                    else if (time > _vidoeTimeSection[5] && time < _vidoeTimeSection[6])
                    {


                        //세부 바다속
                        currentCount = 6;

                        if (_isNext == false)
                        {
                            Debug.Log("NEXT  :  " + currentCount);
                            GameObject.Find("ThumPanel").GetComponent<ThumbnailManager>().RightThum(false);
                            _isNext = true;

                        }
                        _curtime = time - _vidoeTimeSection[currentCount - 1];

                        _curduration = _vidoeTimeSection[currentCount] - _vidoeTimeSection[currentCount - 1];
                        _ntValue = _curtime / _curduration;
                        _d1 = Mathf.Clamp(_ntValue, 0.0f, 1.0f);


                        if (_d1 > 0.999f)
                        {
                            if (_isOnclick == false)
                            {
                                _isNext = false;

                                _isOnclick = true;

                                Debug.Log("============================ ON CLICK 6 ============================");
                            }
                        }





                    }
                    else if (time > _vidoeTimeSection[6] && time < _vidoeTimeSection[7])
                    {
                        currentCount = 7;
                        // Debug.Log("currentCount  :  " + currentCount);
                        if (_isNext == false)
                        {
                            Debug.Log("NEXT  :  " + currentCount);
                            GameObject.Find("ThumPanel").GetComponent<ThumbnailManager>().RightThum(false);
                            _isNext = true;

                        }
                        _curtime = time - _vidoeTimeSection[currentCount - 1];

                        _curduration = _vidoeTimeSection[currentCount] - _vidoeTimeSection[currentCount - 1];
                        _ntValue = _curtime / _curduration;
                        _d1 = Mathf.Clamp(_ntValue, 0.0f, 1.0f);


                        if (_d1 > 0.999f)
                        {
                            if (_isOnclick == false)
                            {
                                _isNext = false;

                                _isOnclick = true;

                                Debug.Log("============================ ON CLICK 7 ============================");
                            }
                        }




                    }
                    else if (time > _vidoeTimeSection[7] && time < _vidoeTimeSection[8])  //우주
                    {
                        currentCount = 8;

                        // Debug.Log("============================ ON CLICK 8 ============================");
                        if (_isNext == false)
                        {
                            Debug.Log("NEXT  :  " + currentCount);
                            GameObject.Find("ThumPanel").GetComponent<ThumbnailManager>().RightThum(false);
                            _isNext = true;

                        }
                        _curtime = time - _vidoeTimeSection[currentCount - 1];

                        _curduration = _vidoeTimeSection[currentCount] - _vidoeTimeSection[currentCount - 1];
                        Debug.Log("currentCount  :  " + currentCount + "     /       " + _vidoeTimeSection[currentCount - 1]);
                        //_curduration = duration;
                        _ntValue = _curtime / _curduration;
                        _d1 = Mathf.Clamp(_ntValue, 0.0f, 1.0f);

                        if (_d1 > 0.999f)
                        {
                            if (_isOnclick == false)
                            {
                                _isNext = false;

                                _isOnclick = true;

                                Debug.Log("============================ ON CLICK 8 ============================");
                            }
                        }

                    }
                    else if (time > _vidoeTimeSection[8])   //폭죽
                    {
                        currentCount = 9;

                        // Debug.Log("============================ ON CLICK 9 ============================");
                        if (_isNext == false)
                        {
                            Debug.Log("NEXT  :  " + currentCount);
                            GameObject.Find("ThumPanel").GetComponent<ThumbnailManager>().RightThum(false);
                            _isNext = true;

                        }
                        _curtime = time - _vidoeTimeSection[currentCount - 1];

                        _curduration = _vidoeTimeSection[currentCount] - _vidoeTimeSection[currentCount - 1];
                        Debug.Log("currentCount  :  " + currentCount + "     /       " + _vidoeTimeSection[currentCount - 1]);
                        //_curduration = duration;
                        _ntValue = _curtime / _curduration;
                        _d1 = Mathf.Clamp(_ntValue, 0.0f, 1.0f);


                    }

                    _sliderHander.GetComponent<Slider>().value = _d1;

                }
                else if (_VideoIndex == 2)
                {
                    _ntValue = time / duration;
                    _d1 = Mathf.Clamp(_ntValue, 0.0f, 1.0f);
                    _sliderHander.GetComponent<Slider>().value = _d1;
                }

                float _r = (d * 100f);
                _radial.GetComponent<Michsky.UI.ModernUIPack.RadialSlider>().OnUpdatePer(_r);

                _setVideoSeekSliderValue = d;

                _videoSeekSlider.value = d;

                if (_bufferedSliderRect != null)
                {
                    if (PlayingPlayer.Control.IsBuffering())
                    {
                        float t1 = 0f;
                        float t2 = PlayingPlayer.Control.GetBufferingProgress();

                        if (t2 <= 0f)
                        {
                            if (PlayingPlayer.Control.GetBufferedTimeRangeCount() > 0)
                            {
                                PlayingPlayer.Control.GetBufferedTimeRange(0, ref t1, ref t2);
                                t1 /= PlayingPlayer.Info.GetDurationMs();
                                t2 /= PlayingPlayer.Info.GetDurationMs();

                                isOn = false;
                            }
                        }

                        Vector2 anchorMin = Vector2.zero;
                        Vector2 anchorMax = Vector2.one;

                        if (_bufferedSliderImage != null && _bufferedSliderImage.type == Image.Type.Filled)
                        {
                            _bufferedSliderImage.fillAmount = d;
                        }
                        else
                        {
                            anchorMin[0] = t1;
                            anchorMax[0] = t2;
                        }

                        _bufferedSliderRect.anchorMin = anchorMin;
                        _bufferedSliderRect.anchorMax = anchorMax;
                    }
                }
            }
        }



        public void IsNext()
        {
            _isNext = true;
        }



        public void SaveVideoData()
        {

            if (_VideoIndex == 1)
            {
                int idx = 0;
                if (_currentTime < 60)
                {
                    idx = 0;
                }
                else if (_currentTime > 60 && _currentTime < 120)
                {
                    idx = 1;
                }
                else if (_currentTime > 120 && _currentTime < 180)
                {
                    idx = 2;
                }
                else if (_currentTime > 180 && _currentTime < 240)
                {
                    idx = 3;
                }
                else
                {
                    idx = 3;
                }
                Debug.Log("SaveVideoData  >>>>>>>>>>>>>>>  Index : " + idx + "     TIME  : " + _currentTime);
                GameObject.Find("Canvas").GetComponent<SaveAppData>().SaveSettings(idx);

            }
            // Debug.Log(" VIDEO INDEX : " + _VideoIndex  + "        IDX  :    "  + idx);
        }

        public void OnSaveVideoData(int value)
        {
            int idx = value;
            GameObject.Find("Canvas").GetComponent<SaveAppData>().SaveSettings(idx);
        }

        // Callback function to handle events
        public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
        {
            switch (et)
            {

                case MediaPlayerEvent.EventType.ReadyToPlay:

                    break;
                case MediaPlayerEvent.EventType.Started:

                    break;
                case MediaPlayerEvent.EventType.FirstFrameReady:
                    SwapPlayers();
                    break;

                case MediaPlayerEvent.EventType.FinishedPlaying:
                    // main 으로 ..

                    if (_VideoIndex == 0)
                    {
                        PlayingPlayer.Control.Rewind();
                    }
                    else if (_VideoIndex == 4)
                    {
                        OnOpenVideoInx(1);
                    }
                    else if (_VideoIndex == 13)
                    {
                        ChangeScene("Libirary");
                    }
                    else if (_VideoIndex == 6 || _VideoIndex == 10)
                    {
                        OnOpenVideoInx(10);
                    }
                    else if (_VideoIndex == 12)
                    {
                        OnOpenVideoInx(2);
                    }
                    else if (_VideoIndex == 17)
                    {
                        ChangeScene("Cartoon");
                    }
                    else if (_VideoIndex == 9)
                    {
                        ChangeScene("PaintingGameChoose");
                    }
                    else if (_VideoIndex == 8)
                    {
                        ChangeScene("BubleGameChoose");
                    }
                    else if (_VideoIndex == 18)
                    {
                        SceneManager.LoadScene("Main");
                    }
                    else if (19 <= _VideoIndex && _VideoIndex <= 23)
                    {
                        DataSender.Instance.danceIndex = _VideoIndex - 19;
                        ChangeScene("Dance");
                    }
                    else
                    {
                        SaveVideoData();
                        SceneManager.LoadScene("Main");
                    }


                    break;

                case MediaPlayerEvent.EventType.Stalled:

                    break;

            }

        }

        public void ChangeScene(string _SceneName)
        {
            //StartCoroutine(ChangeScene(_SceneName, 4f));
            UnityEngine.SceneManagement.SceneManager.LoadScene(_SceneName);
        }

        public IEnumerator ChangeScene(string _SceneName, float _time)
        {
            //while (PlayingPlayer.Control.GetCurrentTimeMs() < 4000)
            //    yield return new WaitForEndOfFrame();

            yield return new WaitForSeconds(4f);

            UnityEngine.SceneManagement.SceneManager.LoadScene(_SceneName);
        }


    }
}
#endif
