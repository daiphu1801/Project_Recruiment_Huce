using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Models;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý giao dịch SePay - sử dụng database thực
    /// </summary>
    public class TransactionsController : AdminBaseController
    {
        public ActionResult Index(string q, string gateway = null, int page = 1)
        {
            ViewBag.Title = "Giao dịch SePay";
            ViewBag.Breadcrumbs = new List<Tuple<string, string>> { new Tuple<string, string>("Transactions", null) };

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var query = db.SePayTransactions.AsQueryable();

                // Search by transaction content, code, reference code, or account number
                if (!string.IsNullOrWhiteSpace(q))
                {
                    query = query.Where(t =>
                        (t.TransactionContent != null && t.TransactionContent.Contains(q)) ||
                        (t.Code != null && t.Code.Contains(q)) ||
                        (t.ReferenceCode != null && t.ReferenceCode.Contains(q)) ||
                        (t.AccountNumber != null && t.AccountNumber.Contains(q))
                    );
                }

                // Filter by gateway
                if (!string.IsNullOrWhiteSpace(gateway))
                {
                    query = query.Where(t => t.Gateway == gateway);
                }

                // Pagination
                int pageSize = 20;
                int totalRecords = query.Count();
                int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var transactions = query
                    .OrderByDescending(t => t.TransactionDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new SePayTransactionVm
                    {
                        Id = t.Id,
                        Gateway = t.Gateway,
                        TransactionDate = t.TransactionDate,
                        AccountNumber = t.AccountNumber,
                        SubAccount = t.SubAccount,
                        AmountIn = t.AmountIn,
                        AmountOut = t.AmountOut,
                        Accumulated = t.Accumulated,
                        Code = t.Code,
                        TransactionContent = t.TransactionContent,
                        ReferenceCode = t.ReferenceCode,
                        Description = t.Description,
                        CreatedAt = t.CreatedAt
                    }).ToList();

                ViewBag.GatewayOptions = new SelectList(db.SePayTransactions
                    .Where(t => t.Gateway != null)
                    .Select(t => t.Gateway)
                    .Distinct()
                    .ToList());

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalRecords;
                ViewBag.PageSize = pageSize;

                return View(transactions);
            }
        }

        public ActionResult Details(int id)
        {
            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var transaction = db.SePayTransactions.FirstOrDefault(t => t.Id == id);
                if (transaction == null) return HttpNotFound();

                var vm = new SePayTransactionVm
                {
                    Id = transaction.Id,
                    Gateway = transaction.Gateway,
                    TransactionDate = transaction.TransactionDate,
                    AccountNumber = transaction.AccountNumber,
                    SubAccount = transaction.SubAccount,
                    AmountIn = transaction.AmountIn,
                    AmountOut = transaction.AmountOut,
                    Accumulated = transaction.Accumulated,
                    Code = transaction.Code,
                    TransactionContent = transaction.TransactionContent,
                    ReferenceCode = transaction.ReferenceCode,
                    Description = transaction.Description,
                    CreatedAt = transaction.CreatedAt
                };

                ViewBag.Title = "Chi tiết giao dịch";
                ViewBag.Breadcrumbs = new List<Tuple<string, string>> {
                    new Tuple<string, string>("Transactions", Url.Action("Index")),
                    new Tuple<string, string>($"#{vm.TransactionNo}", null)
                };

                return View(vm);
            }
        }
    }
}


