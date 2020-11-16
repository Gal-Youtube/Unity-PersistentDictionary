using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class PersistentDictionary
{
    static Dictionary<string,object> data = null;
    static Dictionary<string,string> rawData = null;

    public static T Get<T>(string key)
    {
        Load();
        
        if(!data.ContainsKey(key) && rawData.ContainsKey(key))
        {
            data[key] = JsonUtility.FromJson<PersistentValue<T>>(rawData[key]);
        }

        if(data.ContainsKey(key))
            return ((PersistentValue<T>) data[key]).value;

        return new PersistentValue<T>().value;
    }
    public static T Get<T>(string key, T nullValue)
    {
        Load();

        if(!data.ContainsKey(key) && !rawData.ContainsKey(key)) return nullValue;

        return Get<T>(key);
    }

    public static void Set<T>(string key, T value)
    {
        Load();

        data[key] = new PersistentValue<T>(value);
    }

    public static void Save()
    {
        PersistentDictionarySaveData save = new PersistentDictionarySaveData(data,rawData);
        
        string json = JsonUtility.ToJson(save);

        File.WriteAllText(Application.persistentDataPath + "/save.json",json);
    }

    static void Load()
    {
        
        if(data != null) return;

        Application.quitting += Save;

        if(!File.Exists(Application.persistentDataPath + "/save.json"))
        {
            Debug.Log("Creating PersistentDictionary");
            data = new Dictionary<string, object>();
            return;
        }

        Debug.Log("Loading PersistentDictionary");
        string rawJson = File.ReadAllText(Application.persistentDataPath + "/save.json");
        PersistentDictionarySaveData load = JsonUtility.FromJson<PersistentDictionarySaveData>(rawJson);
        rawData = load.ToDictionary();
        data = new Dictionary<string, object>();
        //Debug.Log(rawData.Keys);
        
    }

    public static void Delete(string key)
    {
        data.Remove(key);
        rawData.Remove(key);
    }

    public static void DeleteAll()
    {
        data = null;
        File.Delete(Application.persistentDataPath + "/save.json");
        Load();
    }

}


public class PersistentDictionarySaveData
{
    public List<string> keys = new List<string>();
    public List<string> jsonValues = new List<string>();

    public PersistentDictionarySaveData(Dictionary<string, object> dictionary, Dictionary<string,string> rawDictionary)
    {
        if(rawDictionary != null)
        {
            foreach(KeyValuePair<string, string> entry in rawDictionary)
            {
                if(!dictionary.ContainsKey(entry.Key))
                {
                    keys.Add(entry.Key);

                    jsonValues.Add(entry.Value);
                }
            }
        }
        
        foreach(KeyValuePair<string, object> entry in dictionary)
        {
            if(!keys.Contains(entry.Key)){
                keys.Add(entry.Key);
            
                jsonValues.Add(JsonUtility.ToJson(entry.Value));
            }
        }
    }

    public Dictionary<string,string> ToDictionary()
    {
        Dictionary<string,string> r = new Dictionary<string, string>();
        for (int i = 0; i < keys.Count; i++)
        {
            r[keys[i]] = jsonValues[i];
        }
        return r;
    }
}

public class PersistentValue<T>
{
    public T value;

    public PersistentValue(T value)
    {
        this.value = value;
    }
    public PersistentValue()
    {

    }
}