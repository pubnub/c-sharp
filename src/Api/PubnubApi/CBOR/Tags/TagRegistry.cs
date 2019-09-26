using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.CBOR.Tags
{
    public class TagRegistry
    {
        private static Dictionary<ulong, Type> tagMap { get; set; } = new Dictionary<ulong, Type>();

        private static bool isInit { get; set; }

        protected TagRegistry()
        {

        }

        public static void RegisterTagTypes()
        {
            if (!isInit)
            {
                List<Type> targetTypeList = new List<Type>();
                targetTypeList.Add(typeof(Base64Tag));
                targetTypeList.Add(typeof(BigIntegerTag));
                targetTypeList.Add(typeof(CBORItemTag));
                targetTypeList.Add(typeof(UriTag));

                foreach (var type in targetTypeList)
                {
                    ulong[] tagNum = null;
                    try
                    {
                        if (type == typeof(Base64Tag))
                        {
                            tagNum = Base64Tag.TAG_NUM;
                        }
                        else if (type == typeof(BigIntegerTag))
                        {
                            tagNum = BigIntegerTag.TAG_NUM;
                        }
                        else if (type == typeof(CBORItemTag))
                        {
                            tagNum = CBORItemTag.TAG_NUM;
                        }
                        else if (type == typeof(UriTag))
                        {
                            tagNum = UriTag.TAG_NUM;
                        }
                        if (tagNum != null && tagNum.Length > 0)
                        {
                            foreach (ulong l in tagNum)
                            {
                                tagMap.Add(l, type);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        /* Ignore until we need it */
                    }
                }
            }
            isInit = true;
        }

        public static ItemTag getTagInstance(ulong tagId)
        {
            if (tagMap.ContainsKey(tagId))
            {
                return (ItemTag)Activator.CreateInstance(tagMap[tagId], tagId);
            }
            else
            {
                return new UnknownTag(tagId);
            }

        }

        internal static void registerTag(ulong p, Type type)
        {
            if (tagMap.ContainsKey(p) == false)
            {
                tagMap.Add(p, type);
            }
        }
    }
}
