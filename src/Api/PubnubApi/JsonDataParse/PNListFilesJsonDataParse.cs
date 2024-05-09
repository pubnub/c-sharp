using System;
using System.Collections.Generic;

namespace PubnubApi
{
    internal static class PNListFilesJsonDataParse
    {
        internal static PNListFilesResult GetObject(IJsonPluggableLibrary jsonPlug, List<object> listObject)
        {
            PNListFilesResult result = null;
            for (int listIndex = 0; listIndex < listObject.Count; listIndex++)
            {
                Dictionary<string, object> dicObj = jsonPlug.ConvertToDictionaryObject(listObject[listIndex]);
                if (dicObj != null && dicObj.Count > 0)
                {
                    if (result == null)
                    {
                        result = new PNListFilesResult();
                    }
                    if (dicObj.ContainsKey("data") && dicObj["data"] != null)
                    {
                        object[] fileDataArray = jsonPlug.ConvertToObjectArray(dicObj["data"]);
                        if (fileDataArray != null && fileDataArray.Length > 0)
                        {
                            result.FilesList = new List<PNFileResult>();
                            for (int index = 0; index < fileDataArray.Length; index++)
                            {
                                Dictionary<string, object> getFileDataDic = jsonPlug.ConvertToDictionaryObject(fileDataArray[index]);
                                if (getFileDataDic != null && getFileDataDic.Count > 0)
                                {
                                    int fileSize;
                                    var fileItem = new PNFileResult
                                    {
                                        Name = (getFileDataDic.ContainsKey("name") && getFileDataDic["name"] != null) ? getFileDataDic["name"].ToString() : null,
                                        Id = (getFileDataDic.ContainsKey("id") && getFileDataDic["id"] != null) ? getFileDataDic["id"].ToString() : null,
                                        Created = (getFileDataDic.ContainsKey("created") && getFileDataDic["created"] != null) ? getFileDataDic["created"].ToString() : null
                                    };
                                    if (getFileDataDic.ContainsKey("size") && getFileDataDic["size"] != null && Int32.TryParse(getFileDataDic["size"].ToString(), out fileSize))
                                    {
                                        fileItem.Size = fileSize;
                                    }
                                    result.FilesList.Add(fileItem);
                                }
                            }
                        }
                    }

                    int fileCount;
                    if (dicObj.ContainsKey("count") && dicObj["count"] != null && Int32.TryParse(dicObj["count"].ToString(), out fileCount))
                    {
                        result.Count = fileCount;
                    }
                    if (dicObj.ContainsKey("next") && dicObj["next"] != null)
                    {
                        result.Next = dicObj["next"].ToString();
                    }
                }
            }
            return result;
        }
    }
}
