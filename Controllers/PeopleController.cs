using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using PeopleAPI.Models;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Diagnostics.Eventing.Reader;

namespace PeopleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        public PeopleController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        [HttpGet]
        public JsonResult Get()
        {
            string query = @"
                select Id, Name, Email,
                convert(varchar(10), DateOfBirth, 120) as DateOfBirth, Sex, Phone
                from
                dbo.Person
            ";
            JsonResult table = QueryDB(query, null);
            return table;
        }
       
        [HttpPost]
        public JsonResult Post(Person person)
        {
            string validResult = isDataValidated(person);
            if (validResult!= "Valid")
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return new JsonResult("Bad Request - "+validResult);
            }
            string query = @"
                insert into dbo.Person
                (Id, Name,Email, DateOfBirth, Sex, Phone)
                values(@Id, @Name, @Email, @DateOfBirth, @Sex, @Phone)
            ";
            List<(string, string)> sqlParams = new List<(string, string)>();
            sqlParams.Add(("@Id", person.Id.ToString()));
            sqlParams.Add(("@Name", person.Name));
            sqlParams.Add(("@Email", person.Email));
            sqlParams.Add(("@DateOfBirth", person.DateOfBirth));
            sqlParams.Add(("@Sex", person.Sex.ToString()));
            sqlParams.Add(("@Phone", person.Phone.ToString()));
            var result = QueryDB(query, sqlParams);
            if (Response.StatusCode == 500)
            {
                return result;
            }
            return new JsonResult("Added Successfuly");
        }


        [HttpPut]
        public JsonResult Put(Person per)
        {
            string validResult = isDataValidated(per);
            if (validResult != "Valid")
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return new JsonResult("Bad Request - " + validResult);
            }
            string query = @"
                update dbo.Person
                set Name = @Name,
                Email=@Email,
                DateOfBirth=@DateOfBirth,
                Sex=@Sex,
                Phone=@Phone
                where Id=@Id
            ";
            List<(string, string)> sqlParams = new List<(string, string)>();
            sqlParams.Add(("@Id", per.Id.ToString()));
            sqlParams.Add(("@Name", per.Name));
            sqlParams.Add(("@Email", per.Email));
            sqlParams.Add(("@DateOfBirth", per.DateOfBirth));
            sqlParams.Add(("@Sex", per.Sex.ToString()));
            sqlParams.Add(("@Phone", per.Phone.ToString()));
            var result = QueryDB(query, sqlParams);
            if (Response.StatusCode == 500)
            {
                return result;
            }
            return new JsonResult("Updated Successfuly");
        }
        [HttpDelete("{Id}")]
        public JsonResult Delete(int Id)
        {            
            string query = @"
                delete from dbo.Person
                where Id=@Id
            ";
            List<(string, string)> sqlParams = new List<(string, string)>();
            sqlParams.Add(("@Id", Id.ToString()));
            var result = QueryDB(query, sqlParams);
            if (Response.StatusCode == 500)
            {
                return result;
            }
            return new JsonResult("Deleted Successfuly");
        }


        ///////////////////////
        // private functions //
        ///////////////////////

        private JsonResult QueryDB(string query, List<(string, string)> sqlParams)
        {
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("PeopleAppCon");
            SqlDataReader myReader;
            try
            {
                using (SqlConnection myConn = new SqlConnection(sqlDataSource))
                {
                    myConn.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myConn))
                    {
                        if (sqlParams is not null)
                        {
                            sqlParams.ForEach(sqlParam => myCommand.Parameters.AddWithValue(sqlParam.Item1, sqlParam.Item2));
                        }
                        myReader = myCommand.ExecuteReader();
                        table.Load(myReader);
                        myReader.Close();
                        myConn.Close();
                    }
                }
                return new JsonResult(table);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return new JsonResult(ex.Message);
            }
            
        }

        private string isDataValidated(Person per)
        {
            try
            {
                var addr = new MailAddress(per.Email).Address;
            }
            catch(FormatException e)
            {
                return e.Message;
            }
            string datePattern = @"^\d{4}-\d{2}-\d{2}$";
            
            string Sex = per.Sex.ToString();
            if (!Regex.IsMatch(per.DateOfBirth, datePattern))
                return "date not valid, it should be yyyy-mm-dd";
            if (!Regex.IsMatch(Sex.ToLower(), @"(male|female|other|0|1|2)"))
                return "Sex can be 0 (Male), 1 (Female) or 2 (Other)";
            return "Valid";
        }
    }
}
