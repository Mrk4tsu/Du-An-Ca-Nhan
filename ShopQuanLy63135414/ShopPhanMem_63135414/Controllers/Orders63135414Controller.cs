using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ShopPhanMem_63135414.Models;

namespace ShopPhanMem_63135414.Controllers
{
    public class Orders63135414Controller : Controller
    {
        private QLPM63135414Entities db = new QLPM63135414Entities();

        // GET: Orders63135414
        public async Task<ActionResult> Index()
        {
            // Lấy danh sách các đơn hàng đã thanh toán
            var paidOrders = await db.Orders
                .Include(o => o.User)
                .Where(o => o.Status == "Đã thanh toán")
                .ToListAsync();

            // Lấy danh sách toàn bộ đơn hàng
            var allOrders = await db.Orders
                .Include(o => o.User)
                .ToListAsync();

            // Truyền danh sách đơn hàng đã thanh toán và toàn bộ đơn hàng đến view
            ViewBag.PaidOrders = paidOrders;
            ViewBag.AllOrders = allOrders;

            return View();
        }

        // GET: Orders63135414/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = await db.Orders.FindAsync(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: Orders63135414/Create
        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(db.Users, "userId", "roleId");
            return View();
        }

        // POST: Orders63135414/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "OrderId,UserId,OrderDate,TotalAmount,Status")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Orders.Add(order);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.Users, "userId", "roleId", order.UserId);
            return View(order);
        }

        // GET: Orders63135414/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = await db.Orders.FindAsync(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.Users, "userId", "roleId", order.UserId);
            return View(order);
        }

        // POST: Orders63135414/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "OrderId,UserId,OrderDate,TotalAmount,Status")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.Users, "userId", "roleId", order.UserId);
            return View(order);
        }

        // GET: Orders63135414/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = await db.Orders.FindAsync(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Orders63135414/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Order order = await db.Orders.FindAsync(id);
            db.Orders.Remove(order);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
