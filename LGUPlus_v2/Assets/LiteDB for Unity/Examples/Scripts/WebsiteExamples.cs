using System;
using System.Collections.Generic;
using System.IO;
using LiteDB;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class runs simple examples that are provided from the LiteDB website.
/// </summary>
public class WebsiteExamples : MonoBehaviour
{
    #region Example 1

    /// <summary>
    /// The Customer class from Example 1.
    /// </summary>
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string[] Phones { get; set; }
        public bool IsActive { get; set; }
    }
    
    /// <summary>
    /// Example 1 provides a demonstration of how to get started with LiteDB. It quickly shows how to create
    /// a database file, create a collection and add documents to it, as well as update, read, and create an index.
    /// 
    /// This example is derived from the "Basic example" on the litedb.org website.
    /// </summary>
    public void RunWebsiteExample1()
    {
        OutputText("Running Example 1...");

        // While not a requirement, it is recommended that you use Unity path
        // variables to ensure your code will run across different platforms.
        string fileLocation = Path.Combine(Application.temporaryCachePath, "Example01.db");

        // Open database (or create if doesn't exist)
        using (var db = new LiteDatabase(fileLocation))
        {
            OutputText("Saving database to " + fileLocation);

            // Get customer collection
            var col = db.GetCollection<Customer>("customers");

            // Create your new customer instance
            var customer = new Customer
            {
                Name = "John Doe",
                Phones = new[] { "8000-0000", "9000-0000" },
                Age = 39,
                IsActive = true
            };

            // Create unique index in Name field.
            // Note: The website version of the example requires unique names, but this prevents the example from running multple times.
            col.EnsureIndex(x => x.Name, false);

            // Insert new customer document (Id will be auto-incremented)
            col.Insert(customer);

            // Update a document inside a collection
            customer.Name = "Joana Doe";

            col.Update(customer);

            // Use LINQ to query documents (with no index)
            var results = col.Find(x => x.Age > 20);

            foreach (var c in results)
            {
                OutputText("Id: {0}\r\n Name: {1}\r\n Age: {2}\r\n Is Active: {3}\r\n Phones: '{4}'",
                    c.Id, c.Name, c.Age, c.IsActive, String.Join(Environment.NewLine, c.Phones));
            }
        }

        OutputText("Example 1 finished." + Environment.NewLine);
    }

    #endregion Example 1

    #region Example 2 (DbRef for cross references)

    public class Product
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
    }

    public class Order
    {
        public ObjectId Id { get; set; }
        public DateTime OrderDate { get; set; }
        public Customer Customer { get; set; }
        public List<Product> Products { get; set; }
    }

    /// <summary>
    /// Example 2 demonstrates how to the BsonMapper to define references to
    /// other collections.
    /// 
    /// This example is derived from the "DbRef for cross references" example on the litedb.org website.
    /// </summary>
    public void RunWebsiteExample2()
    {
        OutputText("Running Example 2...");

        // Re-use the Global instance of the Mapper.
        var mapper = BsonMapper.Global;

        // Products and Customers are other collections (not embedded document), and we need to
        // let the LiteDB mapper know that they should appear in the Order collection as references.
        // Alternatively, [BsonRef("colname")] attribute can be used on the model class to specify references.
        mapper.Entity<Order>()
            .DbRef(x => x.Products, "products")
            .DbRef(x => x.Customer, "customers");

        string fileLocation = Path.Combine(Application.temporaryCachePath, "Example02.db");
        using (var db = new LiteDatabase(fileLocation))
        {
            OutputText("Saving database to " + fileLocation);

            var customers = db.GetCollection<Customer>("customers");
            var products = db.GetCollection<Product>("products");
            var orders = db.GetCollection<Order>("orders");

            // create examples
            var john = new Customer { Name = "Jenny Doe", Age = 28, IsActive = true, Phones = new [] {"555-867-5309"} };
            var tv = new Product { Description = "TV Sony 44\"", Price = 799 };
            var iphone = new Product { Description = "iPhone X", Price = 999 };
            var order1 = new Order { OrderDate = new DateTime(2018, 1, 1), Customer = john, Products = new List<Product> { iphone, tv } };
            var order2 = new Order { OrderDate = new DateTime(2018, 10, 1), Customer = john, Products = new List<Product> { iphone } };

            // insert into collections
            customers.Insert(john);
            products.Insert(new Product[] { tv, iphone });
            orders.Insert(new Order[] { order1, order2 });

            // create index in OrderDate
            orders.EnsureIndex(x => x.OrderDate);

            // When querying Order, include references.
            var query = orders
                .Include(x => x.Customer)
                .Include(x => x.Products)
#if !UNITY_IOS
                .Find(x => x.OrderDate == new DateTime(2018, 1, 1));
#else
                .Find(Query.EQ("OrderDate", new DateTime(2018, 1, 1)));
#endif

            // Each instance of Order will load Customer/Products references
            foreach (var c in query)
            {
                OutputText("#{0} - {1}", c.Id, c.Customer.Name);

                foreach (var p in c.Products)
                {
                    OutputText(" > {0} - {1:c}", p.Description, p.Price);
                }
            }
        }

        OutputText("Example 2 finished." + Environment.NewLine);
    }

#endregion

    /// <summary>
    /// Output from the examples is displayed to this Text field.
    /// </summary>
    public Text LogText;

    void Start()
    {
        if (LogText == null)
        {
            Debug.LogError("LogText must be set to view output.");
        }
    }

    internal void OutputText(string s)
    {
        if (String.IsNullOrEmpty(LogText.text))
        {
            LogText.text = s;
        }
        else
        {
            LogText.text += Environment.NewLine + s;
        }
    }

    internal void OutputText(string format, params object[] args)
    {
        if (String.IsNullOrEmpty(LogText.text))
        {
            LogText.text = String.Format(format, args);
        }
        else
        {
            LogText.text += Environment.NewLine + String.Format(format, args);
        }
    }

}
