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
                formErrorMessage frm = new formErrorMessage("Операция: получение источников",
                    new Tuple<string, string>("Невозможно загрузить файл " + fileName, ex.Message));
                frm.ShowDialog();
                return null;
            }
            List<Source> result = new List<Source>();
            try
            {
                foreach (XElement element in xml.Descendants("source"))
                {
                    result.Add(new Source(element.Attribute("id").Value,
                        element.Descendants("server").First().Value,
                        element.Descendants("database").First().Value,
                        element.Descendants("user").First().Value,
                        element.Descendants("password").First().Value,
                        element.Descendants("component").First().Value));
                }
            }
            catch (Exception ex)
            {
                formErrorMessage frm = new formErrorMessage("Операция: получение источников",
                new Tuple<string, string>("Невозможно прочитать файл " + fileName,
                    ex.Message + Environment.NewLine + "Elements added by this point: " + result.Count));
                frm.ShowDialog();
                return null;
            }
            return result;
        }

        public static List<Balance> GetBalances(string fileName)
        {
            List<Source> sources = GetSources(fileName);
            XDocument xml;
            try
            {
                xml = XDocument.Load(fileName);
            }
            catch (Exception ex)
            {
                formErrorMessage frm = new formErrorMessage("Операция: получение описаний балансов",
                    new Tuple<string, string>("Невозможно загрузить файл " + fileName, ex.Message));
                frm.ShowDialog();
                return null;
            }
            List<Balance> result = new List<Balance>();
            try
            {
                foreach (XElement element in xml.Descendants("balance"))
                {
                    List<BalanceComponent> components = new List<BalanceComponent>();
                    foreach (XElement component in element.Descendants("component"))
                    {
                        components.Add(new BalanceComponent(component.Attribute("sign").Value,
                                                            (BalanceSides)Enum.Parse(typeof(BalanceSides), component.Attribute("side").Value, true),
                                                            sources.First(s => s.Id == component.Attribute("source").Value),
                                                            component.Attribute("channel").Value,
                                                            component.Attribute("method").Value == "показания" ?
                                                                    CalculateMethods.integral :
                                                                    CalculateMethods.interval));
                    }
                    result.Add(new Balance(element.Attribute("name").Value,
                        components));                        
                }
            }
            catch (Exception ex)
            {
                formErrorMessage frm = new formErrorMessage("Операция: получение описаний балансов",
                new Tuple<string, string>("Невозможно прочитать файл " + fileName,
                    ex.Message + Environment.NewLine + "Elements added by this point: " + result.Count));
                frm.ShowDialog();
                return null;
            }
            return result;
        }
    }
}
