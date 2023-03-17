using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _19T1021037.DomainModels;
using _19T1021037.BusinessLayers;

namespace _19T1021037.Web.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Authorize]
    [RoutePrefix("product")]
    public class ProductController : Controller
    {
        private const int PAGE_SIZE = 5;
        private const string PRODUCT_SEARCH = "ProductSearchCondition";

        /// <summary>
        /// Tìm kiếm, hiển thị mặt hàng dưới dạng phân trang
        /// </summary>
        /// <returns></returns>
        //public ActionResult Index(int? categoryID, int? supplierID)
        public ActionResult Index()
        {
            Models.PaginationSearchInput condition = Session[PRODUCT_SEARCH] as Models.PaginationSearchInput;
            if(condition == null)
            {
                condition = new Models.PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = "",
                    //CategoryID = categoryID ?? 0,
                    //SupplierID = supplierID ?? 0
                };
            }    
            return View(condition);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public ActionResult Search(Models.PaginationSearchInput condition)
        {
            int rowCount = 0;
            var data = ProductDataService.ListOfProducts(condition.Page, condition.PageSize, condition.SearchValue, condition.CategoryID, condition.SupplierID, out rowCount);
            var result = new Models.ProductSearchOutput()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data,
                //CategoryID = condition.CategoryID,
                //SupplierID = condition.SupplierID
            };

            Session[PRODUCT_SEARCH] = condition;

            return View(result);
        }
        /// <summary>
        /// Tạo mặt hàng mới
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            //ViewBag.Title = "Bổ sung mặt hàng";
            Product data = new Product()
            {
                ProductID = 0
            };
            return View("Edit", data);
        }
        /// <summary>
        /// Cập nhật thông tin mặt hàng, 
        /// Hiển thị danh sách ảnh và thuộc tính của mặt hàng, điều hướng đến các chức năng
        /// quản lý ảnh và thuộc tính của mặt hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>        
        public ActionResult Edit(int id = 0)
        {
            //ViewBag.Title = "Cập nhật nhà cung cấp";
            if (id == 0)
                return RedirectToAction("Index");
            var data = ProductDataService.GetProduct(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(Product data, HttpPostedFileBase uploadPhoto)
        {
            if (string.IsNullOrWhiteSpace(data.ProductName))
                ModelState.AddModelError(nameof(data.ProductName), "Tên mặt hàng không được để trống");

            if (string.IsNullOrWhiteSpace(data.Unit))
                ModelState.AddModelError(nameof(data.Unit), "Đơn vị không được để trống");

            if (data.CategoryID == 0)
                ModelState.AddModelError(nameof(data.CategoryID), "Vui lòng chọn loại hàng");

            if (data.SupplierID == 0)
                ModelState.AddModelError(nameof(data.SupplierID), "Vui lòng chọn nhà cung cấp");

            if (data.Price <= 0)
                ModelState.AddModelError(nameof(data.Price), "Giá không được bằng không");

            data.Photo = data.Photo ?? "";

            //if (!ModelState.IsValid)
            //{
            //    ViewBag.Title = data.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật mặt hàng";
            //    return View("Edit", data);
            //}

            if (!ModelState.IsValid)
            {
                if (data.ProductID == 0)
                {
                    ViewBag.Title = "Bổ sung mặt hàng";
                    return View("Create", data);
                }
                if (data.ProductID != 0)
                {
                    ViewBag.Title = "Cập nhật mặt hàng";
                    return View("Edit", data);
                }
            }

            if (uploadPhoto != null)
            {
                string folder = Server.MapPath("~/Image/Product");
                string fileName = $"{DateTime.Now.Ticks}-{uploadPhoto.FileName}";
                string filePath = System.IO.Path.Combine(folder, fileName);
                uploadPhoto.SaveAs(filePath);
                data.Photo = fileName;
            }

            if (data.ProductID == 0)
            {
                int newProductID = ProductDataService.AddProduct(data);
                return RedirectToAction($"Edit/{newProductID}");
            }
            else
            {
                ProductDataService.UpdateProduct(data);
                return RedirectToAction("Index");
            }    
            
        }
        /// <summary>
        /// Xóa mặt hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>        
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
                return RedirectToAction("Index");

            if (Request.HttpMethod == "POST")
            {
                ProductDataService.DeleteProduct(id);
                return RedirectToAction("Index");
            }   
            else
            {
                var data = ProductDataService.GetProduct(id);
                if (data == null)
                    return RedirectToAction("Index");
                return View(data);
            }   
        }

        /// <summary>
        /// Các chức năng quản lý ảnh của mặt hàng
        /// </summary>
        /// <param name="method"></param>
        /// <param name="productID"></param>
        /// <param name="photoID"></param>
        /// <returns></returns>
        [Route("photo/{method?}/{productID?}/{photoID?}")]
        public ActionResult Photo(string method = "add", int productID = 0, long photoID = 0)
        {
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung ảnh";
                    ProductPhoto data = new ProductPhoto()
                    {
                        PhotoID = 0,
                        ProductID = productID,
                        DisplayOrder = 1
                    };
                    //data.ProductID = productID;
                    return View(data);

                case "edit":
                    ViewBag.Title = "Thay đổi ảnh";
                    var photo = ProductDataService.GetPhoto(photoID);
                    return View(photo);

                case "delete":
                    ProductDataService.DeletePhoto(photoID);
                    return RedirectToAction($"Edit/{productID}"); //return RedirectToAction("Edit", new { productID = productID });
                default:
                    return RedirectToAction("Index");
            }
        }
        /// <summary>
        /// Lưu ảnh
        /// </summary>
        /// <param name="productPhoto"></param>
        /// <param name="uploadPhoto"></param>
        /// <returns></returns>
        public ActionResult SavePhoto(ProductPhoto productPhoto, HttpPostedFileBase uploadPhoto)
        {
            if (string.IsNullOrWhiteSpace(productPhoto.Description))
                ModelState.AddModelError("Description", "Description is required");
            if (productPhoto.DisplayOrder == 0)
                ModelState.AddModelError("DisplayOrder", "DisplayOrder is required");

            if (uploadPhoto != null)
            {
                var path = Server.MapPath("~/SaveImg/SP");
                var fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";
                var filePath = System.IO.Path.Combine(path, fileName);
                uploadPhoto.SaveAs(filePath);
                productPhoto.Photo = fileName;
            }

            if (productPhoto.PhotoID == 0)
            {
                if (uploadPhoto == null)
                    ModelState.AddModelError("Photo", "Photo is required");

                if (!ModelState.IsValid)
                    return View("Photo", productPhoto);

                ProductDataService.AddPhoto(productPhoto);
            }
            else
            {
                if (!ModelState.IsValid)
                    return RedirectToAction("Photo", new { method = "edit", productID = productPhoto.ProductID, photoID = productPhoto.PhotoID });

                ProductDataService.UpdatePhoto(productPhoto);
            }

            return RedirectToAction("Edit", new { id = productPhoto.ProductID });
        }

        /// <summary>
        /// Các chức năng quản lý thuộc tính của mặt hàng
        /// </summary>
        /// <param name="method"></param>
        /// <param name="productID"></param>
        /// <param name="attributeID"></param>
        /// <returns></returns>
        [Route("attribute/{method?}/{productID}/{attributeID?}")]
        public ActionResult Attribute(string method = "add", int productID = 0, int attributeID = 0)
        {
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung thuộc tính";
                    ProductAttribute data = new ProductAttribute()
                    {
                        AttributeID = 0,
                        ProductID = productID,
                        DisplayOrder = 1

                    };
                    return View(data);

                case "edit":
                    ViewBag.Title = "Thay đổi thuộc tính";
                    ViewBag.ProductID = productID;
                    if (attributeID == 0)
                        return RedirectToAction("Index");

                    data = ProductDataService.GetAttribute(attributeID);

                    if (data == null)
                        return RedirectToAction("Index");

                    return View(data);
                case "delete":
                    ProductDataService.DeleteAttribute(attributeID);
                    return RedirectToAction($"Edit/{productID}"); //return RedirectToAction("Edit", new { productID = productID });
                default:
                    return RedirectToAction("Index");
            }
        }
        /// <summary>
        /// Lưu thuộc tính
        /// </summary>
        /// <param name="productAttribute"></param>
        /// <returns></returns>
        public ActionResult SaveAttribute(ProductAttribute productAttribute)
        {

            if (string.IsNullOrWhiteSpace(productAttribute.AttributeName))
                ModelState.AddModelError("AttributeName", "AttributeName is required");
            if (string.IsNullOrWhiteSpace(productAttribute.AttributeValue))
                ModelState.AddModelError("AttributeValue", "AttributeValue is required");
            if (productAttribute.DisplayOrder <= 0)
                ModelState.AddModelError("DisplayOrder", "DisplayOrder is required");

            if (productAttribute.AttributeID == 0)
            {
                if (!ModelState.IsValid)
                    return View("Attribute", productAttribute);

                ProductDataService.AddAttribute(productAttribute);
            }
            else
            {
                if (!ModelState.IsValid)
                    return RedirectToAction("Attribute", new { method = "edit", productID = productAttribute.ProductID, attributeID = productAttribute.AttributeID });

                ProductDataService.UpdateAttribute(productAttribute);
            }

            return RedirectToAction("Edit", new { id = productAttribute.ProductID });
        }
    }
}