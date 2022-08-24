using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Text;


namespace Honeywell.CreateRating
{
    public static class CreateRating_Hackerone
    {
        [FunctionName("createRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestMessage req, 
            [CosmosDB(databaseName: "cos-test", collectionName: "icecreamratings", ConnectionStringSetting = "CosmosDbConnectionString1")] IAsyncCollector<dynamic> documentsOut,
            ILogger log)
        {
                try
            {
                // Convert all request param into Json object

                var content = req.Content;
                string jsonContent = content.ReadAsStringAsync().Result;
                dynamic requestPram = JsonConvert.DeserializeObject<PartnerRatingModel>(jsonContent);

                // Call Your  GetUser API
                HttpClient newClient = new HttpClient();
                HttpRequestMessage newRequest = new HttpRequestMessage(HttpMethod.Get, string.Format("https://serverlessohapi.azurewebsites.net/api/GetUser?userId={0}", requestPram.userId));

                //Read Server Response
                HttpResponseMessage responseUserID = await newClient.SendAsync(newRequest);
                  
             
                HttpRequestMessage newRequest1 = new HttpRequestMessage(HttpMethod.Get, string.Format("https://serverlessohapi.azurewebsites.net/api/GetProduct?productId={0}", requestPram.productId));
                //Read Server Response for product ID
                HttpResponseMessage responseProductID = await newClient.SendAsync(newRequest1);
                Console.WriteLine(responseProductID.StatusCode);

                if(responseProductID.StatusCode==HttpStatusCode.OK) 
                {
                    if(responseUserID.StatusCode==HttpStatusCode.OK) 
                    {
                        if (!string.IsNullOrEmpty(jsonContent))
                        {
                            ResponseOutput functionout = new ResponseOutput() 
                            {
                                id = System.Guid.NewGuid().ToString(),
                                userId = requestPram.userId,
                                productId = requestPram.productId,
                                locationName = requestPram.locationName,
                                rating = requestPram.rating,
                                userNotes = requestPram.userNotes,
                                timestamp = DateTime.Now
                            };
                            // Add a JSON document to the output container.
                            await documentsOut.AddAsync(functionout);
                            return new OkObjectResult(functionout);
                        }
                    
                    



                    }
                } 
                else
                {
                    return new OkObjectResult("User or Product are not part of the list.");
                }

            // Code snippet for ComboDB will come here....
 


           return new OkObjectResult("Executed"); 
           }
            catch (Exception ex)
            {

            

                  return new OkObjectResult("Exception...");
            }
        }
    }
    
   public class PartnerRatingModel
    {
        public string userId { get; set; }
        public string productId { get; set; }
        public string locationName { get; set; }
        public int rating { get; set; }
        public string userNotes { get; set; }
    }


    public class PartnerMpnResponseModel
    {
        public string isValidUserId { get; set; }
            public string isValidProductId { get; set; }
    }

     public class ResponseOutput
    {
        public string userId { get; set; }
        public string productId { get; set; }
        public string locationName { get; set; }
        public int rating { get; set; }
        public string userNotes { get; set; }

        public string id { get; set; }
        public DateTime timestamp { get; set; }
    } 
}