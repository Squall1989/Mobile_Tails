using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tail : MonoBehaviour
{
    public enum TaleType { empty, barrier, railway, start, finish, gasstation, dangerTank, dangerSoldier}
    public TaleType taleType;
    public bool rotatable, movable;
    public Vector3 specialPos;
    protected Vector3 cursorPos;
    protected int posX, posZ;
    protected int myNum;
    protected GameObject specialGraphic;
    public GameObject peron;
    public MeshRenderer meshRender;
    protected char special;

    protected bool blockTail;
    private List<Material> dangMatList;
    // Start is called before the first frame update


    public void RememberPositionInArray(int x, int z)
    {
        posX = x;
        posZ = z;
    }
    
    public void BlockTail(bool block_)
    {
        blockTail = block_;
    }
    public bool IsBlocked()
    {
        return blockTail;
    }

    public void RememberNum(int num)
    {
        myNum = num;
    }
    public int GetNum()
    {
        return myNum;
    }
    public PositionTale GetPos()
    {
        PositionTale myPos;
        myPos.X = posX;
        myPos.Z = posZ;
        return myPos;
    }
    public PositionTale ChangeOtherTale(Tail newTale)
    {
        TailsTable.talesTable.ReplaceTale(newTale, posX, posZ);
        PositionTale myPos;
        myPos.X = posX;
        myPos.Z = posZ;
        return myPos;
    }

    public void PrepareSpecialForRotate(bool before)
    {
        if (!specialGraphic)
            return;
        if(before)
            specialGraphic.transform.parent = transform;
        else
        {
            specialGraphic.transform.parent = null;

        }
    }


    public void SetDangerMat(Material dangerMat)
    {
        if(taleType == TaleType.railway &&  meshRender)
        {
            
            CheckMatList(dangerMat, false);
        }
    }
    public void RemoveDangMat(Material dangMat)
    {
        if (taleType == TaleType.railway && meshRender)
            CheckMatList(dangMat, true);
    }

    protected void CheckMatList(Material MaterialFor, bool remove)
    {
        if (dangMatList == null)
        {
            dangMatList = new List<Material>(2);
        }
        if (remove)
        {
           if(!dangMatList.Remove(MaterialFor))
            {
                Debug.LogError("Can't remove mat, dangMatList: " + dangMatList.Count);
                return;
            }
            MaterialFor = null;
            if(dangMatList.Count > 0)
            {
                MaterialFor = dangMatList[dangMatList.Count - 1];//Test
                
            }
        }
        else
        {
            dangMatList.Add(MaterialFor);
        }
        Material[] mats = meshRender.materials;
        mats[mats.Length - 1] = MaterialFor;
        meshRender.materials = mats;
        Debug.Log("Materials: " + dangMatList.Count + " remove: " + remove );

    }

    public void AddNewSpecial(GameObject specialNew, char name_char)
    {

        if(!specialGraphic)
        {
            specialGraphic = Instantiate(specialNew,transform);
            //specialGraphic = specialNew;
            specialGraphic.transform.localPosition = specialPos;
            //specialGraphic.transform.rotation = new Quaternion();
            special = name_char;
            if (peron)
                peron.SetActive(true);
            //specialGraphic.transform.parent = null;
        }
        else
        {
            //Destroy(specialGraphic);
            special = 'n';
        }
    }

    public char getSpecialName()
    {
        return special;
    }
}


public struct PositionTale
{
    public int X;
    public int Z;
}

