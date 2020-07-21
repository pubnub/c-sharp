using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class JsonDataParseInternalUtil
    {
        public static Dictionary<string, object> ConvertToDictionaryObject(object localContainer)
        {
            Dictionary<string, object> ret = null;

            try
            {
                if (localContainer != null)
                {
                    if (localContainer.GetType().ToString() == "Newtonsoft.Json.Linq.JObject")
                    {
                        ret = new Dictionary<string, object>();

                        IDictionary<string, JToken> jsonDictionary = localContainer as JObject;
                        if (jsonDictionary != null)
                        {
                            foreach (KeyValuePair<string, JToken> pair in jsonDictionary)
                            {
                                JToken token = pair.Value;
                                ret.Add(pair.Key, ConvertJTokenToObject(token));
                            }
                        }
                    }
                    else if (localContainer.GetType().ToString() == "System.Collections.Generic.Dictionary`2[System.String,System.Object]")
                    {
                        ret = new Dictionary<string, object>();
                        Dictionary<string, object> dictionary = localContainer as Dictionary<string, object>;
                        foreach (string key in dictionary.Keys)
                        {
                            ret.Add(key, dictionary[key]);
                        }
                    }
                    else if (localContainer.GetType().ToString() == "Newtonsoft.Json.Linq.JProperty")
                    {
                        ret = new Dictionary<string, object>();

                        JProperty jsonProp = localContainer as JProperty;
                        if (jsonProp != null)
                        {
                            string propName = jsonProp.Name;
                            ret.Add(propName, ConvertJTokenToObject(jsonProp.Value));
                        }
                    }
                    else if (localContainer.GetType().ToString() == "System.Collections.Generic.List`1[System.Object]")
                    {
                        List<object> localList = localContainer as List<object>;
                        if (localList != null && localList.Count > 0)
                        {
                            if (localList[0].GetType() == typeof(KeyValuePair<string, object>))
                            {
                                ret = new Dictionary<string, object>();
                                foreach (object item in localList)
                                {
                                    if (item is KeyValuePair<string, object> kvpItem)
                                    {
                                        ret.Add(kvpItem.Key, kvpItem.Value);
                                    }
                                    else
                                    {
                                        ret = null;
                                        break;
                                    }
                                }
                            }
                            else if (localList[0].GetType() == typeof(Dictionary<string, object>))
                            {
                                ret = new Dictionary<string, object>();
                                foreach (object item in localList)
                                {
                                    if (item is Dictionary<string, object> dicItem)
                                    {
                                        if (dicItem.Count > 0 && dicItem.ContainsKey("key") && dicItem.ContainsKey("value"))
                                        {
                                            ret.Add(dicItem["key"].ToString(), dicItem["value"]);
                                        }
                                    }
                                    else
                                    {
                                        ret = null;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { /* ignore */ }

            return ret;

        }

        public static object[] ConvertToObjectArray(object localContainer)
        {
            object[] ret = null;

            try
            {
                if (localContainer.GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
                {
                    JArray jarrayResult = localContainer as JArray;
                    List<object> objectContainer = jarrayResult.ToObject<List<object>>();
                    if (objectContainer != null && objectContainer.Count > 0)
                    {
                        for (int index = 0; index < objectContainer.Count; index++)
                        {
                            if (objectContainer[index].GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
                            {
                                JArray internalItem = objectContainer[index] as JArray;
                                objectContainer[index] = internalItem.Select(item => (object)item).ToArray();
                            }
                        }
                        ret = objectContainer.ToArray<object>();
                    }
                }
                else if (localContainer.GetType().ToString() == "System.Collections.Generic.List`1[System.Object]")
                {
                    List<object> listResult = localContainer as List<object>;
                    ret = listResult.ToArray<object>();
                }
            }
            catch { /* ignore */ }

            return ret;
        }

        private static object ConvertJTokenToObject(JToken token)
        {
            if (token == null)
            {
                return null;
            }

            var jsonValue = token as JValue;
            if (jsonValue != null)
            {
                return jsonValue.Value;
            }

            var jsonContainer = token as JArray;
            if (jsonContainer != null)
            {
                List<object> jsonList = new List<object>();
                foreach (JToken arrayItem in jsonContainer)
                {
                    jsonList.Add(ConvertJTokenToObject(arrayItem));
                }
                return jsonList;
            }

            IDictionary<string, JToken> jsonObject = token as JObject;
            if (jsonObject != null)
            {
                var jsonDict = new Dictionary<string, object>();
                List<JProperty> propertyList = (from childToken in token
                                                where childToken is JProperty
                                                select childToken as JProperty).ToList();
                foreach (JProperty property in propertyList)
                {
                    jsonDict.Add(property.Name, ConvertJTokenToObject(property.Value));
                }

                return jsonDict;
            }

            return null;
        }

        public static List<KeyValuePair<string, object>> ConvertToKeyValuePairList(object localContainer)
        {
            List<KeyValuePair<string, object>> ret = null;

            try
            {
                if (localContainer != null)
                {
                    if (localContainer.GetType().ToString() == "Newtonsoft.Json.Linq.JObject")
                    {
                        ret = new List<KeyValuePair<string, object>>();

                        IDictionary<string, JToken> jsonDictionary = localContainer as JObject;
                        if (jsonDictionary != null)
                        {
                            foreach (KeyValuePair<string, JToken> pair in jsonDictionary)
                            {
                                JToken token = pair.Value;
                                ret.Add(new KeyValuePair<string, object>(pair.Key,ConvertJTokenToObject(token)));
                            }
                        }
                    }
                    else if (localContainer.GetType().ToString() == "System.Collections.Generic.Dictionary`2[System.String,System.Object]")
                    {
                        ret = new List<KeyValuePair<string, object>>();
                        Dictionary<string, object> dictionary = localContainer as Dictionary<string, object>;
                        foreach (string key in dictionary.Keys)
                        {
                            ret.Add(new KeyValuePair<string, object>(key, dictionary[key]));
                        }
                    }
                    else if (localContainer.GetType().ToString() == "Newtonsoft.Json.Linq.JProperty")
                    {
                        ret = new List<KeyValuePair<string, object>>();

                        JProperty jsonProp = localContainer as JProperty;
                        if (jsonProp != null)
                        {
                            string propName = jsonProp.Name;
                            ret.Add(new KeyValuePair<string, object>(propName, ConvertJTokenToObject(jsonProp.Value)));
                        }
                    }
                    else if (localContainer.GetType().ToString() == "System.Collections.Generic.List`1[System.Object]")
                    {
                        List<object> localList = localContainer as List<object>;
                        if (localList != null)
                        {
                            if (localList.Count > 0 && (localList[0] is Dictionary<string, object>))
                            {
                                ret = new List<KeyValuePair<string, object>>();
                                foreach (object item in localList)
                                {
                                    if (item is Dictionary<string, object> kvpItem)
                                    {
                                        if (kvpItem.Count > 0 && kvpItem.ContainsKey("key") && kvpItem.ContainsKey("value"))
                                        {
                                            ret.Add(new KeyValuePair<string, object>(kvpItem["key"].ToString(), kvpItem["value"].ToString()));
                                        }
                                    }
                                    else
                                    {
                                        ret = null;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { /* ignore */ }

            return ret;

        }
    }
}