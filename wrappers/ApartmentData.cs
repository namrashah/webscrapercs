/*
This class will be used as a wrapper object for data from the ApartmentParser.
*/

using WebScraperModularized.data;
using System.Collections.Generic;

namespace WebScraperModularized.wrappers{
    
    public class ApartmentData{

        public List<Apartments> apartmentsList;

        public List<Amenitytype> amenityTypesList;

        public List<NTPICategory> NTPIList;

        public List<Review> reviewsList;

        public List<School> schoolsList;

        public List<Expensetype> expensesTypeList;

        public string description;

        public SoundScore soundScore;
    }
}