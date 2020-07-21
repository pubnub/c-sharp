using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNGenerateFileUploadUrlDataParse
    {
        internal static PNGenerateFileUploadUrlResult GetObject(List<object> listObject)
        {
            PNGenerateFileUploadUrlResult result = null;
            for (int listIndex = 0; listIndex < listObject.Count; listIndex++)
            {
                Dictionary<string, object> dicObj = JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[listIndex]);
                if (dicObj != null && dicObj.Count > 0)
                {
                    if (result == null)
                    {
                        result = new PNGenerateFileUploadUrlResult();
                    }
                    if (dicObj.ContainsKey("data") && dicObj["data"] != null)
                    {
                        Dictionary<string, object> generateFileUploadUrlDicData = JsonDataParseInternalUtil.ConvertToDictionaryObject(dicObj["data"]);
                        if (generateFileUploadUrlDicData != null && generateFileUploadUrlDicData.Count > 0)
                        {
                            result.FileId = generateFileUploadUrlDicData.ContainsKey("id") && generateFileUploadUrlDicData["id"] != null ? generateFileUploadUrlDicData["id"].ToString() : null;
                            result.FileName = generateFileUploadUrlDicData.ContainsKey("name") && generateFileUploadUrlDicData["name"] != null ? generateFileUploadUrlDicData["name"].ToString() : null;
                        }

                    }
                    else if (dicObj.ContainsKey("file_upload_request") && dicObj["file_upload_request"] != null)
                    {
                        Dictionary<string, object> generateFileUploadUrlDicUploadReq = JsonDataParseInternalUtil.ConvertToDictionaryObject(dicObj["file_upload_request"]);
                        if (generateFileUploadUrlDicUploadReq != null && generateFileUploadUrlDicUploadReq.Count > 0)
                        {
                            result.FileUploadRequest = new PNGenerateFileUploadUrlData()
                            {
                                Url = generateFileUploadUrlDicUploadReq.ContainsKey("url") && generateFileUploadUrlDicUploadReq["url"] != null ? generateFileUploadUrlDicUploadReq["url"].ToString() : null,
                                Method = generateFileUploadUrlDicUploadReq.ContainsKey("method") && generateFileUploadUrlDicUploadReq["method"] != null ? generateFileUploadUrlDicUploadReq["method"].ToString() : null,
                                ExpirationDate = generateFileUploadUrlDicUploadReq.ContainsKey("expiration_date") && generateFileUploadUrlDicUploadReq["expiration_date"] != null ? generateFileUploadUrlDicUploadReq["expiration_date"].ToString() : null,
                                FormFields = generateFileUploadUrlDicUploadReq.ContainsKey("form_fields") && generateFileUploadUrlDicUploadReq["form_fields"] != null ? JsonDataParseInternalUtil.ConvertToDictionaryObject(generateFileUploadUrlDicUploadReq["form_fields"]) : null
                            };
                        }
                    }
                }
            }

            return result;
        }
    }
}
