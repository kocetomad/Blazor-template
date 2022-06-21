using BlazorRecycleApp;
using Npgsql;
using System.Reflection.Metadata;
using System.Text;

namespace RecyclingApp
{
    public class SQLUtil
    {
        public static string connectionString { get; set; } =
            "Host=localhost;Port=5432;Username=postgres;Password=123456;Database=postgres";
        public static string insertUserDetails { get; set; } =
        "insert into rec_item_data (itemname, itemdesc,itemimg_location, itemc, iteml,contact_qnumber,contact_email,posted_by) values(@n,@d,@l,@c,@k,@Q,@E,@b) RETURNING uniqueid;";
        public static string getItemsQuery { get; set; } =
        "select * from rec_item_data";
        public static string itemDeleteQuery { get; set; } =
        "delete from rec_item_data where itemc = @l and iteml = @c and posted_by = @q and itemname = @N and contact_email = @E";
        public static string getUserItemsQuery { get; set; } =
        "select * from rec_item_data where posted_by = @U";
        public static string updateItemQuery { get; set; } =
        "update rec_item_data set itemname = @n, itemdesc = @d, itemc = @c, iteml = @l, contact_qnumber = @q, contact_email = @e where uniqueid = @I::INTEGER";
        public static string insertBlob { get; set; } =
        "insert into img_table(img_string,listing_id) values(@B::BYTEA,@I)";
        public static string getListingImgs { get; set; } =
        "select img_id,img_string::text from img_table where listing_id = @I::INTEGER";
        public static string deleteImg { get; set; } =
        "delete from img_table where img_id = @I";
        public static string deleteImgbyListingID { get; set; } =
        "delete from img_table where listing_id = @I";

        public static async Task<long> insertNewItem(string name, string desc, string dir, string cat, string loc, string Q, string E, string b)
        {
            long id = 0;
            using (var con = new NpgsqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    var cmd = new NpgsqlCommand(insertUserDetails, con);
                    cmd.Parameters.AddWithValue("n", name);
                    cmd.Parameters.AddWithValue("d", desc);
                    cmd.Parameters.AddWithValue("l", dir);
                    cmd.Parameters.AddWithValue("c", cat);
                    cmd.Parameters.AddWithValue("k", loc);
                    cmd.Parameters.AddWithValue("Q", Q);
                    cmd.Parameters.AddWithValue("E", E);
                    cmd.Parameters.AddWithValue("b", b);

                    using (var reader = cmd.ExecuteReader())
                    {
                        
                        while (reader.Read())
                        {
                            id = (int)reader["uniqueid"];
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    con.Close();
                }
            }
            return id;
        }

        public static void itemDelete(Item item, string user)
        {
            using (var con = new NpgsqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    var cmd = new NpgsqlCommand(itemDeleteQuery, con);
                    cmd.Parameters.AddWithValue("l", item.itemc); 
                    cmd.Parameters.AddWithValue("c", item.iteml);
                    cmd.Parameters.AddWithValue("q", user);
                    cmd.Parameters.AddWithValue("N", item.itemname);
                    cmd.Parameters.AddWithValue("E", item.Email);
                    Console.WriteLine(item.itemc +" "+ item.iteml + " " + user + " " + item.itemname + " " + item.Email);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    con.Close();
                }
            }
        }

        public static async Task<List<Item>> getItemsAsync(string options)
        {
            List<Item> itemList = new List<Item>();
            try
            {
                using (var con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    var cmd = new NpgsqlCommand(getItemsQuery + options, con);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Item temp = new Item();
                            temp.itemname = reader["itemname"].ToString();
                            temp.itemdesc = reader["itemdesc"].ToString();
                            temp.itemimg_location = reader["itemimg_location"].ToString();
                            temp.itemc = reader["itemc"].ToString();
                            temp.iteml = reader["iteml"].ToString();
                            temp.QNumber = reader["contact_qnumber"].ToString();
                            temp.Email = reader["contact_email"].ToString();
                            temp.id = (int)reader["uniqueid"];
                            temp.imgs = await fetchListingImgs((int)reader["uniqueid"]);
                            String searchFolder = temp.itemimg_location;
                            var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp", "svg" };
                            //temp.imgs = GetFilesFrom("wwwroot\\"+searchFolder, filters, false);

                            itemList.Add(temp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
            }
            
            return itemList;

        }
        public static async Task<List<Image>> fetchListingImgs(int id)
        {
            List<Image> imgList = new List<Image>();
            try
            {
                using (var con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    var cmd = new NpgsqlCommand(getListingImgs, con);
                    cmd.Parameters.AddWithValue("I", id);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            
                            Image tempimg = new Image();
                            tempimg.id = (int)reader["img_id"];
                            var tempstr = reader["img_string"].ToString().Remove(0, 2);
                            byte[] tempbyte = StringToByteArray(tempstr);
                            string encodedStr = System.Text.Encoding.Default.GetString(tempbyte);
                            tempimg.imgmname = encodedStr;
                            imgList.Add(tempimg);                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
            }

            return imgList;

        }
        public static void updateItem(string name, string desc, string cat, string loc, string q, string e, int id)
        {

            using (var con = new NpgsqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    var cmd = new NpgsqlCommand(updateItemQuery, con);
                    cmd.Parameters.AddWithValue("n", name);
                    cmd.Parameters.AddWithValue("d", desc);
                    cmd.Parameters.AddWithValue("l", loc);
                    cmd.Parameters.AddWithValue("c", cat);
                    cmd.Parameters.AddWithValue("q", q);
                    cmd.Parameters.AddWithValue("e", e);
                    cmd.Parameters.AddWithValue("I", id.ToString());
                    
                    Console.WriteLine(name +" "+ desc + " " + loc + " " + cat + " " + q + " " + e + " old item name:" + id );
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    con.Close();
                }
            }
        }

        public static void insertImages(string blob,long id)
        {
            using (var con = new NpgsqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    var cmd = new NpgsqlCommand(insertBlob, con);
                    cmd.Parameters.AddWithValue("B", blob);
                    cmd.Parameters.AddWithValue("I", id);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    con.Close();
                }
            }
        }

        public static void deleteImage(long id)
        {
            using (var con = new NpgsqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    var cmd = new NpgsqlCommand(deleteImg, con);
                    cmd.Parameters.AddWithValue("I", id);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    con.Close();
                }
            }
        }
        public static void deleteListingImgs(long id)
        {
            using (var con = new NpgsqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    var cmd = new NpgsqlCommand(deleteImgbyListingID, con);
                    cmd.Parameters.AddWithValue("I", id);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    con.Close();
                }
            }
        }

        public static async Task<List<Item>> getUserItemsAsync(string name)
        {
            List<Item> itemList = new List<Item>();
            try
            {
                using (var con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    var cmd = new NpgsqlCommand(getUserItemsQuery, con);
                    cmd.Parameters.AddWithValue("U", name);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Item temp = new Item();
                            temp.itemname = reader["itemname"].ToString();
                            temp.itemdesc = reader["itemdesc"].ToString();
                            temp.itemimg_location = reader["itemimg_location"].ToString();
                            temp.itemc = reader["itemc"].ToString();
                            temp.iteml = reader["iteml"].ToString();
                            temp.QNumber = reader["contact_qnumber"].ToString();
                            temp.Email = reader["contact_email"].ToString();
                            temp.id = (int)reader["uniqueid"];
                          
                            temp.imgs = await fetchListingImgs((int)reader["uniqueid"]);
                            itemList.Add(temp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
            }

            return itemList;

        }
        public static String[] GetFilesFrom(String searchFolder, String[] filters, bool isRecursive)
        {
            List<String> filesFound = new List<String>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
            }
            return filesFound.ToArray();
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

    }


}
