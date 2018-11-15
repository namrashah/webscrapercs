/*
This class will be used to return nearby schools data from the parser.
This will also be used to save data to the DB.
*/

namespace WebScraperModularized.data{
    
    public class School{


        public enum SchoolType : int{
            PUBLIC = 0,
            PRIVATE = 1
        }
        public int id {get; set;}

        public string name {get; set;}

        public string type {get; set;}

        public string grades {get; set;}

        public int numstudents {get; set;}

        public string contactnum {get; set;}

        public int rating {get; set;}//out of 10

        public int property {get; set;}

        public int schooltype;

        public List<PropertySchoolMapping> PropSchoolMapping;
        
    }
}