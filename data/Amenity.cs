/*
This class will be used to return Amenity details from the parser.
This will also be used to save data to the DB.
*/

namespace WebScraperModularized.data{

    public class Amenity{

        public int id {get; set;}

        public string title {get; set;}

        public int amenitytype {get; set;}

        public int property {get; set;}

        public List<PropertyAmenityMapping> PropAmenityMapping;
        
    }
}