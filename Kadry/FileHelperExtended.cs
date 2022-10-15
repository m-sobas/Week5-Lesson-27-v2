using System;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Kadry
{ 
    public class FileHelperExtended<T> : FileHelper<T> where T : new()
    {
        string _filePath;

        public FileHelperExtended(string filePath) : base(Program.dataBasePath)
        {
            _filePath = filePath;
        }


        public void UpdateXML(Employee employee, int id)
        {
            XDocument xDoc = XDocument.Load(_filePath);

            // Wyrażenie Lambda wyszukujące w dokumencie XML element o podanym ID.
            var items = xDoc.Descendants("Employee").Where(x => (string)x.Element("Id") == id.ToString());

            //var items = from item in xDoc.Descendants("Employee") // \
            //            where (string)item.Element("Id") == "3"   //  --> Zamiast wyrażenia Lambda, można użyć zapytania.
            //            select item;                              // /

            // Aktualizacja danych.
            foreach (var item in items)
            {
                item.SetElementValue("FirstName", employee.FirstName);
                item.SetElementValue("LastName", employee.LastName);
                item.SetElementValue("Address", employee.Address);
                item.SetElementValue("PostCode", employee.PostCode);
                item.SetElementValue("City", employee.City);
                item.SetElementValue("EmploymentDate", employee.EmploymentDate);
                item.SetElementValue("DismissalDate", employee.DismissalDate);
                item.SetElementValue("EmployeeEarning", employee.EmployeeEarning);
                item.SetElementValue("Comments", employee.Comments);
            }

            // Zapisanie zmian w pliku XML.
            xDoc.Save(_filePath);
        }
    }
}
