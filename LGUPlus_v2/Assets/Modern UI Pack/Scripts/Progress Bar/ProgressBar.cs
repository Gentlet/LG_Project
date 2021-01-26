using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class ProgressBar : MonoBehaviour
    {
        [Header("OBJECTS")]
        public Transform loadingBar;
        public Transform textPercent;

        [Header("VARIABLES (IN-GAME)")]
        public bool isOn;
        public bool restart;
        [Range(0, 100)] public float currentPercent;
        [Range(0, 100)] public int speed;

        [Header("SPECIFIED PERCENT")]
        public bool enableSpecified;
        public bool enableLoop;
        [Range(0, 100)] public float specifiedValue;


        //public float maxper = 0;

       public float maxPer = 100;

        private int currentCount = 0;
        private bool isPlay = false;


        void Update()
        {
            
            if (currentPercent <= maxPer && isOn == true && enableSpecified == false)
            {
                currentPercent += speed * Time.deltaTime;
              //  Debug.Log("currentCount 0 > " + currentCount);

            }
            

            if (currentPercent <= maxPer && isOn == true && enableSpecified == true)
            {
                if (currentPercent <= specifiedValue)
                {
                    currentPercent += speed * Time.deltaTime;
                }
                    

                if (enableLoop == true && currentPercent >= specifiedValue)
                {
                    currentPercent = 0;
                    currentCount++;
                }
                    
            }


            if (currentPercent == maxPer || currentPercent >= maxPer && restart == true)
            {
                currentPercent = 0;
               // Debug.Log("currentCount  3 > " + currentCount);
            }
            
            if (enableSpecified == true && specifiedValue == 0)
            {
                currentPercent = 0;
               // Debug.Log("currentCount 4 > " + currentCount);
            }
                

            loadingBar.GetComponent<Image>().fillAmount = currentPercent / maxPer;

            textPercent.GetComponent<TextMeshProUGUI>().text = ((int)currentPercent).ToString("F0") + "%";

      

        }
    }
}