/*
This class will help us in reading/writing data from/to the Database.
*/

using System.Collections.Generic;
using WebScraperModularized.data;
using Dapper;
using Z.Dapper.Plus;
using System.Data;
using System;
using WebScraperModularized.wrappers;

namespace WebScraperModularized.helpers{
    public class DBHelper
    { 
        public static String dbConfig = new MyConfigurationHelper().getDBConnectionConfig();
        /*
        Method to return n URLs from DB.
        */
        public static IEnumerable<URL> getURLSFromDB(int n, bool initialLoad){
            IEnumerable<URL> myUrlEnumerable = null;

            

            using(IDbConnection db = DBConnectionHelper.getConnection(dbConfig)){//get connection
                if(db!=null){
                    if(!initialLoad){
                        //if not initial load, we need to get new urls in status INITIAL
                        myUrlEnumerable = 
                                    db.Query<URL>("Select Id, Url, Urltype, Property from URL where status = @status limit @k",
                                    new {status = URL.URLStatus.INITIAL, k = n});
                    }
                    else {
                        //if initial load, we need to get URLs in RUNNING status as well as they were not parseds last time
                        myUrlEnumerable = 
                                db.Query<URL>("Select Id, Url, Urltype, Property from URL where status = ANY(@status) limit @k",
                                new {status = new []{(int)URL.URLStatus.INITIAL, (int)URL.URLStatus.RUNNING}, k = n});
                    }
                }
            }
            return myUrlEnumerable;
        }

        /*
        Method to insert parsed properties into DB
        */
        public static void insertParsedProperties(PropertyData propData){
            
            if(propData==null) return;
            List<PropertyType> propertyTypeList = propData.urlList;
            if(propertyTypeList!=null && propertyTypeList.Count>0){
                String dbConfig = new MyConfigurationHelper().getDBConnectionConfig();
                using(IDbConnection db = DBConnectionHelper.getConnection(dbConfig)){//get connection
                    db.BulkMerge(propertyTypeList)//insert the list of property types
                        .ThenForEach(x => x.properties
                                            .ForEach(y => y.propertytype = x.id))//set property type id for properties
                        .ThenBulkMerge(x => x.properties)//insert properties
                        .ThenForEach(x => x.url.property = x.id)//set property id for urls
                        .ThenBulkMerge(x => x.url);//insert urls
                }
            }
        }

        //insert schools in db

        public static void insertParsedApartment(ApartmentData apartmentData)
        {
                insertParsedSchools(apartmentData.schoolsList);
                insertParsedReviews(apartmentData.reviewsList);
                insertParsedNTPI(apartmentData.NTPIList);
                insertParsedExpenseType(apartmentData.expensesTypeList);
                insertParsedApartmentList(apartmentData.apartmentsList);
                insertParsedPropertyAmenities(apartmentData.amenityTypesList);
                
        }

    
        public static void insertParsedApartmentList(List<Apartments> apartments){
            if(apartments==null) return;
            if(apartments!=null && apartments.Count>0){
                BulkMergeUtil(apartments);
            }
        }

        public static void insertParsedSchools(List<School> schoolsList){
            if(schoolsList==null) return;
            if(schoolsList!=null && schoolsList.Count>0){
                using(IDbConnection db = DBConnectionHelper.getConnection(dbConfig))
                {//get connection
                    db.BulkMerge(schoolsList)
                        .ThenForEach(x => x.PropSchoolMapping)
                            .ForEach(y => y.SchoolId == x.id)
                        .ThenBulkMerge(x => x.PropSchoolMapping);
                }
            }
        }
        public static void insertParsedNTPI(List<NTPI> NtpiCategoryList){
            if(NtpiCategoryList==null) return;
            if(NtpiCategoryList!=null && NtpiCategoryList.Count>0){
                using(IDbConnection db = DBConnectionHelper.getConnection(dbConfig))
                {//get connection
                    db.BulkMerge(NtpiCategoryList)
                        .ThenForEach(x => x.NtpiList)
                            .ForEach(y => y.category == x.id)
                            .ThenForEach(y => y.PropNTPIMapping)
                                .ForEach(z => z.NtpiId == y.id)
                        .ThenBulkMerge(x => x.NtpiList)
                        .ThenBulkMerge(y => y.PropNTPIMapping);
                }
            }
        }

        //insert reviews in db
        public static void insertParsedReviews(List<Review> reviewsList){
            if(reviewsList==null) return;
            if(reviewsList!=null && reviewsList.Count>0)
            {
                BulkMergeUtil(reviewsList);
            }
        }

        public static void insertParsedExpenseType(List<Expensetype> expensesTypeList){
            if(expensesTypeList==null) return;
            if(expensesTypeList!=null && expensesTypeList.Count>0)
            {
                using(IDbConnection db = DBConnectionHelper.getConnection(dbConfig)){//get connection
                   db.BulkMerge(expensesTypeList)//insert the list of property types
                        .ThenForEach(x => x.expensesList
                                            .ForEach(y => y.expensetype = x.id))//set property type id for properties
                        .ThenBulkMerge(x => x.expensesList);
                }
            }
        }
        
        public static void insertParsedPropertyAmenities(List<Amenitytype> amenityTypeList)
        {  
            if(amenityTypeList==null) return;
            if(amenityTypeList!=null && amenityTypeList.Count>0)
            {
                using(IDbConnection db = DBConnectionHelper.getConnection(dbConfig)) {//get connection
                    db.BulkMerge(amenityTypeList) 
                        .ThenForEach(x => x.amenityList
                                .ForEach(y => y.amenitytype = x.id))
                                .ThenForEach(y => y.PropAmenityMapping)
                                    .ForEach(z => z.Amenity == y.id)
                        .ThenBulkMerge(x => x.amenityList)
                        .ThenBulkMerge(y => y.PropAmenityMapping);
                }
            }
        }


        public static void BulkMergeUtil<T>(List<T> list){
            using(IDbConnection db = DBConnectionHelper.getConnection(dbConfig)){
                db.BulkMerge(list);
            }
        }

        /*
        This method simply merges whatever data is passed to it into DB
        */
        public static void updateURLs(Queue<URL> myUrlQueue){
            
            using(IDbConnection db = DBConnectionHelper.getConnection(dbConfig)){
                if(db!=null) db.BulkMerge(myUrlQueue);
            }
        }

        /*
        This method updates the status of url passed to it to DONE.
        */
        public static void markURLDone(URL url){
            
            using(IDbConnection db = DBConnectionHelper.getConnection(dbConfig)){
                if(db!=null) db.Execute("update url set status = @status where id=@id", new {status = (int)URL.URLStatus.DONE, id = url.id});
            }
        }
     }
}
