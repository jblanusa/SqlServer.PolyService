using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlServer.PolyService;
using System.Data.SqlTypes;

namespace SqlServerHostDriver
{
    [TestClass]
    public class ODataNorthwindUnitTest
    {
        string url = "http://services.odata.org/V4/Northwind/Northwind.svc";
        [TestMethod]
        public void LoadRegions()
        {
            string resource = "Regions";
            GetODataRequest(resource);
        }

        [TestMethod]
        public void LoadProducts()
        {
            string resource = "Products";
            GetODataRequest(resource);
        }

        [TestMethod]
        public void LoadCustomers()
        {
            string resource = "Customers";
            GetODataRequest(resource);
        }

        [TestMethod]
        public void LoadEmployees()
        {
            string resource = "Employees";
            GetODataRequest(resource);
        }

        private void GetODataRequest(string resource)
        {
            OData wc = OData.Parse(new SqlString(url + "/" + resource));
            string response = wc.Get();
            var json = JsonConvert.DeserializeObject(response);
            Assert.IsTrue(json is JObject, "Response must be JSON");
            Assert.IsTrue((json as JObject).First.First.ToString().StartsWith(url), "prefix");
            Assert.IsTrue((json as JObject).First.First.ToString().EndsWith(resource), "sufix");

            foreach (var jobject in (json as JObject).First.Next.Children())
            {
                Assert.IsNotNull((jobject.First as JObject).Property(resource.TrimEnd('s') + "ID"), "Key " + resource.TrimEnd('s') + "ID is not found");
            }

            Assert.IsNotNull(response, "Response is null.");
            Assert.IsTrue(response.Length > 0, "Zero-length response is returned.");
        }
    }
}
