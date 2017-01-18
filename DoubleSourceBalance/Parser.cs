using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DoubleSourceBalance
{
    static class Parser
    {
        public static List<Source> GetSources(string fileName)
        {
            XDocument xml;
            try
            {
                xml = XDocument.Load(fileName);
            }
            catch (Exception ex)
            {
                formErrorMessage frm = new formErrorMessage("Ошибка загрузки xml",
                    new Tuple<string, string>("Невозможно прочитать файл " + fileName, ex.Message));
                frm.ShowDialog();
                return null;
            }
            List<Source> result = new List<Source>();
            foreach (XElement element in xml.Descendants("Source"))
            {
                result.Add(new Source(element.Attribute("id").Value,
                    element.Descendants("server").First().Value,
                    element.Descendants("database").First().Value,
                    element.Descendants("user").First().Value,
                    element.Descendants("password").First().Value));
            }
            return result;
            }
        }
    }
}
