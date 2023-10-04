using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.Data;
using SMS.Models;
using SMS.ViewModels;

namespace SMS.Controllers
{
    public class StudentController : Controller
    {
        private readonly SmsDbContext _Context;

        public StudentController(SmsDbContext context)
        {
            _Context = context;
        }

        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult>Register(StudentViewModel studentdto)
        {
    
            Student student = new Student()
            {
                StudentName = studentdto.StudentName,
                Email = studentdto.Email,
                Address = studentdto.Address,
                Phone = studentdto.Phone
            };
            await _Context.Students.AddAsync(student);
            await _Context.SaveChangesAsync();
            return RedirectToAction("");
        }
        /* [HttpGet]
         public async Task<ActionResult>GetStudents()
         {
             var stu = await _Context.Students.ToListAsync();
             return View(stu);
         }*/

        [HttpGet]
        public async Task<ActionResult> GetStudents(string searchQuery)
        {
            IQueryable<Student> students = _Context.Students;

            if (!string.IsNullOrEmpty(searchQuery))
            {
                // Filter students based on the search query
                students = students.Where(s => s.StudentName.Contains(searchQuery));
            }

            var studentList = await students.ToListAsync();
            return View(studentList);
        }




        [HttpGet]
        public async Task<ActionResult> AddPayment (Guid studentId)
        {
            var student = await _Context.Students.FirstOrDefaultAsync
                (s => s.StudentId == studentId);

            if (student == null)
            {
                return NotFound();
            }
            var paymentViewModel = new PaymentViewModel { StudentId = studentId };
            

            return View(paymentViewModel);
        }    

        [HttpPost]
        public async Task<ActionResult> AddPayment(PaymentViewModel paymentViewModel)
        {
            var student = await _Context.Students.FirstOrDefaultAsync(s => s.StudentId == paymentViewModel.StudentId);

            if (student == null)
            {
                return NotFound();
            }

            var payment = new Payment
            {
                Amount = paymentViewModel.Amount,
                PaymentDate = DateTime.Now,
                Date = paymentViewModel.Date
            };

            student.Payments.Add(payment);

            await _Context.SaveChangesAsync();

            return RedirectToAction("GetStudents");
        }

        [HttpGet]
        public async Task<ActionResult> PaymentHistory(Guid studentId)
        {
            var student = await _Context.Students.Include(s => s.Payments).FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
            {
                return NotFound();
            }
            var paymentHistoryViewModel = new PaymentHistoryViewModel
            {
                StudentId = student.StudentId,
                StudentName = student.StudentName,
                Payments = student.Payments,

                
            };

            return View(paymentHistoryViewModel);

        }
        [HttpGet]
        public ActionResult MonthlyPayments()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> MonthlyPayments(DateTime startMonth, DateTime endMonth)
        {
            
            int startYear = startMonth.Year;
            int startMonthValue = startMonth.Month;
            int endYear = endMonth.Year;
            int endMonthValue = endMonth.Month;

            var payments = await _Context.Payments
                .Where(p => p.PaymentDate.Year >= startYear && p.PaymentDate.Year <= endYear
                         && p.PaymentDate.Month >= startMonthValue && p.PaymentDate.Month <= endMonthValue)
                .Include(p => p.Student)
                .ToListAsync();

            return View(payments);
        }

        [HttpGet]
        public ActionResult MonthlyRevenue()
        {
            return View(new RevenueViewModel());
        }

        [HttpPost]
        public async Task<ActionResult> MonthlyRevenue(DateTime startMonth, DateTime endMonth)
        {
            int startYear = startMonth.Year;
            int startMonthValue = startMonth.Month;
            int endYear = endMonth.Year;
            int endMonthValue = endMonth.Month;

            var totalRevenue = await _Context.Payments
                .Where(p => p.PaymentDate.Year >= startYear && p.PaymentDate.Year <= endYear
                         && p.PaymentDate.Month >= startMonthValue && p.PaymentDate.Month <= endMonthValue)
                .SumAsync(p => p.Amount);

            var model = new RevenueViewModel
            {
                StartMonth = startMonth,
                EndMonth = endMonth,
                TotalRevenue = totalRevenue
            };

            return View(model);
        }


    }
}

