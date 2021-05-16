using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailsTable : MonoBehaviour
{
    public static TailsTable talesTable;
    public GameObject Terrain;
    private const float tailsDistance = 2f, fovNorm = 58, fovMax = 66;

    private int sizeX, sizeZ, currentStep;
    
    private const int maxTailsX = 32, maxTailsZ = 32;
    private Tail[,] arrayTails;
    private TailStartInfo[,] tailStartArray;
    private List<StepTailChange> stepList;
    private List<string[]> saveData;

    private Coroutine stepCorout;

    private bool coroutinePaused;

    private void Awake()
    {
        talesTable = this;
    }

    private void OnEnable()
    {
        EventManager.AddListener("PlayLevel", LevelStart);

    }

    private void OnDisable()
    {
        EventManager.RemoveListener("PlayLevel", LevelStart);

    }

    // Start is called before the first frame update
    void Start()
    {
        if(Terrain)
            Terrain.gameObject.SetActive(false);

        sizeX = sizeZ = 0;
        arrayTails = new Tail[maxTailsX, maxTailsZ];
        stepList = new List<StepTailChange>();
        saveData = new List<string[]>();
        currentStep = -1;
        Init();

        FileManager.LoadFile(1, 1);//For presentation only
    }

    private void Init()
    {
        for (int x = 0; x < maxTailsX; x++)
            for (int z = 0; z < maxTailsZ; z++)
                arrayTails[x, z] = null;

        //FileManager.LoadFile("test");
        //ChangeSize(4, 4);
    }

    private void LevelStart(BecomeEvent BE)
    {
        if(Terrain)
            Terrain.gameObject.SetActive(BE.come);
        if (BE.come)
        {

        }
        else
        {

            ClearTaleArray();
        }
    }

    public float GetTailsDistance()
    {
        return tailsDistance;
    }
    private void ClearTaleArray()
    {

        coroutinePaused = false;
        for(int x = 0; x < maxTailsX; x++)
        {
            for(int z = 0; z < maxTailsZ; z++)
            {
                if(arrayTails[x,z] != null)
                {
                    DestroyTail(x, z);
                }
            }
        }
    }

    public void BlockAllTails()
    {

        for (int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                if (arrayTails[x, z] != null)
                {

                    arrayTails[x, z].BlockTail(true);
                }
            }
        }
    }

    private void NextStep(StepTailChange step) //For level editor 
    {
        if (step.rotateTo == 0)
            ExChangeTale(step.taleFrom, step.taleTo, false);
        else
            TaleRotate(step.taleFrom.X, step.taleFrom.Z, step.rotateTo);
    }


    public void TaleRotate(int X, int Z, int deg_)
    {
        //arrayTails[X, Z].PrepareSpecialForRotate(true);
        arrayTails[X, Z].transform.Rotate(Vector3.up, deg_);
        //arrayTails[X, Z].PrepareSpecialForRotate(false);
    }

    private void CreateNewTail(int X, int Z, int num_, int degree, char special)
    {
        GameObject newTail = Instantiate(ControlTales.controlTales.GetTaleFromNum(num_));
        if (!newTail)
        {
            Debug.LogError("tail not load");
            return;
        }
        newTail.transform.parent = transform;
        newTail.transform.localPosition = new Vector3(X * tailsDistance, 0, Z * tailsDistance);
        arrayTails[X, Z] = newTail.GetComponent<Tail>();
        arrayTails[X, Z].RememberPositionInArray(X, Z);
        GameObject newSpecial = ControlTales.controlTales.GetSpecial(special);
        if(newSpecial)
        {
            arrayTails[X, Z].AddNewSpecial(newSpecial, special);

        }
        arrayTails[X, Z].transform.localRotation = Quaternion.identity;
        arrayTails[X, Z].transform.Rotate(Vector3.up, degree);
        arrayTails[X, Z].BlockTail(false);
    }
    private void DestroyTail(int X, int Z)
    {
        Destroy(arrayTails[X, Z].gameObject);
        arrayTails[X, Z] = null;
    }
    private void ChangeSize(int newSizeX, int newSizeZ)
    {

        for (int x = 0; x < maxTailsX; x++)
            for (int z = 0; z < maxTailsZ; z++)
            {
                if(x < newSizeX && z < newSizeZ && arrayTails[x,z] == null)//Тэйл ещё не создан
                {
                    CreateNewTail(x, z, 0, 0, 'n');
                }
                else if ((x >= newSizeX || z >= newSizeZ) && arrayTails[x, z] != null)//Тэйл не нужен, уничтожаем
                {
                    DestroyTail(x, z);
                }
            }
        sizeX = newSizeX;
        sizeZ = newSizeZ;
    }

    public void ExChangeTale(PositionTale pos1, PositionTale pos2, bool stepBack_)
    {
        Tail tail1 = arrayTails[pos1.X, pos1.Z];
        Tail tail2 = arrayTails[pos2.X, pos2.Z];
        Vector3 tail1Pos = tail1.transform.position;
        tail1.transform.position = tail2.transform.position;
        tail2.transform.position = tail1Pos;

        tail1.RememberPositionInArray(pos2.X, pos2.Z);
        tail2.RememberPositionInArray(pos1.X, pos1.Z);

        arrayTails[pos1.X, pos1.Z] = tail2;
        arrayTails[pos2.X, pos2.Z] = tail1;

    }


    public Tail GetTale(PositionTale talePos)
    {
        if (talePos.X < 0 || talePos.Z < 0)
            return null;
        return arrayTails[talePos.X, talePos.Z];
    }

    public Vector3 GetCenterOfTalesArray()
    {
        float centerX = transform.position.x + (float)sizeX * tailsDistance / 2f - tailsDistance / 2f;
        float centerZ = transform.position.z + (float)sizeZ * tailsDistance / 2f - tailsDistance / 2f;
        return new Vector3(centerX, centerZ, 0);
    }

    public void NewLevelCreate()//Level editor
    {
        saveData = new List<string[]>();
        ClearTaleArray();
        ChangeSize(4,4);
        stepList = new List<StepTailChange>();
        
        currentStep = -1;
    }

    public void CheckDangerMats()//Danger mechanic
    {
        for(int z = 0; z < sizeX; z++)
        {
            for(int x = 0; x < sizeZ; x++)
            {
                if(!arrayTails[x,z] || (int)arrayTails[x,z].taleType < 6)
                {
                    continue;
                }
                Tail dangerTail = null;
                float rotDeg = arrayTails[x, z].transform.rotation.eulerAngles.y;
                if (rotDeg < 0)
                    rotDeg += 360;

                switch (rotDeg)
                {
                    case 0:
                        dangerTail = z + 1 < sizeX ? arrayTails[x,z+1] : null;
                        break;
                    case 90:
                        dangerTail = x + 1 < sizeX ? arrayTails[x + 1, z] : null;
                        break;
                    case 180:
                        dangerTail = z - 1 < sizeX ? arrayTails[x, z-1] : null;
                        break;
                    case 270:
                        dangerTail = x - 1 < sizeX ? arrayTails[x - 1, z] : null;
                        break;
                }
                if(dangerTail)
                {
                    //dangerTail.SetDangerMat(arrayTails[x, z].taleType
                      //  == Tail.TaleType.dangerSoldier ? dangerSoldierMat : dangerTankMat);
                }
            }
        }
    }

    public void ReplaceTale(Tail newTale, int X, int Z)//From ControlTales
    {
        arrayTails[X, Z] = newTale;
    }


    private void CreateLevelStart()
    {
        tailStartArray = new TailStartInfo[sizeX, sizeZ];
        for(int x = 0; x < sizeX; x++)
            for(int z =0; z < sizeZ; z++)
            {
                int num_ = arrayTails[x, z].GetNum();
                int deg = (int)arrayTails[x, z].transform.rotation.eulerAngles.y;
                char spec_ = arrayTails[x, z].getSpecialName();
                tailStartArray[x, z] = new TailStartInfo(num_, deg, spec_);
            }
    }

    public void LoadLevel(TailStartInfo[,] levelTales)//From file
    {
        tailStartArray = levelTales;
        ClearTaleArray();
        for(int x = 0; x < levelTales.GetLength(0); x++)
        {
            for(int z = 0; z < levelTales.GetLength(1); z++)
            {
                CreateNewTail(x, z, levelTales[x, z].number, levelTales[x, z].degree, levelTales[x, z].specChar);
            }
        }
        sizeX = levelTales.GetLength(0);
        sizeZ = levelTales.GetLength(1);
        currentStep = -1;
        setCameraLook();
        //CheckDangerMats();
    }

    private void setCameraLook()
    {
        if(sizeX > 4 || sizeZ > 4)
        {
            Camera.main.fieldOfView = fovMax;
            Terrain.SetActive(false);
        }
        else
            Camera.main.fieldOfView = fovNorm;

        float centerX = transform.position.x + (float)sizeX * tailsDistance / 2f - tailsDistance / 2f;
        float centerZ = transform.position.z + (float)sizeZ * tailsDistance / 2f - tailsDistance / 2f;
        Transform cameraTR = Camera.main.transform;
        cameraTR.position = CameraControl.cameraControl.startPosition;
        //Debug.Log("centerX: " + centerX);
        //Debug.Log("centerZ: " + centerZ);
        cameraTR.position += cameraTR.right * (centerX - cameraTR.position.x);
        cameraTR.LookAt(new Vector3(centerX, 0, centerZ), Vector3.up);
        //SelectCanvas.selectCanvas.transform.eulerAngles = Vector3.zero;
    }

    public void ConvertingLoadedLevel(List<string[]> strLoaded)//From loaded file
    {
        int strCount = 0;

        if (strLoaded[strCount][0] == "mechanics")
        {
            //Mechanics.mechanics.gameObject.SetActive(true);
            //Debug.Log("Mechanics: " + strLoaded[strCount][1] + "_" + strLoaded[strCount][2] + "_" + strLoaded[strCount][3] + "_" + strLoaded[strCount][4]);
            //Mechanics.mechanics.SetMechanics(strLoaded[strCount]);
            strCount++;
        }
        else
        {
            //Mechanics.mechanics.gameObject.SetActive(false);
        }

        if (strLoaded[strCount][0] == "start")
        {
            int sizeX = System.Convert.ToInt32(strLoaded[strCount][1]);
            int sizeZ = System.Convert.ToInt32(strLoaded[strCount][2]);

            TailStartInfo[,] newTaleArray = new TailStartInfo[sizeX, sizeZ];
            for (int z = 0; z < sizeZ; z++)
            {
                string[] nextStr = strLoaded[strCount + 1];

                for (int x = 0; x < nextStr.Length; x++)
                {
                    string[] values = nextStr[x].Split('_');
                    int num_ = System.Convert.ToInt32(values[0]);

                    int deg_ = System.Convert.ToInt32(values[1]);
                    char specChar = System.Convert.ToChar(values[2]);
                    newTaleArray[x, z] = new TailStartInfo(num_, deg_, specChar);
                }

                strCount++;
            }

            LoadLevel(newTaleArray);
        }
        string[] str = strLoaded[strCount+1];
        if (strLoaded.Count > strCount && strLoaded[++strCount][0] == "step")
        {
            int stepCount = System.Convert.ToInt32(strLoaded[strCount][1]);
            strCount += 1;
            List<StepTailChange> listChange = new List<StepTailChange>();
            for (int i = 0; i < stepCount; i++)
            {
                int fromX = System.Convert.ToInt32(strLoaded[strCount + i][0]);
                int fromZ = System.Convert.ToInt32(strLoaded[strCount + i][1]);
                int toX = System.Convert.ToInt32(strLoaded[strCount + i][2]);
                int toZ = System.Convert.ToInt32(strLoaded[strCount + i][3]);
                int rotate_ = System.Convert.ToInt32(strLoaded[strCount + i][4]);


                PositionTale positionFrom;
                positionFrom.X = fromX;
                positionFrom.Z = fromZ;
                PositionTale positionTo;
                positionTo.X = toX;
                positionTo.Z = toZ;

                StepTailChange stepTail = new StepTailChange(positionFrom, positionTo, rotate_);

                listChange.Add(stepTail);
            }

            //SetStepList(listChange);
        }
    }


    public void SetStepList(List<StepTailChange> stepsListNew)
    {
        stepList.Clear();
        stepList = stepsListNew;
        currentStep = -1;
        GoToEndState();
    }

    public void GoToEndState()//For level editor & education mechanic
    {
        if(stepList.Count == 0)
        {
            return;
        }

        LoadLevel(tailStartArray);
        currentStep = -1;
        while ( currentStep++ < stepList.Count-1)
        {
            Debug.Log("curr step: " + currentStep);
            StepTailChange step = stepList[currentStep];
            NextStep(step);
        }
        
    }

    public void GoToStartState()
    {
        if (stepList.Count == 0)
        {
            return;
        }
        
        LoadLevel(tailStartArray);
        currentStep = -1;


    }

}

public struct TailStartInfo
{
    public int number;
    public int degree;
    public char specChar;
    public TailStartInfo(int num_, int deg_, char spec_)
    {
        number = num_;
        degree = deg_;
        specChar = spec_;
    }
}

public struct StepTailChange
{
    public StepTailChange(PositionTale from, PositionTale to, int rot)
    {
        taleFrom = from;
        taleTo = to;
        rotateTo = rot;
    }
    public PositionTale taleFrom;
    public PositionTale taleTo;
    public int rotateTo;
}
