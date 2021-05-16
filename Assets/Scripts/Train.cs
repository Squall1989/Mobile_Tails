using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Train : MonoBehaviour
{
    public float trainSpeed;
    private float rotateSpeed;
    public Transform lokoPoint, tailPoint;
    //private int lokoPos;
    private const float speedCorrect = 1/20f;
    private const float rotateMultiplier = 92f / 2.5f ;
    private float lowSpeed, lowRotate, normSpeed, normRotate;
    private LinkedList<WayStruct> wayList;
    LinkedListNode<WayStruct> wayNode;
    //private StartTail startTail;
    //private List<RailConnector> connectorList;
    private Transform tailTS, lokoTS, lokoChild, tailChild;
    private Coroutine wayCorout;
    private bool OnFinishTail;
    // Start is called before the first frame update
    void Start()
    {
        OnFinishTail = false;

        tailTS = transform.GetChild(0);
        lokoTS = transform.GetChild(1);
        lokoChild = lokoTS.GetChild(1);
        tailChild = tailTS.GetChild(1);

        normSpeed = PlayerPrefs.GetFloat("trainSpeed") <= 0 ? trainSpeed/speedCorrect : PlayerPrefs.GetFloat("trainSpeed");
        normRotate = normSpeed * rotateMultiplier;

        
        lowSpeed = PlayerPrefs.GetFloat("lowSpeed") <= 0 ? (trainSpeed/3f)/speedCorrect : PlayerPrefs.GetFloat("lowSpeed");
        lowRotate = lowSpeed * rotateMultiplier;

    }

    public void SetStart(StartTail start_)
    {
        //startTail = start_;
    }

    private void changeSpeed(bool slow)
    {
        if (slow)
        {
            trainSpeed = lowSpeed;
            rotateSpeed = lowRotate;

        }
        else
        {
            trainSpeed = normSpeed;
            rotateSpeed = normRotate;

        }
    }

    public void ComeOnFinish()
    {
        OnFinishTail = true;
    }

    public Transform LokoForPassenger()
    {
        return lokoPoint;
    }
    public Transform TailForPassenger()
    {
        return tailPoint;
    }

    public void Stop()
    {
        StopAllCoroutines();
        trainSpeed = 0;
        rotateSpeed = 0;
    }

    public void ConvertPoints(List<RailConnector> connectors)
    {
        wayList = new LinkedList<WayStruct>();
        for (int i = 0; i < connectors.Count; i++)
        {
            if (wayList.Last != null)
                wayList.Last.Value = new WayStruct(wayList.Last.Value.transform, connectors[i].GetParentTale()); 
            for (int w = 0; w < connectors[i].wayPoints.Length; w++)
            {
                wayList.AddLast(new WayStruct( connectors[i].wayPoints[w], null));
            }
        }
        wayNode = wayList.First;
        if (wayNode == null)
            return;
        //float forwardedWayPoint = lokoTS.InverseTransformPoint(wayNode.Value.position).z;
        while (wayNode.Next != null && lokoTS.InverseTransformPoint(wayNode.Value.transform.position).z > 0)
        { //  wayList.Remove(wayList[0]);

            wayNode = wayNode.Next;
            if (wayNode.Value.tailToBlock != null)
                wayNode.Value.tailToBlock.BlockTail(true);
        }
    }

    public void StartForWayPoints()
    {
        if (wayList == null)
        {
            Debug.LogError("Not set way");
            return;
        }

        trainSpeed = normSpeed;
        rotateSpeed = normRotate;
        wayCorout = StartCoroutine(WayCoroutine());
    }

    public void ChangePoints(List<RailConnector> changeConnectors)
    {

    }


    private void RotateLokoTail(Transform lokoTail, Transform wayPoint)
    {
        float angle = Vector3.Angle(lokoTail.forward, wayPoint.position - lokoTail.position);
        if(angle != 0)//log == Infinity
            angle = Mathf.Log(angle,5f);
        float inverse = lokoTail.InverseTransformPoint(wayPoint.position).x;
        if (inverse < 0 )
            angle *= -1;
        lokoTail.Rotate(Vector3.up, angle * Time.deltaTime * rotateSpeed * speedCorrect, Space.World);
        
    }
    /*
    private int BlockNextRail(int railNum)
    {
        
        if (railNum < connectorList.Count)
        {
            connectorList[railNum].GetParentTale().BlockTail(true);
            return connectorList[railNum].wayPoints.Length;
        }
        else
            return 0;
        
    }
    */
    private IEnumerator WayCoroutine()
    {
        EventManager.Invoke("TrainStart", new BecomeEvent(true, 0, 0));
        //wayNode = wayList.First;
        if (wayNode == null)
        {
            //GameplayController.controller.TrainFinish(OnFinishTail);
            yield break;
        }
        transform.LookAt(wayNode.Value.transform.position);
        //int railToBlock = 1;
        //int wayNextCount = BlockNextRail(railToBlock);
        //int currWayCount = 0;
        while (wayNode != null)
        {
            LinkedListNode<WayStruct> nextNode = wayNode.Next;
            while (nextNode != null)
            {
                float forwardLokoPoint = lokoChild.InverseTransformPoint(nextNode.Value.transform.position).z;
                if (forwardLokoPoint > 0 )
                {
                    RotateLokoTail(lokoTS, nextNode.Value.transform);
                    break;

                }
                nextNode = nextNode.Next;
                
            }

            Vector3 forwardVector = wayNode.Value.transform.position - transform.position;
            transform.position += forwardVector.normalized * Time.deltaTime * trainSpeed * speedCorrect;

            LinkedListNode<WayStruct> prevNode = wayNode.Previous;
            while (prevNode != null)
            {
                float backwardTailPoint = tailChild.InverseTransformPoint(prevNode.Value.transform.position).z;
                if (backwardTailPoint > 0)
                {
                    RotateLokoTail(tailTS, prevNode.Value.transform);

                    break;

                }
                prevNode = prevNode.Previous;
            }



            float forwardedWayPoint = lokoTS.InverseTransformPoint(wayNode.Value.transform.position).z;

            if (Mathf.Abs(forwardedWayPoint) <= 0.05f || forwardedWayPoint < 0)
            { //  wayList.Remove(wayList[0]);
                wayNode = wayNode.Next;
                if (wayNode.Value.tailToBlock != null)
                    wayNode.Value.tailToBlock.BlockTail(true);
                /*
                if (++currWayCount >= wayNextCount)
                {
                    wayNextCount = BlockNextRail(++railToBlock);
                    currWayCount = 0;
                    //Debug.Log("Block " + railToBlock + " rail");
                }
                */
            }

            if (wayNode.Next == null)
            {
                float forwardLokoPoint = lokoChild.InverseTransformPoint(wayNode.Value.transform.position).z;
                if (forwardLokoPoint <= 0.1f)//Finish
                {
                    //GameplayController.controller.TrainFinish(OnFinishTail);
                    wayNode = wayNode.Next;
                }
            }

            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 11)
        {
            Debug.Log("Contact: " + other.gameObject.name);
        }
    }

    public void PausePath()
    {
        changeSpeed(true);
        /*
        if (wayCorout != null)
        {
            StopCoroutine(wayCorout);
            wayCorout = null;
        }
        */
    }
    public void ContinuePath()
    {
        changeSpeed(false);
        //Debug.Log("Train continue path");
        //wayCorout = StartCoroutine(WayCoroutine());
    }
}

public struct WayStruct
{
    public WayStruct(Transform transform_, Tail tail_)
    {
        transform = transform_;
        tailToBlock = tail_;
    }
    public Transform transform;
    public Tail tailToBlock;
}
