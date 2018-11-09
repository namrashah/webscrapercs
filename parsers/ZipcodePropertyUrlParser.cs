using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using HtmlAgilityPack;
using WebScraperModularized.data;
using WebScraperModularized.helpers;

namespace WebScraperModularized.parsers
{
    public class ZipcodePropertyUrlParser
    {
        private String html;//html parsed from the URL.

        private String url;

        public List<String> propertyUrls;

        HtmlDocument htmlDoc;
        
        public ZipcodePropertyUrlParser(string html, String url){//constructor
            this.html = html;
            this.url = url;
            htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            this.propertyUrls = new List<String>();
        }
        
         public void parse(){
            try{
                if(html!=null && html.Length!=0){
                    HtmlNode apartmentsContainer = htmlDoc.GetElementbyId("placardContainer");
                    if(apartmentsContainer!=null){
                        HtmlNode listOfApartments = apartmentsContainer.SelectSingleNode(".//ul");
                        if(listOfApartments!=null){
                            foreach(HtmlNode row in listOfApartments.ChildNodes){
                                if(row!=null && row.Name == "li" && !getReinforcement(row)){
                                    HtmlNode paginationDiv = row.SelectSingleNode(".//div[@id=\"paging\"]");
                                    if(paginationDiv!=null){
                                        
                                        Console.WriteLine(paginationDiv.InnerHtml);
                                        int max = getLargestPage(paginationDiv);
                                        for (int i = 1; i <= max; i++)
                                        {
                                            propertyUrls.Add(url+"/"+i.ToString());
                                        }
                                    }           
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e){
                ExceptionHelper.printException(e);
            }
        }
        
        private bool getReinforcement(HtmlNode row){
            bool reinforcement = false;
            try{
                if(row!=null){
                    HtmlNode articleNode = row.SelectSingleNode(".//article[contains(@class, \"reinforcement\")]");
                    if(articleNode!=null) reinforcement = true;
                }
            }
            catch(Exception e){
                ExceptionHelper.printException(e);
            }
            return reinforcement;
        }
        
        private int getLargestPage(HtmlNode paginationDiv){
            int max = 1;
            try
            {
                IEnumerable<HtmlNode> nextNode = paginationDiv.Descendants();

                foreach (var htmlNode in nextNode.Where(each => each.GetAttributeValue("data-page",1)!=1))
                {
                    if (htmlNode.GetAttributeValue("data-page", 1) > max)
                    {
                        max = htmlNode.GetAttributeValue("data-page",1);
                    }
                }
            }
            catch(Exception e){
                ExceptionHelper.printException(e);
            }
            return max;
        }


    }
    
    
}