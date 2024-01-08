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
        private bool StudentExists(Guid id)
        {
            return _Context.Students.Any(e => e.StudentId == id);
        }
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Register(StudentViewModel studentdto)
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
        public async Task<IActionResult> Edit(Guid? studentId)
        {
            if (studentId == null || studentId == Guid.Empty)
            {
                return NotFound();
            }

            var student = await _Context.Students.FindAsync(studentId);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? studentId, [Bind("StudentId,StudentName,Email,Phone,Address")] Student student)
        {
            if (studentId == null || studentId == Guid.Empty)
            {
                return NotFound();
            }

            if (studentId != student.StudentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _Context.Update(student);
                    await _Context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {

                    throw;
                }
            }

            return View(student);
        }


        [HttpGet]
        public async Task<ActionResult> AddPayment(Guid studentId)
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
                Payments = student.Payments.OrderByDescending(p => p.PaymentDate).ToList(),
                TotalAmount = totalAmount
            };

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(student.Payments.Count / (double)PageSize);


            return View(paymentHistoryViewModel);
        }








        public IActionResult Receipt(Guid paymentId)
        {
            var payment = _Context.Payments
                .Include(p => p.Student)
                .FirstOrDefault(p => p.PaymentId == paymentId);

            if (payment == null)
            {
                return NotFound();
            }

            var receiptViewModel = new PaymentViewModel
            {
                TransactionId = payment.PaymentId,
                AmountPaid = payment.Amount,
                MonthPaid = payment.Date?.ToString("MMMM yyyy"),
                StudentName = payment.Student.StudentName,
                ReceiptDateTime = payment.PaymentDate.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return View(receiptViewModel);
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




        public ActionResult Expense()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Expense(ExpenditureViewModel expense)
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

