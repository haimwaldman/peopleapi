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
using System.Xml.Linq;

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
        public JsonResult Post(Person per)
        {
            string validResult = isDataValidated(per);
            if (validResult != "Valid")
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return new JsonResult("Bad Request - " + validResult);
            }
            string query = buildPostQuery(per);
            List<(string, string)> sqlParams = buildParams(per);

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
            string query = buildPutQuery(per);

            List<(string, string)> sqlParams = buildParams(per);
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

        private string buildPostQuery(Person per)
        {
            string fields = @"(Id, Name,Email";
            string values = @"values(@Id, @Name, @Email";
            if (per.DateOfBirth != null)
            {
                fields += @", DateOfBirth";
                values += @", @DateOfBirth";
            }
            if (per.Sex != null)
            {
                fields += @", Sex";
                values += @", @Sex";
            }
            if (per.Phone != null)
            {
                fields += @", Phone";
                values += @", @Phone";
            }
            return @"insert into dbo.Person "
                + fields + @") "
                + values + @")";
        }
        private string buildPutQuery(Person per)
        {
            string query = @"
                update dbo.Person
                set Name = @Name,
                Email=@Email";
            if (per.DateOfBirth != null) { query += @",DateOfBirth=@DateOfBirth"; };
            if (per.Sex!= null) { query += @",Sex=@Sex"; };
            if (per.Phone != null) { query += @",Phone=@Phone"; };
            query += " where Id = @Id";
            return query;
        }
        private List<(string, string)> buildParams(Person per)
        {
            List<(string, string)> sqlParams = new List<(string, string)>();
            sqlParams.Add(("@Id", per.Id.ToString()));
            sqlParams.Add(("@Name", per.Name));
            sqlParams.Add(("@Email", per.Email));
            if (per.DateOfBirth != null)
            {
                sqlParams.Add(("@DateOfBirth", per.DateOfBirth));
            }
            if (per.Sex != null)
            {
                sqlParams.Add(("@Sex", per.Sex));
            }
            if (per.Phone != null)
            {
                sqlParams.Add(("@Phone", per.Phone.ToString()));
            }
            return sqlParams;
        }
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
                if (per.Email == "")
                    per.Email = null;
                if (per.Email != null)
                {
                    var addr = new MailAddress(per.Email).Address;
                }
            }
            catch(FormatException e)
            {
                return e.Message;
            }
            string datePattern = @"^\d{4}-\d{2}-\d{2}$";

            if (per.DateOfBirth != null)
            {
                if (!Regex.IsMatch(per.DateOfBirth, datePattern))
                    return "date not valid, it should be yyyy-mm-dd";
            }
            string Sex = per.Sex;
            if (Sex != null)
            {
                if (!Regex.IsMatch(Sex.ToLower(), @"(male|female|other)"))
                    return "Sex can be Male, Female or Other";
            }
            return "Valid";
        }
    }
}
