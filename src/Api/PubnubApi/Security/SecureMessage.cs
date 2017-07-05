using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Net;

namespace PubnubApi
{
    internal class SecureMessage
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLib = null;
        private IPubnubLog pubnubLog = null;
        private static SecureMessage secureMessage;

        public static SecureMessage Instance(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubLog log)
        {
            secureMessage = new SecureMessage();
            secureMessage.config = pubnubConfig;
            secureMessage.jsonLib = jsonPluggableLibrary;
            secureMessage.pubnubLog = log;
            return secureMessage;
        }

        public List<object> DecodeDecryptLoop<T>(List<object> message, string[] channels, string[] channelGroups, PNCallback<T> errorCallback)
        {
            List<object> returnMessage = new List<object>();
            if (config.CipherKey.Length > 0)
            {
                PubnubCrypto aes = new PubnubCrypto(config.CipherKey, config, pubnubLog);
                var myObjectArray = (from item in message
                                     select item as object).ToArray();
                IEnumerable enumerable = myObjectArray[0] as IEnumerable;
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
                                string dictionaryValue = aes.Decrypt(historyEnv["message"].ToString());
                                historyEnv["message"] = jsonLib.DeserializeToObject(dictionaryValue);
                                decryptMessage = jsonLib.SerializeToJsonString(historyEnv);
                            }
                            else
                            {
                                decryptMessage = aes.Decrypt(element.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            decryptMessage = "**DECRYPT ERROR**";

                            PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(ex);
                            PNStatus status = new StatusBuilder(config, jsonLib).CreateStatusResponse<T>(PNOperationType.PNHistoryOperation, category, null, (int)HttpStatusCode.NotFound, ex);
                            status.AffectedChannels.AddRange(channels);
                            status.AffectedChannelGroups.AddRange(channelGroups);

                            errorCallback.OnResponse(default(T), status);
                        }
                        object decodeMessage = (decryptMessage == "**DECRYPT ERROR**") ? decryptMessage : jsonLib.DeserializeToObject(decryptMessage);
                        receivedMsg.Add(decodeMessage);
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
                var myObjectArray = (from item in message
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

    }
}
