using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebFinalProj.Models;

namespace WebFinalProj.Controllers
{
    public class ProductsController : Controller
    {
        private ShoppingCartsEntities db = new ShoppingCartsEntities();

        public ActionResult ShoppingCart()
        {
            string UserName = User.Identity.GetUserName();
            var orderDetails = db.OrderDetail.Where(m => m.UserName == UserName).ToList();
            return View(orderDetails);
        }

        [HttpPost]
        public ActionResult ShoppingCart(string Receiver, string Email, string Address)
        {
            string UserName = User.Identity.GetUserName();
            string guid = Guid.NewGuid().ToString("N");
            //加入訂單至 table_Order 資料表
            var order = new Order();
            order.Id = Guid.NewGuid().ToString();
            order.OrderGuid = guid;
            order.UserName = UserName;
            order.Receiver = Receiver;
            order.Email = Email;
            order.Address = Address;
            order.Date = DateTime.Now;
            order.ImageURL = "";
            db.Order.Add(order);

            //訂單加入後，需一併更新訂單明細內容
            var cartList = db.OrderDetail.Where(m => m.UserName == UserName).ToList();
            foreach (var item in cartList)
            {
                item.OrderGuid = guid;
            }
            db.SaveChanges();
            return RedirectToAction("OrderList");
        }
        public ActionResult AddCart(int ProductId)
        {
            //取得目前通過驗證的使用者名稱
            string UserName = User.Identity.GetUserName();

            //取得該使用者目前購物車內是否已有此商品，且尚未形成訂單的資料
            var currentCar = db.OrderDetail
                .Where(m => m.ProductId == ProductId && m.UserName == UserName).FirstOrDefault();
            if (currentCar == null)
            {
                //如果篩選條件資料為null，代表要新增全新一筆訂單明細資料
                //將產品資料欄位一一對照至訂單明細的欄位
                var products = db.Products.Where(m => m.Id == ProductId).FirstOrDefault();
                var orderDetail = new OrderDetail();
                orderDetail.Id = Guid.NewGuid().ToString();
                orderDetail.OrderGuid = Guid.NewGuid().ToString("N");
                orderDetail.UserName = UserName;
                orderDetail.ProductId = products.Id;
                orderDetail.Name = products.Name;
                orderDetail.Price = products.Price;
                orderDetail.Quantity = 1;
                orderDetail.ImageURL = products.ImageURL;
                db.OrderDetail.Add(orderDetail);
            }
            else
            {
                //如果購物車已有此商品，僅需將數量加1
                currentCar.Quantity++;
            }

            //儲存資料庫並導至購物車檢視頁面
            db.SaveChanges();
            return RedirectToAction("ShoppingCart");
        }

        public ActionResult OrderList()
        {
            string UserName = User.Identity.GetUserName();
            var orders = db.Order.Where(m => m.UserName == UserName).OrderByDescending(m => m.Date).ToList();
            return View(orders);
        }

        public ActionResult OrderDetail(string OrderGuid)
        {
            var orderDetails = db.OrderDetail.Where(m => m.OrderGuid == OrderGuid).ToList();
            return View(orderDetails);
        }
        public ActionResult DeleteCart(string Id)
        {
            var orderDetails = db.OrderDetail.Where(m => m.Id == Id).FirstOrDefault();
            db.OrderDetail.Remove(orderDetails);
            db.SaveChanges();
            return RedirectToAction("ShoppingCart");
        }
        public ActionResult UploadCloth()
        {
            var UserName = User.Identity.GetUserName();
            var UploadList = db.Products.Where(m => m.UserName == UserName).ToList();
            if (UploadList.Count == 0)
            {
                TempData["ResultMessage"] = "您尚未上傳商品";
                return RedirectToAction("Index");
            }
            else
            {
                return View(UploadList);
            }
        }

        // GET: Products
        public ActionResult Index()
        {
            return View(db.Products.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                TempData["ResultMessage"] = "指定資料不存在，請重新操作";
                return RedirectToAction("Index");
            }
            Products products = db.Products.Find(id);
            if (products == null)
            {
                TempData["ResultMessage"] = "指定資料不存在，請重新操作";
                return RedirectToAction("Index");
            }
            return View(products);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Price,ImageURL,Description,PublishDate,Quantity,UserName")] Products products)
        {
            if (ModelState.IsValid)
            {
                products.UserName = User.Identity.GetUserName();
                products.PublishDate = DateTime.Now;
                db.Products.Add(products);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(products);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                //無資料則顯示錯誤訊息並回傳到Index頁面
                TempData["ResultMessage"] = "指定資料不存在，無法編輯，請重新操作";
                return RedirectToAction("Index");
            }
            Products products = db.Products.Find(id);
            if (products == null)
            {
                TempData["ResultMessage"] = "指定資料不存在，無法編輯，請重新操作";
                return RedirectToAction("Index");
            }
            return View(products);
        }

        // POST: Products/Edit/5
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Price,ImageURL,Description,PublishDate,Quantity,UserName")] Products products)
        {
            if (ModelState.IsValid)
            {
                products.PublishDate = DateTime.Now;
                products.UserName = User.Identity.GetUserName();
                db.Entry(products).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(products);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                TempData["ResultMessage"] = "指定資料不存在，無法刪除，請重新操作";
                return RedirectToAction("Index");
            }
            Products products = db.Products.Find(id);
            if (products == null)
            {
                TempData["ResultMessage"] = "指定資料不存在，無法刪除，請重新操作";
                return RedirectToAction("Index");
            }
            return View(products);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Products products = db.Products.Find(id);
            db.Products.Remove(products);
            db.SaveChanges();
            TempData["ResultMessage"] = string.Format("商品[{0}]成功刪除", products.Name);
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
