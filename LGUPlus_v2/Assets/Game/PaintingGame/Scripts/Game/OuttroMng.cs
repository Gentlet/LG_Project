using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Painting
{
    public class OuttroMng : MonoBehaviour
    {
        void OnEnable()
        {
            Invoke("Restart", 6f);
        }

        void Restart()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("PaintingGameChoose");
        }
    }
}