using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FileManager
{
    private const string settingFileName = "settings", saveFileName = "save";
    private static List<string[]> strLoaded;


    private static void ReadFile()
    {
        if (!File.Exists(getSavePath()))
        {
            StreamWriter writer = new StreamWriter(new FileStream(getSavePath(), FileMode.OpenOrCreate, FileAccess.Write));
            writer.Close();
        }
    }

    public static string[] GetFiles()
    {

        List<string> strList = new List<string>();
        strList.AddRange(Directory.GetFiles(getSavePath(), "*.csv", SearchOption.TopDirectoryOnly).Select(x => System.IO.Path.GetFileNameWithoutExtension(x)));
        return strList.ToArray();
    }

    public static void LoadFile(int level_, int chapter_)
    {
        string levelPath = "Levels/" + chapter_.ToString() + "." + level_.ToString();
        try
        {
            TextAsset textAsset = Resources.Load<TextAsset>(levelPath);
            strLoaded = new List<string[]>();
            //strLoaded.Clear();
            
            string[] stringData = textAsset.text.Split('\r');
            
            for(int i = 0; i < stringData.Length; i++)
            {
                string line = stringData[i];//reader.ReadLine();
                line = line.TrimStart('\n');
                string[] values = line.Split(';');
                values = values.Where(x => x != "").ToArray();
                if (values.Length >= 2)
                    strLoaded.Add(values);
            }
        }
        catch
        {
            Debug.LogError("Не удалось открыть файл: " + levelPath);
            return;
        }
        TailsTable.talesTable.ConvertingLoadedLevel(strLoaded);
    }

    public static string[] GetSettingStrs()
    {
        Debug.Log("Try get setting");
        TextAsset textAsset = Resources.Load<TextAsset>(settingFileName);

        string[] stringData = textAsset.text.Split('\r');
        return stringData;
    }

    public static void SaveToFile(string[][] saveStrs)
    {
        StreamWriter writer = new StreamWriter(new FileStream(getSavePath() + saveFileName, FileMode.Create, FileAccess.Write));
        for(int j = 0; j < saveStrs.Length; j++)
        for(int i = 0; i < saveStrs[j].Length; i++)
            writer.WriteLine(saveStrs[j][i]);
        writer.Close();
    }

    public static string[] LoadFromFile()
    {
        StreamReader reader = new StreamReader(new FileStream(getSavePath() + saveFileName, FileMode.OpenOrCreate, FileAccess.Read));
        List<string> strTemp = new List<string>();
        while(!reader.EndOfStream)
        {
            strTemp.Add(reader.ReadLine());
        }
        reader.Close();

        return strTemp.ToArray();
    }

    public static void WriteFile(List<string[]> strList, string levelName)
    {
        string[][] output = new string[strList.Count][];
        for(int i = 0; i < output.Length; i++)
        {
            output[i] = strList[i];
        }

        StringBuilder sb = new StringBuilder();
        
        for(int index = 0; index < output.GetLength(0); index++)
        {
            sb.AppendLine(string.Join("",output[index]));
        }
        /*
        if (File.Exists(getPath() + levelName + ".csv"))
        {
            int plusInt = 1;
            string plusText = "(" + plusInt + ")";
            while (File.Exists(getPath() + levelName + plusText + ".csv"))
            {
                plusText = "(" + ++plusInt + ")";
            }
            levelName += plusText;
        }
        */
        //FileStream fs = new FileStream(getPath(), FileMode.Create, FileAccess.Write);
        StreamWriter writer = File.CreateText(getSavePath() + levelName + ".csv");
        
        writer.WriteLine(sb);
        writer.Close();
       
    }

    public static string getStreamAssetsPath()
    {
        string path_ = "";
#if UNITY_IPHONE
        path_ = Application.dataPath + "/Raw";
#endif

#if UNITY_ANDROID
        path_ = "jar:file://" + Application.dataPath + "!/assets/";
#endif

#if UNITY_EDITOR
        path_ = Application.dataPath + "/StreamingAssets/";
#endif
        return path_;
    }
    public static string getSavePath()
    {
#if UNITY_EDITOR
        return Application.dataPath + "/Resources/";
#elif UNITY_ANDROID
        return Application.persistentDataPath;
#elif UNITY_IPHONE
        return Application.persistentDataPath+"/";
#else
        return Application.dataPath +"/Levels/";
#endif
    }

}
