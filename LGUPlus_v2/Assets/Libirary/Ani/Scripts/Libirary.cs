using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Libirary : MonoBehaviour
{
    public GameObject newBooks;
    public Text newBookContents;

    public Transform[] bookPositions;
    public Transform bookParent;
    public Image bookPrefab;

    public int index = 0;
    public Image selecter;
    public Image selecterSomenale;

    public List<Image> bookList = new List<Image>();
    public Dictionary<string, string> bookDatas = new Dictionary<string, string>();


    public Text remoteText;
    public Text remoteDescription;

    public MediaPlayer _mediaPlayer;

    public bool isOpen = false;
    public Animator door;
    public GameObject leftObject;
    public GameObject rightObject;

    public GameObject temperture;

    public GameObject UnderRemocontroller;
    public Slider slider;

    public AudioSource audioSource;
    public AudioClip guideSound;

    private float touchTime;

    // Start is called before the first frame update
    void Start()
    {
        string[] BookListDatas = Resources.Load("BookDatas/Book_v1").ToString().Replace('\r', ' ').Split('\n');

        for (int i = 1; i < BookListDatas.Length - 1; i++)
        {
            string[] bds = BookListDatas[i].Split(',');

            Image obj = Instantiate(bookPrefab, bookParent);
            obj.transform.GetComponent<Image>().sprite = Resources.Load<Sprite>("Book/B_" + int.Parse(bds[0]).ToString("0000") + "_" + bds[1].Replace(" ", "-").Replace("_", ",").Replace("?", ""));
            obj.name = bds[1].Replace("?", "");

            if (i - 1 < bookPositions.Length)
                obj.transform.position = bookPositions[i - 1].position;
            else
                obj.transform.position = bookPositions[bookPositions.Length - 1].position + Vector3.right * 220;

            bookList.Add(obj);
            bookDatas.Add(bds[1].Replace("?", ""), bds[1] + "\n" + bds[2] + "\n" + bds[3] + "\n" + bds[4]);
            bookDatas.Add(bds[1].Replace("?", "") + "con", bds[5]);
        }

        slider.maxValue = BookListDatas.Length - 3;
        slider.value = 0;

        SelectBook(index);

        touchTime = Time.time;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            CloseDoor();

        if (Input.GetMouseButtonDown(0))
            touchTime = Time.time;

        if (Time.time - touchTime >= 30f)
        {
            audioSource.PlayOneShot(guideSound);
            touchTime = Time.time;
        }
    }

    public void SelectBook()
    {
        SelectBook((int)slider.value);
    }

    public void SelectBook(int _idx)
    {
        if (!(0 <= _idx && _idx < bookList.Count))
            return;

        if (_idx / bookPositions.Length != index / bookPositions.Length)
        {
            for (int i = 0; i < bookList.Count; i++)
            {
                bookList[i].transform.position = bookPositions[bookPositions.Length - 1].position + Vector3.right * 220;
            }

            for (int i = 0; i < bookPositions.Length && (_idx / bookPositions.Length) * bookPositions.Length + i < bookList.Count; i++)
            {
                bookList[(_idx / bookPositions.Length) * bookPositions.Length + i].transform.position = bookPositions[i].position;
            }
        }


        {
            string[] bds = bookDatas[bookList[index].name].Split('\n');

            newBooks.GetComponent<Image>().sprite = bookList[index].sprite;
            newBooks.transform.GetChild(0).GetComponent<Text>().text = bds[0];
            newBooks.transform.GetChild(1).GetComponent<Text>().text = bds[1] + ", " + (bds[1].Length > 7 ? "\n" : "") + bds[2] + "\n" + bds[3];
            newBookContents.text = bookDatas[bookList[index].name + "con"].Replace("_", "\n").Replace("+", ",");
        }

        index = _idx;

        selecterSomenale.sprite = bookList[index].sprite;

        remoteText.text = index.ToString();
        remoteDescription.text = bookDatas[bookList[index].name];

        selecter.transform.parent = bookList[index].transform;
        selecter.transform.localPosition = Vector3.down * 30;
    }

    public void RightSelect()
    {
        if (isOpen == false)
        {
            SelectBook(index + 1);
            slider.value = index;
        }
    }
    public void LeftSelect()
    {
        if (isOpen == false)
        {
            SelectBook(index - 1);
            slider.value = index;
        }
    }

    public void BookRead()
    {
        if (isOpen == false)
        {
            _mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "LibiraryVideos/" + bookList[index].name + "_1440.mp4");

            door.Play("ReadBook");

            for (int i = 0; i < bookList.Count; i++)
                bookList[i].transform.parent = bookParent.transform;

            for (int i = 0; i < 12 && (index / 12) * 12 + i < bookList.Count; i++)
                if (i < 6)
                    bookList[(index / 12) * 12 + i].transform.parent = leftObject.transform;
                else
                    bookList[(index / 12) * 12 + i].transform.parent = rightObject.transform;

            isOpen = true;

            StartCoroutine(fade(true));
            StartCoroutine(remocontroller(true));
        }
    }

    public IEnumerator remocontroller(bool hide)
    {
        float t = 1f / 30f;

        Vector3 target = new Vector3(0f, (hide ? -400f : 0f), 0f);
        Vector3 speed = (target - UnderRemocontroller.transform.localPosition) / 30;

        for (int i = 0; i < 30; i++)
        {
            UnderRemocontroller.transform.localPosition += speed;

            if (target == UnderRemocontroller.transform.localPosition)
                break;

            yield return new WaitForSeconds(t);
        }

        UnderRemocontroller.transform.localPosition = target;
    }

    public IEnumerator fade(bool _fade)
    {
        float t = 3f / 50f;

        Image[] imgs = temperture.GetComponentsInChildren<Image>();
        Color color = new Color(0, 0, 0, 1f / 50f * (_fade ? -1 : 1));

        for (int i = 0; i < 50; i++)
        {
            foreach (var img in imgs)
            {
                img.color += color;
            }

            yield return new WaitForSeconds(t);
        }

        foreach (var img in imgs)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, (_fade ? 0 : 1));
        }
    }

    public void CloseDoor()
    {

        door.Play("CloseBookcase");

        _mediaPlayer.Pause();

        StartCoroutine(fade(false));
        StartCoroutine(remocontroller(false));


        isOpen = false;
    }




    public void ChangeScene()
    {
        if (isOpen)
            CloseDoor();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }
}
