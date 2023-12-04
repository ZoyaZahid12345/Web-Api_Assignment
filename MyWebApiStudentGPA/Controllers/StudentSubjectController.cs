using DL.DbModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace MyWebApiStudentGPA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentSubjectController : Controller
    {
        private readonly StudentDbContext _context;

        public StudentSubjectController(StudentDbContext context)
        {
            _context = context;
        }

        // POST: api/student-subjects
        [HttpPost]
        public async Task<IActionResult> AssignSubjectToStudent([FromBody] StudentSubjectDbDto payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingStudent = await _context.studentDbDto.FindAsync(payload.SID);
            var existingSubject = await _context.subjectDbDto.FindAsync(payload.SubjectId);

            if (existingStudent == null || existingSubject == null)
            {
                return NotFound("Student or Subject not found.");
            }


            var studentSubjectDbDto = new StudentSubjectDbDto
            {
                SID = payload.SID,
                SubjectId = payload.SubjectId,
                GPA = payload.GPA,
                Marks = payload.Marks

            };

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,

            };

            var json = JsonSerializer.Serialize(studentSubjectDbDto, options);

            _context.studentSubjectDbDto.Add(studentSubjectDbDto);
            await _context.SaveChangesAsync();

            return Ok(json);
        }


        // PUT: api/student-subjects/{assignment_id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAssignment(int assignment_id, [FromBody] StudentSubjectDbDto updatedPayload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingAssignment = await _context.studentSubjectDbDto.FindAsync(assignment_id);

            if (existingAssignment == null)
            {
                return NotFound("Assignment not found.");
            }

            existingAssignment.GPA = updatedPayload.GPA;
            existingAssignment.Marks = updatedPayload.Marks;

            await _context.SaveChangesAsync();

            return Ok(existingAssignment);
        }


        // DELETE: api/student-subjects/{assignment_id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssignment(int assignment_id)
        {

            var existingAssignment = await _context.studentSubjectDbDto.FindAsync(assignment_id);

            if (existingAssignment == null)
            {
                return NotFound("Assignment not found.");
            }

            _context.studentSubjectDbDto.Remove(existingAssignment);
            await _context.SaveChangesAsync();

            return Ok("Assignment deleted successfully.");
        }


        [HttpGet("~/api/students/{studentId}/subjects")]
        public IActionResult GetStudentSubjects(int studentId)
        {
            var subjects = _context.studentSubjectDbDto
                .Where(ss => ss.SID == studentId)
                .Select(ss => new { ss.SubjectId, ss.GPA, ss.Marks })
                .ToList();

            return Ok(subjects);
        }


        // GET: api/students/{student_id}/subjects/{subject_id}/marks
        [HttpGet("~/api/students/{studentId}/subjects/{subjectId}/marks")]
        public IActionResult GetMarksForSubject(int studentId, int subjectId)
        {
            var marks = _context.studentSubjectDbDto
                .FirstOrDefault(ss => ss.SID == studentId && ss.SubjectId == subjectId)?
                .Marks;

            if (marks == null)
            {
                return NotFound("Marks for the specified subject and student not found.");
            }

            return Ok(new { Marks = marks });
        }


        // GET: api/students/{student_id}/marks
        [HttpGet("~/api/students/{studentId}/marks")]
        public IActionResult GetAllMarksForStudent(int studentId)
        {
            var marksForStudent = _context.studentSubjectDbDto
                .Where(ss => ss.SID == studentId)
                .Select(ss => new { ss.SubjectId, ss.Marks })
                .ToList();

            if (marksForStudent.Count == 0)
            {
                return NotFound("No marks found for the specified student.");
            }

            return Ok(marksForStudent);
        }


        // GET: api/students/{student_id}/gpa
        [HttpGet("~/api/students/{studentId}/gpa")]
        public IActionResult GetGPAForStudent(int studentId)
        {
            var studentSubjects = _context.studentSubjectDbDto
                .Where(ss => ss.SID == studentId)
                .ToList();

            if (studentSubjects.Count == 0)
            {
                return NotFound("No subjects found for the specified student.");
            }

            double totalCredits = 0;
            double totalGPA = 0;

            foreach (var subject in studentSubjects)
            {
                totalCredits += 1;
                totalGPA += subject.GPA;
            }
            double currentGPA = totalGPA / totalCredits;

            return Ok(new { GPA = currentGPA });
        }
    }
}