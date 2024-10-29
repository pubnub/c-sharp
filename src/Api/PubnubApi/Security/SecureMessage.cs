using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Net;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;

namespace PubnubApi
{
    internal class SecureMessage
    {
        private PNConfiguration config;
        private IJsonPluggableLibrary jsonLib;
        private IPubnubLog pubnubLog;

        public static SecureMessage Instance(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubLog log)
        {
            SecureMessage secureMessage = new SecureMessage();
            secureMessage.config = pubnubConfig;
            secureMessage.jsonLib = jsonPluggableLibrary;
            secureMessage.pubnubLog = log;
            return secureMessage;
        }

        public List<object> HistoryDecodeDecryptLoop<T>(PNOperationType type, List<object> messageList, string[] channels, string[] channelGroups, PNCallback<T> errorCallback)
        {
            List<object> returnMessage = new List<object>();

            if (config.CryptoModule != null || config.CipherKey.Length > 0)
            {
                config.CryptoModule ??= new CryptoModule(new LegacyCryptor(config.CipherKey, config.UseRandomInitializationVector, pubnubLog), null);
                object[] myObjectArray = (from item in messageList
                                     select item as object).ToArray();
                object[] enumerable = myObjectArray[0] as object[];
                if (enumerable != null)
                {
                    List<object> receivedMsg = new List<object>();
                    foreach (object element in enumerable)
                    {
                        string decryptMessage = "";
                        try
                        {
                            Dictionary<string, object> historyEnv = jsonLib.ConvertToDictionaryObject(element);
                            if (historyEnv != null && historyEnv.ContainsKey("message"))
                            {
                                string dictionaryValue = config.CryptoModule.Decrypt(historyEnv["message"].ToString());
                                historyEnv["message"] = jsonLib.DeserializeToObject(dictionaryValue);
                                decryptMessage = jsonLib.SerializeToJsonString(historyEnv);
                            }
                            else
                            {
                                decryptMessage = config.CryptoModule.Decrypt(element.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            decryptMessage = "**DECRYPT ERROR**";

                            PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(ex);
                            PNStatus status = new StatusBuilder(config, jsonLib).CreateStatusResponse<T>(type, category,
                                null, (int)HttpStatusCode.NotFound, new PNException(ex));
                            if (channels != null && channels.Length > 0)
                            {
                                status.AffectedChannels.AddRange(channels);
                            }

                            if (channelGroups != null && channelGroups.Length > 0)
                            {
                                status.AffectedChannelGroups.AddRange(channelGroups);
                            }
                            
                            LoggingMethod.WriteToLog(pubnubLog, "Failed to decrypt message!\nMessage might be not encrypted, returning as is...", config.LogVerbosity);
                        }
                        
                        object decodedMessage = jsonLib.DeserializeToObject((decryptMessage == "**DECRYPT ERROR**") ? jsonLib.SerializeToJsonString(element) : decryptMessage);
                        receivedMsg.Add(decodedMessage);
                        
                    }
                    returnMessage.Add(receivedMsg);
                }

                for (int index = 1; index < myObjectArray.Length; index++)
                {
                    returnMessage.Add(myObjectArray[index]);
                }
                return returnMessage;
            }
            else
            {
                var myObjectArray = (from item in messageList
                                     select item as object).ToArray();
                IEnumerable enumerable = myObjectArray[0] as IEnumerable;
                if (enumerable != null)
                {
                    List<object> receivedMessage = new List<object>();
                    foreach (object element in enumerable)
                    {
                        receivedMessage.Add(element);
                    }
                    returnMessage.Add(receivedMessage);
                }
                for (int index = 1; index < myObjectArray.Length; index++)
                {
                    returnMessage.Add(myObjectArray[index]);
                }
                return returnMessage;
            }
        }

        public List<object> FetchHistoryDecodeDecryptLoop<T>(PNOperationType type, Dictionary<string, object> messageContainer, string[] channels, string[] channelGroups, PNCallback<T> errorCallback)
        {
            List<object> returnMessage = new List<object>();

            Dictionary<string, List<object>> dicMessage = new Dictionary<string, List<object>>();
            foreach (KeyValuePair<string, object> kvp in messageContainer)
            {
                List<object> currentVal = kvp.Value as List<object>;
                if (currentVal != null)
                {
                    object[] currentValArray = jsonLib.ConvertToObjectArray(currentVal);
                    List<object> decryptList = (currentValArray != null && currentValArray.Length > 0) ? new List<object>() : null;
                    if (currentValArray != null && decryptList != null)
                    {
                        foreach (object currentObj in currentValArray)
                        {
                            Dictionary<string, object> dicValue = jsonLib.ConvertToDictionaryObject(currentObj);
                            if (dicValue != null && dicValue.Count > 0 && dicValue.ContainsKey("message"))
                            {
                                Dictionary<string, object> dicDecrypt = new Dictionary<string, object>();
                                foreach (KeyValuePair<string, object> kvpValue in dicValue)
                                {
                                    if (kvpValue.Key == "message" && (config.CryptoModule != null || config.CipherKey.Length > 0))
                                    {
                                        config.CryptoModule ??= new CryptoModule(new LegacyCryptor(config.CipherKey, config.UseRandomInitializationVector, pubnubLog), null);
                                        string decryptMessage = "";
                                        try
                                        {
                                            decryptMessage = config.CryptoModule.Decrypt(kvpValue.Value.ToString());
                                        }
                                        catch (Exception ex)
                                        {
                                            #region "Exception"
                                            decryptMessage = "**DECRYPT ERROR**";

                                            PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(ex);
                                            PNStatus status = new StatusBuilder(config, jsonLib).CreateStatusResponse<T>(type, category, null, (int)HttpStatusCode.NotFound, new PNException(ex));
                                            if (channels != null && channels.Length > 0)
                                            {
                                                status.AffectedChannels.AddRange(channels);
                                            }
                                            if (channelGroups != null && channelGroups.Length > 0)
                                            {
                                                status.AffectedChannelGroups.AddRange(channelGroups);
                                            }
                                            LoggingMethod.WriteToLog(pubnubLog, "Failed to decrypt message!\nMessage might be not encrypted, returning as is...",config.LogVerbosity);
                                            #endregion
                                        }
                                        object decodedMessage = jsonLib.DeserializeToObject((decryptMessage == "**DECRYPT ERROR**") ? jsonLib.SerializeToJsonString(kvpValue.Value) : decryptMessage);
                                        dicDecrypt.Add(kvpValue.Key, decodedMessage);
                                    }
                                    else
                                    {
                                        dicDecrypt.Add(kvpValue.Key, kvpValue.Value);
                                    }
                                }
                                decryptList.Add(dicDecrypt);
                            }
                        }
                    }
                    dicMessage.Add(kvp.Key, decryptList);
                }
            }
            if (dicMessage.Count > 0)
            {
                returnMessage.Add(dicMessage);
            }
            return returnMessage;
        }
    }
}
