using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.Data;
using SMS.Migrations;
using SMS.Models;
using SMS.ViewModels;
using DinkToPdf;
using DinkToPdf.Contracts;

namespace SMS.Controllers
{
    public class StudentController : Controller
    {
        private readonly SmsDbContext _Context;
        private readonly IConverter _pdfConverter;
        public StudentController(SmsDbContext context, IConverter pdfConverter)
        {
            _Context = context;
            _pdfConverter = pdfConverter;
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
            return RedirectToAction("Index");
        }
       

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
            var totalAmount = student.Payments.Sum(p => p.Amount);
            var paymentHistoryViewModel = new PaymentHistoryViewModel
            {
                StudentId = student.StudentId,
                StudentName = student.StudentName,
                Payments = student.Payments,
                TotalAmount = totalAmount
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

            var totalAmount = payments.Sum(p => p.Amount);

            ViewBag.TotalAmount = totalAmount; 
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

            var totalPayments = await _Context.Payments
                .Where(p => p.PaymentDate.Year >= startYear && p.PaymentDate.Year <= endYear
                            && p.PaymentDate.Month >= startMonthValue && p.PaymentDate.Month <= endMonthValue)
                .SumAsync(p => p.Amount);
            var totalExpenditures = await _Context.Expenditures
           .Where(e => e.Date.HasValue && e.Date.Value.Year >= startYear && e.Date.Value.Year <= endYear
                       && e.Date.Value.Month >= startMonthValue && e.Date.Value.Month <= endMonthValue)
           .SumAsync(e => e.Amount);

            var revenue = totalPayments - totalExpenditures;

            var model = new RevenueViewModel
            {
                StartMonth = startMonth,
                EndMonth = endMonth,
                TotalRevenue = revenue
            };

            return View(model);
        }



        /*  [HttpPost]
          public async Task<ActionResult> MonthlyRevenue(DateTime startMonth, DateTime endMonth)
          {
              int startYear = startMonth.Year;
              int startMonthValue = startMonth.Month;
              int endYear = endMonth.Year;
              int endMonthValue = endMonth.Month;

              var totalPayments = await _Context.Payments
                  .Where(p => p.PaymentDate.Year >= startYear && p.PaymentDate.Year <= endYear
                              && p.PaymentDate.Month >= startMonthValue && p.PaymentDate.Month <= endMonthValue)
                  .SumAsync(p => p.Amount);


              var totalExpenditures = await _Context.Expenditures
                  .Where(e => e.Date.HasValue && e.Date.Value.Year >= startYear && e.Date.Value.Year <= endYear
                              && e.Date.Value.Month >= startMonthValue && e.Date.Value.Month <= endMonthValue)
                  .SumAsync(e => e.Amount);


              var revenue = totalPayments - totalExpenditures;

              var model = new RevenueViewModel
              {
                  StartMonth = startMonth,
                  EndMonth = endMonth,
                  TotalRevenue = revenue
              };

              return View(model);
          }*/




        public ActionResult Expense()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Expense (ExpenditureViewModel expense)
        {
            Expenditure debit = new Expenditure()
            {
                Expenses = expense.Expenses,
                Amount = expense.Amount,
               /* Description =expense.Description,
                Date = expense.Date*/
            };
            await _Context.Expenditures.AddAsync(debit);
            await _Context.SaveChangesAsync();
            return RedirectToAction("GetExpense");
        }
        [HttpGet]
        public async Task<ActionResult> GetExpense()
        {

            var appoint = await _Context.Expenditures.ToListAsync();
            decimal totalAmount = appoint.Sum(e => e.Amount);

            ViewBag.TotalAmount = totalAmount;
            return View(appoint);
        }

    }
}

