using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class RadialSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private const string PREFS_UI_SAVE_NAME = "Radial";

        [Header("OBJECTS")]
        public Image sliderImage;
        public Transform indicatorPivot;
        public TextMeshProUGUI valueText;

        [Header("SETTINGS")]
        public string sliderTag;
        public float maxValue = 100;
        public float currentValue = 50.0f;
        [Range(0, 8)] public int decimals;
        public bool isPercent;
        public bool rememberValue;
        public bool enableCurrentValue;
        public UnityEvent onValueChanged;

       // public GameObject videoSeekSlider;
        public GameObject vcr;


        private int currentCount = 0;
        public float currentPercent;

        //public GameObject finger;
       

        private GraphicRaycaster graphicRaycaster;
        private RectTransform hitRectTransform;

        private bool isPointerDown;
        private float currentAngle;
        private float currentAngleOnPointerDown;
        private float valueDisplayPrecision;
        private int _currentTouchId = -2; 


        // Sets
        public float SliderAngle
        {
            get { return currentAngle; }
            set { currentAngle = Mathf.Clamp(value, 0.0f, 360.0f); }
        }

        // Slider value with applied display precision, i.e. the number of decimals to show.
        public float SliderValue
        {
            get { return (long)(SliderValueRaw * valueDisplayPrecision) / valueDisplayPrecision; }

            set { SliderValueRaw = value; }

        }

        // Raw slider value, i.e. without any display precision applied to its value.
        public float SliderValueRaw
        {
            get { return SliderAngle / 360.0f * maxValue; }
            set { SliderAngle = value * 360.0f / maxValue; }
        }

        private void Awake()
        {
            graphicRaycaster = GetComponentInParent<GraphicRaycaster>();

            if (graphicRaycaster == null)
            {
                Debug.LogWarning("Could not find GraphicRaycaster component in parent of this GameObject: " + name);
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _currentTouchId = -2;
            valueDisplayPrecision = Mathf.Pow(10, decimals);
            LoadState();
            UpdateUI();

            //finger.SetActive(false);

            if (rememberValue == false && enableCurrentValue == true)
            {
                SliderAngle = currentValue * 3.6f;
                UpdateUI();
            }
        }
        
        public void FingerView()
        {

           // finger.SetActive(true);
        }

        public void FingerActive()
        {
            //finger.SetActive(false);
        }
       

        public void OnPointerDown(PointerEventData eventData)
        {

            Debug.Log(" pointerId   :  " + eventData.pointerId );

            if (_currentTouchId != -2) return;
            /*
            _currentTouchId = eventData.pointerId;

            hitRectTransform = eventData.pointerCurrentRaycast.gameObject.GetComponent<RectTransform>();
                isPointerDown = true;
                currentAngleOnPointerDown = SliderAngle;
                HandleSliderMouseInput(eventData, true);
                vcr.GetComponent<RenderHeads.Media.AVProVideo.Demos.VCRContral>().OnVideoSliderDown();
                */
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != _currentTouchId) return;

            if (HasValueChanged())
                SaveState();

            _currentTouchId = -2;

            hitRectTransform = null;
            isPointerDown = false;
            vcr.GetComponent<RenderHeads.Media.AVProVideo.Demos.VCRContral>().OnVideoSliderUp();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _currentTouchId) return;

            HandleSliderMouseInput(eventData, false);
             vcr.GetComponent<RenderHeads.Media.AVProVideo.Demos.VCRContral>().OnVideoSeekSlider();
        }

        public void LoadState()
        {
            if (!rememberValue)
                return;

            currentAngle = PlayerPrefs.GetFloat(sliderTag + PREFS_UI_SAVE_NAME);
        }

        public void SaveState()
        {
            if (!rememberValue)
                return;

            PlayerPrefs.SetFloat(sliderTag + PREFS_UI_SAVE_NAME, currentAngle);
        }



        public void UpdateUI()
        {
            float normalizedAngle = SliderAngle / 360.0f;

            // Rotate indicator (handle / knob)
            indicatorPivot.transform.localEulerAngles = new Vector3(180.0f, 0.0f, SliderAngle);

            // Update slider fill amount
            sliderImage.fillAmount = normalizedAngle;

            // Update slider label
            valueText.text = string.Format("{0}{1}", SliderValue , isPercent ? "%" : "");

            // Debug.Log("currentCount 0 > " + currentCount);


            


        }


        private bool HasValueChanged()
        {
            return SliderAngle != currentAngleOnPointerDown;
        }


        public void OnUpdatePer(float value)
        {
          //  if (!isPointerDown)
           //     return;

            SliderAngle = value * 3.6f;
            UpdateUI();

        }


        private void HandleSliderMouseInput(PointerEventData eventData, bool allowValueWrap)
        {


            if (!isPointerDown)
                return;


            //OnVideoSeekSlider
          
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(hitRectTransform, eventData.position, eventData.pressEventCamera, out localPos);
            float newAngle = Mathf.Atan2(-localPos.y, localPos.x) * Mathf.Rad2Deg + 180f;
            //Debug.Log("  HandleSliderMouseInput   :   " + newAngle + "     / SliderValue  :" + SliderValue );

            if (!allowValueWrap)
            {
                float currentAngle = SliderAngle;
                bool needsClamping = Mathf.Abs(newAngle - currentAngle) >= 180;

                if (needsClamping)
                    newAngle = currentAngle < newAngle ? 0.0f : 360.0f;
            } 

            SliderAngle = newAngle;
            /// 슬라이더 움직일시 .동영상 하고 믹싱 되게 .
            /// 
          

            UpdateUI();

            if (HasValueChanged())
                onValueChanged.Invoke();
        }



    }
}