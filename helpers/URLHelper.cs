/*
This class will act as the URL manager.
It will get k urls from database and keep them in memory.

We need to make sure that the getNextURL code is thread safe.
If two threads call the getNextURL method at the same time,
it can lead to unexpected results!
*/
using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using WebScraperModularized.data;
using System.Linq;

namespace WebScraperModularized.helpers{

    public static class URLHelper {

        private static readonly object nextURLLock = new object();//simple lock object
        private static int k = 100;//number of URLs to cache
        private static Queue<URL> myURLQueue = new Queue<URL>();//queue to be used as cache

        private static bool INITIALIZED = false;//variable to tell if this is the first load or not

        /*
        This method returns the next url in the queue to be parsed.
        Returns null if not URLs are left to be parsed.
        */
        public static URL getNextURL(){
            lock(nextURLLock){//make sure that this part of the code is thread safe.
                if(myURLQueue.Count==0){//check if queue contains any URLs
                    int loadedURLCount = loadNextURLS(!INITIALIZED);//load new URLs from DB
                    INITIALIZED = true;
                    if(loadedURLCount==0) return null;
                }                
                return myURLQueue.Dequeue();//dequeue one URL from Queue and return
            }
        }
        public static bool hasNextURL(){
            lock(nextURLLock){//make sure that this part of the code is thread safe.
                if(myURLQueue.Count==0){//check if queue contains any URLs
                    return false;
                }                
                return true;
            }
        }

        /*
        This method gets the next k URLs from DB and returns the count of URLs loaded.
        */
        private static int loadNextURLS(bool initialLoad){
            IEnumerable<URL> myUrlEnumerable = DBHelper.getURLSFromDB(k, initialLoad);//load URLs from DB

            if(myUrlEnumerable!=null && myUrlEnumerable.Count()>0){
                foreach(URL url in myUrlEnumerable){
                    url.status = (int)URL.URLStatus.RUNNING;
                    myURLQueue.Enqueue(url);//add url to queue
                }
                if(myURLQueue.Count>0) DBHelper.updateURLs(myURLQueue);//update status to running in DB
                return myURLQueue.Count();//return the count of number of URLs loaded
            }

            return 0;
        }

    }
}
