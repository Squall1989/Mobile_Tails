using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailConnector : MonoBehaviour
{
    public RailConnector opposingConnector;
    private RailConnector[] connectedRails;
    private Tail parentTail;
    public Transform[] wayPoints;
    public delegate void disconnectorTail(RailConnector railConnector);
    public disconnectorTail disconnector;

    private void Awake()
    {
        parentTail = transform.parent.parent.GetComponent<Tail>();
    }
    private void OnEnable()
    {
        if(wayPoints.Length == 0)
        {
            Transform[] tempPoints = opposingConnector.GetWayPoints();
            wayPoints = new Transform[tempPoints.Length];
            if (tempPoints.Length == 0)
                return;
            for (int i = 0; i < tempPoints.Length; i++)
            {
                int endI = tempPoints.Length - 1 - i;
                wayPoints[i] = tempPoints[endI];
            }
        }

    }

    public Tail GetParentTale()
    {
        //Tail parent = transform.parent.parent.GetComponent<Tail>();
        return parentTail;
    }

    public Transform[] GetWayPoints()
    {
        return wayPoints;
    }

    public RailConnector GetOppositeConnector()
    {

        return opposingConnector;
    }

    public RailConnector[] GetNextConnect()
    {
        
        return connectedRails;
    }

    public void CheckForFinishTail(StartTail startTail, List<RailConnector> connectors_)
    {
        if (parentTail.taleType == Tail.TaleType.finish)
        {
            connectors_.Add(opposingConnector);
            //if(connectors_.Count > 4)
            startTail.SetFinishWayList(connectors_);
            return;
        }

        if (connectedRails == null)
            return;


        connectors_.Add(opposingConnector);
        for (int r = 0; r < connectedRails.Length; r++)
        {
            connectedRails[r].opposingConnector.CheckForFinishTail(startTail, connectors_);

        }
    }

    public void CheckForStartTail(List<RailConnector> connectors_)
    {
        
        if(parentTail.taleType == Tail.TaleType.start && connectors_.Count > 1)
        {
            connectors_.Add(opposingConnector);
            RailConnector[] firstConnects = connectors_[0].opposingConnector.GetNextConnect();
            StartTail startTail = parentTail.GetComponent<StartTail>();
            connectors_.Reverse();

            if(connectors_[connectors_.Count-1].GetParentTale().taleType == Tail.TaleType.finish)
            {
                startTail.SetFinishWayList(connectors_);
                Debug.Log("Set finish");
                return;
            }

            startTail.SetStartWayList(connectors_);

            if (firstConnects != null)
              for(int r = 0; r < firstConnects.Length; r++)
                firstConnects[r].opposingConnector. CheckForFinishTail(startTail, connectors_);
            return;
        }

        if (connectedRails == null)
            return;

        connectors_.Add(this);
        for (int r = 0; r < connectedRails.Length; r++)
        {
            connectedRails[r].opposingConnector.CheckForStartTail(connectors_);

        }
        
    }

    protected void OnTriggerEnter(Collider other)
    {

        connectedRails = other.GetComponents<RailConnector>();
        if (connectedRails != null)
        {
            List<RailConnector> first_ = new List<RailConnector>();
            
            CheckForStartTail(first_);

        }
    }
    protected void OnTriggerExit(Collider other)
    {
        if (disconnector != null)
        {
            disconnector.Invoke(this);
            disconnector = null;
        }
        connectedRails = null;
    }
}

