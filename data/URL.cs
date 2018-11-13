/**
This class will contain information for each URL.
This will be returned from the parser or passed to the parser.
 */
namespace WebScraperModularized.data{
    public class URL{

        public enum URLType : int{
            APARTMENT_URL = 0,
            PROPERTY_URL = 1
        }

        public enum URLStatus : int{
            INITIAL = 0,
            RUNNING = 1,
            DONE = 2,
            ERROR = -1
        }

        public int id{get; set;}
        public string url{get; set;}

        public int urltype{get; set;}

        public int status{get; set;}

        public int property {get; set;}

        public string error {get; set;}

        public int zipcode {get; set;}
    }
}