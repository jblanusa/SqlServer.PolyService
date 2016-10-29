using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.PolyService;
using PolyService.Azure;
using PolyService.Service;
using Newtonsoft.Json.Linq;

namespace PolyServiceUnitTest
{
    [TestClass]
    public class PolyserviceUnitTest
    {
        /// <summary>
        /// Test Method that checks whether Get
        /// and UploadContent works properly
        /// </summary>
        [TestMethod]
        public void DataSourceSerDe()
        {
            var DataSource = "my-account";
            var Username = "my-username";
            var Pasword = "my-pasword";
            var Type = "my-type";
            var ds = PolyService.Common.DataSource.Parse(string.Format("Data Source={0};Username={1};Password={2};Provider={3}", DataSource,Username,Pasword,Type));

            Assert.AreEqual(DataSource, ds.Uri, "Uri is not loaded");
            Assert.AreEqual(Username, ds.Username, "Username is not loaded");
            Assert.AreEqual(Pasword, ds.Password, "Password is not loaded");
            Assert.AreEqual(Type, ds.Provider, "DataSourceType is not loaded");

        }
        /// <summary>
        /// Test Method that checks whether Get
        /// and UploadContent works properly
        /// </summary>
        [TestMethod]
        public void BlobGetAndPost()
        {
            //Change account name and account key below
            //Your blob account needs to have $root directory
            const string url = "https://[my account].blob.core.windows.net/testblob";
            const string accountKey = "[account key]";
            if (accountKey != "[account key]")
            {
                Blob bb = Blob.Parse(url);
                Assert.IsTrue(url == bb.Url, "URL match failed");
                bb.SetAccountKey(accountKey);
                Assert.IsTrue(accountKey == bb.Key, "Account key match failed");

                string expected = "Today is " + DateTime.Today.ToString(CultureInfo.InvariantCulture) + ", and time is " +
                                  DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
                try
                {
                    bb.UploadContent(Encoding.ASCII.GetBytes(expected));
                }
                catch (WebException)
                {
                    Assert.Fail("Web exception occurred during UploadContent method");
                }
                catch
                {
                    Assert.Fail("Exception occurred during UploadContent method");
                }

                try
                {
                    byte[] buf = (byte[]) bb.Get();
                    string actual = Encoding.ASCII.GetString(buf);
                    Assert.IsTrue(expected == actual, "Blob content match failed");
                }
                catch (WebException)
                {
                    Assert.Fail("Web exception occurred during Get method");
                }
                catch
                {
                    Assert.Fail("Exception occurred during Get method");
                }
            }
        }
        

        /// <summary>
        /// Test method that checks Serialization for OData
        /// </summary>
        [TestMethod]
        public void ODataSerialize()
        {
            OData original = OData.Parse("http://services.odata.org/V4/Northwind/Northwind.svc/Customers");
            original.Select("CustomerID, CompanyName,Address,City,Country,Phone")
                .Filter("Country eq 'Mexico'")
                .OrderBy("Phone asc")
                .Skip(2)
                .Take(2)
                .Get();
            OData copy = this.RWSSerialize<OData>(original);
            Assert.IsTrue(copy.IsEqual(original), "OData match failed");
        }

        /// <summary>
        /// Test method that checks Serialization for Blob
        /// </summary>
        [TestMethod]
        public void BlobSerialize()
        {
            Blob original = Blob.Parse("BlobRootUrl");
            original.SetAccountKey("pass");
            Blob copy = this.RWSSerialize<Blob>(original);
            Assert.IsTrue(copy.IsEqual(original), "Blob match failed");
        }

        /// <summary>
        /// Test method that checks Serialization for Blob
        /// </summary>
        [TestMethod]
        public void TableSerialize()
        {
            Table original = Table.Parse("https://techready.table.core.windows.net/article");
            original.SetAccountKey("hb5qy6eXLqIdd2D1hElWoIa8EmLxjO+sBGLJcRuKUiQEYedSz4m8ALn8BBj0LvGMHdrTHDvWjUZg3Gu7bubKLg==");
            original.Value("property1", "value1")
                .KeyValue("key")
                .IntValue("property2", 15);

            Table copy = this.RWSSerialize<Table>(original);
            Assert.IsTrue(copy.IsEqual(original), "Table match failed");
        }

        /// <summary>
        /// Test method that checks CRUD operations for TableStorage
        /// </summary>
        [TestMethod]
        public void TableStorageCRUD()
        {
            Table ts = Table.Parse("https://techready.table.core.windows.net");
            ts.SetAccountKey("hb5qy6eXLqIdd2D1hElWoIa8EmLxjO+sBGLJcRuKUiQEYedSz4m8ALn8BBj0LvGMHdrTHDvWjUZg3Gu7bubKLg==");
            //ts.KeyValue("1").Value("Name", "Jovan").Value("Surname", "Popovic").IntValue("Age", 34).InsertInto("article");

            var data = ts.From("article").Select("Name,Surname").Get();

            Assert.IsNotNull(data);


        }

        /// <summary>
        /// Test method that checks  Serialization for AzureSearch
        /// </summary>
        [TestMethod]
        public void AzureSearchSerialize()
        {
            SearchService original = SearchService.Parse("AzureSearchRootUrl"); 
            original.SetApiKey("API-KEY FOR AZURE SEARCH");
            SearchService copy = this.RWSSerialize<SearchService>(original);
            Assert.IsTrue(copy.IsEqual(original), "AzureSearch match failed");
        }

        /// <summary>
        /// Test method that checks  Serialization for DocumentDB
        /// </summary>
        [TestMethod]
        public void DocDBSerialize()
        {
            DocumentDB original = DocumentDB.Parse("DocumentDBRootUrl");
            original.SetAccountKey("pass");
            DocumentDB copy = this.RWSSerialize<DocumentDB>(original);
            Assert.IsTrue(copy.IsEqual(original), "DocumentDB match failed");
        }

        /// <summary>
        /// Test method that checks Serialization for AzureMLApp
        /// </summary>
        [TestMethod]
        public void AzureMLAppSerialize()
        {
            AzureMLApp original = AzureMLApp.Parse("ServiceRootUrl");
            original.SetAccountKey("account-key");
            original.SetUserName("user-name");
            AzureMLApp copy = this.RWSSerialize<AzureMLApp>(original);
            Assert.IsTrue(copy.IsEqual(original), "AzureMLApp match failed");
        }

        /// <summary>
        /// Test method that checks Serialization for Neo4j
        /// </summary>
        [TestMethod]
        public void Neo4jSerialize()
        {
            Neo4j original = Neo4j.Parse("Neo4jRootUrl");
            original.SetNamePass("username", "password");
            original.Match("n:User").Filter("name:'Adam'").InnerJoin("").InnerJoin("").InnerJoin("r").Returns("r").Limit(20);
            Neo4j copy = this.RWSSerialize<Neo4j>(original);
            Assert.IsTrue(copy.IsEqual(original), "Neo4j match failed");
        }

        /// <summary>
        /// Test method for checking query builder for Neo4j
        /// </summary>
        [TestMethod]
        public void Neo4jTest()
        {
            Neo4j nfj = Neo4j.Parse("Neo4jRootUrl");
            nfj.SetNamePass("username", "password");
            nfj.Match("m")
                .WithProperty("name", "Rosie O'Donnell")
                .Filter("born:1962")
                .InnerJoin("")
                .OnRelation(":ACTED_IN")
                .Filter("Some Random Filter")
                .Returns("m")
                .FormatQuery();
            Assert.IsTrue(nfj.queryBody.Trim() == "MATCH (m{name:'Rosie O\\\\'Donnell', born:1962})-[:ACTED_IN{Some Random Filter}]-() RETURN m", "Neo4j query match failed");
        }

        /// <summary>
        /// Test method for checking query builder for Neo4j
        /// </summary>
        [TestMethod]
        public void Neo4jPatternTest()
        {
            Neo4j nfj = Neo4j.Parse("Neo4jRootUrl");
            nfj.SetNamePass("username", "password");
            nfj.MatchPattern("(@tom) -[@r1]-> (movie) <-[@r2]- (director)")
                         .Relationship("tom").WithProperty("name", "Tom Hanks")
                         .Relationship("r1").AsType("DIRECTED")
                         .Relationship("r2")
                         .MatchPattern("(@a)-->(c)")
                         .Node("a").AsType("MOVIE")
                         .Returns("tom, movie, director")
                         .FormatQuery();
            Assert.IsTrue(nfj.queryBody.Trim() == "MATCH (tom{name:'Tom Hanks'}) -[r1:DIRECTED]-> (movie) <-[r2]- (director),(a:MOVIE)-->(c) RETURN tom, movie, director", "Neo4j pattern query match failed");

        }

        /// <summary>
        /// Generic method that checks Read and Write methods of RestWebService
        /// </summary>
        /// <typeparam name="T">RestWebService or inherited class</typeparam>
        /// <param name="original">Original object</param>
        /// <returns>Object created with Read method</returns>
        private T RWSSerialize<T>(T original) where T : RestWebService, new()
        {
            byte[] buff = new byte[1024];
            byte[] buff2 = new byte[1024];
            string expected, actual;
            T copy = RestWebService.GetNullValue<T>();

            using (var ms = new MemoryStream(buff))
            using (var bw = new BinaryWriter(ms))
            {
                try
                {
                    original.Write(bw);
                }
                catch
                {
                    Assert.Fail("Exception occurred during Write method");
                }

                expected = Encoding.UTF8.GetString(ms.ToArray());
            }

            using (var ms = new MemoryStream(buff))
            using (BinaryReader br = new BinaryReader(ms))
            {
                try
                {
                    copy.Read(br);
                }
                catch
                {
                    Assert.Fail("Exception occurred during Read method");
                }
            }

            using (var ms = new MemoryStream(buff2))
            using (BinaryWriter bw2 = new BinaryWriter(ms))
            {
                try
                {
                    copy.Write(bw2);
                }
                catch
                {
                    Assert.Fail("Exception occurred during Write method");
                }

                actual = Encoding.UTF8.GetString(ms.ToArray());

            }

            Assert.IsTrue(expected == actual, "Serialize and Deserialize failed");
            return copy;
        }

        /// <summary>
        /// Test Method that checks whether Get
        /// and UploadContent works properly
        /// </summary>
        [TestMethod]
        public void DeployRLogin()
        {
            //project=PROJECT-574a7a5d-c602-4341-8346-4d1d8d362e18&enableConsoleEvents=true&author=admin&filename=add.r&directory=root&version=&csvinputs=&robjects=zbir%2Cx_numeric_vector&inputs=%7B%22input_x%22%3A%7B%22type%22%3A%22primitive%22%2C%22value%22%3A2%7D%2C%22input_y%22%3A%7B%22type%22%3A%22primitive%22%2C%22value%22%3A5%7D%7D&format=json
            var url = "http://10.190.13.50:7400/deployr/r/user/login";
            var deployR = PolyService.Service.DeployR.Parse(string.Format("Data Source={0};Username={1};Password={2}", url, "admin", "dogmat1C"));
            System.Data.SqlTypes.SqlChars response = deployR.Login();


            Assert.AreNotEqual(new string(response.Value), "");

            string r = string.Empty;
            foreach (var c in response.Value) r += c;

            Assert.AreNotEqual(r, "");
            JObject json = JObject.Parse(r);
            string cookie = (string)json["deployr"]["response"]["httpcookie"];

        }

        /// <summary>
        /// Test Method that checks whether Get
        /// and UploadContent works properly
        /// </summary>
        [TestMethod]
        public void DeployRCode()
        {
            //project=PROJECT-574a7a5d-c602-4341-8346-4d1d8d362e18&enableConsoleEvents=true&author=admin&filename=add.r&directory=root&version=&csvinputs=&robjects=zbir%2Cx_numeric_vector&inputs=%7B%22input_x%22%3A%7B%22type%22%3A%22primitive%22%2C%22value%22%3A2%7D%2C%22input_y%22%3A%7B%22type%22%3A%22primitive%22%2C%22value%22%3A5%7D%7D&format=json
            var url = "http://10.190.13.50:7400/deployr/r/project/execute/code";
            var deployR = PolyService.Service.DeployR.Parse(string.Format("Data Source={0};Username={1};Password={2}", url, "admin", "dogmat1C"));


            //System.Data.SqlTypes.SqlChars responseLogin = deployR.Login();
            //string r = string.Empty;
            //foreach (var c in responseLogin.Value) r += c;
            //JObject json = JObject.Parse(r);
            //string cookie = (string)json["deployr"]["response"]["httpcookie"];

            //deployR.SetCookie(cookie);

            deployR.Project("PROJECT-b00a533f-f0f9-4be2-847c-5a4c4156e392");
            deployR.SetCookie("2700D5199AA22A680E8DDB6F1B90EB69");
            deployR.RObjects("zbir,x_numeric_vector");
            deployR.Inputs(@"{""input_x"":{""type"":""primitive"",""value"":20},""input_y"":{""type"":""primitive"",""value"":500}}");

            System.Data.SqlTypes.SqlChars response = deployR.ExecuteQuery(
@"require(RevoScriptTools)

revoInput('{""name"":""input_x"",""default"":2,""render"":""integer""}')
revoInput('{""name"":""input_y"",""default"":5,""render"":""integer""}')

zbir <- input_x + input_y
x_numeric_vector <- as.double(c(10:25))");

            string r2 = string.Empty;
            foreach (var c in response.Value) r2 += c;

            Assert.AreNotEqual(new string(response.Value), "");

            //string r = string.Empty;
            //foreach(var c in response.ToCharArray())
            //
            //var Username = "my-username";
            //var Pasword = "my-pasword";
            //var Type = "my-type";
            //var ds = PolyService.Service.Parse(string.Format("Data Source={0};Username={1};Password={2};Provider={3}", DataSource, Username, Pasword, Type));

            //Assert.AreEqual(DataSource, ds.Uri, "Uri is not loaded");
            //Assert.AreEqual(Username, ds.Username, "Username is not loaded");
            //Assert.AreEqual(Pasword, ds.Password, "Password is not loaded");
            //Assert.AreEqual(Type, ds.Provider, "DataSourceType is not loaded");

        }

        /// <summary>
        /// Test Method that checks whether Get
        /// and UploadContent works properly
        /// </summary>
        [TestMethod]
        public void DeployRScript()
        {
            //project=PROJECT-574a7a5d-c602-4341-8346-4d1d8d362e18&enableConsoleEvents=true&author=admin&filename=add.r&directory=root&version=&csvinputs=&robjects=zbir%2Cx_numeric_vector&inputs=%7B%22input_x%22%3A%7B%22type%22%3A%22primitive%22%2C%22value%22%3A2%7D%2C%22input_y%22%3A%7B%22type%22%3A%22primitive%22%2C%22value%22%3A5%7D%7D&format=json
            var url = "http://10.190.13.50:7400/deployr/r/project/execute/script";
            var deployR = PolyService.Service.DeployR.Parse(string.Format("Data Source={0};Username={1};Password={2}", url, "admin", "dogmat1C"));
            deployR.Project("PROJECT-b00a533f-f0f9-4be2-847c-5a4c4156e392");
            deployR.SetCookie("2700D5199AA22A680E8DDB6F1B90EB69");
            deployR.RObjects("zbir,x_numeric_vector");
            deployR.Inputs(@"{""input_x"":{""type"":""primitive"",""value"":2},""input_y"":{""type"":""primitive"",""value"":5}}");




            //if (false)
            //{
            //    deployR.SetCookie("2700D5199AA22A680E8DDB6F1B90EB69"); // radi
            //}
            //else
            //{
                System.Data.SqlTypes.SqlChars responseLogin = deployR.Login();
                //string r = string.Empty;
                //foreach (var c in responseLogin.Value) r += c;
                //JObject json = JObject.Parse(r);
                //string cookie = (string)json["deployr"]["response"]["httpcookie"];
                //deployR.SetCookie(cookie);
            //}




            //            var response = deployR.ExecuteQuery(
            //@"require(RevoScriptTools)
            //
            //revoInput('{""name"":""input_x"",""default"":2,""render"":""integer""}')
            //revoInput('{""name"":""input_y"",""default"":5,""render"":""integer""}')
            //
            //zbir <- input_x + input_y
            //x_numeric_vector <- as.double(c(10:25))").Value.ToString();


            var response = deployR.ExecuteScript("add.r");

            string r2 = string.Empty;
            foreach (var c in response.Value) r2 += c;
            var json2 = JObject.Parse(r2);


            Assert.AreNotEqual(response, "");

            //
            //var Username = "my-username";
            //var Pasword = "my-pasword";
            //var Type = "my-type";
            //var ds = PolyService.Service.Parse(string.Format("Data Source={0};Username={1};Password={2};Provider={3}", DataSource, Username, Pasword, Type));

            //Assert.AreEqual(DataSource, ds.Uri, "Uri is not loaded");
            //Assert.AreEqual(Username, ds.Username, "Username is not loaded");
            //Assert.AreEqual(Pasword, ds.Password, "Password is not loaded");
            //Assert.AreEqual(Type, ds.Provider, "DataSourceType is not loaded");

        }
 
    }
}
