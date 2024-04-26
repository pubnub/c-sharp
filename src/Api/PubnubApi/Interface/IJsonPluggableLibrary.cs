using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace PubnubApi
{
    public interface IJsonPluggableLibrary
    {
        object BuildJsonObject(string jsonString);

        bool IsDictionaryCompatible(string jsonString, PNOperationType operationType);

        string SerializeToJsonString(object objectToSerialize);

        List<object> DeserializeToListOfObject(string jsonString);

        object DeserializeToObject(string jsonString);

        T DeserializeToObject<T>(string jsonString);

        T DeserializeToObject<T>(List<object> listObject);
        
        object DeserializeToObject(object rawObject, Type type);
        
        Dictionary<string, object> DeserializeToDictionaryOfObject(string jsonString);

        Dictionary<string, object> ConvertToDictionaryObject(object localContainer);

        object[] ConvertToObjectArray(object localContainer);

        void PopulateObject(string value, object target);

    }
}
