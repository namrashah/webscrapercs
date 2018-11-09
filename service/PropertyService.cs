using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using WebScraperModularized.data;
using WebScraperModularized.helpers;
using WebScraperModularized.parsers;
using WebScraperModularized.wrappers;

namespace WebScraperModularized.queue
{
    public class PropertyService
    {
        private static readonly HttpClient client = new HttpClient();

        public List<String> getPropertyUrlsFromPropertyListPage(String url)
        {
            var response = client.GetAsync(url).Result; //make an HTTP call and get the html for this URL

            URL myUrl = new URL();
            myUrl.url = url;

            string content = response.Content.ReadAsStringAsync().Result; //save HTML into string
            PropertyParser parser = new PropertyParser(content, myUrl);

            //parse the html
            PropertyData propData = parser.parse();

            //insert into DB
            DBHelper.insertParsedProperties(propData);

            Console.WriteLine("Stored {0} properties",
                (propData != null && propData.urlList != null) ? propData.urlList.Count : 0);

            List<String> listOfPropertyUrl = new List<String>();
            foreach (var each in propData.urlList)
            {
                each.properties.ForEach(eachProperty => listOfPropertyUrl.Add(eachProperty.url.url));
            }

            return listOfPropertyUrl;
        }
    }
}