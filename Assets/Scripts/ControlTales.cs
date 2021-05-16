using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlTales : MonoBehaviour
{
    public static ControlTales controlTales;

    public Transform Canvas3D, swingTale;
    public GameObject specialPassenger, specialTrain;
    public Shader shaderGray, shaderStandart;
    public AudioClip moveStart, moveCycle, moveEnd, rotate;
    private const int deltaTime = 30;
    private Tail highLightTale, draggingTale, rotatingTale;
    private Vector3 dragTaleStartPos, dragDeltaVector;
    private Queue<float> distTaleMove = new Queue<float>(deltaTime);
    private Queue<float> timeTaleMove = new Queue<float>(deltaTime);
    private bool touchDrag, touchStart, trainStarted, education;

    private const float dragTouchTime = .13f, deltaTouchDist = .05f;

    private float dragMovedDist, deltaTouch, speedTale;
    private Tail taleMovedTo;
    private Vector2 startTouchPos, startSwipePos;
    private AudioSource playerAudio;
    private Coroutine soundCoroutine, taleSpeedCoroutine;

    private void Awake()
    {
        controlTales = this;
    }
    private void OnEnable()
    {
        EventManager.AddListener("TrainStart", IsTrainStarted);
        EventManager.AddListener("PlayLevel", LevelStart);
    }
    private void OnDisable()
    {
        EventManager.RemoveListener("TrainStart", IsTrainStarted);
        EventManager.RemoveListener("PlayLevel", LevelStart);

    }
    // Start is called before the first frame update
    void Start()
    {
        playerAudio = GetComponent<AudioSource>();
        touchDrag = touchStart = false;
        dragMovedDist = deltaTouch = 0;
        taleMovedTo = null;
        startTouchPos = new Vector2();
    }

    private void stopSound()
    {

        if (soundCoroutine != null)
            StopCoroutine(soundCoroutine);
        if (taleSpeedCoroutine != null)
            StopCoroutine(taleSpeedCoroutine);
        distTaleMove.Clear();
        timeTaleMove.Clear();
        playerAudio.Stop();
        playerAudio.volume = 1f;
    }

    private void playSound(AudioClip clip)
    {
        if (PlayerPrefs.GetInt("Sound") == 0)
            return;
        
        playerAudio.volume = 1f;
        playerAudio.Stop();
        playerAudio.PlayOneShot(clip);
    }

    private void playTwoSound(AudioClip first, AudioClip second)
    {
        if (PlayerPrefs.GetInt("Sound") == 0)
            return;
        playerAudio.PlayOneShot(first);
        soundCoroutine = StartCoroutine(nextSound(first.length, second));
    }

    private void cycleSound(Vector3 deltaPos)
    {
        distTaleMove.Enqueue(deltaPos.magnitude);
        timeTaleMove.Enqueue(Time.deltaTime);
        while(distTaleMove.Count >= deltaTime)
        {
            distTaleMove.Dequeue();
            timeTaleMove.Dequeue();
        }

        float speed = distSumm() / timeSumm();

        playerAudio.volume = speed/10f;
    }

    private float timeSumm()
    {
        float time = 0;
        float[] timeArray = timeTaleMove.ToArray();
        for (int i = 0; i < distTaleMove.Count; i++)
        {
            time += timeArray[i];
        }
        return time;
    }

    private float distSumm()
    {
        float dist = 0;
        float[] distArray = distTaleMove.ToArray();
        for (int i = 0; i < distTaleMove.Count; i++)
        {
            dist += distArray[i];
        }
        return dist;
    }



    private IEnumerator nextSound(float time_, AudioClip next)
    {
        yield return new WaitForSeconds(time_);
        playerAudio.loop = true;
        playerAudio.clip = next;
        playerAudio.Play();
    }
    
    public GameObject GetTaleFromNum(int num)
    {
        return transform.GetChild(num).gameObject;
    }

    private void LevelStart(BecomeEvent BE)
    {
        playerAudio.enabled = BE.come;
        if(BE.come)
        {
            cancelRotate();
            stopDrag();
        }
        else
        {
           
        }
    }

    private void IsTrainStarted(BecomeEvent becomeEvent)
    {

        stopSound();
        stopDrag();
        cancelRotate();
        trainStarted = becomeEvent.come;

    }

    private Vector3 RayForPlatform(Vector2 touchPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(touchPos);
        RaycastHit hit;
        int layerMask = 1 << 8;
        if (Physics.Raycast(ray.origin, ray.direction, out hit, 1000f, layerMask))
        {
            Vector3 hitPoint = hit.point;
            hitPoint.y += 1.005f;
            return hitPoint;
        }
        else
            return Vector3.zero;
    }


    private Transform RayForTales()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
        RaycastHit hit;
        int layerMask = 1 << 9;
        if (Physics.Raycast(ray.origin, ray.direction, out hit, 1000f, layerMask))
        {
            return hit.transform;
        }
        else
            return null;
    }

    private void zoomTouches()
    {

    }

    private void stopDrag()
    {
        //Debug.Log("Stop drag");
        touchDrag = false;
        if (draggingTale)
        {
            

            draggingTale.GetComponent<BoxCollider>().enabled = true;
            if (taleMovedTo && dragMovedDist >= TailsTable.talesTable.GetTailsDistance() / 2f) 
            {
                PositionTale dragPos = draggingTale.GetPos();
                PositionTale taleMovedPos = taleMovedTo.GetPos();
                draggingTale.transform.position = dragTaleStartPos;
                TailsTable.talesTable.ExChangeTale(dragPos, taleMovedPos, false);
            }
            else
            {
                draggingTale.transform.position = dragTaleStartPos;

            }
            
        }
        dragMovedDist = 0;
        taleMovedTo = null;
        draggingTale = null;
    }
    
    private void clickTale()
    {
        if (!Camera.main)
            return;
        Transform rayTale = RayForTales();
        if (rayTale)
        {
            if (!rotatingTale)
            {
                Tail checkingTale = rayTale.GetComponent<Tail>();
                if (!checkingTale.IsBlocked() && checkingTale.rotatable)
                {
                    Canvas3D.gameObject.SetActive(true); 
                    Canvas3D.position = rayTale.position;
                    rotatingTale = checkingTale;
                }
                //else
                  //  StartCoroutine(grayingTale(rayTale.GetComponent<Tail>()));

            }
            else if (rotatingTale == rayTale.GetComponent<Tail>())
            {
                TailsTable.talesTable.TaleRotate(rotatingTale.GetPos().X, rotatingTale.GetPos().Z, 90);

                playSound(rotate);
            }
            
        }
        
    }
    public void setRotZero()//ToDo temp
    {
        StartCoroutine(startControl());
    }

    private void cancelRotate()
    {

        rotatingTale = null;
        Canvas3D.gameObject.SetActive(false);
        Canvas3D.position = new Vector3(2000, 2000, 2000);
    }

    private void dragTale(Vector2 touchPos_)
    {
        
        if (draggingTale)
        {
            Vector3 dragPos = RayForPlatform(touchPos_) - dragDeltaVector;
            //Перемещение строго вдоль осей Х или Z
            bool xMorez = Mathf.Abs(dragPos.x - dragTaleStartPos.x) >= Mathf.Abs(dragPos.z - dragTaleStartPos.z);


            Vector3 prevTalePos = draggingTale.transform.position;
            if (xMorez)
            {
                PositionTale talePos = draggingTale.GetPos();
                talePos.X += dragPos.x - dragTaleStartPos.x > 0 ? 1 : -1;
                Tail movedToTale = TailsTable.talesTable.GetTale(talePos);
                if(movedToTale && movedToTale.taleType == Tail.TaleType.empty)
                {                    
                    dragPos.z = dragTaleStartPos.z;
                    draggingTale.transform.position = dragPos;

                    float distToEnd = (dragTaleStartPos - movedToTale.transform.position).magnitude;
                    float distToMoved = (dragPos - dragTaleStartPos).magnitude;

                    if (distToEnd -.1f <= distToMoved)
                    {
                        Tail dragTale = draggingTale;
                        draggingTale.transform.position = movedToTale.transform.position;
                        dragMovedDist = distToEnd;
                        stopDrag();
                        startTouchPos = Input.touches[0].position;
                        //if (!trainStarted)
                        {
                            startDrag(dragTale, startTouchPos);
                        }
                    }
                    else
                        dragMovedDist = distToMoved;
                    taleMovedTo = movedToTale;
                }
                else
                {
                    draggingTale.transform.position = dragTaleStartPos;
                }

            }
            else
            {
                PositionTale talePos = draggingTale.GetPos();
                talePos.Z += dragPos.z - dragTaleStartPos.z > 0 ? 1 : -1;
                Tail movedToTale = TailsTable.talesTable.GetTale(talePos);
                if (movedToTale && movedToTale.taleType == Tail.TaleType.empty)
                {                    
                    dragPos.x = dragTaleStartPos.x;
                    draggingTale.transform.position = dragPos;

                    float distToEnd = (dragTaleStartPos - movedToTale.transform.position).magnitude;
                    float distToMoved = (dragPos - dragTaleStartPos).magnitude;

                    if (distToEnd -.1f  <= distToMoved)
                    {
                        Tail dragTale = draggingTale;
                        draggingTale.transform.position = movedToTale.transform.position;
                        dragMovedDist = distToEnd;
                        stopDrag();
                        startTouchPos = Input.touches[0].position;
                        //Mechanics.mechanics.StepMinus();
                        //if (!trainStarted)
                        {
                            startDrag(dragTale, startTouchPos);
                        }
                    }
                    else
                        dragMovedDist = distToMoved;
                    taleMovedTo = movedToTale;
                }
                else
                {

                    draggingTale.transform.position = dragTaleStartPos;
                }
            }
            if(draggingTale)
                cycleSound(draggingTale.transform.position - prevTalePos);//Sound
            
            if (!highLightTale)
            {
                Canvas3D.position = new Vector3(2000, 2000, 2000);
            }
        }
        else if (highLightTale && (touchDrag ))//start drag
        {


            startDrag(highLightTale, Input.touches[0].position);
            if (draggingTale)
            {
                //taleSpeedCoroutine = StartCoroutine(dragTaleCoroutine());
                playTwoSound(moveStart, moveCycle);

            }
        }
    }

    public void FalseTaleMoveStart(Tail taleMoved, Vector2 tailScreenPos_)
    {
        startDrag(taleMoved, tailScreenPos_);
        taleMoved.gameObject.layer = 31;
        for (int i = 0; i < taleMoved.transform.childCount; i++)
            taleMoved.transform.GetChild(i).gameObject.layer = 31;
        dragDeltaVector = Vector3.zero;
        deltaTouch = deltaTouchDist;
        taleMoved.transform.GetChild(taleMoved.transform.childCount - 2).gameObject.SetActive(false);
        touchDrag = true;
        dragTaleStartPos = taleMoved.transform.position;
        education = true;
    }
    /*public bool FalseTaleMoving(Vector2 screenMovePos)
    {
        dragTale(screenMovePos, true);
        if (draggingTale)
            return true;
        else
            return false;
    }
    */

    private void CheckSwipe()
    {

        if (startSwipePos == Vector2.zero)
            return;
        Vector2 endSwipePos = Input.touches[0].position;
        float deltaSwipe = endSwipePos.x - startSwipePos.x;

        if (Mathf.Abs(deltaSwipe) >= Screen.width / 4f)
        {
            int wiseInt = (int)(deltaSwipe / Mathf.Abs(deltaSwipe));
            CameraControl.cameraControl.CameraRotate(wiseInt);

        }
        startSwipePos = Vector2.zero;
    }

    public void FalseMoveEnd()
    {
        education = false;
    }

    private void startDrag(Tail draggingTail, Vector2 touchStart)
    {
        if (draggingTail.movable)
        {
            draggingTale = draggingTail.GetComponent<Tail>();
            if(draggingTale.IsBlocked())
            {

                draggingTale = null;
                return;
            }
            //draggingTale.GetComponent<BoxCollider>().enabled = false;
            dragTaleStartPos = draggingTale.transform.position;
            dragDeltaVector = RayForPlatform(touchStart) - dragTaleStartPos;

            swingTale.position = dragTaleStartPos;
            swingTale.rotation = Quaternion.identity;
        }
        else
        {
            //StartCoroutine(grayingTale(draggingTail));
        }
    }

    IEnumerator grayingTale(Tail greyinTail)
    {
        //Если нужны будут разные шейдеры - запоминать каждый
        for (int i = 0; i < greyinTail.transform.childCount; i++)
        {
            if (!greyinTail.transform.GetChild(i).GetComponent<MeshRenderer>())
                continue;
            Material taleMaterial = greyinTail.transform.GetChild(i).GetComponent<MeshRenderer>().material;

            taleMaterial.shader = shaderGray;
        }
        yield return new WaitForSeconds(.2f);
        for (int i = 0; i < greyinTail.transform.childCount; i++)
        {
            if (!greyinTail.transform.GetChild(i).GetComponent<MeshRenderer>())
                continue;
            Material taleMaterial = greyinTail.transform.GetChild(i).GetComponent<MeshRenderer>().material;
            taleMaterial.shader = shaderStandart;
        }
    }

    private IEnumerator startControl()
    {
        trainStarted = true;
        yield return new WaitForSeconds(.05f);
        trainStarted = false;
        cancelRotate();
    }

    public GameObject GetSpecial(char sp)
    {
        if (sp == 'p')
            return specialPassenger;
        else if (sp == 't')
            return specialTrain;
        else
            return null;
    }

    private void Update()
    {
        if (education)
            return;

        if (Input.touchCount > 1)
        {
            zoomTouches();
            return;
        }
        if (Input.touchCount == 1)
        {
            //if (clickTime >= dragTouchTime)
              //  touchDrag = true;
            if(deltaTouch >= deltaTouchDist * Screen.width)
            {
                touchDrag = true;


            }

            if (Input.touches[0].phase == TouchPhase.Moved && touchDrag)
            {
                if (rotatingTale)
                    cancelRotate();

                dragTale(Input.touches[0].position);
            }
            else if(Input.touches[0].phase == TouchPhase.Stationary)
            {
                if (draggingTale)
                    cycleSound(Vector3.zero);
            }
            else if (Input.touches[0].phase == TouchPhase.Ended)
            {
                stopSound();
                bool readyClick = false;
                if (touchDrag && deltaTouch <= deltaTouchDist * Screen.width)
                {
                    stopDrag();
                    touchDrag = false;
                    clickTale();
                    readyClick = true;
                }
                if (touchDrag )//End
                {

                    stopDrag();
                    playSound(moveEnd);
                }
                else if(!readyClick || !rotatingTale)
                    clickTale();
                //else
                {
                    CheckSwipe();
                }
                
                deltaTouch = 0;
                touchDrag = false;
                highLightTale = null;

            }
            else if (Input.touches[0].phase == TouchPhase.Began)
            {
                

                Transform rayTale = RayForTales();
                

                if (rayTale && rayTale.GetComponent<Tail>() && !rayTale.GetComponent<Tail>().IsBlocked())
                {
                    touchStart = true;
                    startTouchPos = Input.touches[0].position;
                    highLightTale = rayTale.GetComponent<Tail>();

                    if (rotatingTale && highLightTale != rotatingTale)
                        cancelRotate();
                }
                else
                {
                    rotatingTale = null;
                    draggingTale = null;
                    Canvas3D.position = new Vector3(2000, 2000, 2000);
                    if(!rayTale)
                    {
                        startSwipePos = Input.touches[0].position;
                    }
                }
                
            }

            if (touchStart)
            {
                //clickTime += Time.deltaTime;
                deltaTouch += Input.touches[0].deltaPosition.magnitude;
            }
            else
            {
                deltaTouch = 0;
                //clickTime = 0;
            }
        }
        
    }
}

