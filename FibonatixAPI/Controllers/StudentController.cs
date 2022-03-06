using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace FibonatixAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IConnectionMultiplexer m_Redis;

        // constructor for init redis db
        public StudentController(IConnectionMultiplexer redis)
        {
            m_Redis = redis;
        }

        // function for init db with  data
        private void initDBwithDummyValues()
        {
            List<Student> _students = new List<Student>
            {
                new Student
                {
                    Id = 1,
                    FirstName = "Gil",
                    LastName = "Nahum",
                    Age = 16,
                    GradesAvg = 80,
                    SchoolName = "Hadar",
                    SchoolAdress = "Hagalil 54"
                },
                new Student
                {
                    Id = 2,
                    FirstName = "Ilay",
                    LastName = "Frenkel",
                    Age = 18,
                    GradesAvg = 70,
                    SchoolName = "Alon",
                    SchoolAdress = "Migdal 46"
                },
                new Student
                {
                    Id = 3,
                    FirstName = "Tal",
                    LastName = "Zvi",
                    Age = 16,
                    GradesAvg = 90,
                    SchoolName = "Hadar",
                    SchoolAdress = "Hagalil 54"
                },
                new Student
                {
                    Id = 4,
                    FirstName = "Dan",
                    LastName = "Sagir",
                    Age = 18,
                    GradesAvg = 100,
                    SchoolName = "Alon",
                    SchoolAdress = "Migdal 46"
                }
            };

            updateStudentsList(_students);
        }

        // GET - get all students
        [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any, NoStore = false)]
        [HttpGet("All Students")]
        public async Task<ActionResult<List<Student>>> Get()
        {
            var students = await getStudentsList();
            return Ok(students);

        }

        // function for getting students list from redis db
        private async Task<List<Student>> getStudentsList()
        {
            var db = m_Redis.GetDatabase();
            var json = await db.StringGetAsync("students");
            if (json == RedisValue.EmptyString || json == RedisValue.Null)
            {
                return new List<Student>();

            }

            return JsonConvert.DeserializeObject<List<Student>>(json.ToString());
        }

        // function for updaing the redis db
        private void updateStudentsList(List<Student> list)
        {
            var db = m_Redis.GetDatabase();
            db.StringSet("students", JsonConvert.SerializeObject(list));
        }

        // GET - get single student data by ID
        [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any, NoStore = false)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetSingleStudent(int id)
        {
            var studentsList = await getStudentsList();

            // finding student in db if exists
            var student = studentsList.Find(x => x.Id == id);

            if (student == null)
            {
                return BadRequest(string.Format("Student with id {0} not found", id));
            }

            return Ok(student);
        }

        // POST - adding student to redis db
        [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any, NoStore = false)]
        [HttpPost]
        public async Task<ActionResult<List<Student>>> AddStudent(Student student)
        {
            var studentsList = await getStudentsList();

            // finding student in db if exists
            var existingStudent = studentsList.Find(x => x.Id == student.Id);

            if (existingStudent == null)
            {
                // preventing inserting students with age above 18
                if (student.Age > 18)
                {
                    return BadRequest("Students age max is 18");
                }

                // add student to list
                studentsList.Add(student);

                // updating redis db
                updateStudentsList(studentsList);

                return Ok(studentsList);
            }

            return BadRequest(string.Format("Student with id {0} is already exists", student.Id));

        }

        // POST - adding dummy data to redis db
        [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any, NoStore = false)]
        [HttpPost("Init Dummy Data")]
        public async Task<ActionResult<List<Student>>> AddDummyData()
        {
            initDBwithDummyValues();

            return Ok("Added dummy data");

        }

        // PUT - editing student data
        [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any, NoStore = false)]
        [HttpPut]
        public async Task<ActionResult<Student>> EditStudent(Student request)
        {
            var studentsList = await getStudentsList();
            var student = studentsList.Find(x => x.Id == request.Id);
            if (student == null)
            {
                return BadRequest(string.Format("Student with id {0} not found", request.Id));
            }

            // updating each property
            student.FirstName = request.FirstName;  
            student.LastName  = request.LastName;    
            student.Age = request.Age;  
            student.GradesAvg = request.GradesAvg;
            student.SchoolName = request.SchoolName;
            student.SchoolAdress = request.SchoolAdress;

            // updaing redis db
            updateStudentsList(studentsList);

            return Ok(studentsList);
        }

        // DELETE - deleting student from db
        [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any, NoStore = false)]
        [HttpDelete("{id}")]
        public async Task<ActionResult<List<Student>>> DeleteStudent(int id)
        {
            var studentsList = await getStudentsList();

            var student = studentsList.Find(x => x.Id == id);
            if (student == null)
            {
                return BadRequest(string.Format("Student with id {0} not found", id));
            }

            // remove student from list
            studentsList.Remove(student);

            // updaing redis db
            updateStudentsList(studentsList);

            return Ok(studentsList);
        }
    }
}
