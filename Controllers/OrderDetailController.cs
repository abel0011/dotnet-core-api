using Microsoft.AspNetCore.Mvc;
using dotnet_core_api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace dotnet_core_api.Controllers
{

    // Ruta base para todos los endpoints /api/order_details/*
    [Authorize]
    [ApiController]
    [Route("api/order_details/")]
    public class OrderDetailController : ControllerBase
    {

        private DB_PAMYSContext db = new DB_PAMYSContext();

        // Todos los endpoints son funciones asincronas, para mejorar
        // el performance, aun falta ver una manera de retornar los estados http
        // y validar errores.
        [Route("all")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IEnumerable<OrderDetail>> getAll()
        {
            return await Task.Run<IEnumerable<OrderDetail>>(() =>
            {
                var orderDetail = this.db.OrderDetails.ToList();
                orderDetail.ForEach(e =>
                {
                    e.id = new OrderDetailsPK { idOrder = e.IdOrder, idProduct = e.IdProduct };
                    e.product = this.db.Products.Find(e.IdProduct);
                    e.product.productsImages = this.db.ProductImages.Where(p => p.IdProduct == e.IdProduct).ToList();
                });
                return orderDetail;
            });
        }

        // Recibe el parametro id
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDetail>> getById(int id)
        {
            return await Task.Run<ActionResult<OrderDetail>>(() =>
            {
                var orderDetail = this.db.OrderDetails.Find(id);

                if (orderDetail != null)
                {
                    orderDetail.id = new OrderDetailsPK { idOrder = orderDetail.IdOrder, idProduct = orderDetail.IdProduct };
                    orderDetail.product = this.db.Products.Find(orderDetail.IdProduct);
                    orderDetail.product.category = this.db.Categorys.Find(orderDetail.product.IdCategory);
                    orderDetail.product.vendor = this.db.Vendors.Find(orderDetail.product.IdVendor);
                    orderDetail.product.productsImages = this.db.ProductImages.Where(e => e.IdProduct == orderDetail.IdProduct).ToList();
                    return Ok(orderDetail);
                }
                else
                    return NotFound();
            });
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderDetail>> save(OrderDetail orderDetail)
        {
            return await Task.Run<ActionResult<OrderDetail>>(() =>
            {
                if (orderDetail == null)
                    return BadRequest();
                this.db.OrderDetails.Add(orderDetail);
                this.db.SaveChanges();
                orderDetail.id = new OrderDetailsPK { idOrder = orderDetail.IdOrder, idProduct = orderDetail.IdProduct };
                orderDetail.product = this.db.Products.Find(orderDetail.IdProduct);
                orderDetail.product.category = this.db.Categorys.Find(orderDetail.product.IdCategory);
                orderDetail.product.vendor = this.db.Vendors.Find(orderDetail.product.IdVendor);
                orderDetail.product.productsImages = this.db.ProductImages.Where(e => e.IdProduct == orderDetail.IdProduct).ToList();
                return Ok(orderDetail);
            });
        }

        [HttpPut]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDetail>> update(OrderDetail orderDetail)
        {
            return await Task.Run<ActionResult<OrderDetail>>(() =>
            {
                try
                {
                    var updateTask = this.db.OrderDetails.Update(orderDetail);
                    if (updateTask.State == EntityState.Modified)
                        this.db.SaveChanges();
                    orderDetail.id = new OrderDetailsPK { idOrder = orderDetail.IdOrder, idProduct = orderDetail.IdProduct };
                    orderDetail.product = this.db.Products.Find(orderDetail.IdProduct);
                    orderDetail.product.category = this.db.Categorys.Find(orderDetail.product.IdCategory);
                    orderDetail.product.vendor = this.db.Vendors.Find(orderDetail.product.IdVendor);
                    orderDetail.product.productsImages = this.db.ProductImages.Where(e => e.IdProduct == orderDetail.IdProduct).ToList();
                    return Ok(orderDetail);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
            });
        }

        [HttpDelete("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> delete(int id)
        {
            var orderDetail = this.db.OrderDetails.Find(id);
            return await Task.Run<IActionResult>(() =>
            {
                try
                {
                    var deleteTask = this.db.OrderDetails.Remove(orderDetail);
                    if (deleteTask.State == EntityState.Deleted)
                        this.db.SaveChanges();
                    return Ok();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
            });
        }

    }
}
