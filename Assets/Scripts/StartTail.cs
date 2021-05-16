using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartTail : Tail
{
    private bool trainStarted;
    private Train train;
    private const float checkTime = .1f;
    public Train gettingTrain;
    public List<RailConnector> connectorList { get; private set; }
    private void Awake()
    {
        connectorList = new List<RailConnector>();
        //StartCoroutine(connectCheck());
        //EventManager.AddListener("connect", ConnectNewTail);
            
    }
    public RailConnector startConnector1;


    private void OnEnable()
    {
        EventManager.AddListener("PlayLevel", LevelStart);

    }

    private void OnDisable()
    {
        EventManager.RemoveListener("PlayLevel", LevelStart);

    }

    public void DisconnectTail(RailConnector disconnectedTail)
    {
        int disconnectPos = connectorList.Count;

        for (int i = 0; i < connectorList.Count; i++)
        {
            if(connectorList[i] == disconnectedTail)
            {
                Debug.Log("Current");

                disconnectPos = i;
                break;
            }
            if(disconnectedTail == connectorList[i].GetOppositeConnector())
            {
                Debug.Log("Opposite");
                disconnectPos = i+1;
                break;
            }
        }
        Debug.Log("disconnectPos: " + disconnectPos);
        Debug.Log("connectorList before: " + connectorList.Count);

        /*
        int disconnectPos = connectorList.Count - 1;
        for(int i = 0; i < connectorList.Count; i++)
        {

            RailConnector[] railConnectors = connectorList[i].GetNextConnect();
            if (railConnectors == null)
            {
                disconnectPos = i;
                Debug.Log("railConnectors null, pos: " + i);

                break;
            }
            bool isConnectNext = false;
            for (int c = 0; c < railConnectors.Length; c++)
            {
                if (i == 0)
                {
                    isConnectNext = true;
                    break;

                }
                RailConnector nextConnector = connectorList[i - 1];

                if (railConnectors[c] == nextConnector)
                {
                    isConnectNext = true;
                }
            }
            if(!isConnectNext)
            {
                Debug.Log("Not railConnectors, pos: " + i);

                disconnectPos = i;
                break;
            }
        }
         */
        if (disconnectPos < connectorList.Count)
        {
            connectorList.RemoveRange(disconnectPos, connectorList.Count - disconnectPos);
            train.ConvertPoints(connectorList);
            Debug.Log("After disconnect: " + connectorList.Count);
        }

    }

    private void SetDelegatesDisconnect()
    {
        for(int i = 0; i < connectorList.Count; i++)
        {
            connectorList[i].disconnector = new RailConnector.disconnectorTail(DisconnectTail);
            RailConnector railConnector = connectorList[i].GetNextConnect()[0];
            railConnector.disconnector = new RailConnector.disconnectorTail(DisconnectTail);



        }
    }

    public void SetFinishWayList(List<RailConnector> connectorListFinish)
    {

        //if (trainStarted)
        {
            //TrainWayRefresh(connectorListFinish);
        }
        //else
        {
            connectorList.Clear();
            connectorList.AddRange(connectorListFinish);
            TrainLookStart();

            Debug.Log("finish ways: " + connectorList.Count);
            if(trainStarted)
            {
                train.ConvertPoints(connectorList);
            }
            else
                StartTrain();

        }
    }

    private void TrainWayRefresh(List<RailConnector> connectorListNew)
    {
        Debug.Log("Train way refresh!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

        bool added = false;
        for (int c = 0; c < connectorListNew.Count; c++)
        {
            int revertC = connectorListNew.Count - c - 1;
            if (c < connectorList.Count)
            {
                if (connectorListNew[revertC] != connectorList[c])
                {
                    Debug.Log("Wrong connect");
                    break;
                }
            }
            else
            {
                added = true;
                //train.AddConnector(connectorListNew[revertC]);
                connectorList.Add(connectorListNew[revertC]);
            }
        }
        if (added)
            train.ConvertPoints(connectorList);
    }

    public void SetStartWayList(List<RailConnector> reverseList)
    {
        //if (trainStarted && connectorList.Count > 0)
        {
            Debug.Log("TrainWayRefresh");
            //TrainWayRefresh(reverseList);

        }
        //else
        {
            connectorList.Clear();
            connectorList.AddRange(reverseList);
            TrainLookStart();
        }

        if (trainStarted)
            train.ConvertPoints(connectorList);

        SetDelegatesDisconnect();

        Debug.Log("connectorList start: " + connectorList.Count);
    }

    private void TrainLookStart()
    {
        if(!train && gettingTrain)
        {
            AddNewSpecial(gettingTrain.gameObject, 't');//Only for presentation
            train = specialGraphic.GetComponent<Train>();
        }
        train.transform.LookAt(connectorList[0].wayPoints[0]);
        train.transform.Rotate(Vector3.up, 180f);
    }


    void Start()
    {
        /*GameplayController.controller.SetStartTail(this);
        if (!train && GameplayController.controller.IsLevelPlaying())
        {

            AddNewSpecial(ControlTales.controlTales.GetSpecial('t'), 't');
            train = specialGraphic.GetComponent<Train>();
        }
        */
    }

    private void LevelStart(BecomeEvent BE)
    {
        
    }




    public void StopTrain()
    {
        train.Stop();
        //specialGraphic.GetComponent<Train>().Stop();
    }






    public void StartTrain()
    {

        train.ConvertPoints(connectorList);

        train.StartForWayPoints();
        trainStarted = true;
        EventManager.Invoke("TrainStart", new BecomeEvent(true, 0,0));
        //if(!Mechanics.mechanics.IsTimeMechanic())
          //  TailsTable.talesTable.BlockAllTails();
    }


    
}