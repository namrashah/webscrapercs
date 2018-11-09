using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Z.Dapper.Plus;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using WebScraperModularized.data;
using WebScraperModularized.helpers;
using WebScraperModularized.parsers;
using WebScraperModularized.queue;
using WebScraperModularized.wrappers;

namespace WebScraperModularized
{
    class Program
    {

        private static String SEED_URL = "https://www.apartments.com/dallas-tx-";

        static void Main(string[] args)
        {
            doDapperMapping();
            String zipcode = "75252";
            IQueueService queueService = AzureServiceBusService.Instance;
            queueService.sendMessageToQueue(zipcode,"propertyqueue");
            String input = "process";
            while (!input.SequenceEqual("exit"))
            {
                Console.WriteLine("Type exit to exit processing");
                input = Console.ReadLine();
            }
        }

        private static void doDapperMapping()
        {
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
        }
    }
}