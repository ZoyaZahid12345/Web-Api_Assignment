using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DL.DbModels;
using System;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class SubjectController : ControllerBase
{
    private readonly StudentDbContext _context;

    public SubjectController(StudentDbContext context)
    {
        _context = context;
    }

    // POST /api/subjects
    [HttpPost]
    public async Task<IActionResult> AddSubject([FromBody] SubjectDbDto subjectDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.subjectDbDto.Add(subjectDto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubjectById), new { id = subjectDto.id }, subjectDto);
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    // PUT /api/subjects/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> EditSubject(int id, [FromBody] SubjectDbDto updatedSubject)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != updatedSubject.id)
            {
                return BadRequest("Subject ID mismatch");
            }

            _context.Entry(updatedSubject).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SubjectExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    // GET /api/subjects/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSubjectById(int id)
    {
        try
        {
            var subject = await _context.subjectDbDto.FindAsync(id);

            if (subject == null)
            {
                return NotFound();
            }

            return Ok(subject);
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    private bool SubjectExists(int id)
    {
        return _context.subjectDbDto.Any(e => e.id == id);
    }

    // DELETE /api/subjects/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubject(int id)
    {
        try
        {
            var subject = await _context.subjectDbDto.FindAsync(id);

            if (subject == null)
            {
                return NotFound("Subject not found.");
            }

            // Check if there are any associations with students
            var associatedStudents = _context.studentSubjectDbDto.Where(ss => ss.SubjectId == id).ToList();
            if (associatedStudents.Count > 0)
            {
                return BadRequest("Cannot delete subject with associated students.");
            }

            _context.subjectDbDto.Remove(subject);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    // GET /api/subjects
    [HttpGet]
    public async Task<IActionResult> GetAllSubjects()
    {
        try
        {
            var subjects = await _context.subjectDbDto.ToListAsync();

            if (subjects == null || subjects.Count == 0)
            {
                return NotFound("No subjects found.");
            }

            return Ok(subjects);
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }




}
