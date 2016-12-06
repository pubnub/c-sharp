using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public interface IJsonPluggableLibrary
    {
        object BuildJsonObject(string jsonString);

        bool IsArrayCompatible(string jsonString);

        bool IsDictionaryCompatible(string jsonString);

        string SerializeToJsonString(object objectToSerialize);

        List<object> DeserializeToListOfObject(string jsonString);

        object DeserializeToObject(string jsonString);

        T DeserializeToObject<T>(string jsonString);

        T DeserializeToObject<T>(List<object> listObject);

        Dictionary<string, object> DeserializeToDictionaryOfObject(string jsonString);

        Dictionary<string, object> ConvertToDictionaryObject(object localContainer);

        Dictionary<string, object>[] ConvertToDictionaryObjectArray(object localContainer);

        object[] ConvertToObjectArray(object localContainer);

        void PopulateObject(string value, object target);
    }
}
