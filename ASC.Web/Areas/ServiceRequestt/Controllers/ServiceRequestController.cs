using System;
using System.Linq;
using System.Threading.Tasks;
using ASC.Business.Interfaces;
using ASC.Model.Models;
using ASC.Model.BaseType;
using ASC.Utilities;
using ASC.Web.Areas.ServiceRequests.Models;
using ASC.Web.Controllers;
using ASC.Web.Data;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequestt")]
    public class ServiceRequestController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IMapper _mapper;
        private readonly IMasterDataCacheOperations _masterData;

        public ServiceRequestController(
            IServiceRequestOperations serviceRequestOperations,
            IMapper mapper,
            IMasterDataCacheOperations masterData)
        {
            _serviceRequestOperations = serviceRequestOperations;
            _mapper = mapper;
            _masterData = masterData;
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequest()
        {
            // Đảm bảo cache đã có dữ liệu
            var cache = await _masterData.GetMasterDataCacheAsync();
            if (cache == null)
            {
                await _masterData.CreateMasterDataCacheAsync();
                cache = await _masterData.GetMasterDataCacheAsync();
            }

            // Lọc dropdown theo MasterKeys
            ViewBag.VehicleNames = cache.Values
                .Where(v => v.PartitionKey == MasterKeys.VehicleName.ToString())
                .ToList();
            ViewBag.VehicleTypes = cache.Values
                .Where(v => v.PartitionKey == MasterKeys.VehicleType.ToString())
                .ToList();

            return View(new NewServiceRequestViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ServiceRequest(NewServiceRequestViewModel request)
        {
            // Lấy lại cache để repopulate dropdown
            var cache = await _masterData.GetMasterDataCacheAsync();
            ViewBag.VehicleNames = cache.Values
                .Where(v => v.PartitionKey == MasterKeys.VehicleName.ToString())
                .ToList();
            ViewBag.VehicleTypes = cache.Values
                .Where(v => v.PartitionKey == MasterKeys.VehicleType.ToString())
                .ToList();

            if (!ModelState.IsValid)
            {
                // Có lỗi validate, trả về view kèm errors
                return View(request);
            }

            // Map sang entity ServiceRequest
            var entity = _mapper.Map<ServiceRequest>(request);

            // Set thêm các khóa, ngày tháng, trạng thái
            entity.PartitionKey = HttpContext.User.GetCurrentUserDetails().Email;
            entity.RowKey = Guid.NewGuid().ToString();
            entity.RequestedDate = request.RequestedDate ?? DateTime.UtcNow;
            entity.Status = Status.New.ToString();

            // Ghi vào database
            await _serviceRequestOperations.CreateServiceRequestAsync(entity);

            // Redirect về Dashboard trong cùng area
            return RedirectToAction("Dashboard", "Dashboard", new { area = "ServiceRequestt" });
        }
    }
}
