/// MasterDataController.cs
using ASC.Business.Interfaces;
using ASC.Model.Models;
using ASC.Utilities;
using ASC.Web.Areas.Configuration.Models;
using ASC.Web.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
namespace ASC.Web.Areas.Configuration.Controllers
{
    [Area("Configuration")]
    [Authorize(Roles = "Admin")]
    public class MasterDataController : BaseController
    {
        private readonly IMasterDataOperations _masterData;
        private readonly IMapper _mapper;

        public MasterDataController(IMasterDataOperations masterData, IMapper mapper)
        {
            _masterData = masterData;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> MasterKeys()
        {
            // Fetch all master keys from data store
            var masterKeys = await _masterData.GetAllMasterKeysAsync();
            var viewModels = _mapper.Map<List<MasterDataKey>, List<MasterDataKeyViewModel>>(masterKeys);

            // Store the list in session for reuse on postbacks
            HttpContext.Session.SetSession("MasterKeys", viewModels);

            // Prepare view model
            var model = new MasterKeysViewModel
            {
                MasterKeys = viewModels.ToList(),
                IsEdit = false,
                MasterKeyInContext = new MasterDataKeyViewModel()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterKeys(MasterKeysViewModel model)
        {
            // Restore stored list to repopulate table in case of validation errors
            model.MasterKeys = HttpContext.Session.GetSession<List<MasterDataKeyViewModel>>("MasterKeys");

            if (!ModelState.IsValid)
                return View(model);

            // Map view model back to domain entity
            var entity = _mapper.Map<MasterDataKeyViewModel, MasterDataKey>(model.MasterKeyInContext);

                if (model.IsEdit)
                {
                    // Update existing master key
                    await _masterData.UpdateMasterKeyAsync(model.MasterKeyInContext.PartitionKey, entity);
                }
            else
            {
                // Set new entity properties
                entity.RowKey = Guid.NewGuid().ToString();
                entity.PartitionKey = entity.Name;
                entity.CreatedBy = HttpContext.User.GetCurrentUserDetails().Name;

                // Insert new master key
                await _masterData.InsertMasterKeyAsync(entity);
            }
            return RedirectToAction("MasterKeys");
        }
        [HttpGet]
        public async Task<IActionResult> MasterValues()
        {
            // Lấy tất cả các khóa chính và lưu trữ chúng trong ViewBag để sử dụng cho thẻ Select
            ViewBag.MasterKeys = await _masterData.GetAllMasterKeysAsync();
            return View(new MasterValuesViewModel
            {
                MasterValues = new List<MasterDataValueViewModel>(),
                IsEdit = false
            });
        }
        [HttpGet]
        public async Task<IActionResult> MasterValuesByKey(string key)
        {
            // Get Master values based on master key.
            return Json(new { data = await _masterData.GetAllMasterValuesByKeyAsync(key) });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterValues(bool isEdit, MasterDataValueViewModel masterValue)
        {
            if (!ModelState.IsValid)
            {
                return Json("Error");
            }
            var masterDataValue = _mapper.Map<MasterDataValueViewModel, MasterDataValue>(masterValue);
            if (isEdit)
            {
                // Update Master Value
                await _masterData.UpdateMasterValueAsync(masterDataValue.PartitionKey, masterDataValue.RowKey, masterDataValue);
            }
            else
            {
                // Insert Master Value
                masterDataValue.RowKey = Guid.NewGuid().ToString();
                await _masterData.InsertMasterValueAsync(masterDataValue);
            }
            return Json(true);
        }
    }
}