using System;
using System.Collections.Generic;
using System.Text;

namespace BlackLight
{
   public class Command
    {
        public string name = "";
         public string[] parameters;
         public Command(string name, string[] parameters)
         {
             this.name = name;
             this.parameters = parameters;
         }
         public Command(string name, string parameters)
         {
             this.name = name;
             parameters = parameters.Replace(" ", "");
             //this.parameters = parameters.Split(new char[] { ' ' });
             //this.parameters = splitToList(',', parameters);
             this.parameters = parameters.Split(new char[] { ',' });

         }
         public string buildCommand(string server, Dictionary<string, string> replacements)
         {
             string outLine = "";
             outLine += ":" + server;
             outLine += " " + name;
             string param = "";
             foreach (string parameter in parameters)
             {
                 param = parameter;
                 foreach (string key in replacements.Keys)
                 {
                     if (param.ToLower().Contains("%" + key.ToLower() + "%"))
                     {
                         param = param.Replace("%" + key.ToLower() + "%", replacements[key]).Trim();
                     }
                 }
                 outLine += " " + param;
             }
             return outLine.Trim();
         }
         public short getParameterIndex(string parameter)
         {
             for (short x = 0; x < parameters.Length; x++)
             {
                 if (parameters[x].ToUpper() == parameter.ToUpper())
                     return x;
             }
             return -1;
         }
         public bool hasParameter(string parameter)
         {
             for (short x = 0; x < parameters.Length; x++)
             {
                 if (parameters[x].ToUpper() == parameter.ToUpper())
                     return true;
             }
             return false;
         }
         public int parameterCount()
         {
             return parameters.Length;
         }
         public static List<string> splitToList(char splitChar, string strSplit)
         {
             List<string> ret = new List<string>();
             string tmp = "";
             for (short x = 0; x < strSplit.Length; x++)
             {
                 if (strSplit[x] == splitChar)
                 {
                     ret.Add(tmp);
                     tmp = "";
                 }
                 else
                     tmp += strSplit[x];
             }
             ret.Add(tmp);
             return ret;
         }
    }
    
}
