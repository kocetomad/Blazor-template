namespace BlazorRecycleApp

{
    public class Item
    {
        public string itemname { get; set; } = "";
        public string itemdesc { get; set; } = "";
        public string itemimg_location { get; set; } = "";//REMOVE AFTER BLOB IMAGES ARE IMPLAMENTED 
        public string itemc { get; set; } = "";
        public string iteml { get; set; } = "";
        public List<Image> imgs { get; set; } = new List<Image>();
        public string QNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public string posted_by { get; set; } = "";
        public int id { get; set; } = 0;

    }
}
