using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using WebScraperModularized.parsers;

namespace WebScraperModularized.queue
{
    public class ZipcodeService
    {
        
        private static readonly HttpClient client = new HttpClient();
        
        private static String SEED_URL = "https://www.apartments.com/dallas-tx-";
        
        public List<String> ProcessZipcodeMessagesAsync(String zipcode)
        {
            String baseurl = SEED_URL + zipcode;
            var response = client.GetAsync(baseurl).Result; //make an HTTP call and get the html for this URL
            string content = response.Content.ReadAsStringAsync().Result; //save HTML into string

            //parse the baseurl for that postcode to get the total number of pages for that url
            ZipcodePropertyUrlParser zipcodePropertyUrlParser = new ZipcodePropertyUrlParser(content, baseurl);
            zipcodePropertyUrlParser.parse();

            //send the total urls to the propertyQueue so that it can be processed by the Property Parser
            return zipcodePropertyUrlParser.propertyUrls;
        }
    }
}