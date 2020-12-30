using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LivestreamDanmuku.Douyu
{
    public static class DouyuSedes
    {
        public static string Escape(string str)
        {
            if (str == null)
                return string.Empty;
            return str.Replace("@", "@A").Replace("/", "@S");
        }

        public static string Unescape(string str)
        {
            if (str == null)
                return string.Empty;
            return str.Replace("@A", "@").Replace("@S", "/");
        }

        public static string Serialize(JToken obj)
        {
            string ret = string.Empty;
            switch(obj)
            {
                case JObject jobj:
                    { 
                        foreach (var kvp in jobj.Properties())
                        {
                            ret += $"{kvp.Name}@={Escape(Serialize(kvp.Value))}/";
                        }
                        break;
                    }
                case JArray jarr:
                    { 
                        foreach (var v in jarr)
                        {
                            ret += $"{Escape(Serialize(v))}/";
                        }
                        break;
                    } 
                default:
                    return obj.ToString(); 
            }
            return ret;
        }

        public static JToken Deserialize(string str)
        { 
            if(str.Contains("//"))
            {
                JArray jarr = new JArray();
                foreach(string s in str.Split("//").Where((s)=>!string.IsNullOrEmpty(s)))
                {
                    jarr.Add(Deserialize(s));
                }
                return jarr;
            }

            if (str.Contains("@="))
            {
                JObject obj = new JObject();
                foreach (string s in str.Split("/").Where((s) => !string.IsNullOrEmpty(s)))
                {
                    string[] kv = s.Split("@=");
                    obj[kv[0]] = Deserialize(Unescape(kv[1]));
                }
                return obj;
            }
            else if (str.Contains("@A="))
            {
                return Deserialize(Unescape(str));
            }
            else
            {
                JToken token = new JValue(str); 
                return token;
            }
        }
    }
}
