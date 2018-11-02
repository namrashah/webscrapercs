/*
This class will be used to parse apartment details.
Multiple threads of this parser will be created by the main method.
This class will run once for each zipcode.
*/
using WebScraperModularized.data;
using System.Collections.Generic;
using HtmlAgilityPack;
using System;
using WebScraperModularized.helpers;
using WebScraperModularized.wrappers;
using System.Text.RegularExpressions;

namespace WebScraperModularized.parsers{
    public class ApartmentParser{
        
        private string html;//html parsed from the URL.

        private URL myUrl;

        HtmlDocument htmlDoc;

        public ApartmentParser(string html, URL myUrl){//constructor
            this.html = html;
            this.myUrl = myUrl;
            htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
        }

        public ApartmentData parse(){
            ApartmentData apartmentData = new ApartmentData();
            apartmentData.apartmentsList = getApartments(getHtmlNodeApartments());
            apartmentData.expensesTypeList = getExpenses(getHtmlNodeExpenses());
            apartmentData.amenityTypesList = getAmenities(getHtmlNodeAmeneties());
        
            apartmentData.description = getDescription(getHtmlNodeDescription());
            apartmentData.soundScore = getSoundScore(getHtmlNodeSoundScore());
            
            apartmentData.reviewsList = getReviews(getHtmlNodeReviews());
            apartmentData.schoolsList = getSchools(getHtmlNodeSchools());
            apartmentData.NTPIList = getNTPI(getHtmlNodeNTPI());
            return apartmentData;
        }


        /*
        Helper methods for parsing the required data start
        */
        private List<Apartments> getApartments(HtmlNode row){
            List<Apartments> apartments = new List<Apartments>();
            try{
                HtmlNodeCollection trCollection = row.SelectNodes(".//tr[contains(@class, \"rentalGridRow\")]");
                if(trCollection!=null){
                    //loop across all trs
                    foreach(HtmlNode tr in trCollection){
                        if(tr.Name.Equals("tr")){
                            Apartments apartment = new Apartments();

                            //get num beds
                            apartment.beds = Util.parseDouble(tr.GetAttributeValue("data-beds","0"), 0);
                            
                            //get num baths
                            apartment.baths = Util.parseDouble(tr.GetAttributeValue("data-baths","0"), 0);
                            
                            //get min and max price
                            HtmlNode rentTd = tr.SelectSingleNode(".//td[contains(@class, \"rent\")]");
                            if(rentTd!=null){
                                string rentString = rentTd.InnerHtml;
                                double[] rents = Util.splitRentString(rentString);
                                apartment.minprice = rents[0];
                                apartment.maxprice = rents[1];
                            }
                            else{
                                apartment.minprice = 0;
                                apartment.maxprice = 0;
                            }

                            //get area
                            HtmlNode areaTd = tr.SelectSingleNode(".//td[contains(@class, \"sqft\")]");
                            if(areaTd!=null){
                                string areaString = areaTd.InnerHtml;
                                areaString = areaString.Trim();
                                areaString = Regex.Replace(areaString, "[^0-9.]", "");
                                apartment.area = Util.parseDouble(areaString, 0);
                            }
                            else{
                                apartment.area = 0;
                            }

                            //set property id
                            apartment.property = myUrl.property;

                            //get Availability
                            HtmlNode availabilityTd = tr.SelectSingleNode(".//td[contains(@class, \"available\")]");
                            if(availabilityTd!=null){
                                apartment.availability = availabilityTd.InnerHtml.Trim();
                            }

                            apartments.Add(apartment);
                        }
                    }
                }
            }
            catch(Exception e){
                ExceptionHelper.printException(e);
            }
            return apartments;
        }

        private List<Amenitytype> getAmenities(HtmlNode row){
            List<Amenitytype> amenitytypes = new List<Amenitytype>();
            if(row!=null){
                foreach(HtmlNode specListDiv in row.SelectNodes(".//div[contains(@class, \"specList\")]")){
                    Amenitytype amenitytype = new Amenitytype();
                    List<Amenity> amenities = new List<Amenity>();

                    //get title
                    HtmlNode titleNode = specListDiv.SelectSingleNode(".//h3");
                    if(titleNode!=null) amenitytype.title = titleNode.InnerHtml;
                    else amenitytype.title = "";

                    HtmlNodeCollection specList = specListDiv.SelectNodes(".//li");
                    if(specList!=null){
                        foreach(HtmlNode liNode in specList){
                            Amenity amenity = new Amenity();

                            //get title
                            if(liNode.ChildNodes.Count>1) amenity.title = liNode.ChildNodes[1].InnerHtml.Trim();
                            else amenity.title = liNode.InnerText.Trim();

                            //set property
                            amenity.property = myUrl.property;

                            amenities.Add(amenity);
                        }
                    }

                    amenitytype.amenityList = amenities;

                    amenitytypes.Add(amenitytype);
                }
            }
            return amenitytypes;

        }

        private List<Expensetype> getExpenses(HtmlNode row){
            List<Expensetype> expensetypes = new List<Expensetype>();
            if(row!=null){
                foreach(HtmlNode divRow in row.ChildNodes){
                    if(divRow.Name.Equals("div")){

                        //initialize new expensetype
                        Expensetype expensetype = new Expensetype();
                        List<Expenses> expenseList = new List<Expenses>();
                        //get the title
                        HtmlNode h3Node = divRow.SelectSingleNode(".//h3");
                        if(h3Node!=null){
                            expensetype.title = h3Node.InnerHtml;
                        }

                        foreach(HtmlNode descriptionWrapperNode in divRow.ChildNodes){
                            if(descriptionWrapperNode.Name.Equals("div")){
                                //initialize new expense
                                Expenses expense = new Expenses();
                                
                                //get title
                                HtmlNode titleNode = descriptionWrapperNode.SelectSingleNode(".//span[1]");
                                if(titleNode!=null) expense.title = titleNode.InnerHtml;

                                //get cost
                                HtmlNode costNode = descriptionWrapperNode.SelectSingleNode(".//span[2]");
                                if(costNode!=null) {
                                    double[] costs = Util.splitRentString(costNode.InnerHtml);
                                    expense.mincost = costs[0];
                                    expense.maxcost = costs[1];
                                }

                                //set property id
                                expense.property = myUrl.property;

                                //add to the list
                                expenseList.Add(expense);
                            }
                        }

                        expensetype.expensesList = expenseList;

                        //add to the list
                        expensetypes.Add(expensetype);
                    }
                }
            }
            return expensetypes;
        }

        
        private string getDescription(HtmlNode row)
        {
            string description = "";
            HtmlNode pNode = row.SelectSingleNode(".//p");
            if(pNode!=null) description = pNode.InnerHtml;
            return description;
        }

        
        private SoundScore getSoundScore(HtmlNode row){
            SoundScore soundScore = new SoundScore();

            return soundScore;
        }

        private List<Review> getReviews(HtmlNode row){
            List<Review> reviews = new List<Review>();
            if(row!=null){
                HtmlNodeCollection reviewsCollection = row.SelectNodes(".//div[contains(@class,\"reviewContainer\")]");
                foreach(HtmlNode reviewNode in reviewsCollection){
                    Review review = new Review();
                    HtmlNode titleH4 = reviewNode.SelectSingleNode(".//h4");
                    if(titleH4!=null){
                        review.title = titleH4.InnerHtml;
                    }

                    HtmlNode descriptionP = reviewNode.SelectSingleNode(".//div[contains(@class, \"reviewTextContainer\")]/p/span");
                    if(descriptionP!=null){
                        review.content = descriptionP.InnerHtml.Trim();
                    }

                    /*Helpful count needs js to be enabled. To-do later. *//*HtmlNode helpfulNode = reviewNode.SelectSingleNode(".//div[contains(@class, \"helpful\")]");
                    if(helpfulNode!=null){
                        review.helpful = Util.parseInt(helpfulNode.InnerHtml.Substring(0,1), 0);
                    }*/

                    HtmlNode ratingSpan = reviewNode.SelectSingleNode(".//div[contains(@class, \"reviewRatingDaysSincePostedContainer\")]/span");
                    if(ratingSpan!=null){
                        review.rating = ratingSpan.GetAttributeValue("content", 0);
                    }

                    review.property = myUrl.property;
                    reviews.Add(review);
                }
            }
            return reviews;
        }

        private List<School> getSchools(HtmlNode row){
            List<School> schools = new List<School>();
            if(row!=null){
                HtmlNodeCollection schoolCollection = row.SelectNodes(".//div[contains(@class, \"schoolCard\")]");
                if(schoolCollection!=null){
                    foreach(HtmlNode schoolNode in schoolCollection){
                        School school = new School();
                        school.property = myUrl.property;

                        IEnumerable<HtmlNode> ancestorCollection = schoolNode.Ancestors("div");
                        foreach(HtmlNode ancestor in ancestorCollection){
                            string ancestorClass = ancestor.GetAttributeValue("class", "");
                            if(ancestorClass.Contains("Public")) {
                                school.schooltype = (int)School.SchoolType.PUBLIC;
                                break;
                            }
                            else if(ancestorClass.Contains("Private")){
                                school.schooltype = (int)School.SchoolType.PRIVATE;
                                break;
                            }
                        }

                        HtmlNode typeP = schoolNode.SelectSingleNode(".//p[contains(@class, \"schoolType\")]");
                        if(typeP!=null) school.type = typeP.InnerHtml.Trim();

                        HtmlNode titleA = schoolNode.SelectSingleNode(".//p[contains(@class, \"schoolName\")]/a");
                        if(titleA!=null) school.name = titleA.InnerHtml.Trim();

                        HtmlNode gradeP = schoolNode.SelectSingleNode(".//p[contains(@class, \"grades\")]");
                        if(gradeP!=null) school.grades = gradeP.InnerHtml.Trim();
                        
                        HtmlNode numStudentsP = schoolNode.SelectSingleNode(".//p[contains(@class, \"numberOfStudents\")]");
                        if(numStudentsP!=null) {
                            string numStudents = numStudentsP.InnerHtml.Trim().Split(" ")[0];
                            numStudents = Regex.Replace(numStudents, "[^0-9]", "");
                            school.numstudents = Util.parseInt(numStudents, 0);
                        }

                        HtmlNode contactNumP = schoolNode.SelectSingleNode(".//p[contains(@class, \"schoolPhone\")]");
                        if(contactNumP!=null) school.contactnum = contactNumP.InnerHtml.Trim();

                        HtmlNode schoolRatingI = schoolNode.SelectSingleNode(".//div[contains(@class, \"schoolRating\")]/i");
                        if(schoolRatingI!=null){
                            string ratingclass = schoolRatingI.GetAttributeValue("class", "");
                            if(!"".Equals(ratingclass)){
                                ratingclass = Regex.Replace(ratingclass, "[^0-9]", "");
                                school.rating = Util.parseInt(ratingclass, 0);
                            }
                        }

                        schools.Add(school);
                    }
                }
            }
            return schools;
        }

        private List<NTPI> getNTPI(HtmlNode row){
            List<NTPI> nTPIs = new List<NTPI>();
            if(row!=null){
                HtmlNodeCollection transportNodes = row.SelectNodes(".//div[contains(@class, \"transportationDetail\")]");
                if(transportNodes!=null){
                    foreach(HtmlNode transportNode in transportNodes){
                        string transportCategory = "";

                        HtmlNode categoryNode = transportNode.SelectSingleNode(".//thead/tr/th[1]");
                        if(categoryNode!=null) transportCategory = categoryNode.ChildNodes[1].InnerHtml.Trim();
                        foreach(HtmlNode trNode in transportNode.SelectNodes(".//tbody/tr")){
                            NTPI ntpi = new NTPI();
                            ntpi.property = myUrl.property;
                            
                            ntpi.category = transportCategory;
                            
                            HtmlNode tdDrive = trNode.SelectSingleNode(".//td[2]");
                            if(tdDrive!=null) ntpi.drivetime = Util.parseDouble(tdDrive.InnerHtml.Trim().Split(" ")[0], 0);

                            HtmlNode tdDistance = trNode.SelectSingleNode(".//td[3]");
                            if(tdDistance!=null) ntpi.distance = Util.parseDouble(tdDistance.InnerHtml.Trim().Split(" ")[0], 0);
                            
                            HtmlNode transportationNameNode = trNode.SelectSingleNode(".//div[contains(@class, \"transportationName\")]/a");
                            if(transportationNameNode!=null){
                                ntpi.name = transportationNameNode.InnerHtml.Trim();
                            }

                            nTPIs.Add(ntpi);
                        }
                    }
                }
            }
            return nTPIs;
        }
        /*
        Helper methods for parsing the required data end
        */

        /*
        Helper methods for getting HtmlNodes from htmlDoc start
        */
        private HtmlNode getHtmlNodeApartments(){
            HtmlNode htmlNode = null;
            if(htmlDoc!=null && htmlDoc.DocumentNode!=null){
                htmlNode = htmlDoc.DocumentNode
                            .SelectSingleNode(".//table[contains(@class, \"availabilityTable\")]/tbody");
            }
            return htmlNode;
        }

        private HtmlNode getHtmlNodeAmeneties(){
            HtmlNode htmlNode = null;
            if(htmlDoc!=null && htmlDoc.DocumentNode!=null){
                htmlNode = htmlDoc.DocumentNode.SelectSingleNode(".//section[contains(@class, \"specGroup\")]");
            }
            return htmlNode;
        }

        private HtmlNode getHtmlNodeExpenses(){
            HtmlNode htmlNode = null;
            if(htmlDoc!=null && htmlDoc.DocumentNode!=null){
                htmlNode = htmlDoc.GetElementbyId("feesWrapper");
            }
            return htmlNode;
        }

        private HtmlNode getHtmlNodeDescription(){
            HtmlNode htmlNode = null;
            if(htmlDoc!=null && htmlDoc.DocumentNode!=null){
                htmlNode = htmlDoc.GetElementbyId("descriptionSection");
            }
            return htmlNode;
        }

        private HtmlNode getHtmlNodeReviews(){
            HtmlNode htmlNode = null;
            if(htmlDoc!=null && htmlDoc.DocumentNode!=null){
                htmlNode = htmlDoc.DocumentNode
                            .SelectSingleNode(".//div[contains(@class, \"reviewsContainer\")]");
            }
            return htmlNode;
        }

        private HtmlNode getHtmlNodeSchools(){
            HtmlNode htmlNode = null;
            if(htmlDoc!=null && htmlDoc.DocumentNode!=null){
                htmlNode = htmlDoc.DocumentNode
                            .SelectSingleNode(".//div[contains(@class, \"schoolsModule\")]");
            }
            return htmlNode;
        }

        private HtmlNode getHtmlNodeNTPI(){
            HtmlNode htmlNode = null;
            if(htmlDoc!=null && htmlDoc.DocumentNode!=null){
                htmlNode = htmlDoc.DocumentNode
                            .SelectSingleNode(".//section[contains(@class, \"nearbyAmenitiesSection\")]");
            }
            return htmlNode;
        }

        private HtmlNode getHtmlNodeSoundScore(){
            HtmlNode htmlNode = null;
            if(htmlDoc!=null && htmlDoc.DocumentNode!=null){
                htmlNode = htmlDoc.DocumentNode
                            .SelectSingleNode(".//div[contains(@class, \"soundScoreWrapper\")]");
            }
            return htmlNode;
        }

        /*
        Helper methods for getting HtmlNodes from htmlDoc end
        */

    }
}