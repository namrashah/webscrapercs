using System;
using WebScraperModularized.helpers;
using System.Net.Http;
using WebScraperModularized.parsers;
using WebScraperModularized.data;
using System.Collections.Generic;
using Z.Dapper.Plus;
using WebScraperModularized.wrappers;
using System.Threading;

namespace WebScraperModularized
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        static void Main(string[] args)
        {   
           //do dapper entitiy mapping to map objects to DB tables
            DapperPlusManager.Entity<URL>().Table("url").Identity(x => x.id);
            DapperPlusManager.Entity<Property>().Table("property").Identity(x => x.id);
            DapperPlusManager.Entity<PropertyType>().Table("propertytype").Identity(x => x.id);
            DapperPlusManager.Entity<School>().Table("school").Identity(x => x.id);
            DapperPlusManager.Entity<Review>().Table("review").Identity(x => x.id);
            DapperPlusManager.Entity<NTPI>().Table("ntpi").Identity(x => x.id);
            DapperPlusManager.Entity<Expenses>().Table("expenses").Identity(x => x.id);
            DapperPlusManager.Entity<Expensetype>().Table("expensetype").Identity(x => x.id);
            DapperPlusManager.Entity<Apartments>().Table("apartments").Identity(x => x.id);
            DapperPlusManager.Entity<Amenity>().Table("amenity").Identity(x => x.id);
            DapperPlusManager.Entity<Amenitytype>().Table("amenitytype").Identity(x => x.id);

            Thread[] threadList = new Thread[100]; // 1 url per thread

            while(URLHelper.getNextURL() != null) // goes till no more urls left
            {
                for(int i = 0; i < threadList.Length; i++)
                {
                    threadList[i] = new Thread(ParseUrl); //Need Url List?
                    threadList[i].Name = $"Thread{i+1}";
                    threadList[i].Start();
                }

                threadList[threadList.Length-1].Join(); // waits until all threads in batch are done.
            }

            //ParseUrl(); // without any lock
        }

        static void ParseUrl()
        {
            URL myUrl;//URL to be parsed

            //get url from url helper and do basic null checks
            if((myUrl = URLHelper.getNextURL())!=null && myUrl.url!=null && myUrl.url.Length>0){
                Console.WriteLine("Parsing URL {0}", myUrl.url);//print the current url
                try{
                    var response = client.GetAsync(myUrl.url).Result;//make an HTTP call and get the html for this URL

                    string content = response.Content.ReadAsStringAsync().Result;//save HTML into string

                    if(myUrl.urltype == (int)URL.URLType.PROPERTY_URL){

                        //if the url is of property type, instantiate property parser
                        PropertyParser parser = new PropertyParser(content, myUrl);
                        
                        //parse the html
                        PropertyData propData = parser.parse();
                        
                        //insert into DB
                        DBHelper.insertParsedProperties(propData);

                        Console.WriteLine("Stored {0} properties", 
                            (propData!=null && propData.urlList!=null)?propData.urlList.Count:0);
                    }
                    else if(myUrl.urltype == (int)URL.URLType.APARTMENT_URL){
                        
                        //if the url is of apartment type, instantiate apartment parser
                        ApartmentParser parser = new ApartmentParser(content, myUrl);

                        //call the parse method
                        ApartmentData apartmentData = parser.parse();

                        DBHelper.insertParsedApartment(apartmentData);

                        Console.WriteLine("Stored data for property id {0}!", myUrl.property);
                    }
                    else{
                        Console.WriteLine("Unknown URL Type");
                    }
                    DBHelper.markURLDone(myUrl);//update the status of URL as done in DB
                }
                catch(Exception e){
                    ExceptionHelper.printException(e);
                }
            }
        }
    }
}
