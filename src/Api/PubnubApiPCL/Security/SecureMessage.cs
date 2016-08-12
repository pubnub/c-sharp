using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace PubnubApi
{
    internal class SecureMessage
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLib = null;
        private static SecureMessage secureMessage;

        public static SecureMessage Instance(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary)
        {
            secureMessage = new SecureMessage();
            secureMessage.config = pubnubConfig;
            secureMessage.jsonLib = jsonPluggableLibrary;
            return secureMessage;
        }

        public List<object> DecodeDecryptLoop(List<object> message, string[] channels, string[] channelGroups, Action<PubnubClientError> errorCallback)
        {
            List<object> returnMessage = new List<object>();
            if (config.CiperKey.Length > 0)
            {
                PubnubCrypto aes = new PubnubCrypto(config.CiperKey);
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
                            decryptMessage = aes.Decrypt(element.ToString());
                        }
                        catch (Exception ex)
                        {
                            decryptMessage = "**DECRYPT ERROR**";

                            string multiChannel = string.Join(",", channels);
                            string multiChannelGroup = (channelGroups != null && channelGroups.Length > 0) ? string.Join(",", channelGroups) : "";

                            new PNCallbackService(config, jsonLib).CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
                                multiChannel, multiChannelGroup, errorCallback, ex, null, null);
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
