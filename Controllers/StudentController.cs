using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.Data;
using SMS.Migrations;
using SMS.Models;
using SMS.ViewModels;
using DinkToPdf;
using System.Linq;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace SMS.Controllers
{
    [Authorize]
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


        /* [HttpGet]
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
         }*/

        [HttpGet]
        public async Task<ActionResult> GetStudents(string searchQuery, int page = 1)
        {
            const int PageSize = 5; // Number of items per page

            IQueryable<Student> students = _Context.Students;

            if (!string.IsNullOrEmpty(searchQuery))
            {
                // Filter students based on the search query
                students = students.Where(s => s.StudentName.Contains(searchQuery));
            }

            int totalItems = await students.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            // Apply pagination using Skip and Take
            var studentList = await students.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchQuery = searchQuery; // Pass searchQuery to the view for maintaining the search state



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

        /*        [HttpGet]
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

                }*/

        [HttpGet]
        public async Task<ActionResult> PaymentHistory(Guid studentId, int page = 1)
        {
            const int PageSize = 5; // Number of items per page

            var student = await _Context.Students
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
            {
                return NotFound();
            }

            var totalAmount = student.Payments.Sum(p => p.Amount);

            // Apply pagination using Skip and Take
            var payments = student.Payments
                .OrderByDescending(p => p.PaymentDate) // Optional: Order payments by date, adjust this based on your requirement
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var paymentHistoryViewModel = new PaymentHistoryViewModel
            {
                StudentId = student.StudentId,
                StudentName = student.StudentName,
                Payments = payments,
                TotalAmount = totalAmount
            };

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(student.Payments.Count / (double)PageSize);
            

            return View(paymentHistoryViewModel);
        }



















        [HttpGet]
        public async Task<ActionResult> MonthlyPayments()
        {
            
            DateTime startMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime endMonth = startMonth.AddMonths(1).AddDays(-1); // Last day of the current month

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

        [HttpPost]
        public async Task<ActionResult> MonthlyPayments(string startMonth, string endMonth, int page = 1)
        {
            DateTime startDate = DateTime.Parse(startMonth);
            DateTime endDate = DateTime.Parse(endMonth).AddDays(1).AddTicks(-1); // Include end date in the query

            const int PageSize = 3; // Number of items per page

            var payments = await _Context.Payments
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .Include(p => p.Student)
                .OrderByDescending(p => p.PaymentDate) // Order by payment date, adjust as needed
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            int totalItems = await _Context.Payments.CountAsync(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate);
            int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.StartDate = startMonth;
            ViewBag.EndDate = endMonth;

            var totalAmount = payments.Sum(p => p.Amount);
            ViewBag.TotalAmount = totalAmount;

            return View(payments);
        }




        /* [HttpPost]
         public async Task<ActionResult> MonthlyPayments(DateTime startMonth, DateTime endMonth)
         {
             // Set the end date to be the end of the selected day
             endMonth = endMonth.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

             var payments = await _Context.Payments
                 .Where(p => p.PaymentDate >= startMonth && p.PaymentDate <= endMonth)
                 .Include(p => p.Student)
                 .ToListAsync();

             var totalAmount = payments.Sum(p => p.Amount);

             ViewBag.TotalAmount = totalAmount;

             if (payments.Any())
             {
                 return View(payments);
             }
             else
             {
                 // If no data is found, return an empty list to the view
                 return View(new List<Payment>());
             }
         }*/

























        /* [HttpGet]
         public ActionResult MonthlyRevenue()
         {
             return View(new RevenueViewModel());
         }*/

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



        /*[HttpPost]*/
        /* public async Task<ActionResult> MonthlyRevenue(DateTime startMonth, DateTime endMonth)
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
 */



        [HttpGet]
        public async Task<IActionResult> MonthlyRevenue()
        {
            DateTime currentDate = DateTime.Now;
            DateTime startMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            DateTime endMonth = startMonth.AddMonths(1).AddDays(-1);

            var totalPaymentsForMonth = await _Context.Payments
                .Where(p => p.PaymentDate >= startMonth && p.PaymentDate <= endMonth)
                .SumAsync(p => p.Amount);

            var totalExpenditureForMonth = await _Context.Expenditures
                .Where(e => e.Date.HasValue && e.Date.Value >= startMonth && e.Date.Value <= endMonth)
                .SumAsync(e => e.Amount);

            var totalRevenueForMonth = totalPaymentsForMonth - totalExpenditureForMonth;

            var startYear = new DateTime(currentDate.Year, 1, 1);
            var endYear = new DateTime(currentDate.Year, 12, 31);

            var totalPaymentsForYear = await _Context.Payments
                .Where(p => p.PaymentDate >= startYear && p.PaymentDate <= endYear)
                .SumAsync(p => p.Amount);

            var totalExpenditureForYear = await _Context.Expenditures
                .Where(e => e.Date.HasValue && e.Date.Value.Year == currentDate.Year)
                .SumAsync(e => e.Amount);

            var totalAllExpenditures = await _Context.Expenditures
                .Where(e => e.Date.HasValue)
                .SumAsync(e => e.Amount);

            var model = new RevenueViewModel
            {
                StartMonth = startMonth,
                EndMonth = endMonth,
                TotalPaymentsForMonth = totalPaymentsForMonth,
                TotalExpenditureForMonth = totalExpenditureForMonth,
                TotalRevenueForMonth = totalRevenueForMonth,
                TotalPaymentsForYear = totalPaymentsForYear,
                TotalExpenditureForYear = totalExpenditureForYear,
                TotalAllExpenditures = totalAllExpenditures
            };

            return View(model);
        }









        /* [HttpPost]
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

             var totalAllExpenditures = await _Context.Expenditures
                 .Where(e => e.Date.HasValue)
                 .SumAsync(e => e.Amount);

             var revenue = totalPayments - totalExpenditures;

             var model = new RevenueViewModel
             {
                 StartMonth = startMonth,
                 EndMonth = endMonth,
                 TotalRevenue = revenue,
                 TotalAllExpenditures = totalAllExpenditures
             };

             return View(model);
         }
 */


        /* [HttpGet]
         public async Task<ActionResult> MonthlyRevenue()
         {
             DateTime currentDate = DateTime.Now;
             DateTime startMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
             DateTime endMonth = startMonth.AddMonths(1).AddDays(-1);

             // Calculate total revenue for the current month
             var totalPayments = await _Context.Payments
                 .Where(p => p.PaymentDate >= startMonth && p.PaymentDate <= endMonth)
                 .SumAsync(p => p.Amount);

             // Calculate total revenue for the current year
             DateTime startYear = new DateTime(currentDate.Year, 1, 1);
             DateTime endYear = new DateTime(currentDate.Year, 12, 31);
             var totalYearPayments = await _Context.Payments
                 .Where(p => p.PaymentDate >= startYear && p.PaymentDate <= endYear)
                 .SumAsync(p => p.Amount);

             var totalExpenditures = await _Context.Expenditures
                 .Where(e => e.Date.HasValue && e.Date.Value >= startMonth && e.Date.Value <= endMonth)
                 .SumAsync(e => e.Amount);

             var totalAllExpenditures = await _Context.Expenditures
                 .Where(e => e.Date.HasValue)
                 .SumAsync(e => e.Amount);

             var revenue = totalPayments - totalExpenditures;
             var yearRevenue = totalYearPayments - totalExpenditures;

             var model = new RevenueViewModel
             {
                 StartMonth = startMonth,
                 EndMonth = endMonth,
                 TotalRevenue = revenue,
                 TotalYearRevenue = yearRevenue,
                 TotalAllExpenditures = totalAllExpenditures
             };

             return View(model);
         }*/

        /*[HttpGet]
        public async Task<IActionResult>
  MonthlyRevenue()
        {
            DateTime currentDate = DateTime.Now;
            DateTime startMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            DateTime endMonth = startMonth.AddMonths(1).AddDays(-1);

            var totalPayments = await _Context.Payments
            .Where(p => p.PaymentDate >= startMonth && p.PaymentDate <= endMonth)
            .SumAsync(p => p.Amount);

            var startYear = new DateTime(currentDate.Year, 1, 1);
            var endYear = new DateTime(currentDate.Year, 12, 31);

            var totalYearPayments = await _Context.Payments
            .Where(p => p.PaymentDate >= startYear && p.PaymentDate <= endYear)
            .SumAsync(p => p.Amount);

            var totalExpenditureForMonth = await _Context.Expenditures
            .Where(e => e.Date.HasValue && e.Date.Value >= startMonth && e.Date.Value <= endMonth)
            .SumAsync(e => e.Amount);

            var totalExpenditureForYear = await _Context.Expenditures
            .Where(e => e.Date.HasValue && e.Date.Value.Year == currentDate.Year)
            .SumAsync(e => e.Amount);

            var totalAllExpenditures = await _Context.Expenditures
            .Where(e => e.Date.HasValue)
            .SumAsync(e => e.Amount);

            var model = new RevenueViewModel
            {
                StartMonth = startMonth,
                EndMonth = endMonth,
                TotalPaymentsForMonth = totalPayments,
                TotalPaymentsForYear = totalYearPayments,
                TotalExpenditureForMonth = totalExpenditureForMonth,
                TotalExpenditureForYear = totalExpenditureForYear,
                TotalAllExpenditures = totalAllExpenditures
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


        /*[HttpGet]
        public async Task<ActionResult> GetExpense()
        {

            var appoint = await _Context.Expenditures.ToListAsync();
            decimal totalAmount = appoint.Sum(e => e.Amount);

            ViewBag.TotalAmount = totalAmount;
            return View(appoint);
        }*/

        [HttpGet]
        public async Task<ActionResult> GetExpense(int page = 1)
        {
            const int PageSize = 5; // Number of items per page

            var expenditures = await _Context.Expenditures.ToListAsync();
            decimal totalAmount = expenditures.Sum(e => e.Amount);

            var paginatedExpenditures = expenditures.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            ViewBag.TotalAmount = totalAmount;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(expenditures.Count / (double)PageSize);

            return View(paginatedExpenditures);
        }


    }
}

