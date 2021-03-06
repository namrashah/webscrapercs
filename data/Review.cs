/*
This class will be used to return property reviews from the parser.
This will also be used to save data to db.
*/

namespace WebScraperModularized.data{
    
    public class Review{
        
        public int id {get; set;}

        public string content {get; set;}

        public string title {get; set;}

        public int rating {get; set;}//out of 5

        public int helpful {get; set;}//number of people who found it helpful

        public int property {get; set;}
    }
}