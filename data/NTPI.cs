/*
This class will be used to return nearby transit and Points of interest data from the parser.
This will also be used to save data to the DB.
*/

namespace WebScraperModularized.data{

    public class NTPI{

        public int id {get; set;}

        public string name{get; set;}

        public double drivetime {get; set;}//drive time in minutes

        public double distance {get; set;}//distance in miles

        public int category {get; set;}

        public int property {get; set;}

        public List<PropertyNTPIMapping> PropNTPIMapping;

    }
}