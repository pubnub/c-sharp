using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNListFilesJsonDataParse
    {
        internal static PNListFilesResult GetObject(List<object> listObject)
        {
            PNListFilesResult result = null;
            for (int listIndex = 0; listIndex < listObject.Count; listIndex++)
            {
                Dictionary<string, object> dicObj = JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[listIndex]);
                if (dicObj != null && dicObj.Count > 0)
                {
                    if (result == null)
                    {
                        result = new PNListFilesResult();
                    }
                    if (dicObj.ContainsKey("data") && dicObj["data"] != null)
                    {
                        object[] fileDataArray = JsonDataParseInternalUtil.ConvertToObjectArray(dicObj["data"]);
                        if (fileDataArray != null && fileDataArray.Length > 0)
                        {
                            result.FilesList = new List<PNFileResult>();
                            for (int index = 0; index < fileDataArray.Length; index++)
                            {
                                Dictionary<string, object> getFileDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(fileDataArray[index]);
                                if (getFileDataDic != null && getFileDataDic.Count > 0)
                                {
                                    long fileSize;
                                    var fileItem = new PNFileResult
                                    {
                                        Name = (getFileDataDic.ContainsKey("name") && getFileDataDic["name"] != null) ? getFileDataDic["name"].ToString() : null,
                                        Id = (getFileDataDic.ContainsKey("id") && getFileDataDic["id"] != null) ? getFileDataDic["id"].ToString() : null,
                                        Created = (getFileDataDic.ContainsKey("created") && getFileDataDic["created"] != null) ? getFileDataDic["created"].ToString() : null
                                    };
                                    if (getFileDataDic.ContainsKey("size") && getFileDataDic["size"] != null && Int64.TryParse(getFileDataDic["size"].ToString(), out fileSize))
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
