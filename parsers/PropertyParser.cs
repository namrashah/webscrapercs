/*
This class will be used to parse property listings.
Multiple threads of this parser will be created by the main method.
This will populate the data in Property table and url table.
This class will run once for each zipcode.
*/
using WebScraperModularized.data;
using System.Collections.Generic;
using HtmlAgilityPack;
using System;
using WebScraperModularized.helpers;
using WebScraperModularized.wrappers;

namespace WebScraperModularized.parsers{
    public class PropertyParser{

        private string html;//html parsed from the URL.

        private URL myUrl;

        HtmlDocument htmlDoc;
        public PropertyParser(string html, URL myUrl){//constructor
            this.html = html;
            this.myUrl = myUrl;
            htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
        }

        public PropertyData parse(){
            PropertyData propertyData= new PropertyData();
            List<PropertyType> propertyTypeList = new List<PropertyType>();
            try{
                if(html!=null && html.Length!=0){
                    HtmlNode apartmentsContainer = htmlDoc.GetElementbyId("placardContainer");
                    if(apartmentsContainer!=null){
                        HtmlNode listOfApartments = apartmentsContainer.SelectSingleNode(".//ul");
                        if(listOfApartments!=null){
                            foreach(HtmlNode row in listOfApartments.ChildNodes){
                                if(row!=null && row.Name == "li" && !getReinforcement(row)){
                                    PropertyType propertyType = getPropertyType(row);
                                    Property property = new Property();
                                    property.url = new URL();
                                    HtmlNode paginationDiv = row.SelectSingleNode(".//div[@id=\"paging\"]");
                                    if(paginationDiv!=null){
                                        continue;
                                    }
                                    else{
                                        property = getProperty(row);
                                        property.url = new URL();
                                        property.url.url = getUrl(row);
                                        property.url.urltype = (int)URL.URLType.APARTMENT_URL;
                                        property.url.status = (int)URL.URLStatus.INITIAL;
                                    }
                                    if(property.url.url!=null && property.url.url.Length!=0 && propertyTypeList!=null){
                                        int propertyTypeIndex = propertyTypeList.FindIndex(x => x.propertytype.Equals(propertyType.propertytype));//check if the proptype is in the list
                                        if(propertyTypeIndex!=-1){//if item in the list, get it and add our property to existing list
                                            propertyType = propertyTypeList[propertyTypeIndex];
                                        }

                                        //set values
                                        if(propertyType.properties == null) propertyType.properties = new List<Property>();

                                        propertyType.properties.Add(property);
                                        
                                        if(propertyTypeIndex==-1) propertyTypeList.Add(propertyType);
                                        else propertyTypeList[propertyTypeIndex] = propertyType;
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
            propertyData.urlList = propertyTypeList;
            return propertyData;
        }
		
		private string getTitle(HtmlNode row){
			var isAttr = false;
            string title = "";
            try{
                if(row!=null){
                    HtmlNode titleNode = row.SelectSingleNode(".//a[contains(@class, \"placardTitle\")]");//try selecting using placardTitle node for basic and gold placards
                    if(titleNode == null) {//if no element found, means we have placard of reinforcement type
                        isAttr = true;
                        titleNode = row.SelectSingleNode(".//div[@class=\"item\"]");
                    }
                    if(titleNode!=null) {
                        if(!isAttr){
                            if(titleNode.InnerHtml!=null && titleNode.InnerHtml.Length!=0){
                                title = titleNode.InnerHtml.Trim();
                            }
                        }
                        else{
                            title = titleNode.GetAttributeValue("title","");
                        }
                    }
                }
            }
            catch(Exception e){
                ExceptionHelper.printException(e);
            }
            return title;
		}

        private string getAddress(HtmlNode row){
            string address = "";
            try{
                if(row!=null){
                    HtmlNode addressNode = row.SelectSingleNode(".//div[@class=\"location\"]");
                    if(addressNode!=null) address = addressNode.InnerHtml;
                }
            }
            catch(Exception e){
                ExceptionHelper.printException(e);
            }
            return address;
        }

        private string getUrl(HtmlNode row){
            string url = "";
            try{
                if(row!=null){
                    HtmlNode linkNode = row.SelectSingleNode(".//a[contains(@class, \"placardTitle\")]");
                    if(linkNode==null){//means we have a placard of reinforcement type
                        linkNode = row.SelectSingleNode(".//a");
                    }
                    if(linkNode!=null){
                        url = linkNode.GetAttributeValue("href","");
                    }
                }
            }
            catch(Exception e){
                ExceptionHelper.printException(e);
            }
            return url;
        }

        private string getContactemail(HtmlNode row){
            string email = "";//we don\"t have email information on this page. To be implemented in the future
            return email;
        }

        private string getContactno(HtmlNode row){
            string contactno = "";
            try{
                if(row!=null){
                    HtmlNode phoneNode = row.SelectSingleNode(".//div[@class=\"phone\"]");
                    if(phoneNode!=null){
                        HtmlNode phoneSpan = phoneNode.SelectSingleNode(".//span");
                        if(phoneSpan!=null){
                            contactno = phoneSpan.InnerHtml;
                        }
                    }
                }
            }
            catch(Exception e){
                ExceptionHelper.printException(e);
            }
            return contactno;
        }

        private double getMaxPrice(HtmlNode row){
            double maxPrice = 0;
            try{
                if(row!=null){
                    HtmlNode rentSpan = row.SelectSingleNode(".//span[@class=\"altRentDisplay\"]");
                    if(rentSpan!=null){
                        string rentString = rentSpan.InnerHtml;
                        maxPrice = Util.splitRentString(rentString)[1];
                    }
                }
            }
            catch(Exception e){
                ExceptionHelper.printException(e);
            }
            return maxPrice;
        }

        private double getMinPrice(HtmlNode row){
            double minPrice = 0;
            try{
                if(row!=null){
                    HtmlNode rentSpan = row.SelectSingleNode(".//span[@class=\"altRentDisplay\"]");
                    if(rentSpan!=null){
                        string rentString = rentSpan.InnerHtml;
                        minPrice = Util.splitRentString(rentString)[0];
                    }
                }
            }
            catch(Exception e){
                ExceptionHelper.printException(e);
            }
            return minPrice;
        }

        private PropertyType getPropertyType(HtmlNode row){
            PropertyType propertyType = new PropertyType();
            propertyType.propertytype = "";//initialize value so that it is not null
            try{
                if(row!=null){
                    HtmlNode proptypeSpan = row.SelectSingleNode(".//span[contains(@class, \"unitLabel\")]");
                    if(proptypeSpan!=null){
                        propertyType.propertytype = proptypeSpan.InnerHtml;
                    }
                }
            }
            catch(Exception e){
                ExceptionHelper.printException(e);
            }
            return propertyType;
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

        private string getNextUrl(HtmlNode row){
            string url = "";
            try{
                HtmlNode nextNode = row.SelectSingleNode(".//a[contains(@class, \"next\")]");
                if(nextNode!=null){
                    url = nextNode.GetAttributeValue("href","");
                }
            }
            catch(Exception e){
                ExceptionHelper.printException(e);
            }
            return url;
        }

        private Property getProperty(HtmlNode row){
            Property Property = new Property();

            Property.name = getTitle(row);
            Property.address = getAddress(row);
            Property.contactemail = getContactemail(row);
            Property.contactno = getContactno(row);
            Property.maxprice = getMaxPrice(row);
            Property.minprice = getMinPrice(row);
            Property.reinforcement = getReinforcement(row);
            
            return Property;
        }
    }
}